**End time:** 2026-09-11 14:46 -0300
**Start time:** 2026-09-11 14:38 -0300
# [0065] ACT-03c-ForkFromVanilla-ADR-seeding

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/12-activation-change-map.md §A3
RefsLibrary/2026090907_custom-road-name-aggregation/04-parallel-system-design-notes.md
RefsLibrary/2026090907_custom-road-name-aggregation/13-activation-sprint-roadmap.md ACT-03c

## User Story

> Acting as **a Cities: Skylines 2 player editing road groups**, I want **first membership edit to fork an ADR name-aggregate with copied seed and full membership buffer**, so that I **custom road groups can diverge from vanilla Aggregate naming without fighting AggregateSystem**.

---

## Background

Workstream 3c (split from former ACT-03). First membership edit forks from vanilla: copy seed S to ADR, Redraw vanilla Aggregate to S2, full AggregateElement copy into ADR buffer then apply the edit. AdrNamesetSystem must seed new ADR aggregates the same way it seeds vanilla Aggregates.

---

## Definition of Ready (DoR)

- [ ] ACT-03b membership CRUD available
- [ ] Locked decisions in 12: fork on first membership change; seed copy + Redraw; full buffer copy then edit

---

## Acceptance Criteria / Definition of Done (DoD)

- [x] ForkFromVanilla copies vanilla ADRRandomizationData seed S onto the new ADR aggregate
- [x] ForkFromVanilla calls vanilla Aggregate Redraw() so leftover vanilla members get new seed S2
- [x] ForkFromVanilla initializes ADRNameAggregateElement as a full copy of vanilla AggregateElement then applies the first add/remove edit
- [x] ADRNameAggregated is set on ADR member edges after fork
- [x] AdrNamesetSystem seeds ADRRandomizationData on new ADR aggregates (not only vanilla Aggregate)
- [x] Plain select does not fork; only first membership change triggers fork (tool wiring may land in ACT-05 but API supports it)
- [x] Vanilla Aggregate membership list is not mutated by fork (seed redraw only)

---

## Implementation Notes

1. Implement ForkFromVanilla on membership service
2. ADR seed = vanilla.ADRRandomizationData (S)
3. vanilla Aggregate.Redraw() → S2 for non-ADR leftover edges
4. ADRNameAggregateElement = copy all AggregateElement.m_Edge (+ defaults for flags) then apply first edit
5. Set ADRNameAggregated on members with link + default flags
6. AdrNamesetSystem: detect/create path for ADRNameAggregate entities and seed like Aggregate
7. ACT-05 will call fork on Ctrl+LMB apply / endpoint-drag release

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Redraw timing leaves ADR and vanilla sharing the same seed | medium | Assert seed S on ADR and S2 on vanilla after fork in unit/playtest checklist |
| Partial buffer init drops members and breaks numbering walks | medium | Copy full buffer before edit; do not start from empty ADR buffer |

---

## Related Tasks

### Depends on

- [0064] ACT-03b-Membership-service-CRUD

### Is dependent for



### Groups with

- ACT-03a
- ACT-03b
- [0063] ACT-03a-ADR-name-aggregate-components-serialize
- [0064] ACT-03b-Membership-service-CRUD

---

## Other Notes

Third of three ACT-03 split tasks. Naming preference for ADR vs S2 is ACT-04.
