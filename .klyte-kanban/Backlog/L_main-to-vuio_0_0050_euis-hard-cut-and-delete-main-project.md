# [0050] euis-hard-cut-and-delete-main-project

**Developed by:** 

## Reference

RefsLibrary/2026082521_main-to-vuio-migration/09-euis-decommission.md

## User Story

> Acting as **a player**, I want **ADR settings available via Universal Mod Menu without EUIS**, so that I **ADR no longer depends on Extra UI Screens for settings**.

---

## Background

Sprint 7 Phase B. EUIS hard cut: disable registration, stop packaging main, delete project.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] UseEuisRegister / EuisIsMandatory / EuisApps removed or disabled in AddressesCs2Mod.cs
- [ ] k45-adr-main.js/.css no longer packaged; main build hooks removed
- [ ] _Frontends/UI/main/ deleted
- [ ] ADR deps on @klyte45/euis-components / EUIS tarballs stripped
- [ ] Mod loads without Extra UI Screens installed

---

## Implementation Notes

1. Follow 09-euis-decommission.md
2. User compiles C# solution after AddressesCs2Mod changes
3. Remove main from npm workspace / CI scripts

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on

- 0049
### Is dependent for

---
