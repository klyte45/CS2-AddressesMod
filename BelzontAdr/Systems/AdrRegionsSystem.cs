using Belzont.Interfaces;
using Belzont.Serialization;
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
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using static BelzontAdr.ADRMapUtils;
using Color = UnityEngine.Color;
using Transform = Game.Objects.Transform;

namespace BelzontAdr
{
    public partial class AdrRegionsSystem : GameSystemBase, IBelzontBindable, IBelzontSerializableSingleton<AdrRegionsSystem>
    {
        private NameSystem m_nameSystem;
        private TerrainSystem m_terrainSystem;
        private WaterSystem m_waterSystem;
        private EntityQuery m_outsideConnectionsObject;
        private EntityQuery m_highwaysQuery;
        private EntityQuery m_urbanRoadsQuery;
        private EntityQuery m_railroadsQuery;

        #region bindings
        public void SetupCallBinder(Action<string, Delegate> eventCaller)
        {
            eventCaller("regions.listOutsideConnections", ListOutsideConnections);
            eventCaller("regions.getCityBounds", GetCityBounds);
            eventCaller("regions.listHighways", ListHighways);
            eventCaller("regions.listTrainTracks", ListTrainTracks);
            eventCaller("regions.listUrbanRoads", ListUrbanRoads);

            eventCaller("regions.getCityTerrain", () => EncodeToBase64(RenderTextureTo2D(m_terrainSystem.heightmap as RenderTexture, x => x.r, MapType.Topographic)));
            eventCaller("regions.getCityWater", () => EncodeToBase64(RenderTextureTo2D(m_waterSystem.WaterTexture, x => x.r, MapType.Transparency)));
            eventCaller("regions.getCityWaterPollution", () => EncodeToBase64(RenderTextureTo2D(m_waterSystem.WaterTexture, x => x.a, MapType.Transparency)));
        }

        private object EncodeToBase64(Texture2D texture2D) => $"data:image/png;base64,{Convert.ToBase64String(texture2D.EncodeToPNG())}";

        public void SetupCaller(Action<string, object[]> eventCaller)
        {

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
                }).ToArray();
        }
        #endregion

        protected override void OnCreate()
        {
            base.OnCreate();
            m_nameSystem = World.GetOrCreateSystemManaged<NameSystem>();
            m_terrainSystem = World.GetExistingSystemManaged<TerrainSystem>();
            m_waterSystem = World.GetExistingSystemManaged<WaterSystem>();
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
        }

        protected override void OnUpdate()
        {

        }

        public struct RegionCity
        {
            public FixedString64Bytes name;
            public ushort azimuthAngle;
            public ushort azimuthCwWidth;
            public ushort azimuthCcwWidth;
            public Color mapColor;

            public readonly bool IsInside(ushort angleAzimuth)
            {
                unchecked
                {
                    var min = azimuthAngle - azimuthCcwWidth;
                    var max = azimuthAngle + azimuthCwWidth;
                    return min > max ? min >= angleAzimuth || angleAzimuth >= max : min <= angleAzimuth && angleAzimuth <= max;
                }
            }

            public static float ToDegreeAngle(ushort azimuthValue) => 360f / ushort.MaxValue * azimuthValue;
            public static ushort ToAzimuthValue(float angle) => (ushort)(angle * 1f / 360f % 1 * ushort.MaxValue);
        }

        public RegionCity[] regionCities;


        #region Serialization
        private const uint CURRENT_VERSION = 0;

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            ((IBelzontSerializableSingleton<AdrRegionsSystem>)this).CheckVersion(reader, CURRENT_VERSION);

        }

        public JobHandle SetDefaults(Context context)
        {

            return Dependency;
        }
        #endregion
    }
}

