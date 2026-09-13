# [0076] HW-01-ADRNameAggregateHighwayData-component-lifecycle

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/20-highway-reattach-on-adr-aggregates.md §C
RefsLibrary/2026090907_custom-road-name-aggregation/21-highway-reattach-roadmap.md HW-01
Archive ACT-01 [0061] / ACT-02 [0062] for former field map

## User Story

> Acting as **a Cities: Skylines 2 mod developer**, I want **authored highway fields hosted on ADR name-aggregates as a serializable component**, so that I **route/mileage/direction can live on ADR without vanilla Aggregate or marker identity**.

---

## Background

First task of epic adr-hw-reattach (single-sprint catalog in RefsLibrary 21). Rehost former Aggregate ADRHighwayAggregationData + marker route/mileage/direction as one authored component on ADRNameAggregate. No mileage cache / CalculateHighwayCacheData / AdrHighwayRoutes2BSystem. Defaults mean inactive / not a highway. Attach on ADR aggregate create; destroy with empty ADR teardown.

---

## Definition of Ready (DoR)

- [ ] ACT-03a…ACT-04 done (ADR name-aggregate entities + serialize exist)
- [ ] Prefer ACT-05/ACT-06 done so membership create/destroy path is stable
- [ ] RefsLibrary 20/21 HW-01 agreed; no-cache decision locked
- [ ] Former field map from ACT-01/02 strip available

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] New ADRNameAggregateHighwayData on ADRNameAggregate entities with fields: highwayDataId (Hash128), routeDirection (enum), mileageOverrideActive (bool), mileageOverrideKm (float), reverseCounting (bool)
- [ ] ISerializable with version discipline; registered like other ADR components
- [ ] Component created with ADR aggregate (defaults: highwayDataId default, override inactive, reverse false, direction NONE or equivalent)
- [ ] Component destroyed when ADR aggregate is torn down
- [ ] No ADRHighwayAggregationCacheData, DirtyHwId-on-Aggregate, CalculateHighwayCacheData, or AdrHighwayRoutes2BSystem revived
- [ ] mileageOverrideKm semantics documented in code/comments: chain-start baseline when override active (former cache startDistanceOverrideKm)
- [ ] RouteDirection enum restored with former marker values (NONE/cardinals/INTERNAL/EXTERNAL) for WE compatibility
- [ ] Compile succeeds after user builds solution

---

## Implementation Notes

1. Add BelzontAdr/Components/ADRNameAggregateHighwayData.cs (name locked in 20)
2. Wire create in AdrNameAggregateMembershipSystem (or equivalent) alongside ADRNameAggregate
3. Teardown path must remove highway component with empty ADR destroy
4. Prefer always-present component with defaults over lazy-add — pick one and stick
5. Do not put highway fields on vanilla Aggregate or ADRHighwayMarkerData
6. Do not serialize a separate cache struct
7. Reference deleted types from git history e65d6fa^ for Aggregate Data/Cache shapes; d3eae02^ for marker RouteDirection/mileage

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Lazy vs always-on component causes SIP/tool null checks to diverge | medium | Lock always-present defaults in HW-01; HW-02/03 assume component exists on ADR |
| Membership teardown forgets highway component | low | DoD teardown case; mirror ADRRandomizationData lifecycle tests |

---

## Related Tasks

### Depends on

- [0063] ACT-03a-ADR-name-aggregate-components-serialize
- [0066] ACT-04-Prefer-ADR-in-name-address-SIP

### Is dependent for

- [0077] HW-02-SIP-backend-isAdrAggregate-highway-bindings
- [0080] HW-04-Naming-highway-override-branch
- [0081] HW-05-Mileage-addressing-authored-fields
- [0082] HW-06-WE-formula-restore-ADR-resolve
- [0083] HW-07-Regions-map-ADR-highwayDataId
- [0084] HW-08-Smoke-acceptance-checklist
### Is related to

- [0061] ACT-01-Strip-Aggregate-highway-host
- [0062] ACT-02-Markers-visual-only-WE-placeholders

### Groups with

- HW-02-SIP-backend-isAdrAggregate-highway-bindings
- HW-03-VUIO-Addresses-SIP-highway-section
- HW-03b-Road-Grouping-tool-options-parity
- HW-04-Naming-highway-override-branch
- HW-05-Mileage-addressing-authored-fields
- HW-06-WE-formula-restore-ADR-resolve
- HW-07-Regions-map-ADR-highwayDataId
- HW-08-Smoke-acceptance-checklist

---

## Other Notes

Epic adr-hw-reattach. Size M. Order 1. Single sprint with HW-01…HW-08. Board IDs start 0076. Status N = ready for sprint after review.
