**Start time:** 2026-09-11 14:36 -0300
# [0064] ACT-03b-Membership-service-CRUD

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/12-activation-change-map.md §A2
RefsLibrary/2026090907_custom-road-name-aggregation/04-parallel-system-design-notes.md
RefsLibrary/2026090907_custom-road-name-aggregation/13-activation-sprint-roadmap.md ACT-03b

## User Story

> Acting as **a Cities: Skylines 2 mod developer**, I want **create/destroy/add/remove/reorder/flag/teardown APIs for ADR name-aggregate membership**, so that I **fork and Road Grouping can call a single ownership API instead of scattering ECS writes**.

---

## Background

Workstream 3b (split from former ACT-03). Membership service operates on ACT-03a components: create/destroy ADR aggregates, mutate ordered buffer, keep edge links in sync, tear down empty. Does not implement ForkFromVanilla or AdrNamesetSystem ADR seeding.

---

## Definition of Ready (DoR)

- [ ] ACT-03a components registered and serializable
- [ ] RefsLibrary 12 §A2 and 04 membership lifecycle agreed

---

## Acceptance Criteria / Definition of Done (DoD)

- [x] API/system can create an ADR name-aggregate entity with ADRNameAggregate + empty/populated ADRNameAggregateElement
- [x] Can destroy ADR aggregate and clear related ADRNameAggregated on edges
- [x] Can add/remove/reorder members in ADRNameAggregateElement
- [x] Can set per-member flags: preventPropagation, numberOverrideActive, numberOverride
- [x] ADRNameAggregated on edges stays consistent with buffer membership and mirrored flags
- [x] Empty aggregates are torn down (no orphan empty ADR entities)
- [x] Vanilla Aggregated / AggregateElement are never mutated by this service

---

## Implementation Notes

1. New membership service/system owning ADR aggregate lifecycle
2. create: spawn entity with ADRNameAggregate (+ ADRRandomizationData later via 03c); buffer as needed
3. destroy: clear ADRNameAggregated on members; destroy entity
4. add/remove/reorder: rewrite ADRNameAggregateElement; sync ADRNameAggregated
5. set flags: update buffer entry and mirrored edge fields
6. teardown empty after last remove
7. Do not mutate vanilla Aggregate membership
8. ForkFromVanilla stays out — ACT-03c

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Buffer vs ADRNameAggregated desync | medium | Single service owns both buffer and edge link writes; assert sync in DoD tests |

---

## Related Tasks

### Depends on

- [0063] ACT-03a-ADR-name-aggregate-components-serialize

### Is dependent for



### Groups with

- ACT-03a
- ACT-03c
- [0063] ACT-03a-ADR-name-aggregate-components-serialize
- [0065] ACT-03c-ForkFromVanilla-ADR-seeding

---

## Other Notes

Second of three ACT-03 split tasks.
