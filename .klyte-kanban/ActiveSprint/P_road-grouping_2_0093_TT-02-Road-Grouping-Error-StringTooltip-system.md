**Start time:** 2026-09-15 00:39 -0300
# [0093] TT-02-Road-Grouping-Error-StringTooltip-system

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/25-road-grouping-tooltips.md TT-02; BelzontAdr/Tooltips/TempRoadMarkerTooltip.cs

## User Story

> Acting as **a Cities Skylines II player**, I want **Error mouse tooltips when a Road Grouping join or drag is rejected**, so that I **so soft-fails are explained at the cursor without PSI spam**.

---

## Background

Vanilla Error/Warning tooltips use TooltipSystemBase + StringTooltip at UITooltip phase. ADR already has TempRoadMarkerTooltip as template. Spec: 25-road-grouping-tooltips.md.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] AdrRoadGroupingTooltipSystem registered at SystemUpdatePhase.UITooltip
- [ ] While Road Grouping is active, kind mismatch / unclassified / drag unreachable show TooltipColor.Error StringTooltip
- [ ] Tooltips gate on activeTool == AdrRoadGroupingTool and use TT-01 public reject state
- [ ] No PSI required for these rejects (removal is TT-05)

---

## Implementation Notes

1. New BelzontAdr/Tooltips/AdrRoadGroupingTooltipSystem.cs extending TooltipSystemBase
2. Reuse kindMismatch / kindUnclassified keys; consume dragUnreachable from TT-04 when present
3. Register beside TempRoadMarkerTooltip in AddressesCs2Mod
4. Suppress conflicting Apply/Toggle hints is TT-03; this task focuses on Error StringTooltips

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- [0092] TT-01-Road-Grouping-tooltip-tool-state-API-reject-hover

### Is dependent for


