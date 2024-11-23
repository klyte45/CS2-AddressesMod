using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Entities;
using Game;
using Game.Common;
using Game.Objects;
using Game.Routes;
using Game.Simulation;
using Game.Tools;
using Game.UI;
using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Transform = Game.Objects.Transform;

namespace BelzontAdr
{
    public partial class AdrRegionsSystem : GameSystemBase, IBelzontBindable
    {
        private NameSystem m_nameSystem;
        private TerrainSystem m_terrainSystem;
        private EntityQuery m_outsideConnectionsObject;

        #region bindings
        public void SetupCallBinder(Action<string, Delegate> eventCaller)
        {
            eventCaller("regions.listOutsideConnections", ListOutsideConnections);
            eventCaller("regions.getCityBounds", GetCityBounds);
        }

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
        #endregion

        protected override void OnCreate()
        {
            base.OnCreate();
            m_nameSystem = World.GetOrCreateSystemManaged<NameSystem>();
            m_terrainSystem = World.GetExistingSystemManaged<TerrainSystem>();
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
        }

        protected override void OnUpdate()
        {

        }
    }
}

