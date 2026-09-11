using Belzont.Interfaces;
using Belzont.Serialization;
using Belzont.Utils;
using BridgeWE;
using Colossal.Entities;
using Colossal.OdinSerializer.Utilities;
using Colossal.Serialization.Entities;
using Game.Common;
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
using Unity.Mathematics;
using static Belzont.Utils.NameSystemExtensions;
using static BelzontAdr.ADRHighwayMarkerData;
using Colossal.Core;







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
        public MultiUIValueBinding<DisplayInformation, int> Tool_DisplayInformation { get; private set; }
        public MultiUIValueBinding<int> Tool_NumericCustomParam1 { get; private set; }
        public MultiUIValueBinding<int> Tool_NumericCustomParam2 { get; private set; }
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
            Tool_DisplayInformation = new(m_dataForNewItem.displayInformation, $"{PREFIX}{nameof(Tool_DisplayInformation)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (DisplayInformation)x);
            Tool_NumericCustomParam1 = new(m_dataForNewItem.numericCustomParam1, $"{PREFIX}{nameof(Tool_NumericCustomParam1)}", EventCaller, CallBinder);
            Tool_NumericCustomParam2 = new(m_dataForNewItem.numericCustomParam2, $"{PREFIX}{nameof(Tool_NumericCustomParam2)}", EventCaller, CallBinder);
            Tool_PylonCount = new(m_dataForNewItem.pylonCount, $"{PREFIX}{nameof(Tool_PylonCount)}", EventCaller, CallBinder);
            Tool_PylonSpacing = new(m_dataForNewItem.pylonSpacing, $"{PREFIX}{nameof(Tool_PylonSpacing)}", EventCaller, CallBinder);
            Tool_PylonMaterial = new(m_dataForNewItem.pylonMaterial, $"{PREFIX}{nameof(Tool_PylonMaterial)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (PylonMaterial)x);
            Tool_PylonHeight = new(m_dataForNewItem.pylonHeight, $"{PREFIX}{nameof(Tool_PylonHeight)}", EventCaller, CallBinder);
            Tool_PylonFormat = new(m_dataForNewItem.pylonFormat, $"{PREFIX}{nameof(Tool_PylonFormat)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (PylonFormat)x);


            CallBinder($"{PREFIX}isCurrentPrefabRoadMarker", IsCurrentPrefabRoadMarker);

            Tool_DisplayInformation.OnScreenValueChanged += (x) => { m_dataForNewItem.displayInformation = x; MarkTempMarksDirty(); };
            Tool_NumericCustomParam1.OnScreenValueChanged += (x) => { m_dataForNewItem.numericCustomParam1 = x; MarkTempMarksDirty(); };
            Tool_NumericCustomParam2.OnScreenValueChanged += (x) => { m_dataForNewItem.numericCustomParam2 = x; MarkTempMarksDirty(); };
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

        public MultiUIValueBinding<DisplayInformation, int> InfoPanel_DisplayInformation { get; private set; }
        public MultiUIValueBinding<int> InfoPanel_NumericCustomParam1 { get; private set; }
        public MultiUIValueBinding<int> InfoPanel_NumericCustomParam2 { get; private set; }
        public MultiUIValueBinding<int> InfoPanel_PylonCount { get; private set; }
        public MultiUIValueBinding<float> InfoPanel_PylonSpacing { get; private set; }
        public MultiUIValueBinding<PylonMaterial, int> InfoPanel_PylonMaterial { get; private set; }
        public MultiUIValueBinding<float> InfoPanel_PylonHeight { get; private set; }
        public MultiUIValueBinding<PylonFormat, int> InfoPanel_PylonFormat { get; private set; }


        private void DoInitValueBindings_InfoPanel(Action<string, object[]> EventCaller, Action<string, Delegate> CallBinder)
        {
            InfoPanel_DisplayInformation = new(default, $"{PREFIX}{nameof(InfoPanel_DisplayInformation)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (DisplayInformation)x);
            InfoPanel_NumericCustomParam1 = new(default, $"{PREFIX}{nameof(InfoPanel_NumericCustomParam1)}", EventCaller, CallBinder);
            InfoPanel_NumericCustomParam2 = new(default, $"{PREFIX}{nameof(InfoPanel_NumericCustomParam2)}", EventCaller, CallBinder);
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

            MainThreadDispatcher.RegisterUpdater(registerSelfOnUI);

            InfoPanel_DisplayInformation.OnScreenValueChanged += (x) => EnqueueModification<DisplayInformation, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.displayInformation = x; return currentItem; });
            InfoPanel_NumericCustomParam1.OnScreenValueChanged += (x) => EnqueueModification<int, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.numericCustomParam1 = x; return currentItem; });
            InfoPanel_NumericCustomParam2.OnScreenValueChanged += (x) => EnqueueModification<int, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.numericCustomParam2 = x; return currentItem; });
            InfoPanel_PylonCount.OnScreenValueChanged += (x) => EnqueueModification<byte, ADRHighwayMarkerData>((byte)x, (x, currentItem, _) => { currentItem.pylonCount = (byte)math.clamp(x, 1, 2); return currentItem; });
            InfoPanel_PylonSpacing.OnScreenValueChanged += (x) => EnqueueModification<float, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.pylonSpacing = math.clamp(x, 0.05f, 3f); return currentItem; });
            InfoPanel_PylonMaterial.OnScreenValueChanged += (x) => EnqueueModification<PylonMaterial, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.pylonMaterial = x; return currentItem; });
            InfoPanel_PylonHeight.OnScreenValueChanged += (x) => EnqueueModification<float, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.pylonHeight = math.clamp(x, .25f, 10); return currentItem; });
            InfoPanel_PylonFormat.OnScreenValueChanged += (x) => EnqueueModification<PylonFormat, ADRHighwayMarkerData>(x, (x, currentItem, _) => { currentItem.pylonFormat = x; return currentItem; });

        }

        void OnSelectionChanged(Entity entity, Entity prefab, Unity.Mathematics.float3 position)
        {
            if (Enabled && EntityManager.TryGetComponent<ADRHighwayMarkerData>(entity, out var component))
            {
                m_executionQueue.Enqueue(() =>
                {
                    InfoPanel_DisplayInformation.Value = component.displayInformation;
                    InfoPanel_NumericCustomParam1.Value = component.numericCustomParam1;
                    InfoPanel_NumericCustomParam2.Value = component.numericCustomParam2;
                    InfoPanel_PylonCount.Value = component.pylonCount;
                    InfoPanel_PylonSpacing.Value = component.pylonSpacing;
                    InfoPanel_PylonMaterial.Value = component.pylonMaterial;
                    InfoPanel_PylonHeight.Value = component.pylonHeight;
                    InfoPanel_PylonFormat.Value = component.pylonFormat;
                });
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
        private ModificationEndBarrier m_modificationEndBarrier;
        private ToolSystem m_toolSystem;
        private Adr_WEIntegrationSystem m_adrWeIntegrationSystem;
        private AdrEditorUISystem m_adrEditorUI;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_modificationEndBarrier = World.GetOrCreateSystemManaged<ModificationEndBarrier>();
            m_adrWeIntegrationSystem = World.GetOrCreateSystemManaged<Adr_WEIntegrationSystem>();
            m_toolSystem = World.GetExistingSystemManaged<ToolSystem>();
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
                        m_CommandBuffer.SetComponent(unfilteredChunkIndex, entity, m_currentToolData);
                    }
                    m_CommandBuffer.RemoveComponent<ADRHighwayMarkerDataDirty>(unfilteredChunkIndex, entity);
                }
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
