**End time:** 2026-04-03 18:57 -0300
**Start time:** 2026-04-03 18:57 -0300
# [0007] fix-thread-safety-name-files-singleton

**Developed by:** claude-sonnet-4-6@kwyt.com.br
## User Story

> Acting as **a developer**, I want **the AdrNameFilesManager singleton to be safely initialized in concurrent contexts**, so that I **prevent multiple instances being created if initialization is triggered from multiple threads**.

---

## Background

AdrNameFilesManager uses `instance ??= new()` which is not thread-safe. If two threads simultaneously evaluate `instance == null` before either sets the field, two instances can be created.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] The singleton initialization is made thread-safe
- [x] No change in observable behavior for single-threaded startup
- [x] The fix is verified by code review

---

## Implementation Notes

1. Replace the lazy field with Lazy<AdrNameFilesManager> or use a static readonly field initialized at declaration time since the class has no setup dependency

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


