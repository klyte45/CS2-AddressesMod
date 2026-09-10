# Road Grouping tool — specification

*Added: 2026-09-09 · Implementation base for the net-only **Road Grouping** tool · Game.dll build GUID `b5fd0b9582554f3c83207d8d5d7a19a0` · Scaffold reference: BelzontWE `WEWorldPickerTool` / `WEWorldPickerController`*

Supersedes open questions in [08](08-association-tool-first-ux.md) under the product name **Road Grouping** (`AdrRoadGrouping*`). Overlay draw contract: [10](10-road-grouping-overlay-render-system.md). Build order: [11](11-road-grouping-tool-roadmap.md).

## Purpose

Dedicated ADR gameplay tool that lets the player focus a road aggregation in the world, see membership overlays, inspect per-segment house-number ranges, and (later) edit membership. **v0** uses **vanilla** `Aggregate` as the data source and **log-only stubs** for membership mutations.

## Naming

| Term | Meaning |
|------|---------|
| Road Grouping | Feature / UX name |
| `AdrRoadGroupingTool` | `IBelzontToolSystem` subclass |
| `AdrRoadGroupingController` | Bindings / UI state companion |
| `selectedAggregation` | Focused Aggregate (or later ADR road-group) **Entity** |
| `toolID` | `K45_ADR_AdrRoadGroupingTool` (stable string for VUIO) |

## BelzontWE → ADR mapping

Copy WE structure; do not invent a parallel activation style.

| WE | ADR |
|----|-----|
| `WEWorldPickerTool` | `AdrRoadGroupingTool` under `BelzontAdr/Tools/` |
| `WEWorldPickerController` | `AdrRoadGroupingController` |
| `WEUISystem` + ModData keybind | Optional ADR keybind + thin UI system |
| Extend `useToolOptionsVisible` + `MouseToolOptions` when `tool.activeTool$.id == "K45_WE_WEWorldPickerTool"` | Same module paths; id `K45_ADR_AdrRoadGroupingTool` |
| `Highlighted` for hover pick | **Not used** — overlay only ([10](10-road-grouping-overlay-render-system.md)) |
| `Select()` / `RequestDisable()` | Same |
| `DebugSystemOverrides` for abstract `ToolBaseSystem` | Already in ADR Commons — keep |

### WE patterns to lock

- Override `toolID` = `K45_ADR_AdrRoadGroupingTool`.
- `Enabled = false` in `OnCreateWithBarrier`; activate only via `Select()`.
- Apply = LMB, cancel = RMB via `ProxyAction` from mod settings (WE mapping).
- Clear selection + disable actions in `OnStopRunning` / `RequestDisable`.
- Split **tool** (input/raycast) vs **controller** (bindings/UI state). Overlay system reads selection from tool or controller.

## Architecture

```text
enableTool / keybind / ADR UI
        │
        ▼
AdrRoadGroupingTool  ──writes──►  AdrRoadGroupingController
  raycast / LMB / drag              MultiUIValueBinding:
  hover edge/node                   selectedAggregation
  drag session                      showNumberCards
        │                           segmentCardPayload[]
        │
        ├──► AdrRoadGroupingOverlayRenderSystem (Rendering)
        └──► VUIO MouseToolOptions + React number cards
```

## Data source (v0)

| Item | Rule |
|------|------|
| Hover eligibility | Edge has `Aggregated` (part of a vanilla Aggregation). Later: also ADR-owned membership. |
| Selection field | `Entity selectedAggregation` — vanilla Aggregate via `Aggregated.m_Aggregate` on clicked edge. Same field later holds ADR road-group entity. |
| Focus count | One aggregate/group at a time. |
| Exit | Clear `selectedAggregation` on tool disable / `OnStopRunning` (WE clears `CurrentEntity`). |

## Raycast

- Override `InitializeRaycast`: `TypeMask.Net` only (edges/nodes).
- Accept hover only when the hit edge has `Aggregated` (or is a node of such an edge chain used for end handles).
- No buildings, vehicles, or labels as primary pick targets.

## Input map

| Gesture | When | Effect (v0) |
|---------|------|-------------|
| **Plain LMB** | `selectedAggregation` **empty**; hovered eligible edge | Set `selectedAggregation` to that edge’s Aggregate Entity |
| **Plain LMB** | Group already selected | **No-op** on edges (edit gestures only). To pick another group: RMB/Esc clear selection, then plain LMB |
| **Ctrl+LMB** | Selected; hit **segment** | `Log.Info` membership toggle stub (edge + aggregate ids) |
| **LMB press/drag** | Selected; hit **terminal end-node** (green circle) | Endpoint drag stub; path walk; **one info log per distinct segment** that would be added or removed |
| **RMB** / **Esc** | Aggregation selected | Clear selection only (stay in tool) |
| **RMB** / **Esc** | No aggregation selected | Exit tool (`RequestDisable`) |
| `RequestDisable` / tool stop | Anytime | Clear selection and leave tool |

### Hit-target disambiguation

While a group is focused, apply/LMB is shared by Ctrl+toggle and endpoint drag:

1. **Ctrl+LMB** + raycast hit on **segment** → toggle stub.
2. **LMB** without Ctrl on **terminal end-node** (end-circle hit radius) → start/continue endpoint drag.
3. Otherwise plain LMB does nothing while selected.

Document end-circle vs edge hit-testing radii in implementation; playtest in T2.

## Endpoint drag path algorithm

Start only on **terminal nodes** of the selected aggregation (degree-1 within the current AggregateElement chain — see [10](10-road-grouping-overlay-render-system.md)).

While dragging toward a hovered **edge-node** (not the edge itself):

1. Walk connected net edges step by step.
2. **Prefer** a connected edge whose `Aggregated.m_Aggregate` equals the **previous step’s** Aggregate.
3. If **no** connected candidate shares that Aggregate, **hop** onto a connected edge belonging to a **different** Aggregate.
4. On release (and optionally continuous preview later): emit **one info log per distinct segment** that would be added or removed relative to the focused membership set.

v0 does **not** mutate membership — logs only.

## Controller bindings

| Binding | Role |
|---------|------|
| `selectedAggregation` | Focused Aggregate / road-group Entity |
| `showNumberCards` | Tool-options toggle |
| Segment card payload | Per belonging segment: edge entity, world midpoint, number range (low–high), stable sort key |
| `enableTool` call | → `tool.Select()` (optional Entity arg unused for v0 pick-in-world flow) |

## Number range UI cards

- **C#** computes per-segment house-number range using ADR numbering rules (same logic family as [`BuildingUtilsOverrides.GetAddress_Override`](../../../BelzontAdr/Overrides/BuildingUtilsOverrides.cs): aggregate walk, length, highway overrides, unit multiplier → odd numbers at segment ends).
- **React** world-anchored cards (screen-project midpoints). Not `OverlayRenderSystem.DrawText` (name-entity based).

### Visibility

Show a card when the segment midpoint projects inside **120% of the viewport** (10% margin beyond each screen edge).

### Refresh triggers

| Trigger | Throttle |
|---------|----------|
| **Aggregation change** (LMB select, RMB cancel/clear, later Ctrl+LMB / endpoint drag that alters membership list) | Immediate invalidate |
| **Camera position change** | Only when camera moved **and** `frameCounter % 8 == 0` |

Between refreshes: reuse **frontend cache** (segment list + last projected screen coords + visibility).

### Sort / priority

**Stable sort** by fixed key (edge entity index / AggregateElement buffer index) so overlap stack order is consistent every frame. Frontend can rely on that ordering for cache identity.

### Tool options

VUIO panel (when tool active) exposes `showNumberCards` toggle; later hooks for flags/overrides are out of v0 stub scope.

## Overlay requirements (summary)

Full contract in [10](10-road-grouping-overlay-render-system.md).

When `selectedAggregation` is set and Road Grouping tool is `activeTool`:

- Opaque centerlines on belonging segments: **#00FF00**.
- Opaque centerlines on non-belonging segments in the same vanilla Aggregate context: **#006600**.
- Terminal **#00FF00** circles on aggregation end **nodes**.

No vanilla `Highlighted` for this feature.

## Activation

1. Controller `enableTool` call binder → `AdrRoadGroupingTool.Select()`.
2. ADR UI control (settings / dedicated button — same spirit as WE top-left / panel entry).
3. Optional ModData keyboard binding (WE `Ctrl+Shift+W` style).

Entry is **not** SIP.

## Non-goals (v0 / this wave)

| Out | Notes |
|-----|--------|
| Real membership add/remove / gap-fill | Logs only until T5; rules in [07](07-membership-maintenance-edge-cases.md) |
| ADR thin contract as hard dependency | May land in parallel; tool v0 stubs on vanilla Aggregate |
| SIP / DefaultTool H1 | [06](06-labels-vs-vanilla-sync-and-harmony.md) later |
| World name labels / forked label pick | Separate epic |
| Highway migrate | Separate epic |
| Multi-select aggregates | Single focus only |

## Relation

- [08](08-association-tool-first-ux.md) — product direction (filename historical; feature name = Road Grouping)
- [10](10-road-grouping-overlay-render-system.md) — overlay system
- [11](11-road-grouping-tool-roadmap.md) — phases T0–T5
- [04](04-parallel-system-design-notes.md) — ADR components for T4+
- BelzontWE: `BelzontWE/Tools/WEWorldPickerTool.cs`, `BelzontWE/Controllers/WEWorldPickerController.cs`, `_Frontends/UI/k45-we-vuio/src/toolOptions/WriteEverywhereToolOptions.tsx`
