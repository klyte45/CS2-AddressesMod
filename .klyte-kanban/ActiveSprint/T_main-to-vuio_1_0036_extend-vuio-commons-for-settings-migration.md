**End time:** 2026-08-25 23:07 -0300
# [0036] extend-vuio-commons-for-settings-migration

**Developed by:** Composer (cursor-agent@klyte.local)
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

- [x] FormLine component available in vuio-commons
- [x] CheckboxProps exposes multistate + onMultistateChange
- [x] ErrorBoundary available for settings panel isolation
- [x] Optional ColorField / FilePicker error-modal prep landed or deferred with note
- [x] vuio-commons builds without TypeScript errors

---

## Implementation Notes

1. Add FormLine
2. Widen CheckboxProps for multistate
3. Port or add ErrorBoundary
4. Optional ColorField and FilePicker error-modal mode
5. FormLine + ErrorBoundary exported from vuio-commons
6. CheckboxProps widened for multistate/onMultistateChange
7. ColorField already on VanillaWidgets; FilePicker error-modal deferred to S3
8. vuio-commons npm install + k45_adr_vuio build green

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


