**Start time:** 2026-09-11 14:54 -0300
# [0069] ACT-03d-ForkFromVanilla-copy-custom-name

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/12-activation-change-map.md §A3
RefsLibrary/2026090907_custom-road-name-aggregation/04-parallel-system-design-notes.md fork/seed rules
Game.UI.NameSystem TryGetCustomName / SetCustomName

## User Story

> Acting as **a Cities: Skylines 2 player with a manually renamed road**, I want **the custom street name to move onto the ADR name-aggregate on first membership fork**, so that I **ADR-linked edges keep the name they already had instead of falling back to a generated nameset name**.

---

## Background

Gap follow-up to ACT-03c. ForkFromVanilla already copies ADRRandomizationData seed S onto the new ADR aggregate and Redraws the vanilla Aggregate. If the vanilla Aggregate has a player custom name (NameSystem / CustomName), that name must also be transferred onto the new ADR aggregate so ADR-linked edges keep the named street.

---

## Definition of Ready (DoR)

- [ ] ACT-03c ForkFromVanilla exists and copies seed S
- [ ] NameSystem.TryGetCustomName / SetCustomName API confirmed
- [ ] Decide whether vanilla Aggregate clears custom name after transfer (open question)

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] ForkFromVanilla: if vanilla Aggregate has a custom name, ADR aggregate receives the same custom name via NameSystem.SetCustomName
- [ ] If vanilla has no custom name, fork behavior unchanged (seed-only naming path)
- [ ] Custom-name transfer does not mutate vanilla AggregateElement membership
- [ ] Verified: ADR-linked edges resolve the transferred custom name after fork

---

## Implementation Notes

1. In TryForkFromVanilla after ADR entity + seed copy: NameSystem.TryGetCustomName(vanilla) → SetCustomName(adr, name) when present
2. Inject/resolve NameSystem like other ADR systems
3. Open product question: after copy, clear custom name on vanilla Aggregate so leftover S2 edges use generated names, or leave custom name on vanilla too?

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Leaving custom name on vanilla after fork makes leftover edges keep the old name despite Redraw S2 | high | Confirm clear-vs-keep with product owner before implement |

---

## Related Tasks

### Depends on

- [0065] ACT-03c-ForkFromVanilla-ADR-seeding

### Is dependent for



### Is related to

- [0066] ACT-04-Prefer-ADR-in-name-address-SIP

### Groups with

- [0065] ACT-03c-ForkFromVanilla-ADR-seeding
- ACT-03c

## Other Notes

Inserts before ACT-05 tool wiring so fork API is complete when mutations land.
