**End time:** 2026-04-03 18:39 -0300
**Start time:** 2026-04-03 18:23 -0300
# [0009] enhance-road-qualifier-rule-conditions

**Developed by:** claude-sonnet-4-6@kwyt.com.br
## User Story

> Acting as **a city builder**, I want **road qualifier rules to support more condition types (lane count, elevation, zone type)**, so that I **differentiate street naming patterns with greater precision beyond basic speed flags**.

---

## Background

The current road qualifier system only supports speed thresholds and a limited set of boolean road characteristic flags. The README notes: 'Very limited options to customization at this moment, might be enhanced later.' ActionPlan_1_RoadQualifierRules.md outlines target condition types. Key file: BelzontAdr/Data/AdrCitywideSettings.cs, AdrRoadPrefixRule.

ATTENTION: This feature shall relay on game code related to roads. So every new condition type added to the rules must be supported by the game code, and the mod should not attempt to add new conditions that require game code changes. If some conditions are not available at the game code but are very easy to be implemented as a new component of the mod, they can be added as a bonus but should not be a requirement for the main goal of this task.

Also, it's important to review the current implementation of the road qualifier rules and how they are evaluated in the mod against the game code related to roads. Some of them may not really work as described in the UI labels, and some of the conditions may be redundant or not useful. So a careful analysis of the current implementation and the game code is required to identify the best way to enhance the road qualifier rules with new condition types.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] At least 3 new condition types are added to AdrRoadPrefixRule (e.g., elevation/bridge, lane count range, has zoning flag enhancements)
- [x] New conditions are serializable and backwards-compatible with existing saves
- [x] The RoadPrefixCmp.tsx UI exposes the new conditions with appropriate input controls
- [x] The GetFirstApplicable method correctly evaluates new conditions in priority order
- [x] Existing road qualifier behavior is unchanged for saves without the new conditions

---

## Implementation Notes



---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


