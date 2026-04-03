**End time:** 2026-04-03 18:44 -0300
**Start time:** 2026-04-03 18:44 -0300
# [0004] fix-settimeout-race-condition-region-save

**Developed by:** claude-sonnet-4-6@kwyt.com.br
## User Story

> Acting as **a player managing neighbor cities**, I want **the region city list to refresh reliably after saving a city entry**, so that I **avoid seeing stale data on slower systems where 250 ms is not enough**.

---

## Background

In RegionCitiesMangement.tsx, after a successful save, the code uses `setTimeout(onCitiesChanged, 250)` to delay the list refresh. This is a timing workaround that can silently race on slower systems, leaving the list stale.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] The setTimeout is removed and replaced with a proper async flow (await the save, then call onCitiesChanged directly)
- [x] The list refreshes correctly after save on both fast and slow systems
- [x] No regression in the save flow behavior

---

## Implementation Notes



---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


