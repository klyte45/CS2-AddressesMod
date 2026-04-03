# [0007] fix-thread-safety-name-files-singleton

**Developed by:** 

## User Story

> Acting as **a developer**, I want **the AdrNameFilesManager singleton to be safely initialized in concurrent contexts**, so that I **prevent multiple instances being created if initialization is triggered from multiple threads**.

---

## Background

AdrNameFilesManager uses `instance ??= new()` which is not thread-safe. If two threads simultaneously evaluate `instance == null` before either sets the field, two instances can be created.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] The singleton initialization is made thread-safe
- [ ] No change in observable behavior for single-threaded startup
- [ ] The fix is verified by code review

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


