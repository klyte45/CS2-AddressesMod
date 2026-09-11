**Start time:** 2026-09-10 20:41 -0300
# [0057] T3 Road Grouping segment number-range UI cards

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/09-road-grouping-tool-spec.md
RefsLibrary/2026090907_custom-road-name-aggregation/11-road-grouping-tool-roadmap.md
BelzontAdr/Overrides/BuildingUtilsOverrides.cs

## User Story

> Acting as **a Cities player**, I want **toggleable number cards over segments**, so that I **so I can see house-number ranges along the focused road group**.

---

## Background

Phase T3. Per-segment house-number range cards in React, ranges from C# ADR numbering rules. Frontend cache; refresh on aggregation change or camera move every 8 frames; 120% viewport cull; stable sort.

---

## Definition of Ready (DoR)

- [ ] T0 selection works (can parallel T2)
- [ ] BuildingUtilsOverrides numbering behavior understood

---

## Acceptance Criteria / Definition of Done (DoD)

- [x] C# publishes per-segment number ranges for selected Aggregate using ADR numbering rules
- [x] Tool options toggle showNumberCards
- [x] React world-anchored cards for visible segments
- [x] Refresh on aggregation change immediately
- [x] 120% viewport (10% margin) cull on midpoint projection
- [x] Stable sort by edge/buffer index; frontend cache between refreshes
- [x] Viewport projection refresh every frame while cards shown
- [x] When >10 segments in 120% viewport, half-km/mi mileage markers (locale decimals) instead of per-segment ranges
- [x] Cards offset above/below midpoint with connector line + anchor circle; non-overlapping pack

---

## Implementation Notes

1. Not Overlay DrawText — React cards
2. Cache segment list + projected coords on frontend
3. Invalidate on LMB select/cancel and later membership edits
4. BindingsReleased resets m_initialized so MultiUIValueBinding re-registers after UI reload
5. Mileage mode threshold is visible-segment count in 120% viewport, not total aggregate length

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Card spam or projection cost on long Aggregates | medium | Throttle + cache; stable keys; start with belonging segments only |

---

## Related Tasks

### Depends on

- [0054] T0 Road Grouping tool scaffold (WE-style)

### Is dependent for


