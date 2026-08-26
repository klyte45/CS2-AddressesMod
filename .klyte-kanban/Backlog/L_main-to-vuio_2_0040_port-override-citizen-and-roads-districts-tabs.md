# [0040] port-override-citizen-and-roads-districts-tabs

**Developed by:** 

## Reference

RefsLibrary/2026082521_main-to-vuio-migration/07-tab-by-tab-migration.md — M1

## User Story

> Acting as **a developer**, I want **citizen and roads/districts override tabs ported to VUIO**, so that I **citizen and district override settings work from the VUIO panel**.

---

## Background

Sprint 2 (Overrides). Port citizen and roads/districts override tabs.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] OverrideCitizenTab ported with selects, checkboxes, number inputs
- [ ] OverrideRoadsDistrictsTab ported with per-district list
- [ ] Values persist via existing k45::adr binders
- [ ] Nameset dropdowns refresh on city nameset change
- [ ] No euis-components imports in overrides folder

---

## Implementation Notes

1. Port OverrideCitizenTab.tsx and OverrideRoadsDistrictsTab.tsx
2. Use vuio-commons FormLine and vanilla selects
3. Keep adr-commons service calls unchanged

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- 0039
### Is dependent for

---
