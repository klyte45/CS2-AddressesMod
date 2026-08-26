# [0041] port-override-plates-serial-and-styles

**Developed by:** 

## Reference

RefsLibrary/2026082521_main-to-vuio-migration/07-tab-by-tab-migration.md — M1

## User Story

> Acting as **a developer**, I want **plate, serial, and address-format override tabs ported with styles**, so that I **vehicle identifiers and address format overrides are fully available in VUIO settings**.

---

## Background

Sprint 2 (Overrides). Port address format, vehicle plate, vehicle serial tabs + CSS modules.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] AddressFormatTab ported (if not already in VUIO)
- [ ] VehiclePlateControllerComponent and VehicleSerialControllerComponent use vuio-commons MultiUI bindings
- [ ] vehiclePlateController.scss converted to CSS module under settings/styles/
- [ ] All 14 override subtabs editable with parity to main
- [ ] Contextual VUIO (selection/editor/tool) unchanged

---

## Implementation Notes

1. Port AddressFormatTab, VehiclePlateControllerComponent, VehicleSerialControllerComponent
2. Convert vehiclePlateController.scss to CSS module
3. Verify plate/serial preview in-game

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- 0040
### Is dependent for

---
