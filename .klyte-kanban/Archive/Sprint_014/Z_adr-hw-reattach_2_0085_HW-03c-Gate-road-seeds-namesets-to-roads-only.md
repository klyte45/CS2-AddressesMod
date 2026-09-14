# [0085] HW-03c-Gate-road-seeds-namesets-to-roads-only

**Developed by:** 
**Cancellation time:** 2026-09-13 02:36 -0300

## Reference

BelzontAdr/Overrides/AdrNameSystemOverrides.cs GetAggregateName (TrackData→RoadData fallback)
BelzontAdr/Systems/AdrSelectionInfoPanelSystem.cs TryAppendAggregateSeedOption
BelzontAdr/Systems/AdrMainSystem.cs GetRoadNameList / TryGetRoadNamesList
BelzontAdr/Systems/AdrNamesetSystem.cs ADRRandomizationData attach on Aggregate/ADRNameAggregate
_Frontends SeedManagementOptions / RoadGroupingToolOptions / AddressesInfoOptionsComponent
RefsLibrary/2026090907_custom-road-name-aggregation/20-highway-reattach-on-adr-aggregates.md (seed surfaces)
Related playtest feedback on HW-03b: seeds shall not apply to non-roads

## User Story

> Acting as **a player naming networks with ADR**, I want **road nameset seeds and road-prefix naming only on vehicle-road aggregates classified by prefab RoadData**, so that I **streets without edge Game.Net.Road still work; tracks/pathways are not renamed by road seeds**.

---

## Background

Former Sprint 014 HW-03c (0085) overflowed/deferred: seed-only scope was too narrow. Full design is RefsLibrary 22–24 (net-kind mix gate + per-kind NetNameData + road-only seeds). This backlog item is the NET-04 slice: road nameset/seed/SIP gating via prefab RoadData / AdrNetKind.Road — never HasComponent Road. Depends conceptually on NET-01 classifier (and prefer NET-02 membership gate) from catalog 24; create those board tasks when explicitly requested.

---

## Definition of Ready (DoR)

- [ ] HW-03b seed surfaces understood (SIP + Road Grouping tool)
- [ ] Clear definition of road for gating: prefer Game.Net.Road on reference edge and/or RoadData on prefab
- [ ] Track / non-car net policy decided (exclude from road nameset unless product says otherwise)
- [ ] Compile path for AdrNameSystemOverrides + AdrSelectionInfoPanelSystem available

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] GetAggregateName does not apply road namesets/prefixes when the reference edge is not a road (per agreed definition)
- [ ] TrackData is not silently treated as RoadData for road nameset naming unless explicitly kept by product decision (document in notes)
- [ ] TryAppendAggregateSeedOption does not report hasCustomNameList / road seed UX for non-road aggregates
- [ ] SIP and Road Grouping tool hide or disable road seed controls for non-road selections (consistent with backend)
- [ ] Seed redraw / nameset UI cannot usefully retarget non-road aggregates via the road seed path
- [ ] District/citizen/household seed paths unchanged
- [ ] Compile + playtest: road aggregate seed still works; pedestrian/path (and track if excluded) keep vanilla name and no road seed UI
- [ ] HW-08 smoke checklist aware of this gate
- [ ] Classifier uses prefab RoadData / stored AdrNetKind.Road — never HasComponent Road as sole gate
- [ ] GetAggregateName applies road namesets/prefixes only for AdrNetKind.Road; no TrackData to RoadData fake for road naming
- [ ] TryAppendAggregateSeedOption / SIP / Road Grouping show road seed UX only for Road kind
- [ ] Street and avenue edges without edge Game.Net.Road still get road seeds when prefab has RoadData
- [ ] Street highway mix remains allowed for Road kind; cross-kind seed path remains closed
- [ ] Docs 22–24 referenced; coordinate with NET-01…03 when those board items exist

---

## Implementation Notes

1. Gate early in GetAggregateName after FindReferenceRoad: if !HasComponent<Road>(refRoad) (or !RoadData and not allowed track), return true (vanilla)
2. Remove or narrow TrackData→RoadData fake once policy is exclude-tracks
3. In TryAppendAggregateSeedOption: only call GetRoadNameList / set hasCustomNameList when ref edge is road; optionally skip appending RoadAggregation seed block entirely for non-roads if CustomName already covers UI
4. Frontend: SeedManagement already keys off hasCustomNameList / option presence — prefer backend truth over extra UI heuristics
5. Optional follow-up: stop attaching ADRRandomizationData to aggregates that never consume road seeds — only if low-risk; otherwise leave attach + unused seed harmless
6. Coordinate with HW-04 so highway name branch also only applies to road ADR aggregates
7. Size: M (name override + SIP DTO + tool/SIP consistency + playtest matrix)
8. Implement after or with NET-01 (AdrNetKind + TryGetNetKind from prefab Data)
9. Prefer NET-02 membership gate already landed so non-road aggregates are rarer
10. GetAggregateName: early-out unless kind is Road; delete TrackData RoadData fake for nameset path
11. TryAppendAggregateSeedOption: hasCustomNameList only when AdrNetKind.Road
12. Replace ADR HasComponent Road seed/address gates with RoadData or stored kind
13. Reference RefsLibrary 22–24; board catalog NET-04 in 24-adr-net-kind-roadmap.md

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Tracks/rails that relied on road namesets suddenly revert to vanilla names | medium | Explicit product decision in DoR; playtest rail aggregations; optional separate track nameset later |
| Partial UI hide without GetAggregateName gate leaves silent renaming | high if UI-only | DoD requires name-system gate first; UI matches backend |
| Touches same file as HW-04 and causes merge conflict | medium | Land gate before or with clear conflict note; highway branch checks road + highwayDataId |

---

## Related Tasks

### Depends on

- [0079] HW-03b-Road-Grouping-tool-options-parity

### Is dependent for

- [0084] HW-08-Smoke-acceptance-checklist

### Is related to

- [0078] HW-03-VUIO-Addresses-SIP-highway-section
- [0080] HW-04-Naming-highway-override-branch
- [0086] NET-04-Road-only-seeds-namesets-prefab-RoadData
### Groups with

- HW-03b-Road-Grouping-tool-options-parity
- HW-04-Naming-highway-override-branch

---

## Other Notes

Epic adr-hw-reattach. Size M. Insert after HW-03b / before or alongside HW-04. Status N = ready once DoR track policy is confirmed at start. Not part of HW-03b validation scope.
