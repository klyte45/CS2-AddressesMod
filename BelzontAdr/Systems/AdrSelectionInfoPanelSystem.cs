using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Entities;
using Colossal.UI.Binding;
using Game.Areas;
using Game.Buildings;
using Game.Common;
using Game.Net;
using Game.UI;
using Game.UI.InGame;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using static Belzont.Utils.NameSystemExtensions;

namespace BelzontAdr
{
    public partial class AdrSelectionInfoPanelSystem : InfoSectionBase, IBelzontBindable
    {
        #region Binding
        private Action<string, object[]> m_eventCaller;

        /// <summary>
        /// Binder surface for Addresses SIP and Road Grouping tool (HW-03b): pass the ADR
        /// aggregate entity (e.g. tool <c>SelectedAggregation</c> / SIP <c>entityValue</c>).
        /// Highway setters no-op on vanilla Aggregate. Aggregate custom-name binders are for the
        /// tool only — SIP uses the selection panel title input.
        /// </summary>
        public void SetupCallBinder(Action<string, Delegate> eventCaller)
        {
            eventCaller("selectionPanel.getEntityOptions", GetEntityOptions);
            eventCaller("selectionPanel.setEntityRoadReference", SetEntityRoadReference);
            eventCaller("selectionPanel.redrawSeed", ResetEntityAdrSeed);
            eventCaller("selectionPanel.changeSeedByDelta", ChangeEntityAdrSeedByDelta);
            eventCaller("selectionPanel.getAggregateCustomName", GetAggregateCustomName);
            eventCaller("selectionPanel.setAggregateCustomName", SetAggregateCustomName);
            eventCaller("selectionPanel.setHighwayDataId", SetHighwayDataId);
            eventCaller("selectionPanel.setRouteDirection", SetRouteDirection);
            eventCaller("selectionPanel.setMileageOverrideActive", SetMileageOverrideActive);
            eventCaller("selectionPanel.setMileageOverrideMeters", SetMileageOverrideMeters);
            eventCaller("selectionPanel.setReverseCounting", SetReverseCounting);
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
        private AdrNameAggregateMembershipSystem membershipSystem;
        private AdrHighwayRoutesSystem highwayRoutesSystem;
        private EntityQueryMask entityExistsQuery;

        protected override string group => "K45.Addresses";

        protected override void OnCreate()
        {
            base.OnCreate();
            nameSystem = World.GetOrCreateSystemManaged<NameSystem>();
            mainSystem = World.GetOrCreateSystemManaged<AdrMainSystem>();
            membershipSystem = World.GetOrCreateSystemManaged<AdrNameAggregateMembershipSystem>();
            highwayRoutesSystem = World.GetOrCreateSystemManaged<AdrHighwayRoutesSystem>();
            m_InfoUISystem.AddMiddleSection(this);
            entityExistsQuery = EntityManager.UniversalQuery.GetEntityQueryMask();
            visible = true;
        }
        protected override void OnUpdate()
        {

        }

        private List<AdrEntityBase> GetEntityOptions(Entity e)
        {
            var resultList = new List<AdrEntityBase>();
            if (EntityManager.HasComponent<ADRHighwayMarkerData>(e))
            {
                resultList.Add(new() { type = AdrEntityType.RoadMark });
            }
            if (EntityManager.HasComponent<ADRVehicleData>(e))
            {
                resultList.Add(new() { type = AdrEntityType.Vehicle });
            }

            // Independent SIP block (same pattern as RoadMark / Vehicle) — not tied to naming/seed.
            TryAppendHighwayAggregateOption(e, resultList);

            bool hasCustomName = EntityManager.HasComponent<CustomName>(e);
            // Informational Addresses block: custom name is edited in the SIP title; erase there to restore mod naming.
            if (hasCustomName)
            {
                resultList.Add(new() { type = AdrEntityType.CustomName });
            }
            if (EntityManager.HasComponent<ADRVehicleBuildingOrigin>(e))
            {
                resultList.Add(new() { type = AdrEntityType.VehicleSource });
            }
            // Custom name: still expose aggregate seed (SIP erase-to-use-mod + Road Grouping tool).
            // Station/building naming refs stay hidden while a custom name is set.
            if (hasCustomName)
            {
                TryAppendAggregateSeedOption(e, resultList);
                PreferNamingBlocksFirst(resultList);
                return resultList;
            }
            if (EntityManager.TryGetComponent<Building>(e, out var building))
            {
                var result = new AdrEntityData
                {
                    targetEntityToName = e,
                };
                if (EntityManager.HasComponent<PublicTransportStation>(e))
                {
                    if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"{e} have building and PublicTransportStation");
                    if (EntityManager.TryGetComponent<Owner>(e, out var owner))
                    {
                        return GetEntityOptions(owner.m_Owner);
                    }
                    result.type = AdrEntityType.PublicTransportStation;
                    StationRefFill(e, result, building);
                }
                else if (EntityManager.HasComponent<Game.Buildings.CargoTransportStation>(e))
                {
                    if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"{e} have building and CargoTransportStation");
                    result.type = AdrEntityType.CargoTransportStation;
                    StationRefFill(e, result, building);
                }
                else
                {
                    if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"{e} have building only");
                }
                resultList.Add(result);
            }
            if (EntityManager.TryGetComponent<Aggregated>(e, out var agg))
            {
                var result = new AdrEntityData
                {
                    targetEntityToName = e,
                };
                if (EntityManager.HasComponent<Road>(e))
                {
                    if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"{e} have Aggregated and Road");
                    result.type = AdrEntityType.RoadAggregation;
                    result.entityValue = AdrRoadAddressNumbering.TryGetAdrAggregate(EntityManager, e, out var adrAggregate)
                        ? adrAggregate
                        : agg.m_Aggregate;
                }
                else
                {
                    if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"{e} have agg only");
                }
                resultList.Add(result);
            }
            if (EntityManager.HasComponent<Aggregate>(e) || EntityManager.HasComponent<ADRNameAggregate>(e))
            {
                TryAppendAggregateSeedOption(e, resultList);
            }
            if (EntityManager.HasComponent<District>(e))
            {
                var result = new AdrEntityData
                {
                    targetEntityToName = e,
                };
                if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"{e} is District");
                result.type = AdrEntityType.District;
                result.hasCustomNameList = mainSystem.TryGetDistrictNamesList(out var districtNamesList);
                if (result.hasCustomNameList)
                {
                    result.customNameListName = districtNamesList.Name;
                }
                result.entityValue = e;
                resultList.Add(result);
            }

            PreferNamingBlocksFirst(resultList);
            return resultList;
        }

        private void TryAppendAggregateSeedOption(Entity e, List<AdrEntityBase> resultList)
        {
            if (!EntityManager.HasComponent<Aggregate>(e) && !EntityManager.HasComponent<ADRNameAggregate>(e))
            {
                return;
            }

            var result = new AdrEntityData
            {
                targetEntityToName = e,
                type = AdrEntityType.RoadAggregation,
                entityValue = e,
                isAdrAggregate = EntityManager.HasComponent<ADRNameAggregate>(e),
            };
            if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"{e} is Aggregate");
            bool foundRef = EntityManager.HasComponent<ADRNameAggregate>(e)
                ? mainSystem.FindReferenceRoad(e, out DynamicBuffer<ADRNameAggregateElement> _, out var refRoadAdr)
                : mainSystem.FindReferenceRoad(e, out DynamicBuffer<AggregateElement> _, out refRoadAdr);
            if (foundRef)
            {
                result.hasCustomNameList = mainSystem.GetRoadNameList(refRoadAdr, out var namesList);
                if (result.hasCustomNameList)
                {
                    result.customNameListName = namesList.Name;
                }
            }
            resultList.Add(result);
        }

        /// <summary>
        /// Naming-reference SIP blocks always lead the list when present (highway / mark / vehicle trail).
        /// </summary>
        private static void PreferNamingBlocksFirst(List<AdrEntityBase> resultList)
        {
            if (resultList == null || resultList.Count <= 1)
            {
                return;
            }

            var naming = new List<AdrEntityBase>(resultList.Count);
            var other = new List<AdrEntityBase>(resultList.Count);
            foreach (var item in resultList)
            {
                if (IsNamingReferenceBlock(item.type))
                {
                    naming.Add(item);
                }
                else
                {
                    other.Add(item);
                }
            }

            if (naming.Count == 0)
            {
                return;
            }

            resultList.Clear();
            resultList.AddRange(naming);
            resultList.AddRange(other);
        }

        private static bool IsNamingReferenceBlock(AdrEntityType type) => type switch
        {
            AdrEntityType.CustomName
                or AdrEntityType.PublicTransportStation
                or AdrEntityType.CargoTransportStation
                or AdrEntityType.RoadAggregation
                or AdrEntityType.District => true,
            _ => false,
        };

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
            if (!entityExistsQuery.MatchesIgnoreFilter(result.entityValue)) result.entityValue = Entity.Null;
            result.roadAggegateOptions.AddRange(GetRoadOptionsForBuildingEntity(building)?.Select(x => new EntityOption
            {
                entity = x,
                name = new ValuableName(nameSystem.GetName(x))
            }));
        }

        private bool SetEntityRoadReference(Entity target, Entity reference, Entity namingUiSource)
        {
            if (EntityManager.TryGetComponent<ADREntityManualBuildingRef>(target, out var refComp))
            {
                refComp.m_refNamedEntity = reference;
                EntityManager.SetComponentData(target, refComp);
            }
            else
            {
                refComp = new()
                {
                    m_refNamedEntity = reference
                };
                EntityManager.AddComponentData(target, refComp);
            }
            EntityManager.AddComponent<Updated>(target);
            EntityManager.AddComponent<Updated>(namingUiSource);
            return true;
        }

        private bool ResetEntityAdrSeed(Entity target)
        {
            if (EntityManager.HasComponent<ADRRandomizationData>(target))
            {
                var data = new ADRRandomizationData();
                data.Redraw();
                EntityManager.SetComponentData(target, data);
                EntityManager.AddComponent<Updated>(target);
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool ChangeEntityAdrSeedByDelta(Entity target, int delta)
        {
            if (EntityManager.TryGetComponent<ADRRandomizationData>(target, out var rand))
            {
                rand.AddDelta(delta);
                EntityManager.SetComponentData(target, rand);
                EntityManager.AddComponent<Updated>(target);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void TryAppendHighwayAggregateOption(Entity selected, List<AdrEntityBase> resultList)
        {
            if (!TryResolveAdrHighwayHost(selected, out Entity adrAggregate))
            {
                return;
            }

            var result = new AdrEntityData
            {
                type = AdrEntityType.HighwayAggregate,
                targetEntityToName = selected,
                entityValue = adrAggregate,
                isAdrAggregate = true,
            };
            FillHighwayAggregateFields(result);
            resultList.Add(result);
        }

        private void FillHighwayAggregateFields(AdrEntityData result)
        {
            Entity host = result.entityValue;
            if (host == Entity.Null || !EntityManager.Exists(host))
            {
                return;
            }

            membershipSystem.EnsureHighwayData(host);
            if (!EntityManager.TryGetComponent(host, out ADRNameAggregateHighwayData highway))
            {
                highway = ADRNameAggregateHighwayData.CreateDefault();
            }

            result.highwayDataId = highway.highwayDataId.ToString();
            result.routeDirection = (int)highway.routeDirection;
            result.mileageOverrideActive = highway.mileageOverrideActive;
            result.mileageOverrideMeters = highway.mileageOverrideMeters;
            result.reverseCounting = highway.reverseCounting;
        }

        /// <summary>
        /// Resolve ADR aggregate host for highway binders (and tool custom-name). Accepts the ADR
        /// entity directly (SIP <c>entityValue</c> / tool <c>SelectedAggregation</c>) or an edge with
        /// <see cref="ADRNameAggregated"/>. Vanilla Aggregate → false.
        /// <para>
        /// SIP custom name stays on the selection panel title input — do not surface it in Addresses
        /// options. <see cref="GetAggregateCustomName"/> / <see cref="SetAggregateCustomName"/> are for
        /// Road Grouping tool options (HW-03b).
        /// </para>
        /// </summary>
        private bool TryResolveAdrHighwayHost(Entity entity, out Entity adrAggregate)
        {
            adrAggregate = Entity.Null;
            if (entity == Entity.Null || !EntityManager.Exists(entity))
            {
                return false;
            }

            if (EntityManager.HasComponent<ADRNameAggregate>(entity))
            {
                adrAggregate = entity;
                membershipSystem.EnsureHighwayData(adrAggregate);
                return true;
            }

            if (AdrRoadAddressNumbering.TryGetAdrAggregate(EntityManager, entity, out adrAggregate))
            {
                membershipSystem.EnsureHighwayData(adrAggregate);
                return true;
            }

            return false;
        }

        private string GetAggregateCustomName(Entity entity)
            => TryResolveAdrHighwayHost(entity, out Entity adr)
               && nameSystem.TryGetCustomName(adr, out string customName)
                ? customName
                : string.Empty;

        private bool SetAggregateCustomName(Entity entity, string name)
        {
            if (!TryResolveAdrHighwayHost(entity, out Entity adr))
            {
                return false;
            }

            nameSystem.SetCustomName(adr, name ?? string.Empty);
            DirtyAdrAggregateConsumers(adr);
            return true;
        }

        private bool SetHighwayDataId(Entity entity, string highwayDataId)
        {
            if (!TryResolveAdrHighwayHost(entity, out Entity adr)
                || !EntityManager.TryGetComponent(adr, out ADRNameAggregateHighwayData highway))
            {
                return false;
            }

            Colossal.Hash128 id;
            if (string.IsNullOrWhiteSpace(highwayDataId))
            {
                id = default;
            }
            else if (!Colossal.Hash128.TryParse(highwayDataId, out id))
            {
                return false;
            }
            else if (id != default && !highwayRoutesSystem.TryGetHighwayData(id, out _))
            {
                return false;
            }

            highway.highwayDataId = id;
            EntityManager.SetComponentData(adr, highway);
            DirtyAdrAggregateConsumers(adr);
            return true;
        }

        private bool SetRouteDirection(Entity entity, int routeDirection)
        {
            if (!TryResolveAdrHighwayHost(entity, out Entity adr)
                || !EntityManager.TryGetComponent(adr, out ADRNameAggregateHighwayData highway))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(ADRNameAggregateHighwayData.RouteDirection), routeDirection))
            {
                return false;
            }

            highway.routeDirection = (ADRNameAggregateHighwayData.RouteDirection)routeDirection;
            EntityManager.SetComponentData(adr, highway);
            DirtyAdrAggregateConsumers(adr);
            return true;
        }

        private bool SetMileageOverrideActive(Entity entity, bool active)
        {
            if (!TryResolveAdrHighwayHost(entity, out Entity adr)
                || !EntityManager.TryGetComponent(adr, out ADRNameAggregateHighwayData highway))
            {
                return false;
            }

            highway.mileageOverrideActive = active;
            EntityManager.SetComponentData(adr, highway);
            DirtyAdrAggregateConsumers(adr);
            return true;
        }

        private bool SetMileageOverrideMeters(Entity entity, int mileageMeters)
        {
            if (!TryResolveAdrHighwayHost(entity, out Entity adr)
                || !EntityManager.TryGetComponent(adr, out ADRNameAggregateHighwayData highway))
            {
                return false;
            }

            highway.mileageOverrideMeters = mileageMeters;
            EntityManager.SetComponentData(adr, highway);
            DirtyAdrAggregateConsumers(adr);
            return true;
        }

        private bool SetReverseCounting(Entity entity, bool reverse)
        {
            if (!TryResolveAdrHighwayHost(entity, out Entity adr)
                || !EntityManager.TryGetComponent(adr, out ADRNameAggregateHighwayData highway))
            {
                return false;
            }

            highway.reverseCounting = reverse;
            EntityManager.SetComponentData(adr, highway);
            DirtyAdrAggregateConsumers(adr);
            return true;
        }

        private void DirtyAdrAggregateConsumers(Entity adrAggregate)
        {
            if (!EntityManager.HasComponent<Updated>(adrAggregate))
            {
                EntityManager.AddComponent<Updated>(adrAggregate);
            }
            if (!EntityManager.HasComponent<BatchesUpdated>(adrAggregate))
            {
                EntityManager.AddComponent<BatchesUpdated>(adrAggregate);
            }
            mainSystem.MarkRoadsDirty();
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
            if (EntityManager.TryGetComponent<Aggregated>(buildingData.m_RoadEdge, out var starterAgg))
            {
                if (AdrRoadAddressNumbering.TryGetAdrAggregate(EntityManager, buildingData.m_RoadEdge, out var starterAdr))
                {
                    result.Add(starterAdr);
                }
                else
                {
                    result.Add(starterAgg.m_Aggregate);
                }
            }
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
                    if (AdrRoadAddressNumbering.TryGetAdrAggregate(EntityManager, item.m_Edge, out var adrAgg))
                    {
                        result.Add(adrAgg);
                    }
                    else if (EntityManager.TryGetComponent<Aggregated>(item.m_Edge, out var agg))
                    {
                        result.Add(agg.m_Aggregate);
                    }
                }
            }
            return result;
        }

        protected override void Reset()
        {
        }

        protected override void OnProcess()
        {
        }

        public override void OnWriteProperties(IJsonWriter writer)
        {
        }
        private class AdrEntityBase
        {
            public AdrEntityType type;
        }

        private class AdrEntityData : AdrEntityBase
        {
            public Entity targetEntityToName;
            public Entity entityValue;
            public readonly List<EntityOption> buildingsOptions = new();
            public readonly List<EntityOption> roadAggegateOptions = new();
            public bool allowDistrict;
            public Entity districtRef;
            public bool hasCustomNameList;
            public string customNameListName;
            /// <summary>True when <see cref="entityValue"/> is an ADR name-aggregate (not vanilla Aggregate).</summary>
            public bool isAdrAggregate;
            public string highwayDataId;
            public int routeDirection;
            public bool mileageOverrideActive;
            public int mileageOverrideMeters;
            public bool reverseCounting;
        }

        private enum AdrEntityType
        {
            CustomName = -1,
            None = 0,
            PublicTransportStation,
            CargoTransportStation,
            RoadAggregation,
            District,
            Vehicle,
            RoadMark,
            VehicleSource,
            HighwayAggregate,
        }

        private class EntityOption
        {
            public Entity entity;
            public ValuableName name;

        }
    }
}
