# [0011] optimize-name-file-loading-performance

**Developed by:** 

## User Story

> Acting as **a player with a large name files library**, I want **the mod to load name files quickly without blocking game startup**, so that I **avoid long freeze at startup when many .txt name files are present**.

---

## Background

AdrNameFilesManager.ReloadNameFiles calls File.ReadAllLines synchronously for every .txt in the namesets folder on the main thread. ActionPlan_3 identifies this as a bottleneck. No caching — every reload re-reads and re-parses everything regardless of file changes.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Name file reload no longer blocks the main thread for folders with >50 files
- [ ] Files that haven't changed since last load are skipped (hash or timestamp check)
- [ ] The UI shows a loading state indicator while reload is in progress
- [ ] No regression in name assignment after async reload completes

---

## Implementation Notes

1. Consider computing a quick directory-level hash or using file last-write timestamps to skip unchanged files
2. Explore loading name files on a background thread and merging results, with a loading state indicator in the UI

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


