# [0031] add-address-format-type-to-adr-commons

**Developed by:** 

## User Story

> Acting as **a frontend developer**, I want **the AdrCitywideSettings TypeScript type and NamingRulesService to include the address format field**, so that I **the frontend can type-safely read and update the address format pattern**.

---

## Background

The AdrCitywideSettings TypeScript type in adr-commons must include the new AddressFormatPattern field and a new setAddressFormatPattern method in NamingRulesService to match the C# binding.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] addressFormatPattern: string field added to AdrCitywideSettings TypeScript type
- [ ] static async setAddressFormatPattern(x: string): Promise<void> added to NamingRulesService
- [ ] adr-commons package builds without TypeScript errors

---

## Implementation Notes



---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- [0027]

### Is dependent for


