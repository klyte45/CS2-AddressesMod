**Start time:** 2026-09-15 00:41 -0300
# [0094] TT-03-Road-Grouping-InputHints-toolActions-DisplayNameOverride

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/25-road-grouping-tooltips.md TT-03; Game.UI.Tooltip.InputHintsTooltipSystem

## User Story

> Acting as **a Cities Skylines II player**, I want **InputHint rows for Road Grouping actions based on current mode**, so that I **so available gestures match vanilla tool hint UX with correct keybinds**.

---

## Background

InputHintsTooltipSystem only shows hints for actions on activeTool.actions with active DisplayNameOverride priority != -1. Road Grouping ModData actions are not in toolActions today. Spec: 25-road-grouping-tooltips.md mode matrix.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] DisplayNameOverride labels follow the locked mode matrix in doc 25
- [x] Remapped keybinds display correctly via vanilla InputHintTooltip
- [x] When Error tooltip applies to the hover/drag target, conflicting Apply/Toggle hints are suppressed
- [x] ModData Apply/Cancel/Toggle show vanilla InputHintTooltip rows via DisplayNameOverride (toolActions override blocked by private protected across assemblies)

---

## Implementation Notes

1. Override toolActions on AdrRoadGroupingTool
2. Own DisplayNameOverride instances; update displayName/active each OnUpdate from mode+hover
3. Locale ids from roadGrouping.hint.* (TT-04)
4. Do not invent InputHint for double-click fixed pin
5. ToolBaseSystem.toolActions is private protected — cannot override from BelzontAdr. Emit InputHintTooltip from AdrRoadGroupingTooltipSystem for ModData Apply/Cancel/Toggle with DisplayNameOverride instead (same widgets vanilla InputHints uses).
6. DisplayNameOverride on ModData Apply/Cancel/Toggle; RefreshInputHintOverrides mode matrix
7. AdrRoadGroupingTooltipSystem emits InputHintTooltip for those actions

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- [0092] TT-01-Road-Grouping-tooltip-tool-state-API-reject-hover

### Is dependent for


