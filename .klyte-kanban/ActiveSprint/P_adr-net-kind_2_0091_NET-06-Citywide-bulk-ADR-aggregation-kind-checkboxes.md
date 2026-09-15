**Start time:** 2026-09-14 23:12 -0300
# [0091] NET-06-Citywide-bulk-ADR-aggregation-kind-checkboxes

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/26-bulk-citywide-adr-aggregation.md
RefsLibrary/2026090907_custom-road-name-aggregation/23-adr-net-kind-gate-labels-seeds.md
RefsLibrary/2026090907_custom-road-name-aggregation/24-adr-net-kind-roadmap.md (NET-06)
_Frontends/UI/k45_adr_vuio/src/settings/tabs/overrides/OverrideRoadsDistrictsTab.tsx
BelzontAdr/Systems/AdrNameAggregateMembershipSystem.cs

## User Story

> Acting as **a city builder with a large existing network**, I want **to convert all applicable segments into ADR aggregations in one action while choosing which net kinds to include**, so that I **I can adopt ADR naming/grouping citywide without manually grouping every corridor**.

---

## Background

Sixth slice of epic adr-net-kind. Players need a settings-side bulk convert after NET-01…03. UI: Roads and Districts tab, button immediately above the district-dependent road nameset section. Confirm dialog body: checkboxes per aggregable AdrNetKind (Road, Pathway, Train, Tram, Subway, Waterway, Taxiway). Only checked kinds convert. Re-run must skip edges already on ADR aggregates. Respect NET-02 mix gate. Spec: RefsLibrary 26-bulk-citywide-adr-aggregation.md.

---

## Definition of Ready (DoR)

- [ ] NET-01 [0087] classifier + stored kind shipped
- [ ] NET-02 [0088] membership mix gate shipped
- [ ] NET-03 [0089] non-road labels / PrefabRef safety shipped
- [ ] RefsLibrary 26 bulk convert UX locked

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Feature button on Roads and Districts tab immediately above per-district road nameset section
- [ ] Confirm dialog body lists checkboxes for aggregable AdrNetKinds; only checked kinds are converted
- [ ] Citywide pass forks/creates ADR aggregates for applicable edges of selected kinds
- [ ] Already-ADR edges skipped; operation is safe to re-run
- [ ] Cross-kind membership never created (NET-02 gate respected)
- [ ] Result feedback (counts or equivalent) after run
- [ ] i18n via pending *_entries.txt (no agent CSV edit)
- [ ] Compile + smoke: convert roads-only; convert roads+train; cancel leaves city unchanged

---

## Implementation Notes

1. UI: OverrideRoadsDistrictsTab — button above overrideSettings.perDistrictRoadsFile block; confirmation dialog with kind checkboxes (default Road on, others off recommended)
2. Backend: binder/system calling membership fork/create for edges matching checked kinds; skip ADRNameAggregated; prefer per-vanilla-Aggregate when members share one checked kind
3. Progress/result toast or summary dialog for large cities
4. Catalog: 24-adr-net-kind-roadmap.md NET-06; design: 26-bulk-citywide-adr-aggregation.md
5. Backend: AdrNameAggregateMembershipSystem.ConvertCitywideToAdrAggregates(kindMask) — full TryForkFromVanillaSeed when remainder matches mask+shared kind; else CreateAggregate per kind bucket. Binder main.convertAllToAdrAggregates. Result DTO AdrBulkConvertResult.
6. UI: OverrideRoadsDistrictsTab button above perDistrictRoadsFile; ConfirmationDialog with kind checkboxes (Road default on); result dialog with counts.
7. i18n pending: BelzontAdr/i18n/bulkConvert_entries.txt (reuse main netKind.* for checkbox labels). Awaiting compile + smoke.

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Full-city scan freezes UI / hitch | medium | Batch work across frames or show progress; avoid main-thread megajob without yield |
| Wrong grouping topology vs manual Road Grouping | medium | Document algorithm (vanilla Aggregate vs contiguous same-kind); keep mix gate; allow Road Grouping to refine after |
| Accidental convert of tracks/pathways | medium | Kind checkboxes default conservative (Road on); confirm required |

---

## Related Tasks

### Depends on

- [0087] NET-01-Classifier-stored-kind-on-ADRNameAggregate
- [0088] NET-02-Membership-Road-Grouping-mix-gate
- [0089] NET-03-Per-kind-ADR-NetNameData-outline-by-kind

### Is dependent for



### Is related to

- [0086] NET-04-Road-only-seeds-namesets-prefab-RoadData
- [0090] NET-05-Playtest-matrix-smoke-notes

### Groups with

- NET-01-Classifier-stored-kind-on-ADRNameAggregate
- NET-02-Membership-Road-Grouping-mix-gate
- NET-03-Per-kind-ADR-NetNameData-outline-by-kind
- NET-04-Road-only-seeds-namesets-prefab-RoadData
- NET-05-Playtest-matrix-smoke-notes

## Other Notes

Epic adr-net-kind. Size M. Order 6. Placement: OverrideRoadsDistrictsTab above per-district roads file section.
