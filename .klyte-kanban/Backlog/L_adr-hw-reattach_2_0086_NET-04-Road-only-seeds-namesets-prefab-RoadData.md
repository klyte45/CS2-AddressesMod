# [0086] NET-04-Road-only-seeds-namesets-prefab-RoadData

**Developed by:** 

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

Continuation of Sprint 014 task 0085 (HW-03c), which was Z-overflowed mid-sprint because seed-only scope was too narrow and edge Game.Net.Road gating is wrong (playtest: streets/avenues may lack Road). Full design: RefsLibrary 22–24. This backlog item is catalog NET-04: road nameset/seed/SIP gating via prefab RoadData / AdrNetKind.Road. Depends conceptually on NET-01 (classifier) and prefer NET-02 (mix gate); create those board IDs when explicitly requested.

---

## Definition of Ready (DoR)

- [ ] RefsLibrary 22–24 accepted
- [ ] NET-01 classifier available (or landed in same pull)
- [ ] HW-03b seed surfaces understood
- [ ] Compile path for AdrNameSystemOverrides + AdrSelectionInfoPanelSystem

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Classifier uses prefab RoadData / stored AdrNetKind.Road — never HasComponent Road as sole gate
- [ ] GetAggregateName applies road namesets/prefixes only for AdrNetKind.Road; no TrackData→RoadData fake for road naming
- [ ] TryAppendAggregateSeedOption / SIP / Road Grouping show road seed UX only for Road kind
- [ ] Street and avenue edges without edge Game.Net.Road still get road seeds when prefab has RoadData
- [ ] Street↔highway mix remains allowed for Road kind; cross-kind seed path remains closed
- [ ] District/citizen/household seed paths unchanged
- [ ] Compile + playtest road seed; non-road kinds keep vanilla name and no road seed UI
- [ ] Docs 22–24 referenced; coordinate with NET-01…03 when those board items exist

---

## Implementation Notes

1. Implement after or with NET-01 (AdrNetKind + TryGetNetKind from prefab *Data)
2. Prefer NET-02 membership gate already landed
3. GetAggregateName: early-out unless kind is Road; delete TrackData→RoadData fake
4. TryAppendAggregateSeedOption: hasCustomNameList only when AdrNetKind.Road
5. Replace ADR HasComponent<Road> seed/address gates with RoadData or stored kind
6. Catalog: 24-adr-net-kind-roadmap.md NET-04

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Tracks that used road namesets revert to vanilla | medium | DoR + playtest; per-kind namesets deferred |
| Partial UI hide without GetAggregateName gate | high if UI-only | Name-system gate first; match UI to backend |

---

## Related Tasks

### Depends on



### Is dependent for



### Is related to

- [0085] HW-03c-Gate-road-seeds-namesets-to-roads-only
- [0079] HW-03b-Road-Grouping-tool-options-parity
- [0080] HW-04-Naming-highway-override-branch

### Groups with

- HW-03b-Road-Grouping-tool-options-parity
- HW-04-Naming-highway-override-branch

### Overflown from

- [0085] HW-03c-Gate-road-seeds-namesets-to-roads-only

## Other Notes

Size M. Catalog NET-04. Deferred from Sprint 014. Overflown from [0085] Z-cancelled HW-03c.
