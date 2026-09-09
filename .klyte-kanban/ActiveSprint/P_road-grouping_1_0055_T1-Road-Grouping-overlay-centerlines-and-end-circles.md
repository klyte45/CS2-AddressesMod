**Start time:** 2026-09-09 19:02 -0300
# [0055] T1 Road Grouping overlay centerlines and end circles

**Developed by:** Composer (composer@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/10-road-grouping-overlay-render-system.md
RefsLibrary/2026090907_custom-road-name-aggregation/11-road-grouping-tool-roadmap.md

## User Story

> Acting as **a Cities player editing road names**, I want **opaque green lines and end circles on the selected aggregation**, so that I **so I can see which segments belong to the focused Aggregate**.

---

## Background

Phase T1. Draw selected Aggregate membership with OverlayRenderSystem.Buffer — not Highlighted. Colors: belonging #00FF00, non-belonging remainder #006600, terminal end circles #00FF00.

---

## Definition of Ready (DoR)

- [ ] T0 scaffold complete (tool + selectedAggregation)
- [ ] 10-road-grouping-overlay-render-system.md available

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] AdrRoadGroupingOverlayRenderSystem runs in Rendering before OverlayRenderSystem
- [ ] When tool active and selectedAggregation set: opaque #00FF00 centerlines on AggregateElement edges
- [ ] Terminal end-nodes get #00FF00 DrawCircle
- [ ] Inactive tool or null selection draws nothing
- [ ] No Highlighted used for membership layer

---

## Implementation Notes

1. Pattern AreaBorderRenderSystem / GuideLinesSystem
2. Iterate AggregateElement -> Curve.m_Bezier DrawCurve fill=outline alpha 1
3. Resolve terminals via degree-1 nodes within Aggregate edge set
4. Starting width ~2-4 m; tune in playtest
5. #006600 only when remainder set exists (may be unused until subset membership)
6. AdrRoadGroupingOverlayRenderSystem draws #00FF00 centerlines + degree-1 end circles from AggregateElement; gated on active Road Grouping tool + selection. Awaiting in-game validation before closing.

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Overlay phase ordering or opaque style flags wrong | medium | Spike DrawCurve/DrawCircle early; keep scope to selected Aggregate only |

---

## Related Tasks

### Depends on

- [0054] T0 Road Grouping tool scaffold (WE-style)

### Is dependent for


