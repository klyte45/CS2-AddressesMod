# [0005] fix-stale-closure-plate-settings-cleanup

**Developed by:** 

## User Story

> Acting as **a developer**, I want **VehiclePlateControllerComponent to properly dispose its bindings on unmount**, so that I **prevent subscription leaks when navigating between UI tabs**.

---

## Background

In VehiclePlateControllerComponent.tsx, the useEffect cleanup returns a function that references `controllerData` from state. On the first render, `controllerData` is undefined, so the cleanup captures the initial undefined value and never disposes the created bindings when the component unmounts.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] A useRef is introduced to hold the bindings instance
- [ ] The cleanup function calls dispose on the ref's current value
- [ ] Unmounting and remounting the component panel does not accumulate orphaned subscriptions

---

## Implementation Notes

1. Use a ref (useRef) to store the controllerData instance instead of relying on state inside the cleanup closure
2. Assign the ref at creation time and use the ref in the cleanup

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


