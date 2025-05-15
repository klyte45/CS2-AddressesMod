using Belzont.Interfaces;
using Belzont.Serialization;
using Belzont.Utils;
using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game.Common;
using Game.Tools;
using Game.UI.InGame;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using static BelzontAdr.ADRHighwayMarkerData;
using Unity.Burst.Intrinsics;




#if BURST
using Unity.Burst;
#else
#endif

namespace BelzontAdr
{
    public partial class AdrHighwayRoutesSystem : SystemBase, IBelzontBindable, IBelzontSerializableSingleton<AdrNamesetSystem>
    {

        private const string PREFIX = "highwayRoutes.";

        private Action<string, object[]> EventCaller { get; set; }
        private Action<string, Delegate> CallBinder { get; set; }

        public void SetupCallBinder(Action<string, Delegate> callBinder)
        {
            CallBinder = callBinder;
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

        #region Tool controller part
        public MultiUIValueBinding<Colossal.Hash128, string> Tool_RouteId { get; private set; }
        public MultiUIValueBinding<RouteDirection, int> Tool_RouteDirection { get; private set; }
        public MultiUIValueBinding<DisplayInformation, int> Tool_DisplayInformation { get; private set; }
        public MultiUIValueBinding<int> Tool_NumericCustomParam1 { get; private set; }
        public MultiUIValueBinding<int> Tool_NumericCustomParam2 { get; private set; }
        public MultiUIValueBinding<float> Tool_NewMileage { get; private set; }
        public MultiUIValueBinding<bool> Tool_OverrideMileage { get; private set; }
        public MultiUIValueBinding<bool> Tool_ReverseMileageCounting { get; private set; }

        private void DoInitValueBindings_Tool(Action<string, object[]> EventCaller, Action<string, Delegate> CallBinder)
        {
            Tool_RouteId = new(default, $"{PREFIX}{nameof(Tool_RouteId)}", EventCaller, CallBinder, (x, _) => x.ToString(), (x, _) => Colossal.Hash128.Parse(x));
            Tool_RouteDirection = new(default, $"{PREFIX}{nameof(Tool_RouteDirection)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (RouteDirection)x);
            Tool_DisplayInformation = new(default, $"{PREFIX}{nameof(Tool_DisplayInformation)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (DisplayInformation)x);
            Tool_NumericCustomParam1 = new(default, $"{PREFIX}{nameof(Tool_NumericCustomParam1)}", EventCaller, CallBinder);
            Tool_NumericCustomParam2 = new(default, $"{PREFIX}{nameof(Tool_NumericCustomParam2)}", EventCaller, CallBinder);
            Tool_NewMileage = new(default, $"{PREFIX}{nameof(Tool_NewMileage)}", EventCaller, CallBinder);
            Tool_OverrideMileage = new(default, $"{PREFIX}{nameof(Tool_OverrideMileage)}", EventCaller, CallBinder);
            Tool_ReverseMileageCounting = new(default, $"{PREFIX}{nameof(Tool_ReverseMileageCounting)}", EventCaller, CallBinder);

            CallBinder($"{PREFIX}isCurrentPrefabRoadMarker", IsCurrentPrefabRoadMarker);
        }

        private bool IsCurrentPrefabRoadMarker() => m_toolSystem.activeTool is ObjectToolSystem && m_toolSystem.activePrefab.Has<ADRRoadMarkerObject>();

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

        private void DoInitValueBindings_InfoPanel(Action<string, object[]> EventCaller, Action<string, Delegate> CallBinder)
        {
            m_selectedInfoUISystem = World.GetExistingSystemManaged<SelectedInfoUISystem>();

            InfoPanel_RouteId = new(default, $"{PREFIX}{nameof(InfoPanel_RouteId)}", EventCaller, CallBinder, (x, _) => x.ToString(), (x, _) => Colossal.Hash128.Parse(x));
            InfoPanel_RouteDirection = new(default, $"{PREFIX}{nameof(InfoPanel_RouteDirection)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (RouteDirection)x);
            InfoPanel_DisplayInformation = new(default, $"{PREFIX}{nameof(InfoPanel_DisplayInformation)}", EventCaller, CallBinder, (x, _) => (int)x, (x, _) => (DisplayInformation)x);
            InfoPanel_NumericCustomParam1 = new(default, $"{PREFIX}{nameof(InfoPanel_NumericCustomParam1)}", EventCaller, CallBinder);
            InfoPanel_NumericCustomParam2 = new(default, $"{PREFIX}{nameof(InfoPanel_NumericCustomParam2)}", EventCaller, CallBinder);
            InfoPanel_NewMileage = new(default, $"{PREFIX}{nameof(InfoPanel_NewMileage)}", EventCaller, CallBinder);
            InfoPanel_OverrideMileage = new(default, $"{PREFIX}{nameof(InfoPanel_OverrideMileage)}", EventCaller, CallBinder);
            InfoPanel_ReverseMileageCounting = new(default, $"{PREFIX}{nameof(InfoPanel_ReverseMileageCounting)}", EventCaller, CallBinder);

            m_selectedInfoUISystem.eventSelectionChanged += OnSelectionChanged;

            InfoPanel_RouteId.OnScreenValueChanged += (x) => EnqueueModification<Colossal.Hash128, ADRHighwayMarkerData>(x, (x, currentItem) => { currentItem.routeDataIndex = x; return currentItem; });
            InfoPanel_RouteDirection.OnScreenValueChanged += (x) => EnqueueModification<RouteDirection, ADRHighwayMarkerData>(x, (x, currentItem) => { currentItem.routeDirection = x; return currentItem; });
            InfoPanel_DisplayInformation.OnScreenValueChanged += (x) => EnqueueModification<DisplayInformation, ADRHighwayMarkerData>(x, (x, currentItem) => { currentItem.displayInformation = x; return currentItem; });
            InfoPanel_NumericCustomParam1.OnScreenValueChanged += (x) => EnqueueModification<int, ADRHighwayMarkerData>(x, (x, currentItem) => { currentItem.numericCustomParam1 = x; return currentItem; });
            InfoPanel_NumericCustomParam2.OnScreenValueChanged += (x) => EnqueueModification<int, ADRHighwayMarkerData>(x, (x, currentItem) => { currentItem.numericCustomParam2 = x; return currentItem; });
            InfoPanel_NewMileage.OnScreenValueChanged += (x) => EnqueueModification<float, ADRHighwayMarkerData>(x, (x, currentItem) => { currentItem.newMileage = x; return currentItem; });
            InfoPanel_OverrideMileage.OnScreenValueChanged += (x) => EnqueueModification<bool, ADRHighwayMarkerData>(x, (x, currentItem) => { currentItem.overrideMileage = x; return currentItem; });
            InfoPanel_ReverseMileageCounting.OnScreenValueChanged += (x) => EnqueueModification<bool, ADRHighwayMarkerData>(x, (x, currentItem) => { currentItem.reverseMileageCounting = x; return currentItem; });
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
            }
        }

        private readonly Queue<Action> m_executionQueue = new();

        internal void EnqueueModification<T, W>(T newVal, Func<T, W, W> x) where W : unmanaged, IComponentData
        {
            var target = m_selectedInfoUISystem.selectedEntity;
            m_executionQueue.Enqueue(() =>
            {
                if (EntityManager.TryGetComponent<W>(target, out var currentItem))
                {
                    currentItem = x(newVal, currentItem);
                    EntityManager.SetComponentData(target, currentItem);
                }
            });

        }
        #endregion


        #region System part

        private EntityQuery m_DirtyMarkerData;
        private ModificationEndBarrier m_modificationEndBarrier;
        private ToolSystem m_toolSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_modificationEndBarrier = World.GetOrCreateSystemManaged<ModificationEndBarrier>();
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

        }

        protected override void OnDestroy()
        {
            m_selectedInfoUISystem.eventSelectionChanged -= OnSelectionChanged;
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            while (m_executionQueue.TryDequeue(out var action))
            {
                action();
            }
            if (!m_DirtyMarkerData.IsEmpty)
            {
                var currentToolData = new ADRHighwayMarkerData
                {
                    routeDataIndex = Tool_RouteId.Value,
                    routeDirection = Tool_RouteDirection.Value,
                    displayInformation = Tool_DisplayInformation.Value,
                    numericCustomParam1 = Tool_NumericCustomParam1.Value,
                    numericCustomParam2 = Tool_NumericCustomParam2.Value,
                    newMileage = Tool_NewMileage.Value,
                    overrideMileage = Tool_OverrideMileage.Value,
                    reverseMileageCounting = Tool_ReverseMileageCounting.Value,
                    Initialized = true
                };

                LogUtils.DoInfoLog($"routeDataIndex = {currentToolData.routeDataIndex}");
                LogUtils.DoInfoLog($"routeDirection  = {currentToolData.routeDirection}");
                LogUtils.DoInfoLog($"displayInformation  = {currentToolData.displayInformation}");
                LogUtils.DoInfoLog($"numericCustomParam1  = {currentToolData.numericCustomParam1}");
                LogUtils.DoInfoLog($"numericCustomParam2  = {currentToolData.numericCustomParam2}");
                LogUtils.DoInfoLog($"newMileage  = {currentToolData.newMileage}");
                LogUtils.DoInfoLog($"overrideMileage  = {currentToolData.overrideMileage}");
                LogUtils.DoInfoLog($"reverseMileageCounting  = {currentToolData.reverseMileageCounting}");
                LogUtils.DoInfoLog($"-------------------------------------------------------------");

                var updater = new HighwayDataUpdater
                {
                    m_currentToolData = currentToolData,
                    m_CommandBuffer = m_modificationEndBarrier.CreateCommandBuffer().AsParallelWriter(),
                    m_EntityType = GetEntityTypeHandle(),
                    m_markerData = GetComponentTypeHandle<ADRHighwayMarkerData>()
                };
                updater.ScheduleParallel(m_DirtyMarkerData, Dependency).Complete();
            }
        }
#if BURST
        [BurstCompile]
#endif
        private struct HighwayDataUpdater : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
            public ComponentTypeHandle<ADRHighwayMarkerData> m_markerData;
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
                    if (!currentMarkData.Initialized)
                    {
                        m_CommandBuffer.SetComponent<ADRHighwayMarkerData>(unfilteredChunkIndex, entity, m_currentToolData);
                    }
                    m_CommandBuffer.RemoveComponent<ADRHighwayMarkerDataDirty>(unfilteredChunkIndex, entity);
                }
            }
        }
        #endregion
        #region Serialization

        private const uint CURRENT_VERSION = 0;
        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
        }

        public JobHandle SetDefaults(Context context)
        {
            return Dependency;
        }
        #endregion
    }
}

