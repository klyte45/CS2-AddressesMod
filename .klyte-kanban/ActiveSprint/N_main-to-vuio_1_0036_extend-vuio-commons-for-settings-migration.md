# [0036] extend-vuio-commons-for-settings-migration

**Developed by:** 

## Reference

RefsLibrary/2026082521_main-to-vuio-migration/11-sprint-roadmap.md — S1

## User Story

> Acting as **a developer migrating ADR settings to VUIO**, I want **vuio-commons extended with FormLine, multistate Checkbox, and ErrorBoundary**, so that I **later feature ports reuse shared vanilla widgets instead of EUIS copies**.

---

## Background

Sprint 1 (Foundations). Extend vuio-commons with widgets required before Overrides, Namesets, and Road prefix ports.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] FormLine component available in vuio-commons
- [ ] CheckboxProps exposes multistate + onMultistateChange
- [ ] ErrorBoundary available for settings panel isolation
- [ ] Optional ColorField / FilePicker error-modal prep landed or deferred with note
- [ ] vuio-commons builds without TypeScript errors

---

## Implementation Notes

1. Add FormLine
2. Widen CheckboxProps for multistate
3. Port or add ErrorBoundary
4. Optional ColorField and FilePicker error-modal mode

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


