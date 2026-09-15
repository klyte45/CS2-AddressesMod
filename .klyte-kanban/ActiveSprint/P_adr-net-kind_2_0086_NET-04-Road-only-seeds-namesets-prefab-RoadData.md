**Start time:** 2026-09-14 22:17 -0300
# [0086] NET-04-Road-only-seeds-namesets-prefab-RoadData

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/22-game-net-edge-kinds-and-aggregation.md
RefsLibrary/2026090907_custom-road-name-aggregation/23-adr-net-kind-gate-labels-seeds.md
RefsLibrary/2026090907_custom-road-name-aggregation/24-adr-net-kind-roadmap.md (NET-04)
BelzontAdr/Overrides/AdrNameSystemOverrides.cs GetAggregateName
BelzontAdr/Systems/AdrSelectionInfoPanelSystem.cs TryAppendAggregateSeedOption
Overflown from [0085] HW-03c-Gate-road-seeds-namesets-to-roads-only (ActiveSprint Z)

## User Story

> Acting as **a player naming networks with ADR**, I want **road nameset seeds and road-prefix naming only on vehicle-road aggregates classified by prefab RoadData**, so that I **streets without edge Game.Net.Road still work; tracks/pathways are not renamed by road seeds**.

---

## Background

Continuation of Sprint 014 task 0085 (HW-03c), which was Z-overflowed mid-sprint because seed-only scope was too narrow and edge Game.Net.Road gating is wrong (playtest: streets/avenues may lack Road). Full design: RefsLibrary 22–24. Catalog NET-04 under epic adr-net-kind: road nameset/seed/SIP gating via prefab RoadData / AdrNetKind.Road. Depends on NET-01 [0087] classifier; prefer NET-02 [0088] mix gate first.

---

## Definition of Ready (DoR)

- [ ] RefsLibrary 22–24 accepted
- [ ] NET-01 classifier available (or landed in same pull)
- [ ] HW-03b seed surfaces understood
- [ ] Compile path for AdrNameSystemOverrides + AdrSelectionInfoPanelSystem

---

## Acceptance Criteria / Definition of Done (DoD)

- [x] Classifier uses prefab RoadData / stored AdrNetKind.Road — never HasComponent Road as sole gate
- [x] GetAggregateName applies road namesets/prefixes only for AdrNetKind.Road; no TrackData→RoadData fake for road naming
- [x] TryAppendAggregateSeedOption / SIP / Road Grouping show road seed UX only for Road kind
- [x] Street and avenue edges without edge Game.Net.Road still get road seeds when prefab has RoadData
- [x] Street↔highway mix remains allowed for Road kind; cross-kind seed path remains closed
- [x] District/citizen/household seed paths unchanged
- [x] Compile + playtest road seed; non-road kinds use ADR netKind locale defaults and no road seed UI
- [x] Docs 22–24 referenced; coordinate with NET-01…03 when those board items exist

---

## Implementation Notes

1. Implement after or with NET-01 (AdrNetKind + TryGetNetKind from prefab *Data)
2. Prefer NET-02 membership gate already landed
3. GetAggregateName: early-out unless kind is Road; delete TrackData→RoadData fake
4. TryAppendAggregateSeedOption: hasCustomNameList only when AdrNetKind.Road
5. Replace ADR HasComponent<Road> seed/address gates with RoadData or stored kind
6. Catalog: 24-adr-net-kind-roadmap.md NET-04
7. Default names for non-road aggregations shall be the localized aggregation type (Railway, Pathway, etc)
8. GetAggregateName: early-out unless AdrNetKind.Road (stored kind or prefab RoadData on ref edge); highway branch only after Road gate; deleted TrackData→RoadData fake; non-road defers to vanilla NameSystem (localized AggregateNet type).
9. AdrNetKindUtil: IsRoadEdge + TryResolveOwnerKind helpers.
10. TryAppendAggregateSeedOption: hasCustomNameList only when kind is Road. Aggregated edge SIP RoadAggregation typed via IsRoadEdge (not HasComponent Road). Highway option gated to Road kind. Station nearby-walk + GetMainReferenceAggregate use IsRoadEdge.
11. Docs 23/24 notes updated. Awaiting user compile + playtest: road seed still works without edge Road; non-road keeps vanilla name and no seed dice.
12. Non-road default names: ADR locale keys K45::ADR.main[netKind.*] via Name.LocalizedName (vanilla AggregateNet has no usable translation). Pending i18n merge: BelzontAdr/i18n/netKind_entries.txt (Road/Pathway/Train/Tram/Subway/Waterway/Taxiway; en-US + pt-BR).
13. User validated. Non-road defaults use K45::ADR.main[netKind.*]; i18n merged into i18n.csv/xlsm.

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Tracks that used road namesets revert to vanilla | medium | DoR + playtest; per-kind namesets deferred |
| Partial UI hide without GetAggregateName gate | high if UI-only | Name-system gate first; match UI to backend |

---

## Related Tasks

### Depends on

- [0087] NET-01-Classifier-stored-kind-on-ADRNameAggregate
- [0088] NET-02-Membership-Road-Grouping-mix-gate
### Is dependent for



### Is related to

- [0085] HW-03c-Gate-road-seeds-namesets-to-roads-only
- [0079] HW-03b-Road-Grouping-tool-options-parity
- [0080] HW-04-Naming-highway-override-branch
- [0089] NET-03-Per-kind-ADR-NetNameData-outline-by-kind
- [0090] NET-05-Playtest-matrix-smoke-notes
### Groups with

- HW-03b-Road-Grouping-tool-options-parity
- HW-04-Naming-highway-override-branch

### Overflown from

- [0085] HW-03c-Gate-road-seeds-namesets-to-roads-only

---

---

## Other Notes

Size M. Catalog NET-04. Deferred from Sprint 014. Overflown from [0085] Z-cancelled HW-03c.
