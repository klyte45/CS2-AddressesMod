**End time:** 2026-08-25 23:07 -0300
# [0038] add-settings-shell-empty-root-tabs-and-translate

**Developed by:** Composer (cursor-agent@klyte.local)
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

- [x] ListWithContentTab nav with 4 root tabs (namesets, overrides, roadPrefix, region) and placeholder bodies
- [x] City-loaded gate message matches main behavior
- [x] settings/translate.ts resolves K45::ADR.main[...] (not vuio namespace)
- [x] Panel closes when another GamePanel opens
- [x] EUIS main still in repo

---

## Implementation Notes

1. Add AdrSettingsNav or inline ListWithContentTab in AdrSettingsPanel
2. Port city gate from root.component.tsx
3. Add settings/translate.ts per 10-i18n-and-assets.md
4. ListWithContentTab 4 tabs with sprint placeholders
5. settings/translate.ts uses K45::ADR.main[...]
6. City gate via AdrMainService.isCityOrEditorLoaded
7. Panel 70vh x 80vw via AdrSettingsPanel.module.scss

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
