**Start time:** 2026-04-04 01:11 -0300
# [0030] patch-transport-stop-address-format

**Developed by:** Claude Sonnet 4.6 (claude-sonnet-4-6@kwytco.com.br)
## User Story

> Acting as **a player**, I want **transport stop addresses to also use my custom format pattern**, so that I **address format is consistent across all building types in-game**.

---

## Background

AdrNameSystemOverrides.GetStaticTransportStopName patches transport stop address naming. The same custom format logic needs to apply here. {brand} is not applicable for transport stops.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] When AddressFormatPattern is null/empty, behavior unchanged
- [ ] When set, applies token substitution ({number}, {street}, {district})
- [ ] {brand} token is not applicable and results in empty string if included
- [ ] Build passes

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
- [0028]

### Is dependent for


