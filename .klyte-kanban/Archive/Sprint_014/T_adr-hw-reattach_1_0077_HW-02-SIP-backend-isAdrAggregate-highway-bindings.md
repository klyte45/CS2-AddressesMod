**End time:** 2026-09-13 00:27 -0300
**Start time:** 2026-09-13 00:18 -0300
# [0077] HW-02-SIP-backend-isAdrAggregate-highway-bindings

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/20-highway-reattach-on-adr-aggregates.md §D
RefsLibrary/2026090907_custom-road-name-aggregation/21-highway-reattach-roadmap.md HW-02
BelzontAdr/Systems/AdrSelectionInfoPanelSystem.cs

## User Story

> Acting as **a player selecting a road aggregation in the Addresses SIP**, I want **the backend to know ADR vs vanilla Aggregate and read/write highway settings on the ADR entity**, so that I **SIP and tool UIs can share one safe write path without touching vanilla Aggregate**.

---

## Background

Second task. Extend AdrSelectionInfoPanelSystem / AdrEntityData with isAdrAggregate when type is RoadAggregation. Bind get/set for ADRNameAggregateHighwayData fields. Setters must be reusable by Road Grouping tool (HW-03b). Custom-name get/set via existing NameSystem custom-name APIs. Writes dirty name/address consumers like seed redraw.

---

## Definition of Ready (DoR)

- [ ] HW-01 complete (component exists on ADR aggregates)
- [ ] RefsLibrary 20 §D / 21 HW-02 agreed
- [ ] Existing selectionPanel seed bindings understood

---

## Acceptance Criteria / Definition of Done (DoD)

- [x] getEntityOptions / AdrEntityData exposes isAdrAggregate for RoadAggregation
- [x] Highway field values returned for ADR selections (and safe defaults / omitted for vanilla)
- [x] Bindings get/set highwayDataId, routeDirection, mileageOverrideActive, mileageOverrideKm, reverseCounting on resolved ADR entity
- [x] Custom-name get/set available for ADR aggregate entity (NameSystem custom-name path)
- [x] Writes dirty naming/address consumers (same family as OnChangedRoadNameGenerationRules / seed redraw)
- [x] No writes to vanilla Aggregate highway components
- [x] Setters usable from tool with SelectedAggregation entity (documented binder surface)
- [x] Compile + binder smoke from editor/game

---

## Implementation Notes

1. Prefer one binder surface: AdrSelectionInfoPanelSystem or AdrHighwayRoutesSystem — document choice when done
2. Reuse HighwayData registry listHighwaysRegistered for picker options
3. Resolve edge → ADR aggregate the same way ACT-04 does for seed UI
4. Validate highwayDataId against registry or allow default clear
5. Ensure vanilla Aggregate path ignores highway setters
6. Binder surface: AdrSelectionInfoPanelSystem (selectionPanel.*) — SIP and Road Grouping tool both pass the ADR aggregate entity (entityValue / SelectedAggregation). Vanilla Aggregate setters return false.
7. getEntityOptions / AdrEntityData: isAdrAggregate + highway fields + customName for ADR RoadAggregation. Highway registry list remains highwayRoutes.listHighwaysRegistered.
8. Setters: setHighwayDataId (registry-validated or clear), setRouteDirection, setMileageOverrideActive/Km, setReverseCounting, get/setAggregateCustomName via NameSystem. Writes call DirtyAdrAggregateConsumers (Updated + BatchesUpdated + MarkRoadsDirty).
9. Clarification: SIP does not surface aggregate custom name in Addresses options — panel title input already owns it. get/setAggregateCustomName binders remain for Road Grouping tool options (HW-03b) only; removed customName from AdrEntityData / getEntityOptions.

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Tool and SIP diverge on entity resolution | medium | Shared resolve helper; HW-03b must call same setters |
| Missing dirty after write leaves stale names/mileage | medium | Mirror seed-redraw dirty; include in DoD |

---

## Related Tasks

### Depends on

- [0076] HW-01-ADRNameAggregateHighwayData-component-lifecycle

### Is dependent for

- [0078] HW-03-VUIO-Addresses-SIP-highway-section
- [0079] HW-03b-Road-Grouping-tool-options-parity
- [0084] HW-08-Smoke-acceptance-checklist
### Is related to

- [0066] ACT-04-Prefer-ADR-in-name-address-SIP

### Groups with

- HW-01-ADRNameAggregateHighwayData-component-lifecycle
- HW-03-VUIO-Addresses-SIP-highway-section
- HW-03b-Road-Grouping-tool-options-parity

---

## Other Notes

Epic adr-hw-reattach. Size M. Order 2. Feeds HW-03 and HW-03b. Status N = ready for sprint after review.
