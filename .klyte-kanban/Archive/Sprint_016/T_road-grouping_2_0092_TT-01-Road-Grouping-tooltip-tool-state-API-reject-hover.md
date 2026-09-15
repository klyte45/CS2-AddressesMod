**End time:** 2026-09-15 00:39 -0300
**Start time:** 2026-09-15 00:38 -0300
# [0092] TT-01-Road-Grouping-tooltip-tool-state-API-reject-hover

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/25-road-grouping-tooltips.md TT-01; BelzontAdr/Tools/AdrRoadGroupingTool.cs

## User Story

> Acting as **a mod developer**, I want **public Road Grouping hover/drag reject state**, so that I **so Error tooltips and overlays stay consistent**.

---

## Background

Road Grouping soft-fails and hover rejects today leave m_DragRejectReason private and hover kind rejects overlay-only. Tooltip systems need a shared public API. Spec: RefsLibrary 2026090907_custom-road-name-aggregation/25-road-grouping-tooltips.md.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] Public accessors expose current hover join result and drag reject reason without duplicating EvaluateNetKindJoin logic
- [x] Overlay and future tooltip system can read the same reject state
- [x] No player-visible behavior change required beyond exposure / consolidation

---

## Implementation Notes

1. Add public CurrentHoverJoinResult / CurrentDragRejectReason (or equivalent) on AdrRoadGroupingTool
2. Ensure live hover and drag preview paths set the same fields overlay already consumes
3. Keep EvaluateNetKindJoin as single gate

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


