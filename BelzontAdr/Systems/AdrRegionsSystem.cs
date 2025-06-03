using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game;
using Game.Buildings;
using Game.Common;
using Game.Net;
using Game.Objects;
using Game.Routes;
using Game.Simulation;
using Game.Tools;
using Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;

#if BURST
using Unity.Burst;
using Unity.Burst.Intrinsics;
#endif
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static BelzontAdr.ADRMapUtils;
using Color = UnityEngine.Color;
using ColorExtensions = Belzont.Utils.ColorExtensions;
using Transform = Game.Objects.Transform;

namespace BelzontAdr
{
    public partial class AdrRegionsSystem : GameSystemBase, IBelzontBindable, IDefaultSerializable
    {
        public const float USHORT_ANGLE_TO_DEGREES = 360f / 0x1_0000;



        private NameSystem m_nameSystem;
        private TerrainSystem m_terrainSystem;
        private WaterSystem m_waterSystem;
        private ModificationEndBarrier m_modificationEndBarrier;
        private EntityQuery m_outsideConnectionsObject;
        private EntityQuery m_highwaysQuery;
        private EntityQuery m_urbanRoadsQuery;
        private EntityQuery m_railroadsQuery;
        private EntityQuery m_regionByLandQuery;
        private EntityQuery m_regionByWaterQuery;
        private EntityQuery m_regionByAirQuery;
        private EntityQuery m_regionByAnyQuery;
        private EntityQuery m_outsideConnectionsUnmapped;
        private EntityQuery m_outsideConnectionsMapped;

        #region bindings
        private const string SYSTEM_PREFIX = "regions.";
        private Action<string, object[]> sendEventToUI;

        public void SetupCallBinder(Action<string, Delegate> eventCaller)
        {
            eventCaller($"{SYSTEM_PREFIX}listOutsideConnections", ListOutsideConnections);
            eventCaller($"{SYSTEM_PREFIX}getCityBounds", GetCityBounds);
            eventCaller($"{SYSTEM_PREFIX}listHighways", ListHighways);
            eventCaller($"{SYSTEM_PREFIX}listTrainTracks", ListTrainTracks);
            eventCaller($"{SYSTEM_PREFIX}listUrbanRoads", ListUrbanRoads);
            eventCaller($"{SYSTEM_PREFIX}getCityTerrain", () => GetCityMapBase64(m_terrainSystem.heightmap as RenderTexture, x => x.r, MapType.Topographic));
            eventCaller($"{SYSTEM_PREFIX}getCityWater", () => GetCityMapBase64(m_waterSystem.WaterTexture, x => x.r, MapType.Transparency));
            eventCaller($"{SYSTEM_PREFIX}getCityWaterPollution", () => GetCityMapBase64(m_waterSystem.WaterTexture, x => x.a, MapType.Transparency));
            eventCaller($"{SYSTEM_PREFIX}getLandRegionNeighborhood", GetLandRegionNeighborhood);
            eventCaller($"{SYSTEM_PREFIX}getWaterRegionNeighborhood", GetWaterRegionNeighborhood);
            eventCaller($"{SYSTEM_PREFIX}getAirRegionNeighborhood", GetAirRegionNeighborhood);
            eventCaller($"{SYSTEM_PREFIX}listAllRegionCities", ListAllRegionCities);
            eventCaller($"{SYSTEM_PREFIX}saveRegionCity", SaveRegionCity);
            eventCaller($"{SYSTEM_PREFIX}removeRegionCity", RemoveRegionCity);
        }

        private string GetCityMapBase64(RenderTexture texture, Func<Color, float> channelSelector, MapType mapType)
        {
            Texture2D t2d = null;
            try
            {
                t2d = RenderTextureTo2D(texture, channelSelector, mapType);
                return EncodeToBase64(t2d);
            }
            finally
            {
                GameObject.Destroy(t2d);
            }
        }
        private string EncodeToBase64(Texture2D texture2D) => $"data:image/png;base64,{Convert.ToBase64String(texture2D.EncodeToPNG())}";

        public void SetupCaller(Action<string, object[]> eventCaller)
        {
            sendEventToUI = eventCaller;
        }

        public void SetupEventBinder(Action<string, Delegate> eventCaller)
        {

        }

        private struct ObjectOutsideConnectionResponse
        {
            public Entity entity;
            public string name;
            public float[] position;
            public float azimuthDirection;
            public Entity netEntity;
            public string netName;
            public OutsideConnectionType outsideConnectionType;

        }

        public enum OutsideConnectionType
        {
            Road,
            Rail,
            Pipe,
            Electricity,
            Waterway,
            Airway
        }

        private float[] GetCityBounds()
        {
            var bounds = m_terrainSystem.GetTerrainBounds();
            return new[] { bounds.min.x, bounds.min.y, bounds.min.z, bounds.max.x, bounds.max.y, bounds.max.z };
        }

        private ObjectOutsideConnectionResponse[] ListOutsideConnections()
        {
            using var list = m_outsideConnectionsObject.ToEntityArray(Allocator.Temp);
            return list.ToArray().Select(x =>
            {
                var transform = EntityManager.GetComponentData<Transform>(x);
                var hasOwner = EntityManager.TryGetComponent<Owner>(x, out var owner);
                return new ObjectOutsideConnectionResponse
                {
                    azimuthDirection = transform.m_Position.GetAngleXZ(),
                    entity = x,
                    name = m_nameSystem.GetName(x).Translate(),
                    netEntity = hasOwner ? owner.m_Owner : Entity.Null,
                    netName = hasOwner ? m_nameSystem.GetName(owner.m_Owner).Translate() : default,
                    position = new[] { transform.m_Position.x, transform.m_Position.y, transform.m_Position.z },
                    outsideConnectionType = EntityManager.HasComponent<ShipStop>(x) ? OutsideConnectionType.Waterway
                    : EntityManager.HasComponent<AirplaneStop>(x) ? OutsideConnectionType.Airway
                    : EntityManager.HasComponent<TrainStop>(x) ? OutsideConnectionType.Rail
                    : EntityManager.HasComponent<ElectricityOutsideConnection>(x) ? OutsideConnectionType.Electricity
                    : EntityManager.HasComponent<WaterPipeOutsideConnection>(x) ? OutsideConnectionType.Pipe
                    : OutsideConnectionType.Road
                };

            }).ToArray();
        }



        private struct AggregationData
        {
            public Entity entity;
            public string name;
            public float[][][] curves;
            public Entity[] nodes;
            public string highwayId;
        }

        private AggregationData[] ListHighways() => ListFromQuery(m_highwaysQuery);
        private AggregationData[] ListTrainTracks() => ListFromQuery(m_railroadsQuery);
        private AggregationData[] ListUrbanRoads() => ListFromQuery(m_urbanRoadsQuery);
        private AggregationData[] ListFromQuery(EntityQuery query)
        {
            using var list = query.ToEntityArray(Allocator.Temp);
            using var listEdge = query.ToComponentDataArray<Edge>(Allocator.Temp);
            using var listCurve = query.ToComponentDataArray<Curve>(Allocator.Temp);
            using var listAggregated = query.ToComponentDataArray<Aggregated>(Allocator.Temp);
            return list.ToArray().Select((x, i) => (x, i)).GroupBy(x => listAggregated[x.i].m_Aggregate)
                .Select(x => new AggregationData
                {
                    entity = x.Key,
                    name = m_nameSystem.GetName(x.Key).Translate(),
                    curves = x.Select(x =>
                    {
                        var bez = listCurve[x.i].m_Bezier;
                        return new[] { bez.a.ToArray(), bez.b.ToArray(), bez.c.ToArray(), bez.d.ToArray() };
                    }).ToArray(),
                    nodes = x.SelectMany(x => new[] { listEdge[x.i].m_Start, listEdge[x.i].m_End }).ToHashSet().ToArray(),
                    highwayId = (EntityManager.TryGetComponent<ADRHighwayAggregationData>(x.Key, out var data) ? data.highwayDataId : default).ToString()
                }).ToArray();
        }
        #endregion

        #region System

        private readonly Queue<Action> runOnUpdate = new Queue<Action>();

        protected override void OnCreate()
        {
            base.OnCreate();
            m_nameSystem = World.GetOrCreateSystemManaged<NameSystem>();
            m_terrainSystem = World.GetExistingSystemManaged<TerrainSystem>();
            m_waterSystem = World.GetExistingSystemManaged<WaterSystem>();
            m_modificationEndBarrier = World.GetOrCreateSystemManaged<ModificationEndBarrier>();
            m_outsideConnectionsObject = GetEntityQuery(new EntityQueryDesc[]
              {
                    new() {
                        Any = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Game.Objects.OutsideConnection>(),
                            ComponentType.ReadOnly<Game.Objects.WaterPipeOutsideConnection>(),
                            ComponentType.ReadOnly<Game.Objects.ElectricityOutsideConnection>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
              });
            m_highwaysQuery = GetEntityQuery(new EntityQueryDesc[]
              {
                    new() {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Edge>(),
                            ComponentType.ReadOnly<Curve>(),
                            ComponentType.ReadOnly<Road>(),
                            ComponentType.ReadOnly<Aggregated>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<ConnectedBuilding>(),
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
              });
            m_urbanRoadsQuery = GetEntityQuery(new EntityQueryDesc[]
              {
                    new() {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Edge>(),
                            ComponentType.ReadOnly<Curve>(),
                            ComponentType.ReadOnly<Road>(),
                            ComponentType.ReadOnly<Aggregated>(),
                            ComponentType.ReadOnly<ConnectedBuilding>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
              });
            m_railroadsQuery = GetEntityQuery(new EntityQueryDesc[]
              {
                    new() {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Edge>(),
                            ComponentType.ReadOnly<Curve>(),
                            ComponentType.ReadOnly<TrainTrack>(),
                            ComponentType.ReadOnly<Aggregated>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<ConnectedBuilding>(),
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
              });

            m_regionByLandQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<ADRRegionCity>(),
                        ComponentType.ReadOnly<ADRRegionLandCity>(),
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Deleted>(),
                    }
                }
            });

            m_regionByWaterQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<ADRRegionCity>(),
                        ComponentType.ReadOnly<ADRRegionWaterCity>(),
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Deleted>(),
                    }
                }
            });

            m_regionByAirQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<ADRRegionCity>(),
                        ComponentType.ReadOnly<ADRRegionAirCity>(),
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Deleted>(),
                    }
                }
            });

            m_regionByAnyQuery = GetEntityQuery(new EntityQueryDesc[]
            {
                new()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<ADRRegionCity>(),
                    },
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Temp>(),
                        ComponentType.ReadOnly<Deleted>(),
                    }
                }
            });
            m_outsideConnectionsUnmapped = GetEntityQuery(new EntityQueryDesc[]
              {
                    new() {
                        All = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Transform>(),
                        },
                        Any = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Game.Objects.OutsideConnection>(),
                            ComponentType.ReadOnly<Game.Objects.WaterPipeOutsideConnection>(),
                            ComponentType.ReadOnly<Game.Objects.ElectricityOutsideConnection>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<ADRRegionCityReference>(),
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
              });
            m_outsideConnectionsMapped = GetEntityQuery(new EntityQueryDesc[]
              {
                    new() {
                        All =new ComponentType[]
                        {
                            ComponentType.ReadOnly<Transform>(),
                            ComponentType.ReadOnly<ADRRegionCityReference>(),
                        },
                        Any = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Game.Objects.OutsideConnection>(),
                            ComponentType.ReadOnly<Game.Objects.WaterPipeOutsideConnection>(),
                            ComponentType.ReadOnly<Game.Objects.ElectricityOutsideConnection>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
              });
        }

        protected override void OnUpdate()
        {
            while (runOnUpdate.Count > 0)
            {
                runOnUpdate.Dequeue().Invoke();
            }
            if (!m_outsideConnectionsUnmapped.IsEmpty)
            {
                var landArray = new NativeArray<CityJobData>(GetLandRegionNeighborhood().Select(x => x.ToCityJobData()).ToArray(), Allocator.TempJob);
                var waterArray = new NativeArray<CityJobData>(GetWaterRegionNeighborhood().Select(x => x.ToCityJobData()).ToArray(), Allocator.TempJob);
                var airArray = new NativeArray<CityJobData>(GetAirRegionNeighborhood().Select(x => x.ToCityJobData()).ToArray(), Allocator.TempJob);

                if (BasicIMod.DebugMode) LogUtils.DoLog($"land = {landArray.Length} | water = {waterArray.Length} | air = {airArray.Length}");
                if (BasicIMod.TraceMode)
                {
                    LogUtils.DoLog($"land = \n\t{string.Join("\n\t", landArray.Select(x => $"- E: {x.entity} | {x.azimuthAngleStart} => {x.azimuthAngleCenter} => {x.azimuthAngleEnd}"))}");
                    LogUtils.DoLog($"water = \n\t{string.Join("\n\t", waterArray.Select(x => $"- E: {x.entity} | {x.azimuthAngleStart} => {x.azimuthAngleCenter} => {x.azimuthAngleEnd}"))}");
                    LogUtils.DoLog($"air = \n\t{string.Join("\n\t", airArray.Select(x => $"- E: {x.entity} | {x.azimuthAngleStart} => {x.azimuthAngleCenter} => {x.azimuthAngleEnd}"))}");
                }

                var job = new MapOutsideConnectionsJob
                {
                    m_airplaneStopLookup = GetComponentLookup<AirplaneStop>(true),
                    m_shipStopLookup = GetComponentLookup<ShipStop>(true),
                    m_cmdBuffer = m_modificationEndBarrier.CreateCommandBuffer().AsParallelWriter(),
                    m_entityHandle = GetEntityTypeHandle(),
                    m_transformHandle = GetComponentTypeHandle<Transform>(true),
                    m_regionCitiesLand = landArray,
                    m_regionCitiesWater = waterArray,
                    m_regionCitiesAir = airArray,
                    doLog = BasicIMod.TraceMode
                }.ScheduleParallel(m_outsideConnectionsUnmapped, Dependency);
                landArray.Dispose(job);
                waterArray.Dispose(job);
                airArray.Dispose(job);
                job.Complete();
                sendEventToUI($"{SYSTEM_PREFIX}outsideConnectionsChanged", new object[0]);
            }
        }
        #endregion
        #region Jobs
        public struct CityJobData
        {
            public ushort azimuthAngleStart;
            public ushort azimuthAngleCenter;
            public ushort azimuthAngleEnd;
            public Entity entity;
        }
#if BURST
        [BurstCompile]
#endif
        private struct MapOutsideConnectionsJob : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter m_cmdBuffer;
            public EntityTypeHandle m_entityHandle;
            public ComponentTypeHandle<Transform> m_transformHandle;
            public NativeArray<CityJobData> m_regionCitiesLand;
            public NativeArray<CityJobData> m_regionCitiesWater;
            public NativeArray<CityJobData> m_regionCitiesAir;

            public ComponentLookup<ShipStop> m_shipStopLookup;
            public ComponentLookup<AirplaneStop> m_airplaneStopLookup;
            public bool doLog;


            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(m_entityHandle);
                var transforms = chunk.GetNativeArray(ref m_transformHandle);
                for (int i = 0; i < chunk.Count; i++)
                {
                    var entity = entities[i];
                    var transform = transforms[i];
                    var azimuthDirection = (ushort)((450 - transform.m_Position.GetAngleXZ()) % 360 / USHORT_ANGLE_TO_DEGREES);
                    var mapToCheck = m_shipStopLookup.HasComponent(entity) ? m_regionCitiesWater
                        : m_airplaneStopLookup.HasComponent(entity) ? m_regionCitiesAir
                        : m_regionCitiesLand;
                    if (doLog) UnityEngine.Debug.Log($"${entity.Index}:${entity.Version}|${azimuthDirection}");
                    for (int j = 0; j < mapToCheck.Length; j++)
                    {
                        if (mapToCheck[j].azimuthAngleEnd < mapToCheck[j].azimuthAngleStart)
                        {
                            if (mapToCheck[j].azimuthAngleStart <= azimuthDirection || azimuthDirection < mapToCheck[j].azimuthAngleEnd)
                            {
                                m_cmdBuffer.AddComponent(unfilteredChunkIndex, entity, new ADRRegionCityReference
                                {
                                    cityEntity = mapToCheck[j].entity
                                });
                                goto continueOuterLoop;
                            }
                        }
                        else
                        {
                            if (mapToCheck[j].azimuthAngleStart <= azimuthDirection && azimuthDirection < mapToCheck[j].azimuthAngleEnd)
                            {
                                m_cmdBuffer.AddComponent(unfilteredChunkIndex, entity, new ADRRegionCityReference
                                {
                                    cityEntity = mapToCheck[j].entity
                                });
                                goto continueOuterLoop;
                            }
                        }
                    }
                    m_cmdBuffer.AddComponent(unfilteredChunkIndex, entity, new ADRRegionCityReference
                    {
                        cityEntity = Entity.Null
                    });
                continueOuterLoop:
                    continue;
                }
            }
        }

        #endregion

        #region Region data

        public struct CityResponseData
        {
            public string name;
            public float azimuthAngleStart;
            public float azimuthAngleCenter;
            public float azimuthAngleEnd;
            public bool reachableByLand;
            public bool reachableByWater;
            public bool reachableByAir;
            public string mapColor;
            public Entity entity;

            internal CityJobData ToCityJobData()
            {
                return new CityJobData
                {
                    azimuthAngleStart = (ushort)(azimuthAngleStart / USHORT_ANGLE_TO_DEGREES),
                    azimuthAngleCenter = (ushort)(azimuthAngleCenter / USHORT_ANGLE_TO_DEGREES),
                    azimuthAngleEnd = (ushort)(azimuthAngleEnd / USHORT_ANGLE_TO_DEGREES),
                    entity = entity
                };
            }
        }

        private List<CityResponseData> GetRegionNeighborhood(EntityQuery query)
        {
            var cities = new List<CityResponseData>();
            using var list = query.ToEntityArray(Allocator.Temp);
            var tempList = new List<(ADRRegionCity data, Entity entity)>();
            foreach (var city in list)
            {
                tempList.Add((EntityManager.GetComponentData<ADRRegionCity>(city), city));
            }
            tempList.Sort((a, b) => a.data.azimuthAngle.CompareTo(b.data.azimuthAngle));
            if (tempList.Count == 1)
            {
                var city = tempList[0].data;
                cities.Add(new CityResponseData
                {
                    name = city.name.ToString(),
                    azimuthAngleStart = (ushort)(city.azimuthAngle - city.azimuthWidthLeft) * USHORT_ANGLE_TO_DEGREES,
                    azimuthAngleCenter = city.azimuthAngle * USHORT_ANGLE_TO_DEGREES,
                    azimuthAngleEnd = (ushort)(city.azimuthAngle + city.azimuthWidthRight) * USHORT_ANGLE_TO_DEGREES,
                    reachableByLand = EntityManager.HasComponent<ADRRegionLandCity>(tempList[0].entity),
                    reachableByWater = EntityManager.HasComponent<ADRRegionWaterCity>(tempList[0].entity),
                    reachableByAir = EntityManager.HasComponent<ADRRegionAirCity>(tempList[0].entity),
                    mapColor = city.mapColor.ToRGB(true),
                    entity = tempList[0].entity
                });
            }
            for (int i = 0; i < tempList.Count; i++)
            {
                var prevCity = tempList[(i - 1 + tempList.Count) % tempList.Count].data;
                var city = tempList[i].data;
                var nextCity = tempList[(i + 1) % tempList.Count].data;
                var startAngle = (ushort)(city.azimuthAngle - Math.Min(city.azimuthWidthLeft, (ushort)Mathf.RoundToInt(Math.Abs(city.azimuthAngle - prevCity.azimuthAngle) * -(1 + (city.azimuthWidthLeft / (float)(city.azimuthWidthLeft + prevCity.azimuthWidthRight))))));
                var endAngle = (ushort)(city.azimuthAngle + Math.Min(city.azimuthWidthRight, (ushort)Mathf.RoundToInt(Math.Abs(nextCity.azimuthAngle - city.azimuthAngle) * -(1 + (city.azimuthWidthRight / (float)(nextCity.azimuthWidthLeft + city.azimuthWidthRight))))));
                cities.Add(new CityResponseData
                {
                    name = city.name.ToString(),
                    azimuthAngleStart = startAngle * USHORT_ANGLE_TO_DEGREES,
                    azimuthAngleCenter = (ushort)(startAngle + ((ushort)(endAngle - startAngle + 0x1_0000) / 2)) * USHORT_ANGLE_TO_DEGREES,
                    azimuthAngleEnd = endAngle * USHORT_ANGLE_TO_DEGREES,
                    reachableByLand = EntityManager.HasComponent<ADRRegionLandCity>(tempList[i].entity),
                    reachableByWater = EntityManager.HasComponent<ADRRegionWaterCity>(tempList[i].entity),
                    reachableByAir = EntityManager.HasComponent<ADRRegionAirCity>(tempList[i].entity),
                    mapColor = city.mapColor.ToRGB(true),
                    entity = tempList[i].entity
                });
            }
            return cities;
        }

        private List<CityResponseData> m_cachedRegionLandNeighborhood = null;
        private List<CityResponseData> m_cachedRegionWaterNeighborhood = null;
        private List<CityResponseData> m_cachedRegionAirNeighborhood = null;
        private List<CityResponseData> GetLandRegionNeighborhood() => m_cachedRegionLandNeighborhood ??= GetRegionNeighborhood(m_regionByLandQuery);
        private List<CityResponseData> GetWaterRegionNeighborhood() => m_cachedRegionWaterNeighborhood ??= GetRegionNeighborhood(m_regionByWaterQuery);
        private List<CityResponseData> GetAirRegionNeighborhood() => m_cachedRegionAirNeighborhood ??= GetRegionNeighborhood(m_regionByAirQuery);


        private struct RegionCityEditingDTO
        {
            public Entity entity;
            public string name;
            public float centerAzimuth;
            public float degreesLeft;
            public float degreesRight;
            public bool reachableByLand;
            public bool reachableByWater;
            public bool reachableByAir;
            public string mapColor;
        }

        private List<RegionCityEditingDTO> ListAllRegionCities()
        {
            var cities = new List<RegionCityEditingDTO>();
            using var list = m_regionByAnyQuery.ToEntityArray(Allocator.Temp);
            foreach (var city in list)
            {
                var data = EntityManager.GetComponentData<ADRRegionCity>(city);
                var mapColor = data.mapColor;
                cities.Add(new RegionCityEditingDTO
                {
                    entity = city,
                    name = data.name.ToString(),
                    centerAzimuth = data.azimuthAngle * USHORT_ANGLE_TO_DEGREES,
                    degreesLeft = data.azimuthWidthLeft * USHORT_ANGLE_TO_DEGREES,
                    degreesRight = data.azimuthWidthRight * USHORT_ANGLE_TO_DEGREES,
                    reachableByLand = EntityManager.HasComponent<ADRRegionLandCity>(city),
                    reachableByWater = EntityManager.HasComponent<ADRRegionWaterCity>(city),
                    reachableByAir = EntityManager.HasComponent<ADRRegionAirCity>(city),
                    mapColor = mapColor.ToRGB(true)
                });
            }
            return cities;
        }

        private void SaveRegionCity(RegionCityEditingDTO input)
        {
            if (input.entity == Entity.Null)
            {
                var newCity = EntityManager.CreateEntity();
                EntityManager.AddComponentData(newCity, new ADRRegionCity
                {
                    name = input.name,
                    azimuthAngle = (ushort)(input.centerAzimuth / USHORT_ANGLE_TO_DEGREES),
                    azimuthWidthLeft = (ushort)(input.degreesLeft / USHORT_ANGLE_TO_DEGREES),
                    azimuthWidthRight = (ushort)(input.degreesRight / USHORT_ANGLE_TO_DEGREES),
                    mapColor = ColorExtensions.FromRGB(input.mapColor, true)
                });

                runOnUpdate.Enqueue(() =>
                {
                    var cmdBuffer = m_modificationEndBarrier.CreateCommandBuffer();
                    if (input.reachableByLand)
                    {
                        cmdBuffer.AddComponent<ADRRegionLandCity>(newCity);
                    }
                    if (input.reachableByWater)
                    {
                        cmdBuffer.AddComponent<ADRRegionWaterCity>(newCity);
                    }
                    if (input.reachableByAir)
                    {
                        cmdBuffer.AddComponent<ADRRegionAirCity>(newCity);
                    }
                });
            }
            else if (EntityManager.TryGetComponent<ADRRegionCity>(input.entity, out var city))
            {
                runOnUpdate.Enqueue(() =>
                {
                    var cmdBuffer = m_modificationEndBarrier.CreateCommandBuffer();
                    city.name = input.name;
                    city.azimuthAngle = (ushort)(input.centerAzimuth / USHORT_ANGLE_TO_DEGREES);
                    city.azimuthWidthLeft = (ushort)(input.degreesLeft / USHORT_ANGLE_TO_DEGREES);
                    city.azimuthWidthRight = (ushort)(input.degreesRight / USHORT_ANGLE_TO_DEGREES);
                    city.mapColor = ColorExtensions.FromRGB(input.mapColor, true);
                    cmdBuffer.SetComponent(input.entity, city);

                    var isLand = EntityManager.HasComponent<ADRRegionLandCity>(input.entity);
                    var isWater = EntityManager.HasComponent<ADRRegionWaterCity>(input.entity);
                    var isAir = EntityManager.HasComponent<ADRRegionAirCity>(input.entity);

                    if (input.reachableByLand != isLand)
                    {
                        if (input.reachableByLand)
                        {
                            cmdBuffer.AddComponent<ADRRegionLandCity>(input.entity);
                        }
                        else
                        {
                            cmdBuffer.RemoveComponent<ADRRegionLandCity>(input.entity);
                        }
                    }
                    if (input.reachableByWater != isWater)
                    {
                        if (input.reachableByWater)
                        {
                            cmdBuffer.AddComponent<ADRRegionWaterCity>(input.entity);
                        }
                        else
                        {
                            cmdBuffer.RemoveComponent<ADRRegionWaterCity>(input.entity);
                        }
                    }
                    if (input.reachableByAir != isAir)
                    {
                        if (input.reachableByAir)
                        {
                            cmdBuffer.AddComponent<ADRRegionAirCity>(input.entity);
                        }
                        else
                        {
                            cmdBuffer.RemoveComponent<ADRRegionAirCity>(input.entity);
                        }
                    }
                });
            }
            m_cachedRegionLandNeighborhood = null;
            m_cachedRegionWaterNeighborhood = null;
            m_cachedRegionAirNeighborhood = null;
            runOnUpdate.Enqueue(() => m_modificationEndBarrier.CreateCommandBuffer().RemoveComponent<ADRRegionCityReference>(m_outsideConnectionsMapped, EntityQueryCaptureMode.AtPlayback));
        }

        private void RemoveRegionCity(Entity entity)
        {
            if (EntityManager.HasComponent<ADRRegionCity>(entity))
            {
                runOnUpdate.Enqueue(() =>
                {
                    var cmdBuffer = m_modificationEndBarrier.CreateCommandBuffer();
                    cmdBuffer.DestroyEntity(entity);
                });
            }
            runOnUpdate.Enqueue(() => m_modificationEndBarrier.CreateCommandBuffer().RemoveComponent<ADRRegionCityReference>(m_outsideConnectionsMapped, EntityQueryCaptureMode.AtPlayback));
        }

        #endregion

        #region Serialization
        private const uint CURRENT_VERSION = 0;

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.CheckVersionK45(CURRENT_VERSION, GetType());


            m_cachedRegionLandNeighborhood = null;
            m_cachedRegionWaterNeighborhood = null;
            m_cachedRegionAirNeighborhood = null;
        }

        public void SetDefaults(Context context)
        {
            m_cachedRegionLandNeighborhood = null;
            m_cachedRegionWaterNeighborhood = null;
            m_cachedRegionAirNeighborhood = null;
        }
        #endregion
    }
}

