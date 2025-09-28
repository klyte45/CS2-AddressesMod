using Belzont.Interfaces;
using Belzont.Serialization;
using Belzont.Utils;
using BridgeWE;
using Colossal.Entities;
using Colossal.Mathematics;
using Colossal.OdinSerializer.Utilities;
using Colossal.Serialization.Entities;
using Game.Common;
using Game.Net;
using Game.Objects;
using Game.SceneFlow;
using Game.Tools;
using Game.UI;
using Game.UI.InGame;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using static Belzont.Utils.NameSystemExtensions;
using static BelzontAdr.ADRHighwayMarkerData;







#if BURST
using Unity.Burst;
#endif
namespace BelzontAdr
{
    public partial class AdrHighwayRoutesSystem : SystemBase, IBelzontBindable, IDefaultSerializable
    {
        #region Endpoints general
        private const string PREFIX = "highwayRoutes.";

        private Action<string, object[]> EventCaller { get; set; }
        private Action<string, Delegate> CallBinder { get; set; }

        public void SetupCallBinder(Action<string, Delegate> callBinder)
        {
            CallBinder = callBinder;

            callBinder($"{PREFIX}getOptionsMetadataFromCurrentLayout", GetOptionsMetadataFromCurrentLayout);
            callBinder($"{PREFIX}getOptionsNamesFromMetadata", GetOptionsNamesFromMetadata);
            callBinder($"{PREFIX}getOptionsMetadataFromLayout", GetOptionsMetadataFromLayout);
            callBinder($"{PREFIX}listHighwaysRegistered", ListHighwaysRegistered);
            callBinder($"{PREFIX}saveHighwayData", SaveHighwayData);

            if (EventCaller != null) InitValueBindings();
        }
        public virtual void SetupEventBinder(Action<string, Delegate> eventBinder)
        {
        }


        public void SetupCaller(Action<string, object[]> eventCaller)
        {
            EventCaller = eventCaller;
            if (CallBinder != null) InitValueBindings();
        }

        private void InitValueBindings()
        {
            DoInitValueBindings_Tool(EventCaller, CallBinder);
            DoInitValueBindings_InfoPanel(EventCaller, CallBinder);
            CallBinder = null;
            EventCaller = null;
        }

        private string GetOptionsMetadataFromCurrentLayout()
            => !m_adrWeIntegrationSystem.WeAvailable || m_dataForNewItem.displayInformation == DisplayInformation.ORIGINAL ? null
                : WETemplatesManagementBridge.GetMetadatasFromReplacement(GetType().Assembly, m_dataForNewItem.displayInformation.ToString()) is not Dictionary<string, string> metadata ? null
                : metadata.TryGetValue("RoadMarker_ExtraFields", out var result) ? result
                : null;

        private string GetOptionsMetadataFromLayout(int diplayInformation)
            => !m_adrWeIntegrationSystem.WeAvailable || !Enum.TryParse<DisplayInformation>(diplayInformation.ToString(), out var enumValue) ? null
                : WETemplatesManagementBridge.GetMetadatasFromReplacement(GetType().Assembly, enumValue.ToString()) is not Dictionary<string, string> metadata ? null
                : metadata.TryGetValue("RoadMarker_ExtraFields", out var result) ? result
                : null;
        private string[] GetOptionsNamesFromMetadata()
        {
            return !m_adrWeIntegrationSystem.WeAvailable
                ? new string[0]
                : new DisplayInformation[] {
                DisplayInformation.CUSTOM_1,
                DisplayInformation.CUSTOM_2,
                DisplayInformation.CUSTOM_3,
                DisplayInformation.CUSTOM_4,
                DisplayInformation.CUSTOM_5,
                DisplayInformation.CUSTOM_6,
                DisplayInformation.CUSTOM_7,
               }.Select(x => WETemplatesManagementBridge.GetMetadatasFromReplacement(GetType().Assembly, x.ToString()) is Dictionary<string, string> metadata && metadata.TryGetValue("RoadMarker_OptionName", out var result)
                       ? NameSystem.Name.LocalizedName(result).Translate()
                       : NameSystem.Name.LocalizedName($"K45::ADR.vuio[DisplayInformation.{x}]").Translate()).ToArray();
        }
        #endregion

        #region Tool controller part
        public MultiUIValueBinding<Colossal.Hash128, string> Tool_RouteId { get; private set; }
        public MultiUIValueBinding<RouteDirection, int> Tool_RouteDirection { get; private set; }
        public MultiUIValueBinding<DisplayInformation, int> Tool_DisplayInformation { get; private set; }
        public MultiUIValueBinding<int> Tool_NumericCustomParam1 { get; private set; }
        public MultiUIValueBinding<int> Tool_NumericCustomParam2 { get; private set; }
        public MultiUIValueBinding<float> Tool_NewMileage { get; private set; }
        public MultiUIValueBinding<bool> Tool_OverrideMileage { get; private set; }
        public MultiUIValueBinding<bool> Tool_ReverseMileageCounting { get; private set; }
        public MultiUIValueBinding<byte> Tool_PylonCount { get; private set; }
        public MultiUIValueBinding<float> Tool_PylonSpacing { get; private set; }
        public MultiUIValueBinding<PylonMaterial, int> Tool_PylonMaterial { get; private set; }
        public MultiUIValueBinding<float> Tool_PylonHeight { get; private set; }
        public MultiUIValueBinding<PylonFormat, int> Tool_PylonFormat { get; private set; }


        private ADRHighwayMarkerData m_dataForNewItem = new ADRHighwayMarkerData
        {
            Initialized = true,
            pylonCount = 1,
            pylonHeight = 2,
            pylonFormat = PylonFormat.Cylinder,
            pylonMaterial = PylonMaterial.Metal,
            pylonSpacing = .25f,
        };

        private void DoInitValueBindings_Tool(Action<string, object[]> EventCaller, Action<string, Delegate> CallBinder)
        {
            Tool_RouteId = new(m_dataForNewItem.routeDataIndex, $"{PREFIX}{nameof(Tool_RouteId)}", EventCaller, CallBinder, (x, _) => x.ToString(), (x, _) => Colossal.Hash128.Parse(x));
            Tool_RouteDirection = new(m_dataForNewItem.routeDirection, $"{PREFIX}{nameof(Tool_RouteDirection)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (RouteDirection)x);
            Tool_DisplayInformation = new(m_dataForNewItem.displayInformation, $"{PREFIX}{nameof(Tool_DisplayInformation)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (DisplayInformation)x);
            Tool_NumericCustomParam1 = new(m_dataForNewItem.numericCustomParam1, $"{PREFIX}{nameof(Tool_NumericCustomParam1)}", EventCaller, CallBinder);
            Tool_NumericCustomParam2 = new(m_dataForNewItem.numericCustomParam2, $"{PREFIX}{nameof(Tool_NumericCustomParam2)}", EventCaller, CallBinder);
            Tool_NewMileage = new(m_dataForNewItem.newMileage, $"{PREFIX}{nameof(Tool_NewMileage)}", EventCaller, CallBinder);
            Tool_OverrideMileage = new(m_dataForNewItem.overrideMileage, $"{PREFIX}{nameof(Tool_OverrideMileage)}", EventCaller, CallBinder);
            Tool_ReverseMileageCounting = new(m_dataForNewItem.reverseMileageCounting, $"{PREFIX}{nameof(Tool_ReverseMileageCounting)}", EventCaller, CallBinder);
            Tool_PylonCount = new(m_dataForNewItem.pylonCount, $"{PREFIX}{nameof(Tool_PylonCount)}", EventCaller, CallBinder);
            Tool_PylonSpacing = new(m_dataForNewItem.pylonSpacing, $"{PREFIX}{nameof(Tool_PylonSpacing)}", EventCaller, CallBinder);
            Tool_PylonMaterial = new(m_dataForNewItem.pylonMaterial, $"{PREFIX}{nameof(Tool_PylonMaterial)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (PylonMaterial)x);
            Tool_PylonHeight = new(m_dataForNewItem.pylonHeight, $"{PREFIX}{nameof(Tool_PylonHeight)}", EventCaller, CallBinder);
            Tool_PylonFormat = new(m_dataForNewItem.pylonFormat, $"{PREFIX}{nameof(Tool_PylonFormat)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (PylonFormat)x);


            CallBinder($"{PREFIX}isCurrentPrefabRoadMarker", IsCurrentPrefabRoadMarker);

            Tool_RouteId.OnScreenValueChanged += (x) => { m_dataForNewItem.routeDataIndex = x; MarkTempMarksDirty(); };
            Tool_RouteDirection.OnScreenValueChanged += (x) => { m_dataForNewItem.routeDirection = x; MarkTempMarksDirty(); };
            Tool_DisplayInformation.OnScreenValueChanged += (x) => { m_dataForNewItem.displayInformation = x; MarkTempMarksDirty(); };
            Tool_NumericCustomParam1.OnScreenValueChanged += (x) => { m_dataForNewItem.numericCustomParam1 = x; MarkTempMarksDirty(); };
            Tool_NumericCustomParam2.OnScreenValueChanged += (x) => { m_dataForNewItem.numericCustomParam2 = x; MarkTempMarksDirty(); };
            Tool_NewMileage.OnScreenValueChanged += (x) => { m_dataForNewItem.newMileage = x; MarkTempMarksDirty(); };
            Tool_OverrideMileage.OnScreenValueChanged += (x) => { m_dataForNewItem.overrideMileage = x; MarkTempMarksDirty(); };
            Tool_ReverseMileageCounting.OnScreenValueChanged += (x) => { m_dataForNewItem.reverseMileageCounting = x; MarkTempMarksDirty(); };
            Tool_PylonCount.OnScreenValueChanged += (x) => { m_dataForNewItem.pylonCount = (byte)math.clamp(x, 1, 2); MarkTempMarksDirty(); };
            Tool_PylonSpacing.OnScreenValueChanged += (x) => { m_dataForNewItem.pylonSpacing = math.clamp(x, 0.05f, 3f); MarkTempMarksDirty(); };
            Tool_PylonMaterial.OnScreenValueChanged += (x) => { m_dataForNewItem.pylonMaterial = x; MarkTempMarksDirty(); };
            Tool_PylonHeight.OnScreenValueChanged += (x) => { m_dataForNewItem.pylonHeight = math.clamp(x, .25f, 10); MarkTempMarksDirty(); };
            Tool_PylonFormat.OnScreenValueChanged += (x) => { m_dataForNewItem.pylonFormat = x; MarkTempMarksDirty(); };
        }

        private void MarkTempMarksDirty()
        {
            m_executionQueue.Enqueue(() => m_modificationEndBarrier.CreateCommandBuffer().AddComponent<ADRHighwayMarkerDataDirty>(m_markTempDirtyTargets, EntityQueryCaptureMode.AtPlayback));
        }

        private bool IsCurrentPrefabRoadMarker() => m_toolSystem.activeTool is ObjectToolSystem && (m_toolSystem.activePrefab?.Has<ADRRoadMarkerObject>() ?? false);

        #endregion

        #region Info panel controller part

        private SelectedInfoUISystem m_selectedInfoUISystem;

        public MultiUIValueBinding<Colossal.Hash128, string> InfoPanel_RouteId { get; private set; }
        public MultiUIValueBinding<RouteDirection, int> InfoPanel_RouteDirection { get; private set; }
        public MultiUIValueBinding<DisplayInformation, int> InfoPanel_DisplayInformation { get; private set; }
        public MultiUIValueBinding<int> InfoPanel_NumericCustomParam1 { get; private set; }
        public MultiUIValueBinding<int> InfoPanel_NumericCustomParam2 { get; private set; }
        public MultiUIValueBinding<float> InfoPanel_NewMileage { get; private set; }
        public MultiUIValueBinding<bool> InfoPanel_OverrideMileage { get; private set; }
        public MultiUIValueBinding<bool> InfoPanel_ReverseMileageCounting { get; private set; }
        public MultiUIValueBinding<int> InfoPanel_PylonCount { get; private set; }
        public MultiUIValueBinding<float> InfoPanel_PylonSpacing { get; private set; }
        public MultiUIValueBinding<PylonMaterial, int> InfoPanel_PylonMaterial { get; private set; }
        public MultiUIValueBinding<float> InfoPanel_PylonHeight { get; private set; }
        public MultiUIValueBinding<PylonFormat, int> InfoPanel_PylonFormat { get; private set; }


        private void DoInitValueBindings_InfoPanel(Action<string, object[]> EventCaller, Action<string, Delegate> CallBinder)
        {

            InfoPanel_RouteId = new(default, $"{PREFIX}{nameof(InfoPanel_RouteId)}", EventCaller, CallBinder, (x, _) => x.ToString(), (x, _) => Colossal.Hash128.Parse(x));
            InfoPanel_RouteDirection = new(default, $"{PREFIX}{nameof(InfoPanel_RouteDirection)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (RouteDirection)x);
            InfoPanel_DisplayInformation = new(default, $"{PREFIX}{nameof(InfoPanel_DisplayInformation)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (DisplayInformation)x);
            InfoPanel_NumericCustomParam1 = new(default, $"{PREFIX}{nameof(InfoPanel_NumericCustomParam1)}", EventCaller, CallBinder);
            InfoPanel_NumericCustomParam2 = new(default, $"{PREFIX}{nameof(InfoPanel_NumericCustomParam2)}", EventCaller, CallBinder);
            InfoPanel_NewMileage = new(default, $"{PREFIX}{nameof(InfoPanel_NewMileage)}", EventCaller, CallBinder);
            InfoPanel_OverrideMileage = new(default, $"{PREFIX}{nameof(InfoPanel_OverrideMileage)}", EventCaller, CallBinder);
            InfoPanel_ReverseMileageCounting = new(default, $"{PREFIX}{nameof(InfoPanel_ReverseMileageCounting)}", EventCaller, CallBinder);
            InfoPanel_PylonCount = new(default, $"{PREFIX}{nameof(InfoPanel_PylonCount)}", EventCaller, CallBinder);
            InfoPanel_PylonSpacing = new(default, $"{PREFIX}{nameof(InfoPanel_PylonSpacing)}", EventCaller, CallBinder);
            InfoPanel_PylonMaterial = new(default, $"{PREFIX}{nameof(InfoPanel_PylonMaterial)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (PylonMaterial)x);
            InfoPanel_PylonHeight = new(default, $"{PREFIX}{nameof(InfoPanel_PylonHeight)}", EventCaller, CallBinder);
            InfoPanel_PylonFormat = new(default, $"{PREFIX}{nameof(InfoPanel_PylonFormat)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (PylonFormat)x);

            bool registerSelfOnUI()
            {
                m_selectedInfoUISystem = World.GetExistingSystemManaged<SelectedInfoUISystem>();
                if (m_selectedInfoUISystem is null)
                {
                    return false;
                }
                m_adrEditorUI = World.GetExistingSystemManaged<AdrEditorUISystem>();
                m_adrEditorUI.eventSelectionChanged += OnSelectionChanged;
                m_selectedInfoUISystem.eventSelectionChanged += OnSelectionChanged;
                return true;
            }

            GameManager.instance.RegisterUpdater(registerSelfOnUI);

            InfoPanel_RouteId.OnScreenValueChanged += (x) => EnqueueModification<Colossal.Hash128, ADRHighwayMarkerData>(x, (x, currentItem, entity) =>
            {
                currentItem.routeDataIndex = x;
                if (EntityManager.TryGetComponent<Attached>(entity, out var attached)
                && EntityManager.TryGetComponent<Aggregated>(attached.m_Parent, out var aggregated)
                && EntityManager.TryGetComponent<ADRHighwayAggregationData>(aggregated.m_Aggregate, out var highwayAggregationData))
                {
                    highwayAggregationData.highwayDataId = x;
                    EntityManager.SetComponentData(aggregated.m_Aggregate, highwayAggregationData);
                    m_modificationEndBarrier.CreateCommandBuffer().AddComponent<ADRHighwayAggregationDataDirtyHwId>(aggregated.m_Aggregate);
                }
                return currentItem;
            });
            InfoPanel_RouteDirection.OnScreenValueChanged += (x) => EnqueueModification<RouteDirection, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.routeDirection = x; m_cacheToBeErased.Add(currentItem.routeDataIndex); return currentItem; });
            InfoPanel_DisplayInformation.OnScreenValueChanged += (x) => EnqueueModification<DisplayInformation, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.displayInformation = x; m_cacheToBeErased.Add(currentItem.routeDataIndex); return currentItem; });
            InfoPanel_NumericCustomParam1.OnScreenValueChanged += (x) => EnqueueModification<int, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.numericCustomParam1 = x; m_cacheToBeErased.Add(currentItem.routeDataIndex); return currentItem; });
            InfoPanel_NumericCustomParam2.OnScreenValueChanged += (x) => EnqueueModification<int, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.numericCustomParam2 = x; m_cacheToBeErased.Add(currentItem.routeDataIndex); return currentItem; });
            InfoPanel_NewMileage.OnScreenValueChanged += (x) => EnqueueModification<float, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.newMileage = x; m_cacheToBeErased.Add(currentItem.routeDataIndex); return currentItem; });
            InfoPanel_OverrideMileage.OnScreenValueChanged += (x) => EnqueueModification<bool, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.overrideMileage = x; m_cacheToBeErased.Add(currentItem.routeDataIndex); return currentItem; });
            InfoPanel_ReverseMileageCounting.OnScreenValueChanged += (x) => EnqueueModification<bool, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.reverseMileageCounting = x; m_cacheToBeErased.Add(currentItem.routeDataIndex); return currentItem; });
            InfoPanel_PylonCount.OnScreenValueChanged += (x) => EnqueueModification<byte, ADRHighwayMarkerData>((byte)x, (x, currentItem, _) => { currentItem.pylonCount = (byte)math.clamp(x, 1, 2); m_cacheToBeErased.Add(currentItem.routeDataIndex); return currentItem; });
            InfoPanel_PylonSpacing.OnScreenValueChanged += (x) => EnqueueModification<float, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.pylonSpacing = math.clamp(x, 0.05f, 3f); m_cacheToBeErased.Add(currentItem.routeDataIndex); return currentItem; });
            InfoPanel_PylonMaterial.OnScreenValueChanged += (x) => EnqueueModification<PylonMaterial, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.pylonMaterial = x; m_cacheToBeErased.Add(currentItem.routeDataIndex); return currentItem; });
            InfoPanel_PylonHeight.OnScreenValueChanged += (x) => EnqueueModification<float, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.pylonHeight = math.clamp(x, .25f, 10); m_cacheToBeErased.Add(currentItem.routeDataIndex); return currentItem; });
            InfoPanel_PylonFormat.OnScreenValueChanged += (x) => EnqueueModification<PylonFormat, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.pylonFormat = x; m_cacheToBeErased.Add(currentItem.routeDataIndex); return currentItem; });

        }

        void OnSelectionChanged(Entity entity, Entity prefab, Unity.Mathematics.float3 position)
        {
            if (Enabled && EntityManager.TryGetComponent<ADRHighwayMarkerData>(entity, out var component))
            {
                InfoPanel_RouteId.Value = component.routeDataIndex;
                InfoPanel_RouteDirection.Value = component.routeDirection;
                InfoPanel_DisplayInformation.Value = component.displayInformation;
                InfoPanel_NumericCustomParam1.Value = component.numericCustomParam1;
                InfoPanel_NumericCustomParam2.Value = component.numericCustomParam2;
                InfoPanel_NewMileage.Value = component.newMileage;
                InfoPanel_OverrideMileage.Value = component.overrideMileage;
                InfoPanel_ReverseMileageCounting.Value = component.reverseMileageCounting;
                InfoPanel_PylonCount.Value = component.pylonCount;
                InfoPanel_PylonSpacing.Value = component.pylonSpacing;
                InfoPanel_PylonMaterial.Value = component.pylonMaterial;
                InfoPanel_PylonHeight.Value = component.pylonHeight;
                InfoPanel_PylonFormat.Value = component.pylonFormat;
            }
        }

        private readonly Queue<Action> m_executionQueue = new();

        internal void EnqueueModification<T, W>(T newVal, Func<T, W, Entity, W> x) where W : unmanaged, IComponentData
        {
            var target = GameManager.instance.gameMode == Game.GameMode.Editor ? m_adrEditorUI.SelectedEntity : m_selectedInfoUISystem.selectedEntity;
            if (target.Index > 0)
            {
                m_executionQueue.Enqueue(() =>
                {
                    if (EntityManager.TryGetComponent<W>(target, out var currentItem))
                    {
                        currentItem = x(newVal, currentItem, target);
                        EntityManager.SetComponentData(target, currentItem);
                    }
                });
            }
        }
        #endregion

        #region System part

        private EntityQuery m_DirtyMarkerData;
        private EntityQuery m_DirtyMarkerDataTemp;
        private EntityQuery m_markTempDirtyTargets;
        private EntityQuery m_unprocessedAggregations;
        private EntityQuery m_dirtyHwDataAggregations;
        private EntityQuery m_uncachedAggregations;
        private EntityQuery m_cachedAggregations;
        private EntityQuery m_aggregatedDeleted;
        private ModificationEndBarrier m_modificationEndBarrier;
        private ToolSystem m_toolSystem;
        private Adr_WEIntegrationSystem m_adrWeIntegrationSystem;
        private AdrMainSystem m_mainSystem;
        private AdrEditorUISystem m_adrEditorUI;
        private readonly NativeParallelHashSet<Colossal.Hash128> m_cacheToBeErased = new(20, Allocator.Persistent);
        protected override void OnCreate()
        {
            base.OnCreate();
            m_modificationEndBarrier = World.GetOrCreateSystemManaged<ModificationEndBarrier>();
            m_adrWeIntegrationSystem = World.GetOrCreateSystemManaged<Adr_WEIntegrationSystem>();
            m_toolSystem = World.GetExistingSystemManaged<ToolSystem>();
            m_mainSystem = World.GetExistingSystemManaged<AdrMainSystem>();
            m_DirtyMarkerData = GetEntityQuery(new EntityQueryDesc[]
            {
                new()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<ADRHighwayMarkerData>(),
                        ComponentType.ReadOnly<ADRHighwayMarkerDataDirty>()
                    },
                    None = new ComponentType[] {
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Deleted>()
                    }
                }
            });
            m_DirtyMarkerDataTemp = GetEntityQuery(new EntityQueryDesc[]
            {
                new()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<ADRHighwayMarkerData>(),
                        ComponentType.ReadOnly<ADRHighwayMarkerDataDirty>(),
                        ComponentType.ReadOnly<Temp>()
                    },
                    None = new ComponentType[] {
                        ComponentType.ReadOnly<Deleted>()
                    }
                }
            });
            m_markTempDirtyTargets = GetEntityQuery(new EntityQueryDesc[]
            {
                new()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<ADRHighwayMarkerData>(),
                        ComponentType.ReadOnly<Temp>()
                    },
                    None = new ComponentType[] {
                        ComponentType.ReadOnly<ADRHighwayMarkerDataDirty>(),
                        ComponentType.ReadOnly<Deleted>()
                    }
                }
            });
            m_unprocessedAggregations = GetEntityQuery(new EntityQueryDesc[]
            {
                new()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Aggregate>(),
                    },
                    None = new ComponentType[] {
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<ADRHighwayAggregationData>(),
                        ComponentType.ReadOnly<Deleted>()
                    }
                }
            });
            m_dirtyHwDataAggregations = GetEntityQuery(new EntityQueryDesc[]
            {
                new()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Aggregate>(),
                        ComponentType.ReadOnly<ADRHighwayAggregationData>(),
                        ComponentType.ReadOnly<ADRHighwayAggregationDataDirtyHwId>(),
                    },
                    None = new ComponentType[] {
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Deleted>()
                    }
                }
            });
            m_uncachedAggregations = GetEntityQuery(new EntityQueryDesc[]
            {
                new()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Aggregate>(),
                        ComponentType.ReadOnly<ADRHighwayAggregationData>(),
                    },
                    None = new ComponentType[] {
                        ComponentType.ReadOnly<ADRHighwayAggregationCacheData>(),
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Deleted>()
                    }
                }
            });
            m_cachedAggregations = GetEntityQuery(new EntityQueryDesc[]
            {
                new()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Aggregate>(),
                        ComponentType.ReadOnly<ADRHighwayAggregationData>(),
                        ComponentType.ReadOnly<ADRHighwayAggregationCacheData>(),
                    },
                    None = new ComponentType[] {
                        ComponentType.ReadOnly<Updated>(),
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Deleted>()
                    }
                }
            });
            m_aggregatedDeleted = GetEntityQuery(new EntityQueryDesc[]
            {
                new()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Aggregated>(),
                        ComponentType.ReadOnly<Deleted>()
                    },
                    None = new ComponentType[] {
                        ComponentType.ReadOnly<Temp>(),
                    }
                }
            });
        }

        protected override void OnDestroy()
        {
            m_selectedInfoUISystem.eventSelectionChanged -= OnSelectionChanged;
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            if (GameManager.instance.isGameLoading)
            {
                return;
            }
            while (m_executionQueue.TryDequeue(out var action))
            {
                action();
            }
            if (!m_DirtyMarkerData.IsEmpty)
            {
                var updater = new HighwayDataUpdater
                {
                    m_currentToolData = m_dataForNewItem,
                    m_CommandBuffer = m_modificationEndBarrier.CreateCommandBuffer().AsParallelWriter(),
                    m_EntityType = GetEntityTypeHandle(),
                    m_markerData = GetComponentTypeHandle<ADRHighwayMarkerData>(),
                    m_tempLookup = GetComponentLookup<Temp>(),
                    m_adrHwAggregationDataLookup = GetComponentLookup<ADRHighwayAggregationData>(),
                    m_aggregatedLookup = GetComponentLookup<Aggregated>(),
                    m_attachedLookup = GetComponentLookup<Attached>(),
                    m_adrHwAggregationDataDirtyHwIdLookup = GetComponentLookup<ADRHighwayAggregationDataDirtyHwId>(),
                };
                updater.ScheduleParallel(m_DirtyMarkerData, Dependency).Complete();
            }
            else if (!m_DirtyMarkerDataTemp.IsEmpty)
            {
                using var entities = m_DirtyMarkerDataTemp.ToEntityArray(Allocator.Temp);
                for (var i = 0; i < entities.Length; i++)
                {
                    EntityManager.SetComponentData(entities[i], m_dataForNewItem);
                    EntityManager.RemoveComponent<ADRHighwayMarkerDataDirty>(entities[i]);
                }
            }
            else if (!m_unprocessedAggregations.IsEmpty)
            {
                var updater = new NewAggregateFiller
                {
                    m_CommandBuffer = m_modificationEndBarrier.CreateCommandBuffer().AsParallelWriter(),
                    m_EntityType = GetEntityTypeHandle(),
                    m_aggregateElementsData = GetBufferLookup<AggregateElement>(),
                    m_markerDataLookup = GetComponentLookup<ADRHighwayMarkerData>(),
                    m_subObjectsLookup = GetBufferLookup<SubObject>(),
                };
                updater.ScheduleParallel(m_unprocessedAggregations, Dependency).Complete();
                m_mainSystem.OnChangedRoadNameGenerationRules();
            }
            else if (!m_dirtyHwDataAggregations.IsEmpty)
            {
                var updater = new ReplicateAggregateSetting
                {
                    m_CommandBuffer = m_modificationEndBarrier.CreateCommandBuffer().AsParallelWriter(),
                    m_EntityType = GetEntityTypeHandle(),
                    m_aggregateElementsData = GetBufferLookup<AggregateElement>(),
                    m_markerDataLookup = GetComponentLookup<ADRHighwayMarkerData>(),
                    m_subObjectsLookup = GetBufferLookup<SubObject>(),
                    m_HighwayAggregationData = GetComponentTypeHandle<ADRHighwayAggregationData>()
                };
                updater.ScheduleParallel(m_dirtyHwDataAggregations, Dependency).Complete();
                m_mainSystem.OnChangedRoadNameGenerationRules();
            }
            else if (m_cacheToBeErased.Count() > 0 && !m_cachedAggregations.IsEmpty)
            {
                var updater = new AggregationCacheEraser
                {
                    m_CommandBuffer = m_modificationEndBarrier.CreateCommandBuffer().AsParallelWriter(),
                    m_EntityType = GetEntityTypeHandle(),
                    m_HashesToErase = m_cacheToBeErased,
                    m_hwTypeHandle = GetComponentTypeHandle<ADRHighwayAggregationData>()
                };
                updater.ScheduleParallel(m_cachedAggregations, Dependency).Complete();
                m_cacheToBeErased.Clear();
                m_mainSystem.OnChangedRoadNameGenerationRules();
            }
            else if (!m_uncachedAggregations.IsEmpty)
            {
                var highwayRefPointMap = new NativeHashMap<Colossal.Hash128, float2>(highwaysDataRegistry.Count, Allocator.Temp);
                foreach (var value in highwaysDataRegistry.Values)
                {
                    highwayRefPointMap[value.Id] = value.refStartPoint;
                }
                var updater = new CalculateHighwayCacheData
                {
                    m_CommandBuffer = m_modificationEndBarrier.CreateCommandBuffer().AsParallelWriter(),
                    m_EntityType = GetEntityTypeHandle(),
                    m_aggregateElementsData = GetBufferLookup<AggregateElement>(),
                    m_markerDataLookup = GetComponentLookup<ADRHighwayMarkerData>(),
                    m_subObjectsLookup = GetBufferLookup<SubObject>(),
                    m_attachedLookup = GetComponentLookup<Attached>(),
                    m_curveLookup = GetComponentLookup<Curve>(),
                    m_edgeLookup = GetComponentLookup<Edge>(),
                    m_hwAggregationData = GetComponentTypeHandle<ADRHighwayAggregationData>(),
                    m_globalZeroMarker = m_mainSystem.GetZeroMarkerPosition(),
                    m_refPointHighway = highwayRefPointMap
                };
                updater.ScheduleParallel(m_uncachedAggregations, Dependency).Complete();
                highwayRefPointMap.Dispose();
                m_mainSystem.OnChangedRoadNameGenerationRules();
            }
        }
#if BURST
        [BurstCompile]
#endif
        private struct NewAggregateFiller : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            public EntityTypeHandle m_EntityType;
            public BufferLookup<AggregateElement> m_aggregateElementsData;
            public BufferLookup<SubObject> m_subObjectsLookup;
            public ComponentLookup<ADRHighwayMarkerData> m_markerDataLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(m_EntityType);
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    Colossal.Hash128 idFound = default;
                    if (m_aggregateElementsData.TryGetBuffer(entity, out var edges))
                    {
                        for (int j = 0; j < edges.Length; j++)
                        {
                            if (m_subObjectsLookup.TryGetBuffer(edges[j].m_Edge, out var subObjects))
                            {
                                for (int k = 0; k < subObjects.Length; k++)
                                {
                                    if (m_markerDataLookup.TryGetComponent(subObjects[k].m_SubObject, out var markerData))
                                    {
                                        if (idFound == default)
                                        {
                                            if (markerData.routeDataIndex != default)
                                            {
                                                idFound = markerData.routeDataIndex;
                                            }
                                        }
                                        else
                                        {
                                            markerData.routeDataIndex = idFound;
                                            m_CommandBuffer.SetComponent(unfilteredChunkIndex, subObjects[k].m_SubObject, markerData);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    m_CommandBuffer.AddComponent(unfilteredChunkIndex, entity, new ADRHighwayAggregationData
                    {
                        highwayDataId = idFound
                    });
                }
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct AggregationCacheEraser : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            public EntityTypeHandle m_EntityType;
            public ComponentTypeHandle<ADRHighwayAggregationData> m_hwTypeHandle;
            public NativeParallelHashSet<Colossal.Hash128> m_HashesToErase;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(m_EntityType);
                var hwDataArray = chunk.GetNativeArray(ref m_hwTypeHandle);
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var hwData = hwDataArray[i];
                    if (hwData.highwayDataId != default && m_HashesToErase.Contains(hwData.highwayDataId))
                    {
                        m_CommandBuffer.RemoveComponent<ADRHighwayAggregationCacheData>(unfilteredChunkIndex, entity);
                    }
                }
            }
        }
#if BURST
        [BurstCompile]
#endif
        private struct ReplicateAggregateSetting : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            public ComponentTypeHandle<ADRHighwayAggregationData> m_HighwayAggregationData;
            public EntityTypeHandle m_EntityType;
            public BufferLookup<AggregateElement> m_aggregateElementsData;
            public BufferLookup<SubObject> m_subObjectsLookup;
            public ComponentLookup<ADRHighwayMarkerData> m_markerDataLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(m_EntityType);
                var highwaysDatas = chunk.GetNativeArray(ref m_HighwayAggregationData);
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var highwayData = highwaysDatas[i];
                    var idToReplicate = highwayData.highwayDataId;
                    var anyChanged = false;
                    if (m_aggregateElementsData.TryGetBuffer(entity, out var edges))
                    {
                        for (int j = 0; j < edges.Length; j++)
                        {
                            if (m_subObjectsLookup.TryGetBuffer(edges[j].m_Edge, out var subObjects))
                            {
                                for (int k = 0; k < subObjects.Length; k++)
                                {
                                    if (m_markerDataLookup.TryGetComponent(subObjects[k].m_SubObject, out var markerData))
                                    {
                                        markerData.routeDataIndex = idToReplicate;
                                        m_CommandBuffer.SetComponent(unfilteredChunkIndex, subObjects[k].m_SubObject, markerData);
                                        anyChanged = true;
                                    }
                                }
                            }
                        }
                    }
                    if (!anyChanged)
                    {
                        highwayData.highwayDataId = default;
                        m_CommandBuffer.SetComponent(unfilteredChunkIndex, entity, highwayData);
                    }
                    m_CommandBuffer.RemoveComponent<ADRHighwayAggregationDataDirtyHwId>(unfilteredChunkIndex, entity);
                    m_CommandBuffer.RemoveComponent<ADRHighwayAggregationCacheData>(unfilteredChunkIndex, entity);
                }
            }
        }

#if BURST
        [BurstCompile]
#endif
        private struct HighwayDataUpdater : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            public ComponentTypeHandle<ADRHighwayMarkerData> m_markerData;
            public ComponentLookup<Temp> m_tempLookup;
            public ADRHighwayMarkerData m_currentToolData;
            public EntityTypeHandle m_EntityType;
            public NativeParallelHashSet<Colossal.Hash128>.ParallelWriter m_markedToRecalculate;

            public ComponentLookup<Aggregated> m_aggregatedLookup;
            public ComponentLookup<Attached> m_attachedLookup;
            public ComponentLookup<ADRHighwayAggregationData> m_adrHwAggregationDataLookup;
            public ComponentLookup<ADRHighwayAggregationDataDirtyHwId> m_adrHwAggregationDataDirtyHwIdLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(m_EntityType);
                var markDatas = chunk.GetNativeArray(ref m_markerData);
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var currentMarkData = markDatas[i];
                    if (!currentMarkData.Initialized || m_tempLookup.HasComponent(entity))
                    {
                        if (!m_tempLookup.HasComponent(entity)
                            && m_attachedLookup.TryGetComponent(entity, out var attached)
                            && m_aggregatedLookup.TryGetComponent(attached.m_Parent, out var aggregationLink)
                            && m_adrHwAggregationDataLookup.TryGetComponent(aggregationLink.m_Aggregate, out var adrHwAggregationData)
                            && adrHwAggregationData.highwayDataId != m_currentToolData.routeDataIndex
                            && !m_adrHwAggregationDataDirtyHwIdLookup.HasComponent(aggregationLink.m_Aggregate)
                            )
                        {
                            if (m_currentToolData.routeDataIndex == default)
                            {
                                var dataCopy = m_currentToolData;
                                dataCopy.routeDataIndex = adrHwAggregationData.highwayDataId;
                                m_CommandBuffer.SetComponent(unfilteredChunkIndex, entity, dataCopy);
                                m_markedToRecalculate.Add(dataCopy.routeDataIndex);
                            }
                            else
                            {
                                m_CommandBuffer.SetComponent(unfilteredChunkIndex, entity, m_currentToolData);
                                adrHwAggregationData.highwayDataId = m_currentToolData.routeDataIndex;
                                m_CommandBuffer.SetComponent(unfilteredChunkIndex, aggregationLink.m_Aggregate, adrHwAggregationData);
                                m_CommandBuffer.AddComponent<ADRHighwayAggregationDataDirtyHwId>(unfilteredChunkIndex, aggregationLink.m_Aggregate);
                            }
                        }
                        else
                        {
                            m_CommandBuffer.SetComponent(unfilteredChunkIndex, entity, m_currentToolData);
                            m_markedToRecalculate.Add(m_currentToolData.routeDataIndex);
                        }
                    }
                    m_CommandBuffer.RemoveComponent<ADRHighwayMarkerDataDirty>(unfilteredChunkIndex, entity);
                }
            }
        }


#if BURST
        [BurstCompile]
#endif
        private struct CalculateHighwayCacheData : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            public ComponentTypeHandle<ADRHighwayAggregationData> m_hwAggregationData;
            public EntityTypeHandle m_EntityType;
            public BufferLookup<AggregateElement> m_aggregateElementsData;
            public BufferLookup<SubObject> m_subObjectsLookup;
            public ComponentLookup<ADRHighwayMarkerData> m_markerDataLookup;
            public NativeHashMap<Colossal.Hash128, float2> m_refPointHighway;
            public ComponentLookup<Curve> m_curveLookup;
            public ComponentLookup<Edge> m_edgeLookup;
            public float2 m_globalZeroMarker;
            public ComponentLookup<Attached> m_attachedLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(m_EntityType);
                var aggregationDatas = chunk.GetNativeArray(ref m_hwAggregationData);
                for (int h = 0; h < entities.Length; h++)
                {
                    var entity = entities[h];
                    var currentAggregationData = aggregationDatas[h];

                    if (!m_aggregateElementsData.TryGetBuffer(entity, out var aggregationBuffer)
                        || !m_curveLookup.TryGetComponent(aggregationBuffer[0].m_Edge, out Curve e0)
                        || !m_curveLookup.TryGetComponent(aggregationBuffer[^1].m_Edge, out Curve e1))
                    {
                        m_CommandBuffer.SetComponent(unfilteredChunkIndex, entity, new ADRHighwayAggregationCacheData());
                        continue;
                    }


                    var refE0 = CheckSegmentReversion(aggregationBuffer, false, 0, aggregationBuffer[0]) ? e0.m_Bezier.d.xz : e0.m_Bezier.a.xz;
                    var refE1 = CheckSegmentReversion(aggregationBuffer, false, aggregationBuffer.Length - 1, aggregationBuffer[^1]) ? e1.m_Bezier.a.xz : e1.m_Bezier.d.xz;

                    var zeroMarker = currentAggregationData.highwayDataId != default
                        && m_refPointHighway.TryGetValue(currentAggregationData.highwayDataId, out var refPoint) ? refPoint : m_globalZeroMarker;
                    bool isInverseAggregate = refE0.SqrDistance(zeroMarker) > refE1.SqrDistance(zeroMarker);

                    var currentDistance = 0f;
                    var isInverseOverride = false;
                    bool found = false;

                    for (int i = isInverseAggregate ? aggregationBuffer.Length - 1 : 0; isInverseAggregate ? i >= 0 : i < aggregationBuffer.Length; i += isInverseAggregate ? -1 : 1)
                    {
                        AggregateElement aggregateElement = aggregationBuffer[i];
                        float newDistance = currentDistance;
                        if (m_curveLookup.TryGetComponent(aggregateElement.m_Edge, out Curve curve))
                        {
                            newDistance += curve.m_Length;
                        }

                        if (m_subObjectsLookup.TryGetBuffer(aggregateElement.m_Edge, out var items) && items.Length > 0)
                        {
                            bool? isReversedSegment = null;
                            float overridePosition = -1;
                            float targetNewNumber = 0f;
                            float maxTarget = 1;
                            for (int j = 0; j < items.Length; j++)
                            {
                                if (m_attachedLookup.TryGetComponent(items[j].m_SubObject, out var attachmentData)
                                    && m_markerDataLookup.TryGetComponent(items[j].m_SubObject, out var markerData)
                                    && markerData.overrideMileage)
                                {
                                    isReversedSegment ??= CheckSegmentReversion(aggregationBuffer, isInverseAggregate, i, aggregateElement);
                                    var thisPosition = isReversedSegment.Value ? 1 - attachmentData.m_CurvePosition : attachmentData.m_CurvePosition;
                                    if (thisPosition > overridePosition && thisPosition <= maxTarget)
                                    {
                                        overridePosition = thisPosition;
                                        targetNewNumber = markerData.newMileage;
                                        maxTarget = thisPosition;
                                        isInverseOverride = markerData.reverseMileageCounting;
                                    }
                                }
                            }
                            if (overridePosition >= 0)
                            {
                                var t = new Bounds1(0f, overridePosition);
                                float s = math.saturate(MathUtils.Length(curve.m_Bezier, t) / math.max(1f, curve.m_Length));

                                m_CommandBuffer.AddComponent(unfilteredChunkIndex, entity, new ADRHighwayAggregationCacheData
                                {
                                    startDistanceOverrideKm = targetNewNumber - (math.lerp(currentDistance, newDistance, s) * (isInverseOverride ? -.001f : .001f)),
                                    reverseCounting = isInverseOverride
                                });
                                found = true;
                                break;
                            }
                        }
                        currentDistance = newDistance;
                    }
                    if (!found)
                    {
                        m_CommandBuffer.AddComponent(unfilteredChunkIndex, entity, new ADRHighwayAggregationCacheData { startDistanceOverrideKm = 0, reverseCounting = false });
                    }
                }
            }

            private bool CheckSegmentReversion(DynamicBuffer<AggregateElement> dynamicBuffer, bool isInverseAggregate, int i, AggregateElement aggregateElement)
            {
                bool isReversedSegment = false;
                if (i > 0)
                {
                    if (m_edgeLookup.TryGetComponent(aggregateElement.m_Edge, out Edge edge2)
                        && m_edgeLookup.TryGetComponent(dynamicBuffer[i - 1].m_Edge, out Edge edge3)
                        && (edge2.m_End == edge3.m_Start || edge2.m_End == edge3.m_End))
                    {
                        isReversedSegment = true;
                    }
                }
                else if (i < dynamicBuffer.Length - 1
                    && m_edgeLookup.TryGetComponent(aggregateElement.m_Edge, out Edge edge4)
                    && m_edgeLookup.TryGetComponent(dynamicBuffer[i + 1].m_Edge, out Edge edge5)
                    && (edge4.m_Start == edge5.m_Start || edge4.m_Start == edge5.m_End))
                {
                    isReversedSegment = true;
                }
                if (isInverseAggregate) isReversedSegment = !isReversedSegment;
                return isReversedSegment;
            }
        }
        #endregion

        #region Highways data part

        private readonly Dictionary<Colossal.Hash128, HighwayData> highwaysDataRegistry = new();

        public bool TryGetHighwayData(Colossal.Hash128 id, out HighwayData data) => highwaysDataRegistry.TryGetValue(id, out data);

        private List<HighwayData.UIData> ListHighwaysRegistered() => highwaysDataRegistry.Values.Select(x => x.ToUI()).ToList();

        private void SaveHighwayData(HighwayData.UIData newDataUI)
        {
            var newData = newDataUI.ToData();
            if (newData.Id == default)
            {
                newData.RegenerateId();
            }
            highwaysDataRegistry[newData.Id] = newData;
        }

        #endregion

        #region Serialization

        private const uint CURRENT_VERSION = 0;
        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());
            reader.Read(out int countHwData);
            highwaysDataRegistry.Clear();
            for (int i = 0; i < countHwData; i++)
            {
                reader.ReadNullCheck(out HighwayData highwayData);
                highwaysDataRegistry[highwayData.Id] = highwayData;
            }
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(highwaysDataRegistry.Count);
            foreach (var item in highwaysDataRegistry.Values)
            {
                writer.WriteNullCheck(item);
            }
        }

        public void SetDefaults(Context context)
        {
        }
        #endregion
    }

    public class HighwayData : ISerializable
    {
        private const uint CURRENT_VERSION = 0;
        public Colossal.Hash128 Id { get; private set; }
        public string prefix;
        public string suffix;
        public string name;
        public float2 refStartPoint;

        public HighwayData()
        {
            Id = Guid.NewGuid();
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());
            reader.Read(out Colossal.Hash128 id);
            Id = id;
            reader.Read(out prefix);
            reader.Read(out suffix);
            reader.Read(out name);
            reader.Read(out refStartPoint);
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(Id);
            writer.Write(prefix);
            writer.Write(suffix);
            writer.Write(name);
            writer.Write(refStartPoint);
        }

        internal void RegenerateId()
        {
            Id = Guid.NewGuid();
        }
        public UIData ToUI() => new()
        {
            Id = Id.ToString(),
            name = name,
            prefix = prefix,
            suffix = suffix,
            refStartPoint = new float[] { refStartPoint.x, refStartPoint.y }
        };

        public struct UIData
        {
            public string Id;
            public string prefix;
            public string suffix;
            public string name;
            public float[] refStartPoint;

            public readonly HighwayData ToData() => new()
            {
                Id = Colossal.Hash128.TryParse(Id ?? "", out var id) ? id : default,
                name = name.IsNullOrWhitespace() ? "?" : name,
                prefix = prefix.IsNullOrWhitespace() ? "?" : prefix,
                suffix = suffix.IsNullOrWhitespace() ? "?" : suffix,
                refStartPoint = refStartPoint?.Length >= 2 ? new float2(refStartPoint[0], refStartPoint[1]) : default
            };
        }
    }
}
