**End time:** 2026-04-03 18:40 -0300
**Start time:** 2026-04-03 18:40 -0300
# [0001] fix-stale-event-listener-override-settings

**Developed by:** claude-sonnet-4-6@kwyt.com.br
## User Story

> Acting as **a developer**, I want **the OverrideSettingsCmp to register its city nameset change handler only once, inside useEffect**, so that I **avoid listener accumulation and redundant re-registrations on every render cycle**.

---

## Background

In OverrideSettingsCmp.tsx, `NamesetService.doOnCityNamesetsUpdated(callback)` is called at the component's top level instead of inside a useEffect. This means a new event handler is registered on every render, causing accumulation of listeners and unexpected repeated callbacks.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] The doOnCityNamesetsUpdated call is moved inside the existing useEffect that handles mount/unmount
- [x] The matching offCityNamesetsUpdated is confirmed in the useEffect cleanup return function
- [x] No redundant subscriptions are registered across multiple renders (verified by manual inspection)

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


