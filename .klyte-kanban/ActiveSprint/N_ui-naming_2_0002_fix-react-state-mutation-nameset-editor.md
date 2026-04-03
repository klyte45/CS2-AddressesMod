# [0002] fix-react-state-mutation-nameset-editor

**Developed by:** 

## User Story

> Acting as **a player editing name sets**, I want **the nameset editor to correctly detect and reflect state changes**, so that I **prevent stale UI rendering caused by React not detecting mutations on the same object reference**.

---

## Background

In NamesetEditorCmp.tsx, `onEditDone` calls `setNamesetData(Object.assign(namesetData, {...}))` which mutates the existing state object before passing it to the setter. Since the reference doesn't change, React's reconciler may skip re-renders, leaving the UI in an inconsistent state.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Object.assign usage is replaced with an immutable spread pattern: setNamesetData({ ...namesetData, Values: ..., ValuesAlternative: ... })
- [ ] The textarea onBlur handler correctly triggers a re-render when values change
- [ ] Verified that after editing and blurring the textarea, the displayed state reflects the new values

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


