**Start time:** 2026-09-13 00:27 -0300
# [0078] HW-03-VUIO-Addresses-SIP-highway-section

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/20-highway-reattach-on-adr-aggregates.md §D
RefsLibrary/2026090907_custom-road-name-aggregation/21-highway-reattach-roadmap.md HW-03
_Frontends/UI/k45_adr_vuio/src/components/AddressesInfoOptionsComponent.tsx
_Frontends/UI/k45_adr_vuio/src/components/SeedManagementOptions.tsx

## User Story

> Acting as **a player inspecting an ADR road grouping in the Addresses SIP**, I want **highway route, direction, and mileage override controls next to existing seed settings**, so that I **I can configure highway identity without placing road markers**.

---

## Background

Third task. VUIO Addresses SIP: keep SeedManagement for both Aggregate types; add HighwayAggregateSettings only when isAdrAggregate. Prefer shared presentational components so HW-03b can reuse them. i18n via pending *_entries.txt only — never edit i18n.csv.

---

## Definition of Ready (DoR)

- [ ] HW-02 binders + isAdrAggregate DTO live
- [ ] RefsLibrary 20 §D frontend notes agreed
- [ ] SeedManagementOptions current API understood

---

## Acceptance Criteria / Definition of Done (DoD)

- [x] RoadAggregation SIP still shows SeedManagement for vanilla and ADR
- [x] ADR selections also show HighwayAggregateSettings: registry picker, routeDirection, mileageOverrideActive + mileageOverrideKm, reverseCounting
- [x] Vanilla Aggregate SIP has no highway controls
- [x] Highway UI uses HW-02 bindings; edits persist and dirty consumers
- [x] Shared/extractable components preferred for tool reuse (or explicit note if not extractable yet)
- [x] i18n pending *_entries.txt for new keys (en-US + pt-BR columns); no i18n.csv edits
- [ ] npm frontend build succeeds; playtest SIP on vanilla Aggregate vs ADR

---

## Implementation Notes

1. Branch AddressesInfoOptionsComponent on isAdrAggregate
2. Registry options from listHighwaysRegistered / HighwayRegisterManagement data shape
3. Clear highwayDataId to default for not-a-highway
4. Do not show marker route/mileage UI on RoadMarkSettings — markers stay visual-only
5. Remove translate fallbacks only in files you touch (i18n rule)
6. Added shared HighwayAggregateSettings.tsx (registry picker, routeDirection, mileage override + km, reverseCounting) wired via HW-02 SelectInfoPanelService setters on response.entityValue.
7. AddressesInfoOptionsComponent: RoadAggregation always shows SeedManagement; HighwayAggregateSettings only when isAdrAggregate. Vanilla Aggregate has no highway UI.
8. i18n pending BelzontAdr/i18n/highway_aggregate_settings_entries.txt (en-US + pt-BR); RouteDirection.NONE added. npm run build succeeded.
9. Mileage override storage corrected to mileageOverrideMeters (int meters) on ADRNameAggregateHighwayData; binder setMileageOverrideMeters. UI converts via game UnitSystem (Metric=km, Freedom=mi) with matching label keys MileageOverrideKm/Mi.
10. Custom name on Aggregate/ADR no longer early-returns getEntityOptions — SIP title owns the name; seed + HighwayAggregateSettings still shown for road aggregation selections.

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| isAdrAggregate false negatives hide highway UI on ADR edges | medium | Dogfood edge→ADR resolve; align with ACT-04 entityValue rules |
| i18n keys missing after merge | low | Pending entries file + assembled keys in DoD |

---

## Related Tasks

### Depends on

- [0077] HW-02-SIP-backend-isAdrAggregate-highway-bindings

### Is dependent for

- [0079] HW-03b-Road-Grouping-tool-options-parity
- [0084] HW-08-Smoke-acceptance-checklist
### Groups with

- HW-02-SIP-backend-isAdrAggregate-highway-bindings
- HW-03b-Road-Grouping-tool-options-parity

---

## Other Notes

Epic adr-hw-reattach. Size M. Order 3. Status N = ready for sprint after review.
