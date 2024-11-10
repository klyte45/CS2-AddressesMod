using Belzont.Interfaces;
using Belzont.Serialization;
using Colossal;
using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using Game.SceneFlow;
using Game.Simulation;
using Game.Tools;
using Game.Vehicles;
using System;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using Unity.Jobs;

namespace BelzontAdr
{
    public partial class AdrVehicleSystem : GameSystemBase, IBelzontBindable, IBelzontSerializableSingleton<AdrVehicleSystem>
    {
        #region Controller endpoints
        public void SetupCallBinder(Action<string, Delegate> eventCaller)
        {

        }

        private Action<string, object[]> eventCaller;

        public void SetupCaller(Action<string, object[]> eventCaller)
        {
            this.eventCaller = eventCaller;
        }

        public void SetupEventBinder(Action<string, Delegate> eventCaller)
        {
        }
        #endregion

        private ModificationEndBarrier m_Barrier;
        private TimeSystem m_timeSystem;
        private EntityQuery m_unregisteredVehiclesQuery;

        private ulong currentSerialNumber;
        protected override void OnCreate()
        {
            m_Barrier = World.GetOrCreateSystemManaged<ModificationEndBarrier>();
            m_timeSystem = World.GetOrCreateSystemManaged<TimeSystem>();
            m_unregisteredVehiclesQuery = GetEntityQuery(new EntityQueryDesc[]
             {
                    new ()
                    {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Vehicle>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<ADRVehicleData>(),
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
             });
        }


        protected unsafe override void OnUpdate()
        {
            if (GameManager.instance.isGameLoading || GameManager.instance.isLoading) return;
            if (!m_unregisteredVehiclesQuery.IsEmptyIgnoreFilter)
            {
                var counter = new NativeCounter(Unity.Collections.Allocator.Temp);
                var job = new ADRRegisterVehicles
                {
                    m_cmdBuffer = m_Barrier.CreateCommandBuffer().AsParallelWriter(),
                    m_entityHdl = GetEntityTypeHandle(),
                    m_refDateTime = m_timeSystem.GetCurrentDateTime().Ticks,
                    m_serialNumber = counter.ToConcurrent(),
                    refSerialNumber = currentSerialNumber
                };
                Dependency = job.ScheduleParallel(m_unregisteredVehiclesQuery, Dependency);
                Dependency.GetAwaiter().OnCompleted(() =>
                {
                    currentSerialNumber += (uint)counter.Count;
                    counter.Dispose();
                });
            }
        }

        [BurstCompile]
        private unsafe struct ADRRegisterVehicles : IJobChunk
        {
            public EntityTypeHandle m_entityHdl;
            public NativeCounter.Concurrent m_serialNumber;
            public EntityCommandBuffer.ParallelWriter m_cmdBuffer;
            public ulong refSerialNumber;
            public long m_refDateTime;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entites = chunk.GetNativeArray(m_entityHdl);
                var length = entites.Length;
                //   Debug.Log($"Chunk #{unfilteredChunkIndex} size: {length}");
                for (int i = 0; i < length; i++)
                {
                    var newItem = new ADRVehicleData
                    {
                        serialNumber = refSerialNumber + (uint)m_serialNumber.Increment(),
                        manifactureTicks = m_refDateTime,
                    };
                    m_cmdBuffer.AddComponent(unfilteredChunkIndex, entites[i], newItem);
#if DEBUG
                    if (newItem.serialNumber % 5 == 0) UnityEngine.Debug.Log($"Added serial nº: {newItem.serialNumber}");
#endif
                }

            }
        }

        #region Serialization
        World IBelzontSerializableSingleton<AdrVehicleSystem>.World => World;
        void IBelzontSerializableSingleton<AdrVehicleSystem>.Deserialize<TReader>(TReader reader)
        {
        }

        void IBelzontSerializableSingleton<AdrVehicleSystem>.Serialize<TWriter>(TWriter writer)
        {
        }

        JobHandle IJobSerializable.SetDefaults(Context context)
        {
            currentSerialNumber = 0;
            return default;
        }
        #endregion
    }
}

