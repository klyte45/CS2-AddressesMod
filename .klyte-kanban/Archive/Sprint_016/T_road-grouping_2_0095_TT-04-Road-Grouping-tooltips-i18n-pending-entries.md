**End time:** 2026-09-15 00:41 -0300
**Start time:** 2026-09-15 00:39 -0300
# [0095] TT-04-Road-Grouping-tooltips-i18n-pending-entries

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/25-road-grouping-tooltips.md TT-04; .cursor/rules/i18n-entries.mdc

## User Story

> Acting as **a mod developer**, I want **pending i18n entries for Road Grouping tooltip strings**, so that I **so tooltips localize without touching CSV merge workflow**.

---

## Background

New hint and drag-unreachable strings must ship via pending entries file only. Agents never edit i18n.csv. Spec key table in 25-road-grouping-tooltips.md.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] BelzontAdr/i18n/roadGrouping_tooltips_entries.txt exists with 7-column rows for all new keys listed in doc 25
- [x] en-US and pt-BR mock copy present
- [x] No edits to i18n.csv / locale CSVs by this task
- [x] No translate() fallbacks for new keys

---

## Implementation Notes

1. Keys: reject.dragUnreachable; hint.select; hint.toggleAdd; hint.toggleRemove; hint.dragStart; hint.dragApply; hint.cancelSelection; hint.exitTool; hint.cancelDrag
2. Reuse existing kindMismatch / kindUnclassified — do not duplicate
3. Created BelzontAdr/i18n/roadGrouping_tooltips_entries.txt with 9 rows (7-col)
4. Reused existing kindMismatch / kindUnclassified — not duplicated

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


