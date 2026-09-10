# Road Grouping tool — implementation roadmap

*Added: 2026-09-09 · Build order after [09](09-road-grouping-tool-spec.md) / [10](10-road-grouping-overlay-render-system.md) · Docs first; product code in these phases*

## Goal

Ship a playable **Road Grouping** tool scaffolded like BelzontWE, with opaque overlay feedback, log stubs for membership edits, then swap to ADR thin contract and real mutations.

## Phase table

| Phase | Outcome | Depends on |
|-------|---------|------------|
| **T0** | Scaffold after WE: `AdrRoadGroupingTool` + `AdrRoadGroupingController` + `toolID` `K45_ADR_AdrRoadGroupingTool` + `Select` / `RequestDisable` + VUIO `useToolOptionsVisible` / `MouseToolOptions` panel + net raycast + Aggregated hover filter + plain LMB → `selectedAggregation` (clear on stop) | Commons `IBelzontToolSystem`; WE as reference |
| **T1** | `AdrRoadGroupingOverlayRenderSystem` — opaque **#008800** centerlines + **#00BB00** end circles from selected Aggregate; **#004400** pick-preview of hovered Aggregate when selection empty; **#004400** remainder when subset exists; not `Highlighted` | T0 |
| **T2** | Ctrl+LMB segment toggle **log**; endpoint LMB-drag with Aggregate-preference path + **per-segment logs**; hit-target disambiguation (segment vs end-node) | T0–T1 |
| **T3** | Segment number-range helper (ADR rules / GetAddress family) + MultiUI binding + React cards: 120% viewport cull, camera-delta / **8-frame** throttle, aggregation-change invalidate, stable sort, frontend cache + tool-options toggle | T0 (+ selection); can parallel T2 |
| **T4** | Swap overlay/selection source to ADR thin contract ([04](04-parallel-system-design-notes.md)); keep same tool/controller surface and `selectedAggregation` field | Thin contract + T0–T1 |
| **T5** | Replace logs with real add/remove / gap-fill ([07](07-membership-maintenance-edge-cases.md)) | T4 + maintenance APIs |

## Dependency notes

```text
T0 ──► T1 ──► T2 ──► T5
 │         │
 │         └──► T4 ──► T5
 │
 └──► T3 (parallel with T2 once selection works)

Thin contract ──► T4 (not required for T0–T3)
```

- **T0→T1→T2** can proceed **before** the ADR thin contract (vanilla Aggregate stub).
- **T3** can parallel **T2** once selection exists.
- **T4** needs thin contract from [04](04-parallel-system-design-notes.md).
- **T0** explicitly ports WE wiring before ADR-specific gestures.

## Suggested code locations

| Piece | Path (indicative) |
|-------|-------------------|
| Tool | `BelzontAdr/Tools/AdrRoadGroupingTool.cs` |
| Controller | `BelzontAdr/Controllers/AdrRoadGroupingController.cs` (or Systems/) |
| Overlay | `BelzontAdr/Systems/AdrRoadGroupingOverlayRenderSystem.cs` |
| VUIO | `_Frontends/UI/k45_adr_vuio` — tool options extend (mirror WE `WriteEverywhereToolOptions`) |
| Number cards | Same frontend package; world-anchored React + cache |

Register overlay in mod `DoOnCreateWorld` Rendering phase before `OverlayRenderSystem`. Tool self-registers ToolUpdate via `IBelzontToolSystem`.

## Acceptance sketches (per phase)

### T0

- Tool appears as `activeTool` with id `K45_ADR_AdrRoadGroupingTool`.
- Only Aggregated nets hover; plain LMB with empty selection sets Aggregate Entity; RMB / stop clears.
- Tool options panel visibility tied to active tool id.

### T1

- With selection: opaque **#008800** centerlines + **#00BB00** end circles; no Highlighted wash for membership.
- With empty selection: #004400 pick-preview of the hovered Aggregated edge’s Aggregate; no hover → no draws.
- Inactive tool → no draws.

### T2

- Ctrl+LMB on segment → one info log (toggle intent).
- LMB-drag from terminal node → path prefers same Aggregate, hops when needed; one log per distinct would-add/remove segment.
- Mixed pivot (path walks tip then hops): both would-remove (members on path) and would-add (non-members); remove painted **#FF8800**, add **#00BB00**.

### T3

- Toggle shows cards for segments whose midpoints fall in 120% viewport.
- Cards refresh on aggregation change immediately; on camera move at most every 8 frames; stable order; cache between refreshes.

### T4

- Same UX; data from ADR road-group membership; #004400 remainder / hovered non-member vs #008800 members meaningful; #00BB00 add / #FF8800 remove / #00FF00 handle hover.

### T5

- Same gestures write real membership per [07](07-membership-maintenance-edge-cases.md); logs replaced or demoted to debug.

## Out of this roadmap

- SIP / DefaultTool H1
- World name labels / forked `RaycastLabelsJob`
- Highway migrate onto ADR aggregates
- Kanban board creation (separate ask)

## Relation

- [09](09-road-grouping-tool-spec.md) — full tool contract
- [10](10-road-grouping-overlay-render-system.md) — overlay contract
- [08](08-association-tool-first-ux.md) — epic-order context
- [05](05-research-gaps-and-next.md) — older gap list (tool-first reading)
