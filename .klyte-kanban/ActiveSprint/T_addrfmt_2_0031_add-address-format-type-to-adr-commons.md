**End time:** 2026-04-04 01:13 -0300
**Start time:** 2026-04-04 01:12 -0300
# [0031] add-address-format-type-to-adr-commons

**Developed by:** Claude Sonnet 4.6 (claude-sonnet-4-6@kwytco.com.br)
## User Story

> Acting as **a frontend developer**, I want **the AdrCitywideSettings TypeScript type and NamingRulesService to include the address format field**, so that I **the frontend can type-safely read and update the address format pattern**.

---

## Background

The AdrCitywideSettings TypeScript type in adr-commons must include the new AddressFormatPattern field and a new setAddressFormatPattern method in NamingRulesService to match the C# binding.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] addressFormatPattern: string field added to AdrCitywideSettings TypeScript type
- [x] static async setAddressFormatPattern(x: string): Promise<void> added to NamingRulesService
- [x] adr-commons package builds without TypeScript errors

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


