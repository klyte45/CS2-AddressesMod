# [0033] integrate-address-format-tab-in-override-settings

**Developed by:** 

## User Story

> Acting as **a player**, I want **the address format tab to be visible in the Override Settings panel**, so that I **I can access the address format settings from the same panel as other naming overrides**.

---

## Background

The new AddressFormatTab needs to be added to the OverrideSettingsCmp.tsx tab list following the existing tab registration pattern.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] TabsNames.AddressFormat enum value added
- [ ] Tab inserted in a logical position in tabsOrder (after RoadsDistricts or equivalent)
- [ ] getComponents() map includes TabsNames.AddressFormat mapped to AddressFormatTab
- [ ] No existing tabs broken
- [ ] Frontend builds without errors

---

## Implementation Notes



---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- [0032]

### Is dependent for


