# [0046] port-road-prefix-rules-editor

**Developed by:** 

## Reference

RefsLibrary/2026082521_main-to-vuio-migration/07-tab-by-tab-migration.md — M3

## User Story

> Acting as **a developer**, I want **road prefix rules editor ported with multistate checkboxes and XML file picker**, so that I **road prefix rules are editable from VUIO with correct tri-state flag UX**.

---

## Background

Sprint 4 (Road prefix). Port RoadPrefixCmp with multistate Checkbox and XML defaults picker.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] RoadPrefixCmp ported under settings/tabs/roadPrefix/
- [ ] Tri-state flags use game Checkbox multistate (null↔undefined mapped if needed)
- [ ] Defaults save/load uses FilePickerDialog for disk .xml paths
- [ ] roadPrefixRuleEditor.scss converted to CSS module
- [ ] Reorder, flags, save/load defaults work from VUIO panel

---

## Implementation Notes

1. Port RoadPrefixCmp.tsx
2. Wire multistate Checkbox from vuio-commons
3. Convert roadPrefixRuleEditor.scss to CSS module

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- 0045
- 0041
### Is dependent for

---
