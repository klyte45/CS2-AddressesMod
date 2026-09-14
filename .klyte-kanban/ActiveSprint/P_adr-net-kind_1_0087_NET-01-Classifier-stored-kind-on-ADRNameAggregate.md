**Start time:** 2026-09-14 19:56 -0300
# [0087] NET-01-Classifier-stored-kind-on-ADRNameAggregate

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/22-game-net-edge-kinds-and-aggregation.md
RefsLibrary/2026090907_custom-road-name-aggregation/23-adr-net-kind-gate-labels-seeds.md
RefsLibrary/2026090907_custom-road-name-aggregation/24-adr-net-kind-roadmap.md (NET-01)
BelzontAdr/Components/ADRNameAggregate.cs
BelzontAdr/Systems/AdrNameAggregateMembershipSystem.cs

## User Story

> Acting as **a Cities: Skylines 2 mod developer**, I want **prefab-first AdrNetKind classification persisted on ADR name-aggregates**, so that I **later mix gates, seeds, and per-kind labels can key off a stable stored kind instead of unreliable Game.Net.Road**.

---

## Background

First task of epic adr-net-kind (catalog RefsLibrary 24). Add AdrNetKind enum and TryGetNetKind from prefab *Data (RoadData / PathwayData / TrackData / WaterwayData / TaxiwayData). Persist kind on ADRNameAggregate at create/fork. Do not implement cross-kind reject (NET-02), NetNameData prefabs (NET-03), or road-only seed UI (NET-04/0086) in this task. Never sole-gate on HasComponent Game.Net.Road. Train|Tram maps to Train.

---

## Definition of Ready (DoR)

- [ ] RefsLibrary 22–24 accepted; AdrNetKind table and resolve order locked
- [ ] ACT-03a+ ADR name-aggregate components and membership create/fork exist
- [ ] Compile path for ADRNameAggregate + AdrNameAggregateMembershipSystem
- [ ] No sprint required yet — backlog N until sprint pull

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] AdrNetKind enum exists: None, Road, Pathway, Train, Tram, Subway, Waterway, Taxiway
- [ ] TryGetNetKind(Entity edge) resolves from prefab *Data in locked order; RoadData wins regardless of edge Road presence/absence
- [ ] TrackData maps: Subway→Subway; Tram-only→Tram; Train or Train|Tram→Train
- [ ] Kind is stored on ADRNameAggregate (or equivalent always-present host field) and ISerializable with version discipline
- [ ] CreateAggregate / ForkFromVanilla / first-member paths set stored kind from seed edge(s)
- [ ] Empty aggregate destroy clears kind with the entity
- [ ] No mix-gate reject, no NetNameData prefab creation, no GetAggregateName/seed UI changes in this task
- [ ] Compile succeeds after user builds solution

---

## Implementation Notes

1. Add AdrNetKind enum (BelzontAdr/Utils or Components — match repo convention)
2. Add TryGetNetKind helper reading PrefabRef then RoadData/PathwayData/TrackData/WaterwayData/TaxiwayData
3. Change ADRNameAggregate from empty tag to carry AdrNetKind (or adjacent always-present component); serialize like ADRNameAggregateHighwayData
4. Set kind in CreateAggregate(edges) / TryForkFromVanilla* from first/seed edge; reject or leave None only if out-of-scope prefab (document choice)
5. Maintenance backfill for pre-NET-01 saves: classify from first member when kind is None
6. Do not mutate vanilla AggregateSystem
7. Catalog: 24-adr-net-kind-roadmap.md NET-01
8. Added BelzontAdr/Utils/AdrNetKind.cs: AdrNetKind enum (None/Road/Pathway/Train/Tram/Subway/Waterway/Taxiway) + AdrNetKindUtil.TryGetNetKind from PrefabRef *Data (RoadData first; never HasComponent Road). TrackTypes: Subway alone→Subway; Tram alone→Tram; Train or Train|Tram→Train. Out-of-scope prefabs → false/None.
9. ADRNameAggregate upgraded from IEmptySerializable tag to ISerializable v0 carrying m_Kind; CreateDefault() = None. CreateAggregate uses AddComponentData(CreateDefault()).
10. CreateAggregate(edges) / TryForkFromVanillaSeed / AddMemberCore assign kind when still None (first classifiable edge). EnsureNetKind() backfills from first member; maintenance calls it alongside EnsureHighwayData. Cross-kind reject deferred to NET-02.
11. Awaiting user solution compile for DoD compile item. Validate pre-NET-01 save load (empty→ISerializable upgrade).

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Existing ADR aggregates load without kind and break later gates | high | Backfill on maintenance from first member edge; default None until classified |
| Wrong TrackTypes mapping for Train|Tram | medium | Lock Train|Tram→Train in DoD; playtest dual-track prefabs in NET-05 |

---

## Related Tasks

### Depends on

- [0063] ACT-03a-ADR-name-aggregate-components-serialize

### Is dependent for

- [0086] NET-04-Road-only-seeds-namesets-prefab-RoadData
- [0088] NET-02-Membership-Road-Grouping-mix-gate
- [0089] NET-03-Per-kind-ADR-NetNameData-outline-by-kind
### Is related to

- [0079] HW-03b-Road-Grouping-tool-options-parity
- [0085] HW-03c-Gate-road-seeds-namesets-to-roads-only

### Groups with

- NET-02-Membership-Road-Grouping-mix-gate
- NET-03-Per-kind-ADR-NetNameData-outline-by-kind
- NET-04-Road-only-seeds-namesets-prefab-RoadData
- NET-05-Playtest-matrix-smoke-notes

---

## Other Notes

Epic adr-net-kind. Size M. Order 1. Backlog N — ready for sprint after review. Next: NET-02 mix gate. Sprint init deferred.
