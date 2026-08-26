# [0037] register-adr-settings-gamepanel-and-umm

**Developed by:** 

## Reference

RefsLibrary/2026082521_main-to-vuio-migration/03-shell-and-umm.md — S1

## User Story

> Acting as **a developer**, I want **ADR settings registered as a GamePanel with a Universal Mod Menu entry**, so that I **I can open ADR settings from the vanilla Mods menu during development**.

---

## Background

Sprint 1 (Foundations). Register ADR settings on vanilla GamePanel host + Universal Mod Menu. VS/WE pattern; not Editor Portal.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] UniversalModMenu in vuio + vuio-commons AppendHookTargets typings
- [x] GamePanelType extended with AdrSettingsPanelId
- [x] gamePanelComponents maps AdrSettingsPanel
- [x] AdrUniversalModButton calls toggleGamePanel(AdrSettingsPanelId)
- [x] Right-menu Mods glyph appears when ADR appends UMM
- [x] UMM button opens/closes GamePanel (not Portal)

---

## Implementation Notes

1. Extend modding.d.ts for UniversalModMenu
2. Extend GamePanelType + gamePanelComponents in k45_adr_vuio index.tsx
3. Add AdrUniversalModButton.tsx
4. Add AdrSettingsPanel.tsx with centerLeftLayout + Panel at 70vh x 80vw
5. GamePanelType K45_ADR + gamePanelComponents registration in index.tsx
6. AdrUniversalModButton toggles toggleGamePanel(AdrSettingsPanelId)
7. UniversalModMenu in modding.d.ts typings

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- 0036
### Is dependent for

---
