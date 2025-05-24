using Belzont.Interfaces;
using Belzont.Serialization;
using Belzont.Utils;
using Colossal;
using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game;
using Game.Buildings;
using Game.Common;
using Game.Companies;
using Game.Objects;
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

        private EndFrameBarrier m_Barrier;
        private TimeSystem m_timeSystem;
        private EntityQuery m_unregisteredVehiclesQuery;
        private EntityQuery m_dirtyVehiclesPlateQuery;
        private EntityQuery m_unregisteredVehicleSpawnerQuery;
        private ulong currentSerialNumberVehicles;
        private uint currentSerialNumberVehicleSources;

        private VehiclePlateSettings roadVehiclesPlatesSettings = new();
        private VehiclePlateSettings railVehiclesPlatesSettings = new();
        private VehiclePlateSettings airVehiclesPlatesSettings = new();
        private VehiclePlateSettings waterVehiclesPlatesSettings = new();



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
            m_Barrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();
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

            m_unregisteredVehicleSpawnerQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new ()
                    {
                        Any = new ComponentType[]
                        {
                            ComponentType.ReadOnly<PoliceStation>(),
                            ComponentType.ReadOnly<Hospital>(),
                            ComponentType.ReadOnly<DeathcareFacility>(),
                            ComponentType.ReadOnly<FireStation>(),
                            ComponentType.ReadOnly<GarbageFacility>(),
                            ComponentType.ReadOnly<TransportDepot>(),
                            ComponentType.ReadOnly<CargoTransportStation>(),
                            ComponentType.ReadOnly<MaintenanceDepot>(),
                            ComponentType.ReadOnly<PostFacility>(),
                            ComponentType.ReadOnly<TransportCompany>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<ADRVehicleSourceData>(),
                            ComponentType.ReadOnly<Owner>(),
                            ComponentType.ReadOnly<OutsideConnection>(),
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
            });
            RequireAnyForUpdate(m_unregisteredVehiclesQuery, m_dirtyVehiclesPlateQuery
#if DEBUG
         , m_unregisteredVehicleSpawnerQuery
#endif
                );
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
            if (GameManager.instance.isGameLoading || GameManager.instance.isLoading)
            {
                roadVehiclesPlatesSettings ??= VehiclePlateSettings.CreateRoadVehicleDefault(m_timeSystem);
                airVehiclesPlatesSettings ??= VehiclePlateSettings.CreateAirVehicleDefault(m_timeSystem);
                waterVehiclesPlatesSettings ??= VehiclePlateSettings.CreateWaterVehicleDefault(m_timeSystem);
                railVehiclesPlatesSettings ??= VehiclePlateSettings.CreateRailVehicleDefault(m_timeSystem);
                return;
            }
            if (!m_unregisteredVehiclesQuery.IsEmptyIgnoreFilter)
            {
                var counter = new NativeCounter(Unity.Collections.Allocator.Temp);
                var roadPlatesSettings = roadVehiclesPlatesSettings.ForBurstJob;
                var airPlatesSettings = airVehiclesPlatesSettings.ForBurstJob;
                var waterPlatesSettings = waterVehiclesPlatesSettings.ForBurstJob;
                var railPlatesSettings = railVehiclesPlatesSettings.ForBurstJob;
                var job = new ADRRegisterVehicles
                {
                    m_cmdBuffer = m_Barrier.CreateCommandBuffer().AsParallelWriter(),
                    m_entityHdl = GetEntityTypeHandle(),
                    m_refDateTime = m_timeSystem.GetCurrentDateTime().ToMonthsEpoch(),
                    m_serialNumber = counter.ToConcurrent(),
                    refSerialNumber = currentSerialNumberVehicles,
                    roadPlatesSettings = roadPlatesSettings,
                    airPlatesSettings = airPlatesSettings,
                    waterPlatesSettings = waterPlatesSettings,
                    railPlatesSettings = railPlatesSettings,
                    m_aircraftLkp = GetComponentLookup<Aircraft>(),
                    m_watercraftLkp = GetComponentLookup<Watercraft>(),
                    m_trainLkp = GetComponentLookup<Train>(),
                    m_adrVehicleDataLkp = GetComponentLookup<ADRVehicleData>(),
                    m_adrVehiclePlateDataLkp = GetComponentLookup<ADRVehiclePlateDataDirty>(),
                    m_controllerLkp = GetComponentLookup<Controller>(),
                    m_layoutElementLkp = GetBufferLookup<LayoutElement>(),
                };
                Dependency = job.ScheduleParallel(m_unregisteredVehiclesQuery, Dependency);
                Dependency.GetAwaiter().OnCompleted(() =>
                {
                    currentSerialNumberVehicles += (uint)counter.Count;
                    counter.Dispose();
                });
                roadPlatesSettings.Dispose(Dependency);
                airPlatesSettings.Dispose(Dependency);
                waterPlatesSettings.Dispose(Dependency);
                railPlatesSettings.Dispose(Dependency);
            }
            if (!m_dirtyVehiclesPlateQuery.IsEmptyIgnoreFilter)
            {
                var roadPlatesSettings = roadVehiclesPlatesSettings.ForBurstJob;
                var airPlatesSettings = airVehiclesPlatesSettings.ForBurstJob;
                var waterPlatesSettings = waterVehiclesPlatesSettings.ForBurstJob;
                var railPlatesSettings = railVehiclesPlatesSettings.ForBurstJob;
                var job = new ADRUpdateVehiclesPlates
                {
                    m_cmdBuffer = m_Barrier.CreateCommandBuffer().AsParallelWriter(),
                    m_entityHdl = GetEntityTypeHandle(),
                    roadPlatesSettings = roadPlatesSettings,
                    airPlatesSettings = airPlatesSettings,
                    waterPlatesSettings = waterPlatesSettings,
                    railPlatesSettings = railPlatesSettings,
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

                roadPlatesSettings.Dispose(Dependency);
                airPlatesSettings.Dispose(Dependency);
                waterPlatesSettings.Dispose(Dependency);
                railPlatesSettings.Dispose(Dependency);
            }

#if DEBUG
            if (!m_unregisteredVehicleSpawnerQuery.IsEmptyIgnoreFilter)
            {
                var counter = new NativeCounter(Unity.Collections.Allocator.Temp);
                var job = new ADRRegisterVehicleSources
                {
                    m_cmdBuffer = m_Barrier.CreateCommandBuffer().AsParallelWriter(),
                    m_entityHdl = GetEntityTypeHandle(),
                    m_serialNumberCounter = counter.ToConcurrent(),
                    refSerialNumber = currentSerialNumberVehicleSources,
                    m_policeStation = GetComponentLookup<PoliceStation>(),
                    m_hospital = GetComponentLookup<Hospital>(),
                    m_deathcareFacility = GetComponentLookup<DeathcareFacility>(),
                    m_fireStation = GetComponentLookup<FireStation>(),
                    m_garbageFacility = GetComponentLookup<GarbageFacility>(),
                    m_transportDepot = GetComponentLookup<TransportDepot>(),
                    m_cargoTransportStation = GetComponentLookup<CargoTransportStation>(),
                    m_maintenanceDepot = GetComponentLookup<MaintenanceDepot>(),
                    m_postFacility = GetComponentLookup<PostFacility>(),
                    m_transportCompany = GetComponentLookup<TransportCompany>(),
                    m_industrialCompany = GetComponentLookup<IndustrialCompany>(),
                    m_commercialCompany = GetComponentLookup<CommercialCompany>(),
                };
                Dependency = job.ScheduleParallel(m_unregisteredVehicleSpawnerQuery, Dependency);
                Dependency.GetAwaiter().OnCompleted(() =>
                {
                    currentSerialNumberVehicleSources += (uint)counter.Count;
                    counter.Dispose();
                });
            }
#endif
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
            roadVehiclesPlatesSettings = new();
            waterVehiclesPlatesSettings = new();
            airVehiclesPlatesSettings = new();
            railVehiclesPlatesSettings = new();

            reader.Read(roadVehiclesPlatesSettings);
            reader.Read(waterVehiclesPlatesSettings);
            reader.Read(airVehiclesPlatesSettings);
            reader.Read(railVehiclesPlatesSettings);
            reader.Read(out currentSerialNumberVehicleSources);
            reader.Read(out currentSerialNumberVehicles);

        }

        void IBelzontSerializableSingleton<AdrVehicleSystem>.Serialize<TWriter>(TWriter writer)
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(roadVehiclesPlatesSettings);
            writer.Write(waterVehiclesPlatesSettings);
            writer.Write(airVehiclesPlatesSettings);
            writer.Write(railVehiclesPlatesSettings);
            writer.Write(currentSerialNumberVehicleSources);
            writer.Write(currentSerialNumberVehicles);
        }

        JobHandle IJobSerializable.SetDefaults(Context context)
        {
            currentSerialNumberVehicles = 0;
            currentSerialNumberVehicleSources = 0;

            roadVehiclesPlatesSettings = null;
            airVehiclesPlatesSettings = null;
            waterVehiclesPlatesSettings = null;
            railVehiclesPlatesSettings = null;
            return default;
        }

        #endregion
    }
}

