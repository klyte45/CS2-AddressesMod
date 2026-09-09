# [0056] T2 Road Grouping Ctrl+LMB toggle and endpoint drag log stubs

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/09-road-grouping-tool-spec.md
RefsLibrary/2026090907_custom-road-name-aggregation/11-road-grouping-tool-roadmap.md

## User Story

> Acting as **a mod developer**, I want **stub logs for toggle and endpoint expand/shrink**, so that I **so we can dogfood gestures before real membership APIs exist**.

---

## Background

Phase T2. Log-only membership stubs: Ctrl+LMB on segment toggles; LMB-drag on terminal end-node walks path preferring same Aggregate then hops. No real membership writes.

---

## Definition of Ready (DoR)

- [ ] T0 and T1 done (selection + end circles for hit targets)
- [ ] 09 gesture and path rules locked

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Ctrl+LMB on hovered segment while selected emits Log.Info toggle stub with edge and aggregate ids
- [ ] LMB press/drag starts only on terminal end-nodes (end-circle hit)
- [ ] Path prefers same vanilla Aggregate as prior step; hops to adjacent Aggregate when none share
- [ ] One info log per distinct segment that would be added or removed
- [ ] Plain LMB does not reselect while group focused; RMB cancel clears

---

## Implementation Notes

1. Disambiguate apply/LMB by hit target: Ctrl+segment vs end-node
2. Do not mutate AggregateElement or ADR membership yet
3. Optional: skip overlay drag preview; logs only

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| End-circle vs edge hit-testing fights or path at junctions is ambiguous | medium | Document radii in tool; playtest junction hops |

---

## Related Tasks

### Depends on

- [0054] T0 Road Grouping tool scaffold (WE-style)
- [0055] T1 Road Grouping overlay centerlines and end circles

### Is dependent for


