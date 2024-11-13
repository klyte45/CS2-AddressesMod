using Belzont.Interfaces;
using Belzont.Serialization;
using Belzont.Utils;
using Colossal;
using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using Game.SceneFlow;
using Game.Simulation;
using Game.Tools;
using Game.Vehicles;
using System;
using System.Linq;
using System.Reflection;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using Unity.Jobs;

namespace BelzontAdr
{
    public partial class AdrVehicleSystem : GameSystemBase, IBelzontBindable, IBelzontSerializableSingleton<AdrVehicleSystem>
    {
        private const uint CURRENT_VERSION = 0;

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
        private EntityQuery m_dirtyVehiclesPlateQuery;

        private ulong currentSerialNumber;

        private VehiclePlateSettings roadVehiclesPlatesSettings;
        private VehiclePlateSettings airVehiclesPlatesSettings;
        private VehiclePlateSettings waterVehiclesPlatesSettings;

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
            m_dirtyVehiclesPlateQuery = GetEntityQuery(new EntityQueryDesc[]
             {
                    new ()
                    {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Vehicle>(),
                            ComponentType.ReadOnly<ADRVehicleData>(),
                            ComponentType.ReadOnly<ADRVehiclePlateDataDirty>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
             });
        }


        private bool weInitialized = false;
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            if (!weInitialized)
            {
                weInitialized = true;
                if (AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "BelzontWE") is Assembly weAssembly
                    && weAssembly.GetExportedTypes().FirstOrDefault(x => x.Name == "WEVehicleFn") is Type t
                    && t.GetField("GetVehiclePlate_binding", RedirectorUtils.allFlags) is FieldInfo vehiclePlateField)
                {
                    var originalValue = vehiclePlateField.GetValue(null) as Func<Entity, string>;
                    vehiclePlateField.SetValue(null, (Entity e) => EntityManager.TryGetComponent(e, out ADRVehicleData vehicleData) ? vehicleData.calculatedPlate.ToString() : originalValue(e));
                }
            }
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
                    m_refDateTime = m_timeSystem.GetCurrentDateTime().ToMonthsEpoch(),
                    m_serialNumber = counter.ToConcurrent(),
                    refSerialNumber = currentSerialNumber,
                    roadPlateSettings = roadVehiclesPlatesSettings,
                    airPlatesSettings = airVehiclesPlatesSettings,
                    waterPlatesSettings = waterVehiclesPlatesSettings,
                    m_aircraftLkp = GetComponentLookup<Aircraft>(),
                    m_watercraftLkp = GetComponentLookup<Watercraft>()
                };
                Dependency = job.ScheduleParallel(m_unregisteredVehiclesQuery, Dependency);
                Dependency.GetAwaiter().OnCompleted(() =>
                {
                    currentSerialNumber += (uint)counter.Count;
                    counter.Dispose();
                });
            }
            if (!m_dirtyVehiclesPlateQuery.IsEmptyIgnoreFilter)
            {
                var job = new ADRUpdateVehiclesPlates
                {
                    m_cmdBuffer = m_Barrier.CreateCommandBuffer().AsParallelWriter(),
                    m_entityHdl = GetEntityTypeHandle(),
                    roadPlateSettings = roadVehiclesPlatesSettings,
                    airPlatesSettings = airVehiclesPlatesSettings,
                    waterPlatesSettings = waterVehiclesPlatesSettings,
                    m_aircraftLkp = GetComponentLookup<Aircraft>(),
                    m_watercraftLkp = GetComponentLookup<Watercraft>(),
                    m_vehicleHdl = GetComponentTypeHandle<ADRVehicleData>()
                };
                Dependency = job.ScheduleParallel(m_dirtyVehiclesPlateQuery, Dependency);
            }
        }

        [BurstCompile]
        private unsafe struct ADRRegisterVehicles : IJobChunk
        {
            public EntityTypeHandle m_entityHdl;
            public NativeCounter.Concurrent m_serialNumber;
            public EntityCommandBuffer.ParallelWriter m_cmdBuffer;
            public ComponentLookup<Aircraft> m_aircraftLkp;
            public ComponentLookup<Watercraft> m_watercraftLkp;
            public ulong refSerialNumber;
            public int m_refDateTime;
            public VehiclePlateSettings roadPlateSettings;
            public VehiclePlateSettings airPlatesSettings;
            public VehiclePlateSettings waterPlatesSettings;


            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entites = chunk.GetNativeArray(m_entityHdl);
                var length = entites.Length;
                //   Debug.Log($"Chunk #{unfilteredChunkIndex} size: {length}");
                for (int i = 0; i < length; i++)
                {
                    var settingEffective =
                        m_aircraftLkp.HasComponent(entites[i]) ? airPlatesSettings
                        : m_watercraftLkp.HasComponent(entites[i]) ? waterPlatesSettings
                        : roadPlateSettings;

                    var serialNumber = refSerialNumber + (uint)m_serialNumber.Increment();
                    var newItem = new ADRVehicleData
                    {
                        serialNumber = serialNumber,
                        manufactureMonthsFromEpoch = m_refDateTime,
                        calculatedPlate = settingEffective.GetPlateFor(0, serialNumber, m_refDateTime),
                        checksumRule = settingEffective.Checksum,
                    };
                    m_cmdBuffer.AddComponent(unfilteredChunkIndex, entites[i], newItem);
#if DEBUG
                    if (newItem.serialNumber % 5 == 0) UnityEngine.Debug.Log($"Added serial nº: {newItem.serialNumber} => {newItem.calculatedPlate}");
#endif
                }

            }
        }

        [BurstCompile]
        private unsafe struct ADRUpdateVehiclesPlates : IJobChunk
        {
            public EntityTypeHandle m_entityHdl;
            public ComponentTypeHandle<ADRVehicleData> m_vehicleHdl;
            public EntityCommandBuffer.ParallelWriter m_cmdBuffer;
            public ComponentLookup<Aircraft> m_aircraftLkp;
            public ComponentLookup<Watercraft> m_watercraftLkp;
            public VehiclePlateSettings roadPlateSettings;
            public VehiclePlateSettings airPlatesSettings;
            public VehiclePlateSettings waterPlatesSettings;


            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entites = chunk.GetNativeArray(m_entityHdl);
                var vehicles = chunk.GetNativeArray(ref m_vehicleHdl);
                var length = entites.Length;

                for (int i = 0; i < length; i++)
                {
                    var vehicleData = vehicles[i];
                    var settingEffective =
                        m_aircraftLkp.HasComponent(entites[i]) ? airPlatesSettings
                        : m_watercraftLkp.HasComponent(entites[i]) ? waterPlatesSettings
                        : roadPlateSettings;

                    var serialNumber = vehicleData.serialNumber;

                    vehicleData.calculatedPlate = settingEffective.GetPlateFor(0, serialNumber, vehicleData.manufactureMonthsFromEpoch);
                    vehicleData.checksumRule = settingEffective.Checksum;

                    m_cmdBuffer.SetComponent(unfilteredChunkIndex, entites[i], vehicleData);
                    m_cmdBuffer.RemoveComponent<ADRVehiclePlateDataDirty>(unfilteredChunkIndex, entites[i]);
                }

            }
        }

        #region Serialization
        World IBelzontSerializableSingleton<AdrVehicleSystem>.World => World;
        void IBelzontSerializableSingleton<AdrVehicleSystem>.Deserialize<TReader>(TReader reader)
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception($"Invalid version of {GetType()}!");
            }
            reader.Read(out roadVehiclesPlatesSettings);
            reader.Read(out waterVehiclesPlatesSettings);
            reader.Read(out airVehiclesPlatesSettings);

        }

        void IBelzontSerializableSingleton<AdrVehicleSystem>.Serialize<TWriter>(TWriter writer)
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(roadVehiclesPlatesSettings);
            writer.Write(waterVehiclesPlatesSettings);
            writer.Write(airVehiclesPlatesSettings);
        }

        JobHandle IJobSerializable.SetDefaults(Context context)
        {
            currentSerialNumber = 0;
            roadVehiclesPlatesSettings = VehiclePlateSettings.CreateRoadVehicleDefault();
            airVehiclesPlatesSettings = VehiclePlateSettings.CreateAirVehicleDefault();
            waterVehiclesPlatesSettings = VehiclePlateSettings.CreateWaterVehicleDefault();
            return default;
        }

        #endregion
    }


}

