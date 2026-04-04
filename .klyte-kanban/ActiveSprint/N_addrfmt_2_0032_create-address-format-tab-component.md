# [0032] create-address-format-tab-component

**Developed by:** 

## User Story

> Acting as **a player**, I want **a UI tab where I can enter a custom address format**, so that I **I can configure address format without editing config files**.

---

## Background

A new AddressFormatTab.tsx component is needed for the Override Settings panel. It should provide a text input for the format pattern, clickable token chips, a live preview, and a reset button.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] AddressFormatTab component created at src/components/overrides/AddressFormatTab.tsx (or equivalent path for the project structure)
- [ ] Contains format pattern text input (single line)
- [ ] Contains clickable token chips: {number}, {street}, {district}, {brand} — clicking inserts the token at cursor position
- [ ] Contains live preview with mock substitution (number=42, street=Oak Avenue, district=Downtown, brand=SuperMart)
- [ ] Contains Use game default reset button that clears the pattern
- [ ] Calls NamingRulesService.setAddressFormatPattern on change
- [ ] Component compiles without TypeScript errors

---

## Implementation Notes



---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- [0031]

### Is dependent for


