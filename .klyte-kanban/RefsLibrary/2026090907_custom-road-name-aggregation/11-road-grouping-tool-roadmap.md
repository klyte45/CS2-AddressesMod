# Road Grouping tool — implementation roadmap

*Added: 2026-09-09 · Updated: 2026-09-10 · Build order after [09](09-road-grouping-tool-spec.md) / [10](10-road-grouping-overlay-render-system.md)*

## Goal

Ship a playable **Road Grouping** tool scaffolded like BelzontWE, with opaque overlay feedback, then bind to **full ADR aggregation** (not a thin contract) and real mutations.

## Phase table

| Phase | Outcome | Status |
|-------|---------|--------|
| **T0** | Scaffold: tool + controller + VUIO + net raycast + Aggregated hover + plain LMB select | **Shipped** |
| **T1** | Overlay — belonging / pick-preview / end circles (route-ribbon; not `Highlighted`) | **Shipped** |
| **T2** | Ctrl+LMB / endpoint-drag **logs**; fixed pin; path preview colors | **Shipped** |
| **T3** | Segment number cards + tool-options toggle | **Shipped** |
| **T4** *(superseded)* | Was: thin contract swap | → **ACT-03a…ACT-03c / ACT-04** ([13](13-activation-sprint-roadmap.md)) |
| **T5** *(superseded)* | Was: real mutations + gap-fill | → **ACT-05 / ACT-06** |

## Post-T3: activation sprint

Do **not** implement a separate “thin contract.” Use full architecture in **one** sprint ([13](13-activation-sprint-roadmap.md)):

| Order | Task | Maps from | See |
|-------|------|-----------|-----|
| 1 | ACT-01 | Strip Aggregate highway | [12](12-activation-change-map.md) §B |
| 2 | ACT-02 | Markers visual-only | [12](12-activation-change-map.md) §C |
| 3 | ACT-03a | ADR components + serialize | [12](12-activation-change-map.md) §A1 |
| 4 | ACT-03b | Membership service CRUD | [12](12-activation-change-map.md) §A2 |
| 5 | ACT-03c | ForkFromVanilla + ADR seeding | [12](12-activation-change-map.md) §A3 |
| 6 | ACT-04 | Name/address prefer ADR | [12](12-activation-change-map.md) §D |
| 7 | ACT-05 | Tool real mutations | [12](12-activation-change-map.md) §E |
| 8 | ACT-06 | Membership maintenance | [07](07-membership-maintenance-edge-cases.md) |

## Dependency notes (historical T0–T3)

```text
T0 ──► T1 ──► T2     (SHIPPED)
 │
 └──► T3             (SHIPPED)

Activation (one sprint): ACT-01 → ACT-02; ACT-01 → ACT-03a → ACT-03b → ACT-03c → ACT-04 → ACT-05 → ACT-06
```

## Suggested code locations

| Piece | Path |
|-------|------|
| Tool | `BelzontAdr/Tools/AdrRoadGroupingTool.cs` |
| Controller | `BelzontAdr/Controllers/AdrRoadGroupingController.cs` |
| Overlay | `BelzontAdr/Systems/AdrRoadGroupingOverlayRenderSystem.cs` |
| VUIO | `_Frontends/UI/k45_adr_vuio` — Road Grouping tool options + number cards |

## Acceptance sketches (T0–T3 — shipped)

### T0–T2

Tool id `K45_ADR_AdrRoadGroupingTool`; Aggregated-only hover; select / cancel; overlays; Ctrl+LMB + endpoint-drag logs; fixed pin.

### T3

- Toggle shows cards for segments whose midpoints fall in 120% viewport.
- Cards refresh on aggregation change; viewport projection; stable order; frontend cache.
- **Shipped (0057):** `AdrRoadAddressNumbering` + `AdrRoadGroupingController.SegmentCards`; VUIO `RoadGroupingNumberCardsHost`.

### T4 / T5 (do not implement as thin contract)

See activation **ACT-03a…ACT-06** in [13](13-activation-sprint-roadmap.md).

## Out of this roadmap

- SIP / DefaultTool H1
- World name labels / forked `RaycastLabelsJob`
- Highway reattach via Addresses SIP (after Aggregate strip) — [20](20-highway-reattach-on-adr-aggregates.md) / [21](21-highway-reattach-roadmap.md)
- Creating Kanban board items (catalog in [13](13-activation-sprint-roadmap.md) only)

## Relation

- [09](09-road-grouping-tool-spec.md) — full tool contract
- [10](10-road-grouping-overlay-render-system.md) — overlay contract
- [08](08-association-tool-first-ux.md) — epic-order context
- [12](12-activation-change-map.md) — activation change map
- [13](13-activation-sprint-roadmap.md) — sprints + Kanban catalog
- [05](05-research-gaps-and-next.md) — older gap list (points here / 12–13)
