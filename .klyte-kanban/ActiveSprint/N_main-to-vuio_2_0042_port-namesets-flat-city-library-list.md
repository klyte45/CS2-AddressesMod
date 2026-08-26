# [0042] port-namesets-flat-city-library-list

**Developed by:** 

## Reference

RefsLibrary/2026082521_main-to-vuio-migration/07-tab-by-tab-migration.md — M2

## User Story

> Acting as **a developer**, I want **city nameset library ported with a flat raw-name list**, so that I **city nameset library matches the simplified flat-list UX decision**.

---

## Background

Sprint 3 (Namesets). Port city nameset library with flat raw-name list — no hierarchy.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] CityNamesetLibraryCmp ported under settings/tabs/namesets/
- [ ] Flat raw-name left list via ListWithContentTab
- [ ] NamesetCategoryCmp / categorizeFiles / hierarchy UI not ported
- [ ] City library shows raw names only
- [ ] NamesetWordsContainer uses EditorScrollable

---

## Implementation Notes

1. Port CityNamesetLibraryCmp and NamesetWordsContainer
2. Drop CategoryCmp and categorizeFiles
3. Convert wordContainer.scss to CSS module

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- 0038
### Is dependent for

---
