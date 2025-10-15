using Belzont.Interfaces;
using Belzont.Utils;
using Colossal;
using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using Game.Companies;
using Game.SceneFlow;
using Game.Simulation;
using Game.Tools;
using Game.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using static BelzontAdr.ADRVehicleBuildingOrigin;

namespace BelzontAdr
{

    public partial class AdrVehicleSystem : GameSystemBase, IBelzontBindable, IDefaultSerializable
    {
        private const uint CURRENT_VERSION = 2;

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
        private EntityQuery m_vehicleToUpdateConvoyId;
        private EntityQuery m_buildingWithVehicleToUpdateConvoyId;
        private EntityQuery m_buildingOwnSerialDirty;
        private ulong currentSerialNumberVehicles;
        private Dictionary<VehicleSourceKind, ushort> currentSerialNumberVehicleSources;

        private VehiclePlateSettings roadVehiclesPlatesSettings = new();
        private VehiclePlateSettings railVehiclesPlatesSettings = new();
        private VehiclePlateSettings airVehiclesPlatesSettings = new();
        private VehiclePlateSettings waterVehiclesPlatesSettings = new();

        private VehicleSerialSettings busSerialSettings = new();
        private VehicleSerialSettings taxiSerialSettings = new();
        private VehicleSerialSettings policeSerialSettings = new();
        private VehicleSerialSettings firetruckSerialSettings = new();
        private VehicleSerialSettings ambulanceSerialSettings = new();
        private VehicleSerialSettings garbageSerialSettings = new();
        private VehicleSerialSettings postalSerialSettings = new();


        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 16;
        }

        internal static ushort GetNextSerial(VehicleSourceKind kind)
        {
            if (!Instance.currentSerialNumberVehicleSources.TryGetValue(kind, out var current))
            {
                Instance.currentSerialNumberVehicleSources[kind] = current = 0;
            }
            if (Mathf.Log10(Instance.currentSerialNumberVehicleSources[kind]) % 1 == 0)
            {
                Instance.MarkEntitiesSerialDirty(ComponentType.ReadOnly<Car>());
            }
            Instance.currentSerialNumberVehicleSources[kind]++;
            return current;
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
                MarkEntitiesSerialDirty(ComponentType.ReadOnly<Train>());
            }
        }
        public VehiclePlateSettings AirVehiclesPlatesSettings
        {
            get => airVehiclesPlatesSettings; set
            {
                airVehiclesPlatesSettings = value;

                MarkEntitiesPlateDirty(ComponentType.ReadOnly<Aircraft>());
                MarkEntitiesSerialDirty(ComponentType.ReadOnly<Aircraft>());
            }
        }
        public VehiclePlateSettings WaterVehiclesPlatesSettings
        {
            get => waterVehiclesPlatesSettings; set
            {
                waterVehiclesPlatesSettings = value;
                MarkEntitiesPlateDirty(ComponentType.ReadOnly<Watercraft>());
                MarkEntitiesSerialDirty(ComponentType.ReadOnly<Watercraft>());
            }
        }

        private void MarkEntitiesSerialDirty(ComponentType specificType)
        {
            EntityManager.AddComponent<ADRVehicleSerialDataDirty>(GetEntityQuery(new EntityQueryDesc[]
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

        public VehiclePlateSettings RoadVehiclesPlatesSettings
        {
            get => roadVehiclesPlatesSettings; set
            {
                roadVehiclesPlatesSettings = value;
                MarkEntitiesSerialDirty(ComponentType.ReadOnly<Car>());
                MarkEntitiesPlateDirty(ComponentType.ReadOnly<Car>());
            }
        }

        public VehicleSerialSettings BusSerialSettings
        {
            get => busSerialSettings; set
            {
                busSerialSettings = value;
                MarkEntitiesSerialDirty(ComponentType.ReadOnly<Car>());
            }
        }

        public VehicleSerialSettings TaxiSerialSettings
        {
            get => taxiSerialSettings; set
            {
                taxiSerialSettings = value;
                MarkEntitiesSerialDirty(ComponentType.ReadOnly<Car>());
            }
        }
        public VehicleSerialSettings PoliceSerialSettings
        {
            get => policeSerialSettings; set
            {
                policeSerialSettings = value;
                MarkEntitiesSerialDirty(ComponentType.ReadOnly<Car>());
            }
        }
        public VehicleSerialSettings FiretruckSerialSettings
        {
            get => firetruckSerialSettings; set
            {
                firetruckSerialSettings = value;
                MarkEntitiesSerialDirty(ComponentType.ReadOnly<Car>());
            }
        }
        public VehicleSerialSettings AmbulanceSerialSettings
        {
            get => ambulanceSerialSettings; set
            {
                ambulanceSerialSettings = value;
                MarkEntitiesSerialDirty(ComponentType.ReadOnly<Car>());
            }
        }
        public VehicleSerialSettings GarbageSerialSettings
        {
            get => garbageSerialSettings; set
            {
                garbageSerialSettings = value;
                MarkEntitiesSerialDirty(ComponentType.ReadOnly<Car>());
            }
        }
        public VehicleSerialSettings PostalSerialSettings
        {
            get => postalSerialSettings; set
            {
                postalSerialSettings = value;
                MarkEntitiesSerialDirty(ComponentType.ReadOnly<Car>());
            }
        }

        private static AdrVehicleSystem Instance;

        protected override void OnCreate()
        {
            Instance = this;
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
                            ComponentType.ReadOnly<Game.Buildings.PoliceStation>(),
                            ComponentType.ReadOnly<Game.Buildings.Hospital>(),
                            ComponentType.ReadOnly<Game.Buildings.DeathcareFacility>(),
                            ComponentType.ReadOnly<Game.Buildings.FireStation>(),
                            ComponentType.ReadOnly<Game.Buildings.GarbageFacility>(),
                            ComponentType.ReadOnly<Game.Buildings.TransportDepot>(),
                            ComponentType.ReadOnly<Game.Buildings.CargoTransportStation>(),
                            ComponentType.ReadOnly<Game.Buildings.MaintenanceDepot>(),
                            ComponentType.ReadOnly<Game.Buildings.PostFacility>(),
                            ComponentType.ReadOnly<TransportCompany>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<ADRVehicleBuildingOrigin>(),
                            ComponentType.ReadOnly<Owner>(),
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
            });
            m_vehicleToUpdateConvoyId = GetEntityQuery(new EntityQueryDesc[]
            {
                new ()
                    {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<ADRVehicleData>(),
                            ComponentType.ReadOnly<ADRVehicleSerialDataDirty>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
            });
            m_buildingWithVehicleToUpdateConvoyId = GetEntityQuery(new EntityQueryDesc[]
            {
                new ()
                    {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<ADRBuildingVehiclesSerialDirty>(),
                            ComponentType.ReadOnly<ADRVehicleBuildingOrigin>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<ADRBuildingOwnSerialUnset>(),
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
            });
            m_buildingOwnSerialDirty = GetEntityQuery(new EntityQueryDesc[]
            {
                new ()
                    {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<ADRBuildingOwnSerialUnset>(),
                            ComponentType.ReadOnly<ADRVehicleBuildingOrigin>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
            });

            GameManager.instance.RegisterUpdater(() =>
            {
                if (AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "BelzontWE") is Assembly weAssembly
                    && weAssembly.GetExportedTypes().FirstOrDefault(x => x.Name == "WEVehicleFn") is Type t)
                {
                    if (t.GetField("GetVehiclePlate_binding", RedirectorUtils.allFlags) is FieldInfo vehiclePlateField)
                    {
                        var originalValue = vehiclePlateField.GetValue(null) as Func<Entity, string>;
                        vehiclePlateField.SetValue(null, (Entity e) => EntityManager.TryGetComponent(e, out ADRVehicleData vehicleData) ? vehicleData.calculatedPlate.ToString() : originalValue(e));
                    }
                    if (t.GetField("GetSerialNumber_binding", RedirectorUtils.allFlags) is FieldInfo getSerialNumber)
                    {
                        var originalValue = getSerialNumber.GetValue(null) as Func<Entity, string>;
                        getSerialNumber.SetValue(null, (Entity e) => EntityManager.TryGetComponent(e, out ADRVehicleData vehicleData) ? vehicleData.serialNumber.ToString() : originalValue(e));
                    }
                    if (t.GetField("GetConvoyId_binding", RedirectorUtils.allFlags) is FieldInfo getConvoyId)
                    {
                        var originalValue = getConvoyId.GetValue(null) as Func<Entity, string>;
                        getConvoyId.SetValue(null, (Entity e) =>
                        {
                            if (EntityManager.TryGetComponent(e, out ADRVehicleData vehicleData))
                            {
                                if (vehicleData.plateCategory == ADRVehicleData.VehiclePlateCategory.Rail || vehicleData.plateCategory == ADRVehicleData.VehiclePlateCategory.Road) return vehicleData.calculatedConvoyPrefix.ToString();
                            }
                            return originalValue(e);

                        });
                    }
                }
            });

        }


        protected override void OnStartRunning()
        {
            base.OnStartRunning();
        }

        private readonly Queue<Action> actionsToRunOnMain = new();

        protected unsafe override void OnUpdate()
        {
            if (GameManager.instance.isGameLoading)
            {
                return;
            }
            while (actionsToRunOnMain.TryDequeue(out Action action))
            {
                action.Invoke();
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
                    m_layoutElementLkp = GetBufferLookup<LayoutElement>(),
                    m_spawnerDataLkp = GetComponentLookup<ADRVehicleBuildingOrigin>(),
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
                var job = new ADRRegisterVehicleSources
                {
                    m_cmdBuffer = m_Barrier.CreateCommandBuffer().AsParallelWriter(),
                    m_entityHdl = GetEntityTypeHandle(),
                    m_policeStation = GetComponentLookup<Game.Buildings.PoliceStation>(),
                    m_hospital = GetComponentLookup<Game.Buildings.Hospital>(),
                    m_deathcareFacility = GetComponentLookup<Game.Buildings.DeathcareFacility>(),
                    m_fireStation = GetComponentLookup<Game.Buildings.FireStation>(),
                    m_garbageFacility = GetComponentLookup<Game.Buildings.GarbageFacility>(),
                    m_transportDepot = GetComponentLookup<Game.Buildings.TransportDepot>(),
                    m_cargoTransportStation = GetComponentLookup<Game.Buildings.CargoTransportStation>(),
                    m_maintenanceDepot = GetComponentLookup<Game.Buildings.MaintenanceDepot>(),
                    m_postFacility = GetComponentLookup<Game.Buildings.PostFacility>(),
                    m_transportCompany = GetComponentLookup<TransportCompany>(),
                    m_industrialCompany = GetComponentLookup<IndustrialCompany>(),
                    m_commercialCompany = GetComponentLookup<CommercialCompany>(),
                    m_prefabRef = GetComponentLookup<Game.Prefabs.PrefabRef>(),
                    m_depotData = GetComponentLookup<Game.Prefabs.TransportDepotData>(),
                };
                job.ScheduleParallel(m_unregisteredVehicleSpawnerQuery, Dependency);
            }

            if (!m_buildingOwnSerialDirty.IsEmptyIgnoreFilter)
            {
                var entities = m_buildingOwnSerialDirty.ToEntityArray(Allocator.Temp);
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    if (EntityManager.TryGetComponent<ADRVehicleBuildingOrigin>(entity, out var sourceData))
                    {
                        sourceData.DoRegisterCategorySerialNumber();
                        EntityManager.SetComponentData(entity, sourceData);
                    }
                    EntityManager.RemoveComponent<ADRBuildingOwnSerialUnset>(entity);
                }
                entities.Dispose();
            }

            if (!m_vehicleToUpdateConvoyId.IsEmptyIgnoreFilter)
            {

                var job = new ADRUpdateVehiclesConvoyId
                {
                    m_cmdBuffer = m_Barrier.CreateCommandBuffer().AsParallelWriter(),
                    m_entityHdl = GetEntityTypeHandle(),
                    m_ownerLookup = GetComponentLookup<Owner>(),

                };
                job.ScheduleParallel(m_vehicleToUpdateConvoyId, Dependency);

            }

            if (!m_buildingWithVehicleToUpdateConvoyId.IsEmpty)
            {
                var job = new ADRUpdateBuildingVehiclesConvoyId
                {
                    ambulanceSerialSettings = ambulanceSerialSettings.GetForBurstJob(currentSerialNumberVehicleSources.GetValueOrDefault(VehicleSourceKind.Hospital)),
                    busSerialSettings = busSerialSettings.GetForBurstJob(currentSerialNumberVehicleSources.GetValueOrDefault(VehicleSourceKind.PublicTransport_Bus)),
                    firetruckSerialSettings = firetruckSerialSettings.GetForBurstJob(currentSerialNumberVehicleSources.GetValueOrDefault(VehicleSourceKind.FireResponse)),
                    garbageSerialSettings = garbageSerialSettings.GetForBurstJob(currentSerialNumberVehicleSources.GetValueOrDefault(VehicleSourceKind.Garbage)),
                    policeSerialSettings = policeSerialSettings.GetForBurstJob(currentSerialNumberVehicleSources.GetValueOrDefault(VehicleSourceKind.Police)),
                    postalSerialSettings = postalSerialSettings.GetForBurstJob(currentSerialNumberVehicleSources.GetValueOrDefault(VehicleSourceKind.Post)),
                    taxiSerialSettings = taxiSerialSettings.GetForBurstJob(currentSerialNumberVehicleSources.GetValueOrDefault(VehicleSourceKind.PublicTransport_Taxi)),
                    m_cmdBuffer = m_Barrier.CreateCommandBuffer().AsParallelWriter(),
                    entityTypeHandle = GetEntityTypeHandle(),
                    m_ownedVehiclesLkp = GetBufferLookup<OwnedVehicle>(),
                    m_sourceDataLkp = GetComponentLookup<ADRVehicleBuildingOrigin>(),
                    m_vehicleDataLkp = GetComponentLookup<ADRVehicleData>(),
                    m_layoutElementLkp = GetBufferLookup<LayoutElement>(),
                    m_controllerLkp = GetComponentLookup<Controller>(),

                };
                var deps = job.ScheduleParallel(m_buildingWithVehicleToUpdateConvoyId, Dependency);
                job.Dispose(deps);
            }

#endif
            Dependency.Complete();
        }

        #region Serialization


        public void Deserialize<R>(R reader) where R : IReader
        {

            var version = reader.CheckVersionK45(CURRENT_VERSION, GetType());
            roadVehiclesPlatesSettings = new();
            waterVehiclesPlatesSettings = new();
            airVehiclesPlatesSettings = new();
            railVehiclesPlatesSettings = new();

            reader.Read(roadVehiclesPlatesSettings);
            reader.Read(waterVehiclesPlatesSettings);
            reader.Read(airVehiclesPlatesSettings);
            reader.Read(railVehiclesPlatesSettings);
            if (version <= 1)
            {
                reader.Read(out uint _);
            }
            reader.Read(out currentSerialNumberVehicles);

            if (version == 0)
            {
                railVehiclesPlatesSettings = VehiclePlateSettings.CreateRailVehicleDefault(m_timeSystem);
                actionsToRunOnMain.Enqueue(() => m_Barrier.CreateCommandBuffer().RemoveComponent<ADRVehicleData>(GetEntityQuery(new[]{
                    new EntityQueryDesc
                    {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<ADRVehicleData>(),
                        }
                    }
                }), EntityQueryCaptureMode.AtPlayback));
            }
            if (version >= 2)
            {
                reader.Read(out int count);
                currentSerialNumberVehicleSources = new Dictionary<VehicleSourceKind, ushort>(count);
                for (int i = 0; i < count; i++)
                {
                    reader.Read(out VehicleSourceKind key);
                    reader.Read(out ushort value);
                    currentSerialNumberVehicleSources[key] = value;
                }
            }
            else
            {
                currentSerialNumberVehicleSources = new Dictionary<VehicleSourceKind, ushort>();

                busSerialSettings = VehicleSerialSettings.CreateBusSerialSettings();
                taxiSerialSettings = VehicleSerialSettings.CreateTaxiSerialSettings();
                policeSerialSettings = VehicleSerialSettings.CreateCityServicesSerialSettings();
                firetruckSerialSettings = VehicleSerialSettings.CreateCityServicesSerialSettings();
                ambulanceSerialSettings = VehicleSerialSettings.CreateCityServicesSerialSettings();
                garbageSerialSettings = VehicleSerialSettings.CreateCityServicesSerialSettings();
                postalSerialSettings = VehicleSerialSettings.CreateCityServicesSerialSettings();
                actionsToRunOnMain.Enqueue(() => EntityManager.AddComponent<ADRBuildingVehiclesSerialDirty>(GetEntityQuery(new EntityQueryDesc[]
                 {
                    new ()
                    {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<ADRVehicleBuildingOrigin>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
             })));
            }
        }
        public void Serialize<W>(W writer) where W : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(roadVehiclesPlatesSettings);
            writer.Write(waterVehiclesPlatesSettings);
            writer.Write(airVehiclesPlatesSettings);
            writer.Write(railVehiclesPlatesSettings);
            writer.Write(currentSerialNumberVehicles);
            writer.Write(currentSerialNumberVehicleSources.Count);
            foreach (var kvp in currentSerialNumberVehicleSources)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
        }

        public void SetDefaults(Context context)
        {
            currentSerialNumberVehicles = 0;
            currentSerialNumberVehicleSources = new();
            roadVehiclesPlatesSettings = VehiclePlateSettings.CreateRoadVehicleDefault(m_timeSystem);
            airVehiclesPlatesSettings = VehiclePlateSettings.CreateAirVehicleDefault(m_timeSystem);
            waterVehiclesPlatesSettings = VehiclePlateSettings.CreateWaterVehicleDefault(m_timeSystem);
            railVehiclesPlatesSettings = VehiclePlateSettings.CreateRailVehicleDefault(m_timeSystem);
            busSerialSettings = VehicleSerialSettings.CreateBusSerialSettings();
            taxiSerialSettings = VehicleSerialSettings.CreateTaxiSerialSettings();
            policeSerialSettings = VehicleSerialSettings.CreateCityServicesSerialSettings();
            firetruckSerialSettings = VehicleSerialSettings.CreateCityServicesSerialSettings();
            ambulanceSerialSettings = VehicleSerialSettings.CreateCityServicesSerialSettings();
            garbageSerialSettings = VehicleSerialSettings.CreateCityServicesSerialSettings();
            postalSerialSettings = VehicleSerialSettings.CreateCityServicesSerialSettings();
        }

        #endregion
    }
}

