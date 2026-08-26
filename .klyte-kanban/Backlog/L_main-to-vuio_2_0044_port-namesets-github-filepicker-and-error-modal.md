# [0044] port-namesets-github-filepicker-and-error-modal

**Developed by:** 

## Reference

RefsLibrary/2026082521_main-to-vuio-migration/07-tab-by-tab-migration.md — M2 GitHub

## User Story

> Acting as **a developer**, I want **GitHub nameset browsing migrated to FilePickerDialog with rate-limit error UX**, so that I **GitHub nameset import handles rate limits with a clear recovery path**.

---

## Background

Sprint 3 (Namesets). GitHub browse via FilePickerDialog; rate-limit error modal.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] NamesetGitHubSelectorCmp uses FilePickerDialog + GitHub generateDataProvider
- [ ] GitHub listing/rate-limit failure shows error modal with Open GitHub + Cancel
- [ ] Cancel string added via *_entries.txt if missing (never edit i18n.csv)
- [ ] No NamesetGitHubCategoryCmp / custom GitHub browse UI
- [ ] GitHub import works from VUIO panel

---

## Implementation Notes

1. Port NamesetGitHubSelectorCmp to FilePickerDialog
2. Implement error modal with goToRepository + Cancel
3. Verify XHR from game UI for GitHub provider

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


