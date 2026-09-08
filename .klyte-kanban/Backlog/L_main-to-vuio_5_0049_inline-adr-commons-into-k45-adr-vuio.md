# [0049] inline-adr-commons-into-k45-adr-vuio

**Developed by:** 

## Reference

RefsLibrary/2026082521_main-to-vuio-migration/08-shared-libs.md — Path A

## User Story

> Acting as **a developer**, I want **adr-commons inlined into k45_adr_vuio and the shared package deleted**, so that I **ADR ships a single VUIO frontend package without a separate adr-commons npm package**.

---

## Background

Sprint 7 Phase A. Inline adr-commons into k45_adr_vuio before EUIS cut.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] adr-commons sources copied to k45_adr_vuio/src/adr/
- [ ] Entity/name.utils/EnumValueType imported from vuio-commons
- [ ] All VUIO imports updated off @klyte45/adr-commons
- [ ] @klyte45/adr-commons removed from k45_adr_vuio/package.json
- [ ] _Frontends/UI/_shared/adr-commons/ deleted
- [ ] rg clean: no @klyte45/adr-commons in ADR product paths

---

## Implementation Notes

1. Path A per 08-shared-libs.md
2. Copy service/ and model/ under src/adr/
3. Strip EUIS triple-reference from inlined code

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- 0048
### Is dependent for

---
