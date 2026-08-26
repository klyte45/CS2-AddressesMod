**End time:** 2026-08-26 01:41 -0300
# [0045] port-namesets-editor-import-delete-crud

**Developed by:** Composer (cursor-agent@klyte.local)
## Reference

RefsLibrary/2026082521_main-to-vuio-migration/07-tab-by-tab-migration.md — M2

## User Story

> Acting as **a developer**, I want **nameset editor, import, view, and delete flows ported to VUIO**, so that I **players can manage namesets entirely from the VUIO settings panel**.

---

## Background

Sprint 3 (Namesets). Port editor, import, view, delete flows.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] NamesetImportingCmp, NamesetEditorCmp, NamesetViewingCmp, NamesetDeletingCmp ported
- [x] String prompts use StringInputDialog / vuio dialogs
- [x] Full nameset CRUD + export works from VUIO
- [x] No euis-components imports in namesets folder
- [x] Listener cleanup on every screen

---

## Implementation Notes

1. Port fileManagement screen components to settings/tabs/namesets/
2. Replace EUIS prompts with vuio-commons dialogs
3. Use settings/translate.ts for K45::ADR.main keys

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- 0044
### Is dependent for

---
