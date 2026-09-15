**Start time:** 2026-09-15 00:46 -0300
# [0097] TT-06-Road-Grouping-tooltips-playtest-checklist

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/25-road-grouping-tooltips.md TT-06

## User Story

> Acting as **a mod developer**, I want **a playtest checklist covering Road Grouping tooltip errors and hints**, so that I **so the tooltip polish can ship with confidence**.

---

## Background

Verify locked tooltip UX from doc 25 after TT-02/03/05 land. Extend playtest notes as needed (see also 27-adr-net-kind-playtest-smoke-notes.md).

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Hover kind reject shows Error tooltip; no PSI
- [ ] Ctrl+toggle soft-fail shows Error tooltip; no PSI; logs remain
- [ ] Invalid endpoint drag shows Error tooltip; valid drag shows Apply/Cancel hints
- [ ] Mode matrix hints verified for no-selection, selected, and dragging
- [ ] Esc/RMB cancel behavior unchanged

---

## Implementation Notes

1. Manual in-game checklist against 25-road-grouping-tooltips.md success criteria
2. Optionally append results under RefsLibrary playtest notes if useful

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- [0093] TT-02-Road-Grouping-Error-StringTooltip-system
- [0094] TT-03-Road-Grouping-InputHints-toolActions-DisplayNameOverride
- [0096] TT-05-Road-Grouping-remove-PSI-kind-reject-pops

### Is dependent for


