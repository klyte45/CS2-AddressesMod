**Start time:** 2026-09-11 14:46 -0300
# [0066] ACT-04-Prefer-ADR-in-name-address-SIP

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/12-activation-change-map.md §D
RefsLibrary/2026090907_custom-road-name-aggregation/04-parallel-system-design-notes.md
RefsLibrary/2026090907_custom-road-name-aggregation/13-activation-sprint-roadmap.md ACT-04

## User Story

> Acting as **a Cities: Skylines 2 player with ADR road groups**, I want **names and addresses to resolve through ADR when an edge is ADR-linked**, so that I **associated edges share one street name and house-number sequence while leftover vanilla edges keep a distinct redrawn name**.

---

## Background

Workstream 4. After fork exists, naming and addressing must prefer ADR when an edge has ADRNameAggregated. Split naming: non-ADR edges on the vanilla Aggregate use redrawn seed S2; ADR-linked members use ADR name/seed. No Aggregate highway cache (removed in ACT-01).

---

## Definition of Ready (DoR)

- [ ] ACT-03c ForkFromVanilla available
- [ ] ACT-01 highway Aggregate name/cache branches already stripped
- [ ] RefsLibrary 12 §D agreed

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] AdrNameSystemOverrides: if ADRNameAggregated present, name the ADR entity; else vanilla Aggregate; FindReferenceRoad walks ADR buffer when linked
- [ ] BuildingUtilsOverrides GetAddress walks ADRNameAggregateElement when linked; road entity returned is ADR aggregate; respects numberOverrideActive/numberOverride; no highway cache
- [ ] AdrRoadAddressNumbering uses same ADR membership resolution for Road Grouping card ranges
- [ ] AdrSelectionInfoPanelSystem targets ADR entity when edge has ADR link (RoadAggregation / seed UI)
- [ ] SeedManagementOptions / AddressesInfoOptionsComponent work when backend returns ADR as naming entity
- [ ] Station / road-options lists prefer ADR aggregates when present
- [ ] Split naming after fork verified: leftover vanilla edges get S2 name; ADR members get ADR name

---

## Implementation Notes

1. Resolution order from 04: ADR link → ADR name/number walk; else vanilla
2. Apply numberOverride when numberOverrideActive on walked member
3. Seed UI should not require frontend-only hacks if backend entity is correct
4. Station road-reference options list ADR when present
5. SIP RoadSection / H1 still out of scope

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Split naming leaves mismatched address road entity vs displayed name | medium | Playtest fork then compare leftover vanilla vs ADR-linked edge names/addresses |

---

## Related Tasks

### Depends on

- [0065] ACT-03c-ForkFromVanilla-ADR-seeding

### Is dependent for



## Other Notes

Depends on ACT-03c. ACT-05 needs this for cards/overlays to show correct numbers.
