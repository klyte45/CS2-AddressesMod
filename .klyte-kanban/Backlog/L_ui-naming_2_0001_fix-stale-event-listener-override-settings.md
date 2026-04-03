# [0001] fix-stale-event-listener-override-settings

**Developed by:** 

## User Story

> Acting as **a developer**, I want **the OverrideSettingsCmp to register its city nameset change handler only once, inside useEffect**, so that I **avoid listener accumulation and redundant re-registrations on every render cycle**.

---

## Background

In OverrideSettingsCmp.tsx, `NamesetService.doOnCityNamesetsUpdated(callback)` is called at the component's top level instead of inside a useEffect. This means a new event handler is registered on every render, causing accumulation of listeners and unexpected repeated callbacks.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] The doOnCityNamesetsUpdated call is moved inside the existing useEffect that handles mount/unmount
- [ ] The matching offCityNamesetsUpdated is confirmed in the useEffect cleanup return function
- [ ] No redundant subscriptions are registered across multiple renders (verified by manual inspection)

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


