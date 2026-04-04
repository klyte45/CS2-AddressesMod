# [0023] fix-tristatebutton-forbid-icon

**Developed by:** 

## User Story

> Acting as **a developer**, I want **the tri-state checkbox third state to show a visible X/close icon**, so that I **Users can clearly see that a flag is in the 'forbidden' state**.

---

## Background

The Cs2TriCheckbox component uses a rotated plus.svg from assetdb://gameui/Media/Glyphs/plus.svg for the 'forbid' (null) state. This asset is no longer present in the game. The UIL icons library (coui://uil/) has Standard/XClose.svg which is more semantically correct for a 'forbidden' indicator. Since euis-components is a submodule, the submodule must be committed separately before updating the parent project.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] The forbid state of Cs2TriCheckbox shows a visible icon in-game
- [ ] The CSS mask-image for .cs2-checkmark.forbid uses coui://uil/Standard/XClose.svg
- [ ] The rotateZ(45deg) transform is removed (XClose is already an X shape)
- [ ] The euis-components submodule is committed before the parent project commit
- [ ] No visual regression on the checked state

---

## Implementation Notes

1. In _Frontends/UI/_replacements/euis-components/src/styles/cs2-form-style.scss
2. Change .cs2-checkmark.forbid mask-image from url(assetdb://gameui/Media/Glyphs/plus.svg) to url(coui://uil/Standard/XClose.svg)
3. Remove the transform: rotateZ(45deg) rule — XClose.svg is already an X shape
4. Commit the submodule at _Frontends/UI/_replacements first, then commit main project

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


