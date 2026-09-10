**End time:** 2026-09-10 20:37 -0300
**Start time:** 2026-09-09 21:08 -0300
# [0056] T2 Road Grouping Ctrl+LMB toggle and endpoint drag log stubs

**Developed by:** Composer (composer@kwyt.com.br)
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

- [x] Ctrl+LMB on hovered segment while selected emits Log.Info toggle stub with edge and aggregate ids
- [x] LMB press/drag starts only on terminal end-nodes (end-circle hit)
- [x] Path prefers same vanilla Aggregate as prior step; hops to adjacent Aggregate when none share
- [x] One info log per distinct segment that would be added or removed
- [x] Plain LMB does not reselect while group focused; RMB cancel clears

---

## Implementation Notes

1. Disambiguate apply/LMB by hit target: Ctrl+segment vs end-node
2. Do not mutate AggregateElement or ADR membership yet
3. Optional: skip overlay drag preview; logs only
4. Ctrl+LMB on Edge while selected → Log.Info toggle stub (add/remove vs AggregateElement).
5. LMB on end-handle circle (chain termini + degree-1; radius = EndHandleDiameter/2) starts endpoint drag; 0-1 BFS prefers prior-step Aggregate then hops; on release one info log per distinct would-add or would-remove edge.
6. Overlay exposes EndHandleDiameter + CollectDragHandleNodes for hit testing.
7. Overlay dual layer: while selected, #006600 wash on hovered non-selected Aggregate; live #006600 path ribbons during endpoint drag; brighter/larger end-handle circle on HoveredDragHandle. Separate belonging/preview materials+buffers so deferred draws do not clobber.
8. Mixed endpoint-drag paths classify member edges as would-remove and non-members as would-add; remove ribbons #FF8800, add #00BB00.
9. Double-click member segment pins fixed membership (#FFFF00 + mid square); BFS cannot traverse the pin; at most one pin, cleared with selection.
10. No dedicated FixMembership ProxyAction — double-click timing on Apply.

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


