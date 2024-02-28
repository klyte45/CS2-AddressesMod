using Belzont.Interfaces;
using Colossal.Entities;
using Game;
using Game.Areas;
using Game.Buildings;
using Game.Net;
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
                    if (EntityManager.TryGetComponent<CurrentDistrict>(e, out var currDistrict)
                        && currDistrict.m_District != Entity.Null)
                    {
                        result.allowDistrict = mainSystem.CurrentCitySettings.DistrictNameAsNameStation;
                        if (EntityManager.TryGetComponent<ADREntityStationRef>(currDistrict.m_District, out var adrData) && adrData.m_refStationBuilding == e)
                        {
                            result.entityValue = currDistrict.m_District;
                        }
                        if (result.allowDistrict)
                        {
                            result.districtRef = currDistrict.m_District;
                        }
                    }
                    result.roadAggegateOptions.AddRange(GetRoadOptionsForBuildingEntity(building).Select(x => new EntityOption
                    {
                        entity = x,
                        name = new ValuableName(nameSystem.GetName(x))
                    }));
                    result.entityValue = AdrNameSystemOverrides.GetMainReferenceAggregate(e, building);
                }
            }


            return result;
        }


        private List<Entity> GetRoadOptionsForBuildingEntity(Building buildingData)
        {
            Queue<Entity> nodesToMap = new Queue<Entity>();
            HashSet<Entity> roadsMapped = new();
            List<Entity> result = new();
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
                    if (EntityManager.TryGetComponent<Aggregated>(item.m_Edge, out var agg)) continue;
                    if (agg.m_Aggregate == starterAgg.m_Aggregate)
                    {
                        if (EntityManager.TryGetComponent<Edge>(item.m_Edge, out var edgeItem)) continue;
                        if (edgeItem.m_Start != nextItem)
                        {
                            nodesToMap.Enqueue(edgeItem.m_Start);
                        }
                        else
                        {
                            nodesToMap.Enqueue(edgeItem.m_End);
                        }
                    }
                    else
                    {
                        result.Add(agg.m_Aggregate);
                    }
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

        }

        private class EntityOption
        {
            public Entity entity;
            public ValuableName name;

        }
    }
}
