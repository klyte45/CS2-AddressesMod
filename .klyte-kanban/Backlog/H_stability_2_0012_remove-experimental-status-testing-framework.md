# [0012] remove-experimental-status-testing-framework

**Developed by:** 

## User Story

> Acting as **a new player**, I want **the mod to be clearly marked as stable**, so that I **install with confidence that save files won't be corrupted**.

---

## Background

The README still contains 'Experimental mod warning!' and notes about early-stage development. ActionPlan_4 outlines a testing strategy. No test project exists yet. Goal is to establish a test baseline, validate serialization round-trip, and remove the warning.

Write Everywhere mod already has a test project with a lot of tests, including serialization tests. It would be good to review their test project and see if we can use some of their testing patterns and approaches for our own test project. We can also use their test project as a reference for how to set up our own test project and how to write tests for our mod. The WE mod test project shall be located at a `_auxFiles/` subfolder of the `RefsLibrary/` before this task is started.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] A test project (BelzontAdr.Tests) is created with at least serialization round-trip tests for AdrCitywideSettings, AdrNameFile, and VehiclePlateSettings
- [ ] All serialization version migration paths (v0→current) have passing tests
- [ ] The experimental warning is removed from README.md
- [ ] A stability section is added describing what was tested

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


