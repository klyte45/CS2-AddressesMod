**Start time:** 2026-09-10 23:25 -0300
# [0061] ACT-01-Strip-Aggregate-highway-host

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/12-activation-change-map.md §B
RefsLibrary/2026090907_custom-road-name-aggregation/13-activation-sprint-roadmap.md ACT-01

## User Story

> Acting as **a Cities: Skylines 2 mod developer**, I want **Aggregate-hosted ADR highway Data/Dirty/Cache removed while the highway registry remains**, so that I **ADR can own name aggregation without Aggregate-hosted highway state blocking the fork model**.

---

## Background

Workstream 1 of ADR aggregation activation. Highway route/mileage must leave vanilla Aggregate so ADR name-aggregates can own membership without Aggregate-hosted highway state. Remove the Aggregate highway component fields from the structs (prefer deleting ADRHighwayAggregationData / DirtyHwId / CacheData types). Previously saved Aggregate highway component bytes are ignored on load — no migrate/convert of old highwayDataId or cache values. Also remove frontend/VUIO fields and bindings that only exist for Aggregate-hosted highway Data/Cache when those backend fields go away (keep HighwayRegisterManagement registry UI). Marker route/mileage UI strip remains ACT-02.

---

## Definition of Ready (DoR)

- [ ] Road Grouping T0-T3 shipped
- [ ] RefsLibrary 12 §B and 13 ACT-01 catalog agreed
- [ ] Decision locked: remove Aggregate highway host now (not migrate-in-place)

---

## Acceptance Criteria / Definition of Done (DoD)

- [x] AdrHighwayRoutesSystem Aggregate-touching jobs removed (NewAggregateFiller, ReplicateAggregateSetting, CalculateHighwayCacheData, AggregationCacheEraser, InfoPanel Aggregate writes); highwaysDataRegistry CRUD + serialize kept
- [x] AdrHighwayRoutes2BSystem removed or gutted; AddressesCs2Mod registration updated if emptied
- [x] Consumers no longer read Aggregate highway Data/Cache: AdrNameSystemOverrides highway branch, BuildingUtilsOverrides, AdrRoadAddressNumbering, AdrRegionsSystem highwayId, Road Grouping overlay/controller numbering
- [x] HighwayRegisterManagement.tsx + registry APIs still compile and run (orphaned from Aggregate until later reattach epic)
- [x] Unused Aggregate highway fields are removed from the structs (ADRHighwayAggregationData.highwayDataId; CacheData.startDistanceOverrideKm and reverseCounting; DirtyHwId tag gone) — not left as dead serialized members
- [x] Previously saved Aggregate highway component data is ignored on load (no migration of old highwayDataId / cache values into new state)
- [x] Frontend/VUIO: remove fields, DTO properties, and bindings that only exist for Aggregate-hosted highway Data/Cache (e.g. Aggregate-linked highwayId / cache-driven UI) when those backend fields are gone — do not leave dead TS types or binders
- [x] Keep HighwayRegisterManagement.tsx + registry list APIs; do not strip marker route/mileage UI here (that is ACT-02)

---

## Implementation Notes

1. Delete ADRHighwayAggregationData.highwayDataId (Hash128)
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
13. Audit _Frontends/UI/k45_adr_vuio for Aggregate highway host consumers (RegionEditor highwayId from AdrRegionsSystem Aggregate Data, any binder/DTO exposing highwayDataId or AggregationCache fields). Remove or empty those frontend fields when applicable after backend strip.
14. Do not treat AddressesToolOptions / RoadMarkSettings route-mileage strip as ACT-01 — those are ACT-02 marker visual-only.

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
