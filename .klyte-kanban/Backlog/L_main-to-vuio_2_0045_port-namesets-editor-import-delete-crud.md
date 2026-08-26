# [0045] port-namesets-editor-import-delete-crud

**Developed by:** 

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

- [ ] NamesetImportingCmp, NamesetEditorCmp, NamesetViewingCmp, NamesetDeletingCmp ported
- [ ] String prompts use StringInputDialog / vuio dialogs
- [ ] Full nameset CRUD + export works from VUIO
- [ ] No euis-components imports in namesets folder
- [ ] Listener cleanup on every screen

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



### Is dependent for


