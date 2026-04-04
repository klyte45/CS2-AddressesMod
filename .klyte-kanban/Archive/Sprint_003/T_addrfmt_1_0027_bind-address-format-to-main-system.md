**End time:** 2026-04-04 01:01 -0300
**Start time:** 2026-04-04 00:56 -0300
# [0027] bind-address-format-to-main-system

**Developed by:** Claude Sonnet 4.6 (claude-sonnet-4-6@kwytco.com.br)
## User Story

> Acting as **a developer**, I want **AdrMainSystem to expose a binding for the address format field**, so that I **the frontend can read and update the format pattern via the established binding pattern**.

---

## Background

AdrMainSystem exposes city settings to the frontend via SetupCallBinder. A new binding for the address format field is needed following the existing pattern for other settings.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] main.setAddressFormatPattern binding added to SetupCallBinder
- [x] Setter treats empty string as null (clears to default)
- [x] Setter calls MarkRoadsDirty() and NotifyChanges() after update
- [x] getCurrentCitywideSettings already returns the full settings object including new field (no extra work needed)
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

### Is dependent for


