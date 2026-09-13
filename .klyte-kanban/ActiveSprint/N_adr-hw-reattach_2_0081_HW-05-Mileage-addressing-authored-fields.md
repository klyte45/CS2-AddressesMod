# [0081] HW-05-Mileage-addressing-authored-fields

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/20-highway-reattach-on-adr-aggregates.md §E / Why no cache
RefsLibrary/2026090907_custom-road-name-aggregation/21-highway-reattach-roadmap.md HW-05
BelzontAdr/Utils/AdrRoadAddressNumbering.cs
BelzontAdr/Overrides/BuildingUtilsOverrides.cs
BelzontAdr/Controllers/AdrRoadGroupingController.cs

## User Story

> Acting as **a player using house numbers / mileage along an ADR highway grouping**, I want **mileage override and reverse counting from the ADR aggregate to drive numbering**, so that I **I can set chain-start km without marker-baked cache**.

---

## Background

Sixth task. Plumb AdrRoadAddressNumbering / BuildingUtilsOverrides to read authored mileageOverrideActive / mileageOverrideKm / reverseCounting from ADRNameAggregateHighwayData. Road Grouping number cards inherit via shared helpers. No marker scan / cache rebuild.

---

## Definition of Ready (DoR)

- [ ] HW-01 component fields stable
- [ ] ACT-04 ADR walk for addressing already preferred
- [ ] RefsLibrary chain-start baseline semantics agreed

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] When mileageOverrideActive, numbering uses mileageOverrideKm as chain-start baseline
- [ ] reverseCounting flips mileage/house-number growth orientation along ADR element walk
- [ ] When override inactive, behavior matches post-ACT-01 non-highway ADR numbering
- [ ] Road Grouping segment number cards reflect the same helpers
- [ ] No marker SubObject scan for mileage overrides
- [ ] No ADRHighwayAggregationCacheData
- [ ] Compile + playtest cards + building address on overridden ADR highway

---

## Implementation Notes

1. Former cache startDistanceOverrideKm is now authored mileageOverrideKm — do not subtract marker curve position
2. Keep per-edge numberOverride* for house numbers separate from highway km
3. Orientation may still use registry refStartPoint where applicable — document interaction with reverseCounting

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Semantics mismatch vs old marker newMileage-at-position | high | Document chain-start baseline in UI tooltip/i18n; accept product change from 20 |
| Cards and GetAddress disagree | medium | Single helper path for both |

---

## Related Tasks

### Depends on

- [0076] HW-01-ADRNameAggregateHighwayData-component-lifecycle

### Is dependent for

- [0084] HW-08-Smoke-acceptance-checklist
### Is related to

- [0067] ACT-05-Road-Grouping-ADR-mutations
- [0057] T3-Road-Grouping-segment-number-range-UI-cards

### Groups with

- HW-04-Naming-highway-override-branch

---

## Other Notes

Epic adr-hw-reattach. Size M. Order 6. Status N = ready for sprint after review.
