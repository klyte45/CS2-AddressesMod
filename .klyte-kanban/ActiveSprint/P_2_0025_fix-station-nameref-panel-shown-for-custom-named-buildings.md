**Start time:** 2026-04-04 00:45 -0300
# [0025] fix-station-nameref-panel-shown-for-custom-named-buildings

**Developed by:** Claude Sonnet 4.6 (claude-sonnet-4-6@kwytco.com.br)
## User Story

> Acting as **a player**, I want **the name-reference panel for transport stations to be hidden when the building already has a custom name**, so that I **the info panel isn't cluttered with useless options for buildings I've already named manually**.

---

## Background

In the game info panel (k45_adr_vuio), AdrSelectionInfoPanelSystem.GetEntityOptions returns transport station name-reference options (road aggregation choices) for PublicTransportStation and CargoTransportStation buildings. However, when a building already has a CustomName component, showing the name reference panel is meaningless, because the custom name takes priority and the road reference has no effect on what's displayed.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] AdrSelectionInfoPanelSystem.GetEntityOptions does not add transport station AdrEntityData to the result list when the entity has a CustomName component
- [ ] The check is applied to both PublicTransportStation and CargoTransportStation branches
- [ ] Entities without CustomName continue to show the name-reference options as before
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



### Is dependent for


