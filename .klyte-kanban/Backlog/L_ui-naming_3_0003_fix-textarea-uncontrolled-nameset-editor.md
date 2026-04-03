# [0003] fix-textarea-uncontrolled-nameset-editor

**Developed by:** 

## User Story

> Acting as **a player editing name files**, I want **the nameset editor textarea to reflect the current data when switching between entries**, so that I **avoid seeing stale content from a previously opened nameset**.

---

## Background

NamesetEditorCmp.tsx uses `defaultValue` on the main textarea, making it an uncontrolled component. If the parent provides a different entryData (e.g., after opening another nameset), the textarea will keep displaying the old content because React only sets defaultValue on initial mount.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] The textarea is refactored to use a controlled pattern with a local text state variable
- [ ] Switching between two different nameset entries correctly shows each entry's content in the textarea
- [ ] The onBlur parsing logic is preserved and tested

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


