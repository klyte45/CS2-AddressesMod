﻿using Belzont.Interfaces;
using Colossal.Entities;
using Game;
using Game.Areas;
using Game.Buildings;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using static Belzont.Utils.NameSystemExtensions;

namespace BelzontAdr
{
    public partial class AdrSelectionInfoPanelSystem : GameSystemBase, IBelzontBindable
    {
        #region Binding
        private Action<string, object[]> m_eventCaller;

        public void SetupCallBinder(Action<string, Delegate> eventCaller)
        {
            eventCaller("selectionPanel.getEntityOptions", GetEntityOptions);
            eventCaller("selectionPanel.setEntityRoadReference", SetEntityRoadReference);
        }

        public void SetupCaller(Action<string, object[]> eventCaller)
        {
            m_eventCaller = eventCaller;
        }

        public void SetupEventBinder(Action<string, Delegate> eventCaller)
        {
        }
        #endregion

        private NameSystem nameSystem;
        private AdrMainSystem mainSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            nameSystem = World.GetOrCreateSystemManaged<NameSystem>();
            mainSystem = World.GetOrCreateSystemManaged<AdrMainSystem>();
        }
        protected override void OnUpdate() { }

        private AdrEntityData GetEntityOptions(Entity e)
        {
            var result = new AdrEntityData();
            if (EntityManager.TryGetComponent<Building>(e, out var building))
            {
                if (EntityManager.HasComponent<PublicTransportStation>(e))
                {
                    result.type = AdrEntityType.PublicTransportStation;
                    StationRefFill(e, result, building);
                }
                else if (EntityManager.HasComponent<Game.Buildings.CargoTransportStation>(e))
                {
                    result.type = AdrEntityType.PublicTransportStation;
                    StationRefFill(e, result, building);
                }
            }


            return result;
        }

        private void StationRefFill(Entity e, AdrEntityData result, Building building)
        {
            if (EntityManager.TryGetComponent<ADREntityManualBuildingRef>(e, out var manualRef)) result.entityValue = manualRef.m_refNamedEntity;
            if (EntityManager.TryGetComponent<CurrentDistrict>(e, out var currDistrict)
                && currDistrict.m_District != Entity.Null)
            {
                result.allowDistrict = true;
                if (result.entityValue == Entity.Null && EntityManager.TryGetComponent<ADREntityStationRef>(currDistrict.m_District, out var adrData) && adrData.m_refStationBuilding == e)
                {
                    result.entityValue = currDistrict.m_District;
                }
                result.districtRef = currDistrict.m_District;
            }
            if (result.entityValue == Entity.Null) result.entityValue = AdrNameSystemOverrides.GetMainReferenceAggregate(e, building);
            result.roadAggegateOptions.AddRange(GetRoadOptionsForBuildingEntity(building).Select(x => new EntityOption
            {
                entity = x,
                name = new ValuableName(nameSystem.GetName(x))
            }));
        }

        private bool SetEntityRoadReference(Entity target, Entity reference)
        {
            if (EntityManager.TryGetComponent<ADREntityManualBuildingRef>(target, out var refComp))
            {
                refComp.m_refNamedEntity = reference;
                EntityManager.SetComponentData(target, refComp);
            }
            else
            {
                refComp = new ADREntityManualBuildingRef();
                refComp.m_refNamedEntity = reference;
                EntityManager.AddComponentData(target, refComp);
            }
            EntityManager.AddComponent<Updated>(target);
            return true;
        }

        private HashSet<Entity> GetRoadOptionsForBuildingEntity(Building buildingData)
        {
            Queue<Entity> nodesToMap = new Queue<Entity>();
            HashSet<Entity> roadsMapped = new();
            HashSet<Entity> result = new();
            if (!EntityManager.TryGetComponent<Edge>(buildingData.m_RoadEdge, out var edge)) return null;
            nodesToMap.Enqueue(edge.m_Start);
            nodesToMap.Enqueue(edge.m_End);
            roadsMapped.Add(buildingData.m_RoadEdge);
            if (EntityManager.TryGetComponent<Aggregated>(buildingData.m_RoadEdge, out var starterAgg)) result.Add(starterAgg.m_Aggregate);
            var maxHops = 30;
            while (maxHops-- > 0 && nodesToMap.TryDequeue(out Entity nextItem))
            {
                if (!EntityManager.TryGetBuffer<ConnectedEdge>(nextItem, true, out var connections)) continue;
                foreach (var item in connections)
                {
                    if (roadsMapped.Contains(item.m_Edge)) continue;
                    roadsMapped.Add(item.m_Edge);
                    if (!EntityManager.HasComponent<Road>(item.m_Edge)) continue;
                    if (!EntityManager.TryGetComponent<Edge>(item.m_Edge, out var edgeItem)) continue;
                    if (edgeItem.m_Start != nextItem)
                    {
                        nodesToMap.Enqueue(edgeItem.m_Start);
                    }
                    else
                    {
                        nodesToMap.Enqueue(edgeItem.m_End);
                    }
                    if (!EntityManager.TryGetComponent<Aggregated>(item.m_Edge, out var agg)) continue;
                    result.Add(agg.m_Aggregate);
                }
            }
            return result;
        }

        private class AdrEntityData
        {
            public AdrEntityType type = AdrEntityType.None;
            public Entity entityValue;
            public readonly List<EntityOption> buildingsOptions = new();
            public readonly List<EntityOption> roadAggegateOptions = new();
            public bool allowDistrict;
            public Entity districtRef;
        }

        private enum AdrEntityType
        {
            None = 0,
            PublicTransportStation,
            CargoTransportStation,

        }

        private class EntityOption
        {
            public Entity entity;
            public ValuableName name;

        }
    }
}
