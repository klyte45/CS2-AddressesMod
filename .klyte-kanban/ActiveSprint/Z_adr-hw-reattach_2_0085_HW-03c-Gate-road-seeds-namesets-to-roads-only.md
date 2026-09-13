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

> Acting as **a player naming networks with ADR**, I want **road nameset seeds and road-prefix naming to apply only to road aggregates**, so that I **pedestrian, track, and other non-road networks keep vanilla (or their own) naming and are not renamed by road seeds**.

---

## Background

Surfaced during HW-03b Road Grouping tool work. GetAggregateName currently applies GetRoadNameList + road prefix patterns to any Aggregate/ADRNameAggregate with a reference edge, and fakes TrackData into RoadData when RoadData is missing. TryAppendAggregateSeedOption sets hasCustomNameList from the same road nameset path (always falls back to DefaultRoadNameOverrides), so SIP and the Road Grouping tool expose seed dice for non-road aggregations. ADRRandomizationData is still attached broadly to all aggregates — redrawing a seed on a non-road aggregate is misleading if naming ignores it. This is deeper than a tool UI hide: name-system overrides + SIP seed DTO + tool seed visibility must agree. Do not fold into HW-03b (tool parity) or HW-04 (highway registry name branch); shared file AdrNameSystemOverrides.cs but distinct concern. Decide track policy while implementing (historically TrackData was treated as road-like).

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

---

## Implementation Notes

1. Gate early in GetAggregateName after FindReferenceRoad: if !HasComponent<Road>(refRoad) (or !RoadData and not allowed track), return true (vanilla)
2. Remove or narrow TrackData→RoadData fake once policy is exclude-tracks
3. In TryAppendAggregateSeedOption: only call GetRoadNameList / set hasCustomNameList when ref edge is road; optionally skip appending RoadAggregation seed block entirely for non-roads if CustomName already covers UI
4. Frontend: SeedManagement already keys off hasCustomNameList / option presence — prefer backend truth over extra UI heuristics
5. Optional follow-up: stop attaching ADRRandomizationData to aggregates that never consume road seeds — only if low-risk; otherwise leave attach + unused seed harmless
6. Coordinate with HW-04 so highway name branch also only applies to road ADR aggregates
7. Size: M (name override + SIP DTO + tool/SIP consistency + playtest matrix)

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

### Groups with

- HW-03b-Road-Grouping-tool-options-parity
- HW-04-Naming-highway-override-branch

## Other Notes

Epic adr-hw-reattach. Size M. Insert after HW-03b / before or alongside HW-04. Status N = ready once DoR track policy is confirmed at start. Not part of HW-03b validation scope.
