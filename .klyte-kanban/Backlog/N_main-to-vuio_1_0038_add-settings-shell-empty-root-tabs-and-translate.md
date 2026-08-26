# [0038] add-settings-shell-empty-root-tabs-and-translate

**Developed by:** 

## Reference

RefsLibrary/2026082521_main-to-vuio-migration/11-sprint-roadmap.md — S1 exit criteria

## User Story

> Acting as **a developer**, I want **an empty four-tab ADR settings shell with correct i18n namespace**, so that I **subsequent sprints can drop feature bodies into a working shell**.

---

## Background

Sprint 1 (Foundations). Complete empty settings shell: four root tabs, city gate, settings translate helper.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] ListWithContentTab nav with 4 root tabs (namesets, overrides, roadPrefix, region) and placeholder bodies
- [ ] City-loaded gate message matches main behavior
- [ ] settings/translate.ts resolves K45::ADR.main[...] (not vuio namespace)
- [ ] Panel closes when another GamePanel opens
- [ ] EUIS main still in repo

---

## Implementation Notes

1. Add AdrSettingsNav or inline ListWithContentTab in AdrSettingsPanel
2. Port city gate from root.component.tsx
3. Add settings/translate.ts per 10-i18n-and-assets.md

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- 0037
### Is dependent for

---
