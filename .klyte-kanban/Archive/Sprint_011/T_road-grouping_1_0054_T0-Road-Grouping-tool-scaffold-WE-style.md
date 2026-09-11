**End time:** 2026-09-09 19:02 -0300
**Start time:** 2026-09-09 18:30 -0300
# [0054] T0 Road Grouping tool scaffold (WE-style)

**Developed by:** Composer (composer@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/09-road-grouping-tool-spec.md
RefsLibrary/2026090907_custom-road-name-aggregation/11-road-grouping-tool-roadmap.md
BelzontWE WEWorldPickerTool / WEWorldPickerController / WriteEverywhereToolOptions

## User Story

> Acting as **a mod developer**, I want **a Road Grouping tool activated like Write Everywhere**, so that I **so later overlay and membership gestures have a real activeTool to hang on**.

---

## Background

Phase T0 of the Road Grouping roadmap. ADR has IBelzontToolSystem but no gameplay tool yet. Scaffold after BelzontWE: tool + controller + VUIO tool-options visibility. Data source remains vanilla Aggregate; no overlay draw yet.

---

## Definition of Ready (DoR)

- [ ] 09 and 11 Road Grouping docs accepted
- [ ] WE tool/controller/tool-options paths available as reference

---

## Acceptance Criteria / Definition of Done (DoD)

- [x] AdrRoadGroupingTool exists with toolID K45_ADR_AdrRoadGroupingTool
- [x] Select and RequestDisable set activeTool like WE
- [x] AdrRoadGroupingController has selectedAggregation binding and enableTool call
- [x] InitializeRaycast is TypeMask.Net; hover only Aggregated edges
- [x] Plain LMB with empty selection stores Aggregate Entity; clear on OnStopRunning/RequestDisable
- [x] VUIO extends useToolOptionsVisible and MouseToolOptions for the tool id

---

## Implementation Notes

1. Mirror WEWorldPickerTool lifecycle: Enabled=false until Select; apply LMB cancel RMB ProxyActions
2. Place tool under BelzontAdr/Tools/; controller beside WE pattern
3. Register VUIO extends in k45_adr_vuio index like WE
4. Do not implement overlay or membership logs in this task
5. Scaffold landed: AdrRoadGroupingTool/Controller/UISystem + AdrModData keybinds + VUIO tool options/UMM button. Awaiting in-game validation before closing (Ctrl+Shift+G or UMM road button; LMB select; RMB exit).
6. Validated in-game: Ctrl+Shift+A / UMM activate; LMB select Aggregate; pipette re-pick; RMB exit; WE-style tool options (title body + actions gated on selection).

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| First ADR gameplay ToolBaseSystem hits DebugSystem abstract-tool pitfalls | medium | Commons DebugSystemOverrides already mirrors WE; verify after tool exists |

---

## Related Tasks

### Depends on



### Is dependent for



## Other Notes

Sprint fit: T0 is the foundation for T1-T3 in one focused sprint.
