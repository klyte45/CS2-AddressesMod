# [0058] T4 Road Grouping rewire selection/overlay to ADR thin contract

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/04-parallel-system-design-notes.md
RefsLibrary/2026090907_custom-road-name-aggregation/11-road-grouping-tool-roadmap.md

## User Story

> Acting as **a mod developer**, I want **the tool and overlay driven by ADR membership**, so that I **so Road Grouping edits real ADR road groups instead of only vanilla Aggregates**.

---

## Background

Phase T4 (deferred). Swap Road Grouping selection/overlay data source from vanilla Aggregate to ADR thin contract (ADRNameAggregate / membership buffers). Keep same tool/controller field selectedAggregation. Requires thin contract epic first.

---

## Definition of Ready (DoR)

- [ ] ADR thin contract types + serialize + member APIs exist (04 parallel design)
- [ ] T0-T1 tool and overlay surface stable

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] selectedAggregation can hold ADR road-group entity
- [ ] Overlay membership from ADRNameAggregateElement (or equivalent)
- [ ] #00FF00 belonging vs #006600 remainder meaningful for ADR subsets
- [ ] Tool gestures unchanged at surface level

---

## Implementation Notes

1. Do not start until thin contract ships
2. Keep toolID and VUIO surface stable
3. Related: thin-contract tasks should be isDependentFor / dependsOn when created

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Starting T4 before thin contract forces throwaway membership formats | high | Keep T0-T3 on vanilla Aggregate; gate this task on thin contract DoD |

---

## Related Tasks

### Depends on

- [0054] T0 Road Grouping tool scaffold (WE-style)
- [0055] T1 Road Grouping overlay centerlines and end circles

### Is dependent for



## Other Notes

Deferred (status L) and lower priority until thin contract is ready.
