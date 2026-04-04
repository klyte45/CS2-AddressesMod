**End time:** 2026-04-04 01:11 -0300
**Start time:** 2026-04-04 01:08 -0300
# [0029] patch-spawnable-building-address-format

**Developed by:** Claude Sonnet 4.6 (claude-sonnet-4-6@kwytco.com.br)
## User Story

> Acting as **a player**, I want **spawnable building addresses to use my custom format pattern**, so that I **I can display addresses in the format that matches the real-world conventions of my city**.

---

## Background

AdrNameSystemOverrides.GetSpawnableBuildingName already formats building addresses using the game's FormattedName. It needs to apply the custom format when AddressFormatPattern is set. Supports {number}, {street}, {district}, {brand} placeholders. {district} via the helper from task 0028.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] When AddressFormatPattern is null/empty, behavior is unchanged (uses game FormattedName/ADDRESS_NAME_FORMAT)
- [x] When AddressFormatPattern is set, token substitution is applied: {number}, {street}, {district}, {brand}
- [x] {district} resolved via GetBuildingSideDistrictName helper
- [x] {brand} resolved only if brand exists, otherwise empty string
- [x] Consecutive extra spaces from empty optional tokens are collapsed (trim pass)
- [x] NAMED_ADDRESS_NAME_FORMAT path also applies custom format when set
- [x] Build passes

---

## Implementation Notes



---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- [0026]
- [0027]
- [0028]

### Is dependent for


