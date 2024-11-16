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
            eventCaller("vehicles.getAdrData", GetVehicleData);
        }

        private Action<string, object[]> eventCaller;

        public void SetupCaller(Action<string, object[]> eventCaller)
        {
            this.eventCaller = eventCaller;
        }

        public void SetupEventBinder(Action<string, Delegate> eventCaller)
        {
        }

        private ADRVehicleData.CohtmlSafe GetVehicleData(Entity e) => ADRVehicleData.CohtmlSafe.From(EntityManager.TryGetComponent<ADRVehicleData>(e, out var result) ? result : default);
        #endregion

        private ModificationEndBarrier m_Barrier;
        private TimeSystem m_timeSystem;
        private EntityQuery m_unregisteredVehiclesQuery;
        private EntityQuery m_dirtyVehiclesPlateQuery;

        private ulong currentSerialNumber;

        private VehiclePlateSettings roadVehiclesPlatesSettings;
        private VehiclePlateSettings railVehiclesPlatesSettings;
        private VehiclePlateSettings airVehiclesPlatesSettings;
        private VehiclePlateSettings waterVehiclesPlatesSettings;



        public VehiclePlateSettings RoadVehiclesPlatesSettings
        {
            get => roadVehiclesPlatesSettings; set
            {
                roadVehiclesPlatesSettings = value;
                MarkEntitiesPlateDirty(ComponentType.ReadOnly<Car>());
            }
        }

        private void MarkEntitiesPlateDirty(ComponentType specificType)
        {
            EntityManager.AddComponent<ADRVehiclePlateDataDirty>(GetEntityQuery(new EntityQueryDesc[]
                 {
                    new ()
                    {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Vehicle>(),
                            ComponentType.ReadOnly<ADRVehicleData>(),
                            specificType
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
             }));
        }

        public VehiclePlateSettings RailVehiclesPlatesSettings
        {
            get => railVehiclesPlatesSettings; set
            {
                railVehiclesPlatesSettings = value;
                MarkEntitiesPlateDirty(ComponentType.ReadOnly<Train>());
            }
        }
        public VehiclePlateSettings AirVehiclesPlatesSettings
        {
            get => airVehiclesPlatesSettings; set
            {
                airVehiclesPlatesSettings = value;

                MarkEntitiesPlateDirty(ComponentType.ReadOnly<Aircraft>());
            }
        }
        public VehiclePlateSettings WaterVehiclesPlatesSettings
        {
            get => waterVehiclesPlatesSettings; set
            {
                waterVehiclesPlatesSettings = value;
                MarkEntitiesPlateDirty(ComponentType.ReadOnly<Watercraft>());
            }
        }

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
                    m_watercraftLkp = GetComponentLookup<Watercraft>(),
                    m_trainLkp = GetComponentLookup<Train>(),
                    railVehiclesPlatesSettings = railVehiclesPlatesSettings,
                    m_adrVehicleDataLkp = GetComponentLookup<ADRVehicleData>(),
                    m_adrVehiclePlateDataLkp = GetComponentLookup<ADRVehiclePlateDataDirty>(),
                    m_controllerLkp = GetComponentLookup<Controller>(),
                    m_layoutElementLkp = GetBufferLookup<LayoutElement>(),
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
                    railVehiclesPlatesSettings = railVehiclesPlatesSettings,
                    m_trainLkp = GetComponentLookup<Train>(),
                    m_aircraftLkp = GetComponentLookup<Aircraft>(),
                    m_watercraftLkp = GetComponentLookup<Watercraft>(),
                    m_vehicleHdl = GetComponentTypeHandle<ADRVehicleData>(),
                    m_adrVehicleDataLkp = GetComponentLookup<ADRVehicleData>(),
                    m_adrVehiclePlateDataLkp = GetComponentLookup<ADRVehiclePlateDataDirty>(),
                    m_controllerLkp = GetComponentLookup<Controller>(),
                    m_layoutElementLkp = GetBufferLookup<LayoutElement>()
                };
                Dependency = job.ScheduleParallel(m_dirtyVehiclesPlateQuery, Dependency);
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
            reader.Read(out railVehiclesPlatesSettings);

        }

        void IBelzontSerializableSingleton<AdrVehicleSystem>.Serialize<TWriter>(TWriter writer)
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(roadVehiclesPlatesSettings);
            writer.Write(waterVehiclesPlatesSettings);
            writer.Write(airVehiclesPlatesSettings);
            writer.Write(railVehiclesPlatesSettings);
        }

        JobHandle IJobSerializable.SetDefaults(Context context)
        {
            currentSerialNumber = 0;
            roadVehiclesPlatesSettings = VehiclePlateSettings.CreateRoadVehicleDefault();
            airVehiclesPlatesSettings = VehiclePlateSettings.CreateAirVehicleDefault();
            waterVehiclesPlatesSettings = VehiclePlateSettings.CreateWaterVehicleDefault();
            railVehiclesPlatesSettings = VehiclePlateSettings.CreateRailVehicleDefault();
            return default;
        }

        #endregion
    }
}

