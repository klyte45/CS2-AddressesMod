**End time:** 2026-04-03 21:07 -0300
**Start time:** 2026-04-03 21:07 -0300
# [0015] add-vehicle-plate-live-preview

**Developed by:** claude-sonnet-4-6@kwyt.com.br
## User Story

> Acting as **a player configuring vehicle plate formats**, I want **to see a sample generated plate while configuring the digit settings**, so that I **quickly understand how the current settings will format a vehicle plate without entering the game**.

---

## Background

The VehiclePlateControllerComponent.tsx exposes all plate format parameters (LettersAllowed, FlagsLocal, FlagsCarNumber, FlagsRandomized, etc.) but shows no preview of a generated plate. The VehiclePlateSettings.GetPlateFor logic lives in C# with Burst, so a preview string could be exposed via a new call binding.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] A 'Preview' row is added to the plate settings UI displaying a sample plate rendered with the current settings
- [x] The preview updates reactively when any setting field changes
- [x] The preview shows at least 3 sample plate values (different serial numbers) to show variability
- [x] The backend exposes a new 'generatePreviewPlates' call binding that returns sample formatted strings

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


