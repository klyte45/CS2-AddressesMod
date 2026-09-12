**Start time:** 2026-09-12 00:34 -0300
# [0068] ACT-06-Membership-maintenance-dogfood

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/07-membership-maintenance-edge-cases.md
RefsLibrary/2026090907_custom-road-name-aggregation/12-activation-change-map.md §E
RefsLibrary/2026090907_custom-road-name-aggregation/13-activation-sprint-roadmap.md ACT-06

## User Story

> Acting as **a Cities: Skylines 2 player with ADR road groups**, I want **net replace/delete to remap ADR membership and gap-fill contiguous spans**, so that I **custom road groups survive net edits without manual re-association of every replaced edge**.

---

## Background

Workstream 6. After tool writes ADR membership, net replace/delete/create must remap ADRNameAggregated / ADRNameAggregateElement and gap-fill between propagating endpoints per 07. Dogfood via Road Grouping tool (not load-time repair).

---

## Definition of Ready (DoR)

- [ ] ACT-05 tool writes ADR membership
- [ ] RefsLibrary 07 edge-case matrix settled (including preventPropagation and no load repair)

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] On applied net change: build old→new edge map from Temp/Deleted/Created; remap ADRNameAggregated; rewrite ADRNameAggregateElement copying preventPropagation + numberOverrideActive + numberOverride
- [ ] Drop deleted members with no successor then run gap-fill
- [ ] Gap-fill adds interstitial edges G between ADR members A and B on the same vanilla AggregateElement order only when both A and B have preventPropagation == false
- [ ] No gap-fill across different vanilla Aggregates
- [ ] No gap-fill on load/deserialize — trust saved membership
- [ ] Empty ADR aggregates torn down after maintenance
- [ ] Dogfood: exercise remap + gap-fill cases via Road Grouping tool after net edits (at least A2/A3/B1/B3/H1/H2 from 07 matrix)
- [ ] Dirty names/tool overlays after maintenance

---

## Implementation Notes

1. New membership maintenance system following 07 pipeline steps 1-6
2. Remap copies flags from original membership to successor
3. Gap-fill defaults for new edges: preventPropagation=false, numberOverrideActive=false
4. Player remove must not immediately gap-fill the removed edge back (H2)
5. Temp preview only: no permanent ADR writes (F1)
6. Highway-on-ADR notes in 07 G* are later-epic; ignore Aggregate highway

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Net replace leaves orphan ADR links or over-aggressive gap-fill joins islands | high | Implement against 07 matrix; dogfood A7 default-to-gap-fill; never gap-fill on load |
| preventPropagation misunderstood and holes heal unexpectedly | medium | Gap-fill only when both endpoints preventPropagation==false; document island UX |

---

## Related Tasks

### Depends on

- [0067] ACT-05-Road-Grouping-ADR-mutations

### Is dependent for



## Other Notes

Depends on ACT-05. Completes the activation sprint.
