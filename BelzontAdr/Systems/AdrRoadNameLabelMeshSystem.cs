using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Entities;
using Colossal.Mathematics;
using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Rendering;
using Game.SceneFlow;
using Game.Serialization;
using Game.Tools;
using Game.UI;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace BelzontAdr
{
    /// <summary>
    /// LBL-02/03/04: ADR TMP bake, dual-membership LabelPosition placement, and FillNameMeshData batch.
    /// Re-diff baseline Game.dll GUID d124bec16c0748da9ebf48366d9c1d68 —
    /// AggregateMeshSystem.UpdateLabelVertices / UpdateLabelPositionsJob / FillNameMeshData / InitializePrefabs.
    /// </summary>
    public partial class AdrRoadNameLabelMeshSystem : GameSystemBase, IPreDeserialize
    {
        internal struct MaterialData
        {
            public Material m_Material;
            public bool m_IsUnderground;
            /// <summary>
            /// <see cref="AdrNetKind.None"/> = vanilla Aggregate outline; otherwise ADR outline-by-kind.
            /// </summary>
            public AdrNetKind m_AdrOutlineKind;
            public bool m_HasMesh;
        }

        internal class MeshData
        {
            public JobHandle m_DataDependencies;
            public Material m_OriginalMaterial;
            public List<MaterialData> m_Materials;
            public Mesh m_Mesh;
            public Mesh.MeshDataArray m_MeshData;
            public bool m_MeshDirty;
            public bool m_HasMeshData;
            public bool m_HasMesh;
        }

        private struct MaterialUpdate
        {
            public Entity m_Entity;
            public int m_Material;
        }

        private PrefabSystem m_PrefabSystem;
        private NameSystem m_NameSystem;
        private OverlayRenderSystem m_OverlayRenderSystem;
        private UndergroundViewSystem m_UndergroundViewSystem;

        private EntityQuery m_CreatedPrefabQuery;
        private EntityQuery m_AllNamePrefabQuery;
        private EntityQuery m_LabelQuery;
        private EntityQuery m_AdrOwnersMissingLabelsQuery;
        private EntityQuery m_TempAggregatesMissingLabelsQuery;
        private EntityQuery m_AllAdrAggregatesQuery;
        private EntityQuery m_S1PlacementQuery;
        private EntityQuery m_S2PlacementQuery;

        private List<MeshData> m_LabelData;
        private Dictionary<Entity, string> m_CachedLabels;
        /// <summary>ADR-owned NetNameData entities keyed by <see cref="AdrNetKind"/> (color metadata; not PrefabRef targets).</summary>
        private Entity[] m_AdrKindNamePrefabs;
        /// <summary>
        /// Real AggregateNet prefab entity that has <see cref="NetNameData"/> + PrefabData (safe PrefabRef target).
        /// Non-road ADR labels borrow this for material index; outline still follows stored <see cref="AdrNetKind"/>.
        /// </summary>
        private Entity m_SafeNameMaterialPrefab = Entity.Null;
        /// <summary>True until first successful PrefabRef repair pass after load / system create.</summary>
        private bool m_PrefabRefRepairPending = true;
        private int m_FaceColor;
        private int m_OutlineColor;
        private bool m_Loaded = true;
        private bool m_TunnelSelectOn;

        [Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_NameSystem = World.GetOrCreateSystemManaged<NameSystem>();
            m_OverlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
            m_UndergroundViewSystem = World.GetOrCreateSystemManaged<UndergroundViewSystem>();

            m_CreatedPrefabQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<Created>(),
                    ComponentType.ReadOnly<AggregateNetData>(),
                    ComponentType.ReadOnly<NetNameData>()
                }
            });
            m_AllNamePrefabQuery = GetEntityQuery(
                ComponentType.ReadOnly<AggregateNetData>(),
                ComponentType.ReadOnly<NetNameData>(),
                ComponentType.Exclude<Deleted>());

            m_LabelQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<LabelMaterial>() },
                Any = new[]
                {
                    ComponentType.ReadOnly<Aggregate>(),
                    ComponentType.ReadOnly<ADRNameAggregate>()
                },
                None = new[] { ComponentType.ReadOnly<Deleted>() }
            });

            m_AdrOwnersMissingLabelsQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<ADRNameAggregate>() },
                None = new[]
                {
                    ComponentType.ReadOnly<LabelMaterial>(),
                    ComponentType.ReadOnly<Deleted>()
                }
            });

            // Selection Temp Aggregate from AggregateNets without NetLabel (train/pathway) gets no Label*
            // archetype — original ADR is Hidden, so labels vanish unless we Ensure them on the Temp.
            m_TempAggregatesMissingLabelsQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<Aggregate>(),
                    ComponentType.ReadOnly<Temp>(),
                    ComponentType.ReadOnly<AggregateElement>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<LabelMaterial>(),
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<ADRNameAggregate>()
                }
            });

            m_AllAdrAggregatesQuery = GetEntityQuery(
                ComponentType.ReadOnly<ADRNameAggregate>(),
                ComponentType.Exclude<Deleted>());

            m_S1PlacementQuery = GetEntityQuery(
                ComponentType.ReadOnly<Aggregate>(),
                ComponentType.ReadOnly<LabelExtents>(),
                ComponentType.ReadOnly<AggregateElement>(),
                ComponentType.ReadWrite<LabelPosition>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<ADRNameAggregate>());

            m_S2PlacementQuery = GetEntityQuery(
                ComponentType.ReadOnly<ADRNameAggregate>(),
                ComponentType.ReadOnly<LabelExtents>(),
                ComponentType.ReadOnly<ADRNameAggregateElement>(),
                ComponentType.ReadWrite<LabelPosition>(),
                ComponentType.Exclude<Deleted>());

            m_FaceColor = Shader.PropertyToID("_FaceColor");
            m_OutlineColor = Shader.PropertyToID("_OutlineColor");
            GameManager.instance.localizationManager.onActiveDictionaryChanged += OnDictionaryChanged;
        }

        [Preserve]
        protected override void OnDestroy()
        {
            DestroyAdrKindNamePrefabs();
            DestroyLabelMaterials();
            if (GameManager.instance?.localizationManager != null)
            {
                GameManager.instance.localizationManager.onActiveDictionaryChanged -= OnDictionaryChanged;
            }
            base.OnDestroy();
        }

        public void PreDeserialize(Context context)
        {
            ClearLabelMeshes();
            DestroyAdrKindNamePrefabs();
            m_SafeNameMaterialPrefab = Entity.Null;
            m_PrefabRefRepairPending = true;
            m_CachedLabels?.Clear();
            m_Loaded = true;
        }

        private void OnDictionaryChanged() => ForceRegenerateLabels();

        /// <summary>
        /// Same path as <c>localizationManager.onActiveDictionaryChanged</c> /
        /// vanilla <c>AggregateMeshSystem.OnDictionaryChanged</c>: mark all road name
        /// label owners <see cref="Updated"/>, drop the rendered-string cache, and
        /// force a full vertex pass next update (survives Updated being cleared before
        /// this system runs in the same frame).
        /// </summary>
        internal void ForceRegenerateLabels()
        {
            EntityManager.AddComponent<Updated>(m_LabelQuery);
            m_CachedLabels?.Clear();
            m_Loaded = true;
        }

        [Preserve]
        protected override void OnUpdate()
        {
            bool loaded = GetLoaded();
            bool hadSafeNamePrefab = m_SafeNameMaterialPrefab != Entity.Null;
            bool needFullPrefabScan = m_LabelData == null || m_LabelData.Count == 0;
            if (needFullPrefabScan || !m_CreatedPrefabQuery.IsEmptyIgnoreFilter)
            {
                InitializePrefabs(fullScan: needFullPrefabScan);
            }

            bool kindPrefabsCreated = EnsureAdrKindNamePrefabs();
            EnsureAdrOwnersHaveLabelComponents();
            EnsureTempSelectionAggregatesHaveLabelComponents();
            bool safePrefabNewlyReady = !hadSafeNamePrefab && m_SafeNameMaterialPrefab != Entity.Null;
            if ((kindPrefabsCreated || safePrefabNewlyReady || m_PrefabRefRepairPending)
                && m_SafeNameMaterialPrefab != Entity.Null)
            {
                RefreshAdrOwnerNamePrefabRefs();
                m_PrefabRefRepairPending = false;
            }

            if (m_LabelData == null || m_LabelData.Count == 0)
            {
                Dependency = ScheduleLabelPlacement(Dependency);
                return;
            }

            bool undergroundOn = m_UndergroundViewSystem.undergroundOn && m_UndergroundViewSystem.tunnelsOn;
            if (undergroundOn != m_TunnelSelectOn)
            {
                UpdateUndergroundState(undergroundOn);
                m_TunnelSelectOn = undergroundOn;
            }

            if (loaded || !m_LabelQuery.IsEmptyIgnoreFilter)
            {
                UpdateLabelVertices(loaded);
            }

            bool placementPending = !m_S1PlacementQuery.IsEmptyIgnoreFilter || !m_S2PlacementQuery.IsEmptyIgnoreFilter;
            JobHandle handle = ScheduleLabelPlacement(Dependency);
            if (placementPending)
            {
                MarkAllNameMeshesDirty();
            }

            Dependency = FillNameMeshes(handle);
        }

        /// <summary>
        /// Real AggregateNet prefab entity with PrefabData + NetNameData (NetLabel archetype).
        /// Used so selection Temp Aggregates for train/pathway get Label* components from prefab.
        /// </summary>
        internal bool TryGetSafeNameMaterialPrefab(out Entity prefab)
        {
            prefab = m_SafeNameMaterialPrefab;
            return prefab != Entity.Null
                && EntityManager.Exists(prefab)
                && EntityManager.HasComponent<PrefabData>(prefab)
                && EntityManager.HasComponent<NetNameData>(prefab);
        }

        internal int GetNameMaterialCount()
        {
            return m_LabelData?.Count ?? 0;
        }

        internal bool TryGetNameMesh(int index, out Mesh mesh, out int subMeshCount)
        {
            mesh = null;
            subMeshCount = 0;
            if (m_LabelData == null || index < 0 || index >= m_LabelData.Count)
            {
                return false;
            }

            MeshData meshData = m_LabelData[index];
            if (meshData.m_Materials == null)
            {
                return false;
            }

            subMeshCount = meshData.m_Materials.Count;
            if (meshData.m_HasMeshData)
            {
                meshData.m_HasMeshData = false;
                meshData.m_DataDependencies.Complete();
                meshData.m_DataDependencies = default;
                if (meshData.m_Mesh == null)
                {
                    meshData.m_Mesh = new Mesh();
                    if (meshData.m_OriginalMaterial != null)
                    {
                        meshData.m_Mesh.name = $"ADR Aggregates ({meshData.m_OriginalMaterial})";
                    }
                }

                Mesh.ApplyAndDisposeWritableMeshData(
                    meshData.m_MeshData,
                    meshData.m_Mesh,
                    MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);

                Bounds bounds = default;
                meshData.m_HasMesh = false;
                for (int i = 0; i < subMeshCount; i++)
                {
                    MaterialData value = meshData.m_Materials[i];
                    SubMeshDescriptor subMesh = meshData.m_Mesh.GetSubMesh(i);
                    value.m_HasMesh = subMesh.vertexCount > 0;
                    if (value.m_HasMesh)
                    {
                        if (meshData.m_HasMesh)
                        {
                            bounds.Encapsulate(subMesh.bounds);
                        }
                        else
                        {
                            bounds = subMesh.bounds;
                            meshData.m_HasMesh = true;
                        }
                    }

                    meshData.m_Materials[i] = value;
                }

                meshData.m_Mesh.bounds = bounds;
            }

            mesh = meshData.m_Mesh;
            return meshData.m_HasMesh;
        }

        internal bool TryGetNameMaterial(int index, int subMeshIndex, out Material material)
        {
            material = null;
            if (m_LabelData == null || index < 0 || index >= m_LabelData.Count)
            {
                return false;
            }

            MeshData meshData = m_LabelData[index];
            if (meshData.m_Materials == null || subMeshIndex < 0 || subMeshIndex >= meshData.m_Materials.Count)
            {
                return false;
            }

            MaterialData materialData = meshData.m_Materials[subMeshIndex];
            material = materialData.m_Material;
            return materialData.m_HasMesh;
        }

        private void MarkAllNameMeshesDirty()
        {
            if (m_LabelData == null)
            {
                return;
            }

            for (int i = 0; i < m_LabelData.Count; i++)
            {
                m_LabelData[i].m_MeshDirty = true;
            }
        }

        /// <summary>
        /// Port of AggregateMeshSystem.FillNameMeshData — schedules FillNameDataJob per dirty bucket.
        /// </summary>
        internal JobHandle FillNameMeshes(JobHandle inputDeps)
        {
            JobHandle jobHandle = inputDeps;
            if (m_LabelData == null)
            {
                return jobHandle;
            }

            for (int i = 0; i < m_LabelData.Count; i++)
            {
                MeshData meshData = m_LabelData[i];
                if (!meshData.m_MeshDirty)
                {
                    continue;
                }

                meshData.m_MeshDirty = false;
                if (meshData.m_Materials == null || meshData.m_Materials.Count == 0)
                {
                    continue;
                }

                m_LabelQuery.ResetFilter();
                m_LabelQuery.SetSharedComponentFilter(new LabelMaterial { m_Index = i });
                if (m_LabelQuery.IsEmptyIgnoreFilter)
                {
                    if (meshData.m_Materials != null)
                    {
                        for (int j = 0; j < meshData.m_Materials.Count; j++)
                        {
                            MaterialData value = meshData.m_Materials[j];
                            value.m_HasMesh = false;
                            meshData.m_Materials[j] = value;
                        }
                    }

                    if (meshData.m_Mesh != null)
                    {
                        Object.Destroy(meshData.m_Mesh);
                        meshData.m_Mesh = null;
                    }

                    if (meshData.m_HasMeshData)
                    {
                        meshData.m_HasMeshData = false;
                        meshData.m_MeshData.Dispose();
                    }

                    meshData.m_HasMesh = false;
                    continue;
                }

                JobHandle outJobHandle;
                NativeList<ArchetypeChunk> chunks = m_LabelQuery.ToArchetypeChunkListAsync(Allocator.TempJob, out outJobHandle);
                if (!meshData.m_HasMeshData)
                {
                    meshData.m_HasMeshData = true;
                    meshData.m_MeshData = Mesh.AllocateWritableMeshData(1);
                }

                JobHandle jobHandle2 = new AdrRoadNameLabelMeshFillJobs.FillNameDataJob
                {
                    m_LabelExtentsType = GetComponentTypeHandle<LabelExtents>(true),
                    m_PrefabRefType = GetComponentTypeHandle<PrefabRef>(true),
                    m_TempType = GetComponentTypeHandle<Temp>(true),
                    m_HiddenType = GetComponentTypeHandle<Hidden>(true),
                    m_LabelPositionType = GetBufferTypeHandle<LabelPosition>(true),
                    m_LabelVertexType = GetBufferTypeHandle<LabelVertex>(true),
                    m_NetNameData = GetComponentLookup<NetNameData>(true),
                    m_Chunks = chunks,
                    m_SubMeshCount = meshData.m_Materials.Count,
                    m_FallbackColor = AdrNetKindUtil.AdrFaceColor32,
                    m_FallbackSelectedColor = AdrNetKindUtil.AdrFaceColor32,
                    m_NameMeshData = meshData.m_MeshData
                }.Schedule(JobHandle.CombineDependencies(outJobHandle, inputDeps));
                chunks.Dispose(jobHandle2);
                meshData.m_DataDependencies = jobHandle2;
                jobHandle = JobHandle.CombineDependencies(jobHandle, jobHandle2);
            }

            m_LabelQuery.ResetFilter();
            return jobHandle;
        }

        private JobHandle ScheduleLabelPlacement(JobHandle inputDeps)
        {
            JobHandle handle = inputDeps;
            ResolveRefHalfHeightSettings(out bool useExtentsHalfHeight, out float refHalfHeight);

            if (!m_S1PlacementQuery.IsEmptyIgnoreFilter)
            {
                handle = new AdrRoadNameLabelPlacementJobs.UpdateS1LabelPositionsJob
                {
                    m_LabelExtentsType = GetComponentTypeHandle<LabelExtents>(true),
                    m_AggregateElementType = GetBufferTypeHandle<AggregateElement>(true),
                    m_LabelPositionType = GetBufferTypeHandle<LabelPosition>(false),
                    m_EdgeData = GetComponentLookup<Edge>(true),
                    m_CurveData = GetComponentLookup<Curve>(true),
                    m_CompositionData = GetComponentLookup<Composition>(true),
                    m_NetCompositionData = GetComponentLookup<NetCompositionData>(true),
                    m_ConnectedEdges = GetBufferLookup<ConnectedEdge>(true),
                    m_AdrAggregated = GetComponentLookup<ADRNameAggregated>(true),
                    m_TempType = GetComponentTypeHandle<Temp>(true),
                    m_UseExtentsHalfHeight = useExtentsHalfHeight,
                    m_RefHalfHeight = refHalfHeight
                }.ScheduleParallel(m_S1PlacementQuery, handle);
            }

            if (!m_S2PlacementQuery.IsEmptyIgnoreFilter)
            {
                handle = new AdrRoadNameLabelPlacementJobs.UpdateS2LabelPositionsJob
                {
                    m_LabelExtentsType = GetComponentTypeHandle<LabelExtents>(true),
                    m_AdrElementType = GetBufferTypeHandle<ADRNameAggregateElement>(true),
                    m_LabelPositionType = GetBufferTypeHandle<LabelPosition>(false),
                    m_EdgeData = GetComponentLookup<Edge>(true),
                    m_CurveData = GetComponentLookup<Curve>(true),
                    m_CompositionData = GetComponentLookup<Composition>(true),
                    m_NetCompositionData = GetComponentLookup<NetCompositionData>(true),
                    m_ConnectedEdges = GetBufferLookup<ConnectedEdge>(true),
                    m_UseExtentsHalfHeight = useExtentsHalfHeight,
                    m_RefHalfHeight = refHalfHeight
                }.ScheduleParallel(m_S2PlacementQuery, handle);
            }

            return handle;
        }

        private static void ResolveRefHalfHeightSettings(out bool useExtentsHalfHeight, out float refHalfHeight)
        {
            int index = AdrModData.RoadNameLabelRefHalfHeightIndexMin + 1;
            if (BasicModData.Instance is AdrModData adrSettings)
            {
                index = adrSettings.RoadNameLabelRefHalfHeightIndex;
            }

            useExtentsHalfHeight = AdrModData.IsAutomaticRefHalfHeight(index);
            refHalfHeight = useExtentsHalfHeight
                ? 1f
                : AdrModData.IndexToRefHalfHeight(index);
        }

        private bool GetLoaded()
        {
            if (m_Loaded)
            {
                m_Loaded = false;
                return true;
            }
            return false;
        }

        private void EnsureAdrOwnersHaveLabelComponents()
        {
            if (m_AdrOwnersMissingLabelsQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            using NativeArray<Entity> entities = m_AdrOwnersMissingLabelsQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                EnsureRoadNameLabelComponents(entities[i], ResolvePrefabSourceForAdr(entities[i]));
            }
        }

        /// <summary>
        /// Hover/select hides the ADR original and draws labels on the Temp Aggregate.
        /// Train/pathway AggregateNets lack <see cref="Game.Prefabs.NetLabel"/>, so Temp has no Label*
        /// archetype — attach the same contract we use for ADR owners.
        /// </summary>
        private void EnsureTempSelectionAggregatesHaveLabelComponents()
        {
            if (m_TempAggregatesMissingLabelsQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            using NativeArray<Entity> entities = m_TempAggregatesMissingLabelsQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                Entity tempAggregate = entities[i];
                Entity prefabSource = Entity.Null;
                if (EntityManager.TryGetComponent(tempAggregate, out Temp temp)
                    && temp.m_Original != Entity.Null
                    && EntityManager.Exists(temp.m_Original))
                {
                    prefabSource = temp.m_Original;
                }

                EnsureRoadNameLabelComponents(tempAggregate, prefabSource);
            }
        }

        private Entity ResolvePrefabSourceForAdr(Entity adrAggregate)
        {
            if (!EntityManager.TryGetBuffer(adrAggregate, true, out DynamicBuffer<ADRNameAggregateElement> elements)
                || elements.Length == 0)
            {
                return Entity.Null;
            }

            Entity edge = elements[0].m_Edge;
            if (EntityManager.TryGetComponent(edge, out Aggregated aggregated)
                && aggregated.m_Aggregate != Entity.Null
                && EntityManager.HasComponent<PrefabRef>(aggregated.m_Aggregate))
            {
                return aggregated.m_Aggregate;
            }

            return Entity.Null;
        }

        /// <summary>
        /// Ensure ADR (or any) name-label owner has Label* contract + PrefabRef for AggregateNet.
        /// PrefabRef stays a real AggregateNet (<see cref="PrefabData"/>) for edit/highlight;
        /// AggregateNets without <see cref="NetNameData"/> (train/pathway) still get a borrowed
        /// <see cref="LabelMaterial"/> index from baked AggregateNet name materials.
        /// ADR-owned kind NetNameData entities are color metadata only — never PrefabRef targets.
        /// </summary>
        public void EnsureRoadNameLabelComponents(Entity owner, Entity prefabSource)
        {
            if (owner == Entity.Null || !EntityManager.Exists(owner))
            {
                return;
            }

            if (!EntityManager.HasBuffer<LabelVertex>(owner))
            {
                EntityManager.AddBuffer<LabelVertex>(owner);
            }
            if (!EntityManager.HasBuffer<LabelPosition>(owner))
            {
                EntityManager.AddBuffer<LabelPosition>(owner);
            }
            if (!EntityManager.HasComponent<LabelExtents>(owner))
            {
                EntityManager.AddComponentData(owner, default(LabelExtents));
            }

            EnsureAdrKindNamePrefabs();
            ResolveOwnerNamePrefabRef(owner, prefabSource);

            if (!TryResolveLabelMaterialIndex(owner, out int materialIndex))
            {
                // No safe AggregateNet name material yet — skip LabelMaterial to avoid index-0 crashes.
                return;
            }

            var labelMaterial = new LabelMaterial { m_Index = materialIndex };
            if (!EntityManager.HasComponent<LabelMaterial>(owner))
            {
                EntityManager.AddSharedComponent(owner, labelMaterial);
            }
            else
            {
                EntityManager.SetSharedComponent(owner, labelMaterial);
            }

            if (!EntityManager.HasComponent<Updated>(owner))
            {
                EntityManager.AddComponent<Updated>(owner);
            }
        }

        private bool TryResolveLabelMaterialIndex(Entity owner, out int materialIndex)
        {
            materialIndex = -1;
            if (m_LabelData == null || m_LabelData.Count == 0)
            {
                return false;
            }

            // Temp selection Aggregate: copy LabelMaterial from ADR/S1 original when present.
            if (EntityManager.TryGetComponent(owner, out Temp temp)
                && temp.m_Original != Entity.Null
                && EntityManager.Exists(temp.m_Original)
                && EntityManager.HasComponent<LabelMaterial>(temp.m_Original))
            {
                int originalIndex = EntityManager.GetSharedComponent<LabelMaterial>(temp.m_Original).m_Index;
                if (originalIndex >= 0
                    && originalIndex < m_LabelData.Count
                    && m_LabelData[originalIndex].m_OriginalMaterial != null)
                {
                    materialIndex = originalIndex;
                    return true;
                }
            }

            // Prefer NetNameData on the owner's PrefabRef when the AggregateNet has name labels (roads).
            if (EntityManager.TryGetComponent(owner, out PrefabRef prefabRef)
                && prefabRef.m_Prefab != Entity.Null
                && EntityManager.TryGetComponent(prefabRef.m_Prefab, out NetNameData nameData)
                && nameData.m_MaterialIndex >= 0
                && nameData.m_MaterialIndex < m_LabelData.Count
                && m_LabelData[nameData.m_MaterialIndex].m_OriginalMaterial != null)
            {
                materialIndex = nameData.m_MaterialIndex;
                return true;
            }

            // Train/pathway AggregateNets lack NetNameData — borrow first baked AggregateNet name material.
            for (int i = 0; i < m_LabelData.Count; i++)
            {
                if (m_LabelData[i].m_OriginalMaterial != null)
                {
                    materialIndex = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// PrefabRef must remain a real AggregateNet (<see cref="PrefabData"/>) for edit/highlight tooling.
        /// Train/pathway AggregateNets often lack <see cref="NetNameData"/> — label material index comes from
        /// <see cref="LabelMaterial"/> instead; never point PrefabRef at ADR synthetic NetNameData-only entities.
        /// </summary>
        private void ResolveOwnerNamePrefabRef(Entity owner, Entity prefabSource)
        {
            Entity desiredPrefab = Entity.Null;
            if (prefabSource != Entity.Null
                && EntityManager.TryGetComponent(prefabSource, out PrefabRef sourcePrefab)
                && sourcePrefab.m_Prefab != Entity.Null
                && EntityManager.Exists(sourcePrefab.m_Prefab)
                && EntityManager.HasComponent<PrefabData>(sourcePrefab.m_Prefab))
            {
                desiredPrefab = sourcePrefab.m_Prefab;
            }

            if (!EntityManager.HasComponent<PrefabRef>(owner))
            {
                if (desiredPrefab != Entity.Null)
                {
                    EntityManager.AddComponentData(owner, new PrefabRef(desiredPrefab));
                }

                return;
            }

            PrefabRef existing = EntityManager.GetComponentData<PrefabRef>(owner);
            bool existingIsSyntheticOrBroken = existing.m_Prefab == Entity.Null
                || !EntityManager.Exists(existing.m_Prefab)
                || !EntityManager.HasComponent<PrefabData>(existing.m_Prefab);

            if (!existingIsSyntheticOrBroken)
            {
                return;
            }

            if (desiredPrefab != Entity.Null)
            {
                EntityManager.SetComponentData(owner, new PrefabRef(desiredPrefab));
            }
            else
            {
                // Drop PrefabRef that would crash PrefabSystem.GetPrefab / CreationDefinition.
                EntityManager.RemoveComponent<PrefabRef>(owner);
            }
        }

        private Entity GetAdrKindNamePrefab(AdrNetKind kind)
        {
            if (m_AdrKindNamePrefabs == null
                || kind <= AdrNetKind.None
                || (int)kind >= m_AdrKindNamePrefabs.Length)
            {
                return Entity.Null;
            }

            return m_AdrKindNamePrefabs[(int)kind];
        }

        /// <summary>
        /// Create ADR-owned entities with <see cref="NetNameData"/> for non-road kinds (face-color metadata).
        /// Never used as PrefabRef targets — they lack PrefabData and would hard-crash edit/highlight paths.
        /// </summary>
        /// <returns>True when prefabs were created this call.</returns>
        private bool EnsureAdrKindNamePrefabs()
        {
            if (m_AdrKindNamePrefabs != null)
            {
                return false;
            }

            if (m_LabelData == null || m_LabelData.Count == 0)
            {
                return false;
            }

            int materialIndex = 0;
            for (int i = 0; i < m_LabelData.Count; i++)
            {
                if (m_LabelData[i].m_OriginalMaterial != null)
                {
                    materialIndex = i;
                    break;
                }
            }

            if (m_LabelData[materialIndex].m_OriginalMaterial == null)
            {
                return false;
            }

            int kindCount = (int)AdrNetKind.Taxiway + 1;
            m_AdrKindNamePrefabs = new Entity[kindCount];
            for (int i = 0; i < kindCount; i++)
            {
                m_AdrKindNamePrefabs[i] = Entity.Null;
            }

            foreach (AdrNetKind kind in AdrNetKindUtil.NonRoadKindsWithAdrNamePrefab)
            {
                Entity prefab = EntityManager.CreateEntity();
                EntityManager.AddComponentData(prefab, new NetNameData
                {
                    m_Color = AdrNetKindUtil.AdrFaceColor32,
                    m_SelectedColor = AdrNetKindUtil.AdrFaceColor32,
                    m_MaterialIndex = materialIndex
                });
                m_AdrKindNamePrefabs[(int)kind] = prefab;
            }

            return true;
        }

        private void RefreshAdrOwnerNamePrefabRefs()
        {
            if (m_AllAdrAggregatesQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            using NativeArray<Entity> entities = m_AllAdrAggregatesQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                Entity owner = entities[i];
                if (!EntityManager.HasBuffer<LabelVertex>(owner))
                {
                    continue;
                }

                ResolveOwnerNamePrefabRef(owner, ResolvePrefabSourceForAdr(owner));
                if (TryResolveLabelMaterialIndex(owner, out int materialIndex))
                {
                    var labelMaterial = new LabelMaterial { m_Index = materialIndex };
                    if (!EntityManager.HasComponent<LabelMaterial>(owner))
                    {
                        EntityManager.AddSharedComponent(owner, labelMaterial);
                    }
                    else
                    {
                        EntityManager.SetSharedComponent(owner, labelMaterial);
                    }
                }

                if (!EntityManager.HasComponent<Updated>(owner))
                {
                    EntityManager.AddComponent<Updated>(owner);
                }
            }
        }

        private void DestroyAdrKindNamePrefabs()
        {
            if (m_AdrKindNamePrefabs == null)
            {
                return;
            }

            for (int i = 0; i < m_AdrKindNamePrefabs.Length; i++)
            {
                Entity prefab = m_AdrKindNamePrefabs[i];
                if (prefab != Entity.Null && EntityManager.Exists(prefab))
                {
                    EntityManager.DestroyEntity(prefab);
                }
            }

            m_AdrKindNamePrefabs = null;
        }

        private void InitializePrefabs(bool fullScan)
        {
            EntityQuery query = fullScan ? m_AllNamePrefabQuery : m_CreatedPrefabQuery;
            if (query.IsEmptyIgnoreFilter)
            {
                return;
            }

            using NativeArray<ArchetypeChunk> chunks = query.ToArchetypeChunkArray(Allocator.TempJob);
            var entityType = GetEntityTypeHandle();
            var prefabDataType = GetComponentTypeHandle<PrefabData>(true);
            var netNameDataType = GetComponentTypeHandle<NetNameData>(false);

            for (int i = 0; i < chunks.Length; i++)
            {
                ArchetypeChunk chunk = chunks[i];
                NativeArray<Entity> entities = chunk.GetNativeArray(entityType);
                NativeArray<PrefabData> prefabDatas = chunk.GetNativeArray(ref prefabDataType);
                NativeArray<NetNameData> nameDatas = chunk.GetNativeArray(ref netNameDataType);
                for (int j = 0; j < prefabDatas.Length; j++)
                {
                    AggregateNetPrefab prefab = m_PrefabSystem.GetPrefab<AggregateNetPrefab>(prefabDatas[j]);
                    if (prefab == null)
                    {
                        continue;
                    }

                    NetLabel netLabel = prefab.GetComponent<NetLabel>();
                    if (netLabel == null || netLabel.m_NameMaterial == null)
                    {
                        continue;
                    }

                    m_LabelData ??= new List<MeshData>();
                    int materialIndex = -1;
                    for (int k = 0; k < m_LabelData.Count; k++)
                    {
                        if (m_LabelData[k].m_OriginalMaterial == netLabel.m_NameMaterial)
                        {
                            materialIndex = k;
                            break;
                        }
                    }

                    if (materialIndex < 0)
                    {
                        materialIndex = m_LabelData.Count;
                        m_LabelData.Add(new MeshData
                        {
                            m_OriginalMaterial = netLabel.m_NameMaterial,
                            m_Materials = new List<MaterialData>(2)
                        });
                    }

                    NetNameData nameData = nameDatas[j];
                    nameData.m_MaterialIndex = materialIndex;
                    nameDatas[j] = nameData;

                    // Cache a real AggregateNet prefab entity safe for PrefabRef (has PrefabData + NetNameData).
                    if (m_SafeNameMaterialPrefab == Entity.Null)
                    {
                        m_SafeNameMaterialPrefab = entities[j];
                    }
                }
            }
        }

        private void UpdateUndergroundState(bool undergroundOn)
        {
            if (m_LabelData == null)
            {
                return;
            }

            for (int i = 0; i < m_LabelData.Count; i++)
            {
                MeshData meshData = m_LabelData[i];
                if (meshData.m_Materials == null)
                {
                    continue;
                }

                for (int j = 0; j < meshData.m_Materials.Count; j++)
                {
                    MaterialData materialData = meshData.m_Materials[j];
                    if (!materialData.m_IsUnderground && materialData.m_Material != null)
                    {
                        ApplyAdrFaceColor(
                            materialData.m_Material,
                            materialData.m_AdrOutlineKind,
                            undergroundOn ? 0.25f : 1f);
                        meshData.m_Materials[j] = materialData;
                    }
                }
            }
        }

        private void UpdateLabelVertices(bool isLoaded)
        {
            if (m_LabelQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            List<MaterialUpdate> materialUpdates = null;
            using NativeArray<ArchetypeChunk> chunks = m_LabelQuery.ToArchetypeChunkArray(Allocator.TempJob);

            TextMeshPro textMesh = m_OverlayRenderSystem.GetTextMesh();
            if (textMesh == null)
            {
                LogUtils.DoWarnLog("AdrRoadNameLabelMeshSystem: OverlayRenderSystem.GetTextMesh() returned null");
                return;
            }

            textMesh.rectTransform.sizeDelta = new Vector2(250f, 100f);
            textMesh.fontSize = 200f;
            textMesh.alignment = TextAlignmentOptions.Center;

            bool undergroundOn = m_UndergroundViewSystem.undergroundOn && m_UndergroundViewSystem.tunnelsOn;
            var entityType = GetEntityTypeHandle();
            var updatedType = GetComponentTypeHandle<Updated>(true);
            var batchesUpdatedType = GetComponentTypeHandle<BatchesUpdated>(true);
            var tempType = GetComponentTypeHandle<Temp>(true);
            var prefabRefType = GetComponentTypeHandle<PrefabRef>(true);
            var extentsType = GetComponentTypeHandle<LabelExtents>(false);
            var labelMaterialType = GetSharedComponentTypeHandle<LabelMaterial>();
            var vertexType = GetBufferTypeHandle<LabelVertex>(false);
            LabelVertex vertex = default;

            for (int i = 0; i < chunks.Length; i++)
            {
                ArchetypeChunk chunk = chunks[i];
                LabelMaterial sharedMaterial = chunk.GetSharedComponent(labelMaterialType, EntityManager);
                bool chunkDirty = isLoaded || chunk.Has(ref updatedType) || chunk.Has(ref batchesUpdatedType);
                if (!chunkDirty)
                {
                    continue;
                }

                NativeArray<Entity> entities = chunk.GetNativeArray(entityType);
                NativeArray<Temp> temps = chunk.GetNativeArray(ref tempType);
                NativeArray<PrefabRef> prefabRefs = chunk.GetNativeArray(ref prefabRefType);
                NativeArray<LabelExtents> extents = chunk.GetNativeArray(ref extentsType);
                BufferAccessor<LabelVertex> vertices = chunk.GetBufferAccessor(ref vertexType);
                if (prefabRefs.Length == 0 || extents.Length == 0 || vertices.Length == 0)
                {
                    continue;
                }

                for (int j = 0; j < entities.Length; j++)
                {
                    Entity entity = entities[j];
                    PrefabRef prefabRef = prefabRefs[j];
                    int materialIndex = sharedMaterial.m_Index;
                    if (EntityManager.TryGetComponent(prefabRef.m_Prefab, out NetNameData nameData)
                        && nameData.m_MaterialIndex >= 0
                        && m_LabelData != null
                        && nameData.m_MaterialIndex < m_LabelData.Count
                        && m_LabelData[nameData.m_MaterialIndex].m_OriginalMaterial != null)
                    {
                        materialIndex = nameData.m_MaterialIndex;
                    }
                    else if (m_LabelData == null
                             || materialIndex < 0
                             || materialIndex >= m_LabelData.Count
                             || m_LabelData[materialIndex].m_OriginalMaterial == null)
                    {
                        continue;
                    }

                    DynamicBuffer<LabelVertex> buffer = vertices[j];
                    MeshData meshData = m_LabelData[materialIndex];
                    if (meshData.m_Materials == null)
                    {
                        buffer.Clear();
                        continue;
                    }

                    meshData.m_MeshDirty = true;

                    if (materialIndex != sharedMaterial.m_Index)
                    {
                        materialUpdates ??= new List<MaterialUpdate>();
                        materialUpdates.Add(new MaterialUpdate
                        {
                            m_Entity = entity,
                            m_Material = materialIndex
                        });
                    }

                    string renderedLabelName;
                    AdrNetKind outlineKind = AdrNetKind.None;
                    if (temps.Length != 0)
                    {
                        Temp temp = temps[j];
                        if (temp.m_Original == Entity.Null)
                        {
                            m_CachedLabels?.Remove(entity);
                            buffer.Clear();
                            continue;
                        }
                        renderedLabelName = m_NameSystem.GetRenderedLabelName(temp.m_Original);
                        outlineKind = ResolveAdrOutlineKind(temp.m_Original);
                    }
                    else
                    {
                        renderedLabelName = m_NameSystem.GetRenderedLabelName(entity);
                        outlineKind = ResolveAdrOutlineKind(entity);
                    }

                    if (m_CachedLabels != null)
                    {
                        if (m_CachedLabels.TryGetValue(entity, out string cached) && cached == renderedLabelName)
                        {
                            continue;
                        }
                        m_CachedLabels[entity] = renderedLabelName;
                    }
                    else
                    {
                        m_CachedLabels = new Dictionary<Entity, string> { { entity, renderedLabelName } };
                    }

                    if (string.IsNullOrEmpty(renderedLabelName))
                    {
                        buffer.Clear();
                        extents[j] = default;
                        continue;
                    }

                    TMP_TextInfo textInfo = textMesh.GetTextInfo(renderedLabelName);
                    int vertexCount = 0;
                    for (int k = 0; k < textInfo.meshInfo.Length; k++)
                    {
                        vertexCount += textInfo.meshInfo[k].vertexCount;
                    }

                    buffer.ResizeUninitialized(vertexCount);
                    vertexCount = 0;
                    for (int l = 0; l < textInfo.meshInfo.Length; l++)
                    {
                        TMP_MeshInfo meshInfo = textInfo.meshInfo[l];
                        if (meshInfo.vertexCount == 0 || meshInfo.material == null)
                        {
                            continue;
                        }

                        Texture mainTexture = meshInfo.material.mainTexture;
                        int2 material = -1;
                        for (int m = 0; m < meshData.m_Materials.Count; m++)
                        {
                            MaterialData existing = meshData.m_Materials[m];
                            if (existing.m_Material == null
                                || existing.m_Material.mainTexture != mainTexture
                                || existing.m_AdrOutlineKind != outlineKind)
                            {
                                continue;
                            }

                            if (existing.m_IsUnderground)
                            {
                                material.y = m;
                            }
                            else
                            {
                                material.x = m;
                            }
                        }

                        if (material.x == -1)
                        {
                            var surface = new MaterialData
                            {
                                m_Material = new Material(meshData.m_OriginalMaterial),
                                m_AdrOutlineKind = outlineKind
                            };
                            ApplyAdrFaceColor(surface.m_Material, outlineKind, undergroundOn ? 0.25f : 1f);
                            if (outlineKind != AdrNetKind.None)
                            {
                                ApplyAdrOutline(surface.m_Material, outlineKind);
                            }
                            m_OverlayRenderSystem.CopyFontAtlasParameters(meshInfo.material, surface.m_Material);
                            material.x = meshData.m_Materials.Count;
                            meshData.m_Materials.Add(surface);
                            string styleTag = outlineKind != AdrNetKind.None ? $"ADR-{outlineKind}" : "Vanilla";
                            surface.m_Material.name = $"{styleTag} Aggregate names {material.x} ({meshData.m_OriginalMaterial.name})";
                        }

                        if (material.y == -1)
                        {
                            var underground = new MaterialData
                            {
                                m_Material = new Material(meshData.m_OriginalMaterial),
                                m_IsUnderground = true,
                                m_AdrOutlineKind = outlineKind
                            };
                            ApplyAdrFaceColor(underground.m_Material, outlineKind, 1f);
                            if (outlineKind != AdrNetKind.None)
                            {
                                ApplyAdrOutline(underground.m_Material, outlineKind);
                            }
                            m_OverlayRenderSystem.CopyFontAtlasParameters(meshInfo.material, underground.m_Material);
                            material.y = meshData.m_Materials.Count;
                            meshData.m_Materials.Add(underground);
                            string styleTag = outlineKind != AdrNetKind.None ? $"ADR-{outlineKind}" : "Vanilla";
                            underground.m_Material.name = $"{styleTag} Aggregate underground names {material.y} ({meshData.m_OriginalMaterial.name})";
                        }

                        Vector3[] positions = meshInfo.vertices;
                        Vector2[] uv0 = meshInfo.uvs0;
                        Vector2[] uv1 = meshInfo.uvs2;
                        Color32[] colors = meshInfo.colors32;
                        for (int n = 0; n < meshInfo.vertexCount; n++)
                        {
                            vertex.m_Position = positions[n];
                            vertex.m_Color = colors[n];
                            vertex.m_UV0 = uv0[n];
                            vertex.m_UV1 = uv1[n];
                            vertex.m_Material = material;
                            buffer[vertexCount + n] = vertex;
                        }
                        vertexCount += meshInfo.vertexCount;
                    }

                    LabelExtents labelExtents = default;
                    for (int line = 0; line < textInfo.lineCount; line++)
                    {
                        Extents lineExtents = textInfo.lineInfo[line].lineExtents;
                        labelExtents.m_Bounds |= new Bounds2(lineExtents.min, lineExtents.max);
                    }
                    extents[j] = labelExtents;
                }
            }

            if (materialUpdates == null)
            {
                return;
            }

            for (int i = 0; i < materialUpdates.Count; i++)
            {
                MaterialUpdate update = materialUpdates[i];
                EntityManager.SetSharedComponent(update.m_Entity, new LabelMaterial
                {
                    m_Index = update.m_Material
                });
            }
        }

        private void ClearLabelMeshes()
        {
            if (m_LabelData == null)
            {
                return;
            }

            for (int i = 0; i < m_LabelData.Count; i++)
            {
                MeshData meshData = m_LabelData[i];
                meshData.m_DataDependencies.Complete();
                meshData.m_DataDependencies = default;
                if (meshData.m_Materials != null)
                {
                    for (int j = 0; j < meshData.m_Materials.Count; j++)
                    {
                        MaterialData materialData = meshData.m_Materials[j];
                        materialData.m_HasMesh = false;
                        meshData.m_Materials[j] = materialData;
                    }
                }

                if (meshData.m_Mesh != null)
                {
                    Object.Destroy(meshData.m_Mesh);
                    meshData.m_Mesh = null;
                }

                if (meshData.m_HasMeshData)
                {
                    meshData.m_MeshData.Dispose();
                    meshData.m_HasMeshData = false;
                }

                meshData.m_HasMesh = false;
                meshData.m_MeshDirty = false;
            }
        }

        private AdrNetKind ResolveAdrOutlineKind(Entity labelOwnerOrOriginal)
        {
            if (!EntityManager.TryGetComponent(labelOwnerOrOriginal, out ADRNameAggregate host))
            {
                return AdrNetKind.None;
            }

            return host.m_Kind != AdrNetKind.None ? host.m_Kind : AdrNetKind.Road;
        }

        private void ApplyAdrFaceColor(Material material, AdrNetKind outlineKind, float alphaScale)
        {
            if (material == null || !material.HasProperty(m_FaceColor))
            {
                return;
            }

            Color face = outlineKind != AdrNetKind.None
                ? AdrNetKindUtil.AdrFaceColor
                : Color.white;
            face.a *= alphaScale;
            material.SetColor(m_FaceColor, face);
        }

        private void ApplyAdrOutline(Material material, AdrNetKind kind)
        {
            if (material != null && material.HasProperty(m_OutlineColor) && kind != AdrNetKind.None)
            {
                material.SetColor(m_OutlineColor, AdrNetKindUtil.GetOutlineColor(kind));
            }
        }

        private void DestroyLabelMaterials()
        {
            if (m_LabelData == null)
            {
                return;
            }

            for (int i = 0; i < m_LabelData.Count; i++)
            {
                MeshData meshData = m_LabelData[i];
                meshData.m_DataDependencies.Complete();
                meshData.m_DataDependencies = default;
                if (meshData.m_HasMeshData)
                {
                    meshData.m_MeshData.Dispose();
                    meshData.m_HasMeshData = false;
                }

                if (meshData.m_Mesh != null)
                {
                    Object.Destroy(meshData.m_Mesh);
                    meshData.m_Mesh = null;
                }

                if (meshData.m_Materials == null)
                {
                    continue;
                }

                for (int j = 0; j < meshData.m_Materials.Count; j++)
                {
                    Material material = meshData.m_Materials[j].m_Material;
                    if (material != null)
                    {
                        Object.Destroy(material);
                    }
                }
            }

            m_LabelData.Clear();
            m_LabelData = null;
        }
    }
}
