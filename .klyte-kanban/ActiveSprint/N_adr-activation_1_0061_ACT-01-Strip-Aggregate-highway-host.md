# [0061] ACT-01-Strip-Aggregate-highway-host

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/12-activation-change-map.md §B
RefsLibrary/2026090907_custom-road-name-aggregation/13-activation-sprint-roadmap.md ACT-01

## User Story

> Acting as **a Cities: Skylines 2 mod developer**, I want **Aggregate-hosted ADR highway Data/Dirty/Cache removed while the highway registry remains**, so that I **ADR can own name aggregation without Aggregate-hosted highway state blocking the fork model**.

---

## Background

Workstream 1 of ADR aggregation activation. Highway route/mileage must leave vanilla Aggregate so ADR name-aggregates can own membership without Aggregate-hosted highway state. Remove the Aggregate highway component fields from the structs (prefer deleting ADRHighwayAggregationData / DirtyHwId / CacheData types). Previously saved Aggregate highway component bytes are ignored on load — no migrate/convert of old highwayDataId or cache values.

---

## Definition of Ready (DoR)

- [ ] Road Grouping T0-T3 shipped
- [ ] RefsLibrary 12 §B and 13 ACT-01 catalog agreed
- [ ] Decision locked: remove Aggregate highway host now (not migrate-in-place)

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] ADRHighwayAggregationData, ADRHighwayAggregationDataDirtyHwId, and ADRHighwayAggregationCacheData are deleted or fully obsolete with no writers
- [ ] AdrHighwayRoutesSystem Aggregate-touching jobs removed (NewAggregateFiller, ReplicateAggregateSetting, CalculateHighwayCacheData, AggregationCacheEraser, InfoPanel Aggregate writes); highwaysDataRegistry CRUD + serialize kept
- [ ] AdrHighwayRoutes2BSystem removed or gutted; AddressesCs2Mod registration updated if emptied
- [ ] Consumers no longer read Aggregate highway Data/Cache: AdrNameSystemOverrides highway branch, BuildingUtilsOverrides, AdrRoadAddressNumbering, AdrRegionsSystem highwayId, Road Grouping overlay/controller numbering
- [ ] Load path ignores/drops leftover Aggregate highway components
- [ ] HighwayRegisterManagement.tsx + registry APIs still compile and run (orphaned from Aggregate until later reattach epic)
- [ ] Unused Aggregate highway fields are removed from the structs (ADRHighwayAggregationData.highwayDataId; CacheData.startDistanceOverrideKm and reverseCounting; DirtyHwId tag gone) — not left as dead serialized members
- [ ] Previously saved Aggregate highway component data is ignored on load (no migration of old highwayDataId / cache values into new state)

---

## Implementation Notes

1. Delete or stop writing ADRHighwayAggregationData.highwayDataId (Hash128)
2. Delete empty dirty tag ADRHighwayAggregationDataDirtyHwId
3. Delete ADRHighwayAggregationCacheData.startDistanceOverrideKm (float) and reverseCounting (bool)
4. Gut AdrHighwayRoutesSystem Aggregate filler/replicate/cache/eraser paths; keep registry
5. Gut AdrHighwayRoutes2BSystem Aggregated C/U/D dirtying of Aggregate highway cache
6. Strip Aggregate→highway name branch in AdrNameSystemOverrides until later tool epic
7. Strip Aggregate cache + marker mileage from BuildingUtilsOverrides and AdrRoadAddressNumbering
8. AdrRegionsSystem: stop reading removed highwayId (default/empty)
9. Road Grouping overlay/controller: walk ADR/vanilla without highway overrides
10. Beta: on load ignore/drop leftover Aggregate highway components
11. Remove unused fields from the Aggregate highway structs — delete the component types or strip every field (highwayDataId; startDistanceOverrideKm; reverseCounting; DirtyHwId). Do not keep empty obsolete field slots for those values.
12. On load: ignore previously saved Aggregate highway data — drop/skip leftover components; do not read or convert old serialized highwayDataId / cache into live state. Beta OK.

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Highway registry UI becomes orphaned from world aggregates | medium | Keep registry settings UI and APIs; document orphaned until reattach epic |
| Old saves with Aggregate highway data break mileage/route until later epic | medium | Explicit load drop/ignore of leftover Aggregate highway components; beta OK |

---

## Related Tasks

### Depends on



### Is dependent for



## Other Notes

Supersedes thin-contract path. Keep registry for later highway-reattach-via-tool epic.
