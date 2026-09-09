# Road Grouping overlay render system

*Added: 2026-09-09 · Companion to [09](09-road-grouping-tool-spec.md) · Game.dll build GUID `b5fd0b9582554f3c83207d8d5d7a19a0`*

## Verdict

**Use a dedicated ADR system** (`AdrRoadGroupingOverlayRenderSystem`) that draws into `OverlayRenderSystem.Buffer`. Do **not** use vanilla `Highlighted` / CreationDefinition wash for membership or context lines — that path is too transparent, wrong ownership for edit feedback, and conflicts with DefaultTool H1 (out of scope this wave).

Number range “cards” are **React** UI ([09](09-road-grouping-tool-spec.md)), not overlay text. `Buffer.DrawText(Entity, …)` is name-entity based and is **not** used for cards.

## Why not Highlighted

| Approach | Fit for Road Grouping? |
|----------|------------------------|
| `Highlighted` + `BatchesUpdated` (WE picker hover) | Soft outline wash; low alpha; not opaque #00FF00 / #006600; couples to net batch rebuild |
| CreationDefinition → GenerateAggregatesSystem → Highlighted (DefaultTool H1) | Selection glow only; no node end-circles; not ADR-owned; **out of wave** |
| `OverlayRenderSystem.Buffer` (`DrawCurve` / `DrawCircle`) | Opaque fill=outline; ADR-owned; matches `AreaBorderRenderSystem` / `GuideLinesSystem` |

BelzontWE’s `ChangeHighlighting_MainThread` is **not** mirrored for Road Grouping.

## System placement

| Item | Choice |
|------|--------|
| Class | `AdrRoadGroupingOverlayRenderSystem` |
| Phase | Rendering; schedule **before** `OverlayRenderSystem` (`UpdateBefore` / ordered `UpdateAt` as Game overlay producers do) |
| Buffer | Obtain `OverlayRenderSystem` buffer each frame and enqueue draws |
| Gate | Draw only when `ToolSystem.activeTool` is `AdrRoadGroupingTool` **and** `selectedAggregation != Entity.Null`; otherwise draw nothing |

Reads selection from `AdrRoadGroupingTool` and/or `AdrRoadGroupingController` (same pattern as WE tooltip reading hover from the tool).

## Colors and style

| Layer | Color | Draw |
|-------|-------|------|
| Belonging segments (selected group members) | **#00FF00** | `DrawCurve` — fill = outline, alpha **1** |
| Non-belonging segments (same vanilla Aggregate remainder / context) | **#006600** | `DrawCurve` — fill = outline, alpha **1** |
| Terminal end-nodes | **#00FF00** | `DrawCircle` — fill = outline, alpha **1** |

- Starting line width: **~2–4 m** world units; tune in playtest against road scale.
- Prefer non-projected style unless playtest shows depth issues (Game `StyleFlags` as needed).
- No transparency for these membership lines.

## Geometry (v0 — vanilla Aggregate)

Input: `selectedAggregation` = vanilla Aggregate entity.

### Member / context edges

1. Read `DynamicBuffer<AggregateElement>` on the Aggregate.
2. For each element’s `m_Edge`, read `Curve.m_Bezier` (and length if needed).
3. `DrawCurve` along the Bezier centerline.
4. **v0 belonging vs non-belonging:** with vanilla Aggregate as the sole membership source, all edges in that Aggregate’s buffer are “belonging” (**#00FF00**). Non-belonging **#006600** becomes meaningful when membership is a subset (ADR thin contract / stub membership set). Until then, still document the color language; draw **#006600** only when a remainder set exists (e.g. hovered Aggregate vs focused membership after T4, or a temporary stub set during T2 playtests).

When the tool later tracks an explicit membership subset on the same Aggregate, belonging = members (**#00FF00**), remainder on that Aggregate’s buffer = non-belonging (**#006600**).

### Terminal nodes (end circles)

Resolve chain endpoints from AggregateElement order + edge connectivity:

1. For each edge, read `Edge` (or equivalent) for `m_Start` / `m_End` node entities.
2. Count how many times each node appears among edges in the AggregateElement buffer.
3. Nodes with **degree 1 within the Aggregate’s edge set** are terminals → `DrawCircle` at node world position (`Node` / transform position).
4. Tool hit-testing for LMB endpoint drag uses these same terminal nodes (and end-circle radius), not edge midpoints.

Cross-check with net `ConnectedEdge` buffers if AggregateElement order is non-linear; prefer connectivity-derived terminals over assuming buffer[0]/buffer[^1] alone when the chain may be branched (vanilla Aggregates are typically linear — note exceptions in playtest).

## Drag preview (follow-up)

v0 **may skip** overlay preview meshes and only log would-add / would-remove segments ([09](09-road-grouping-tool-spec.md)).

When added later (same system):

| Preview | Suggested style |
|---------|-----------------|
| Would-add | Brighter or dashed #00FF00 (or distinct hue) |
| Would-remove | Dim / alternate (e.g. toward #006600 or dashed) |

## Performance

- Draw **only** the selected Aggregate’s edges (and optional adjacent remainder when defined) — **no city-wide scan**.
- Skip all work when tool inactive or selection null.
- Overlay buffer is per-frame ephemeral; no persistent mesh entities required for v0.

## API references (Game.dll)

- `Game.Rendering.OverlayRenderSystem.Buffer.DrawCurve`
- `Game.Rendering.OverlayRenderSystem.Buffer.DrawCircle`
- `Game.Rendering.OverlayRenderSystem.Buffer.DrawText` — **not** for number cards
- Patterns: `AreaBorderRenderSystem`, `GuideLinesSystem`

## Relation

- [09](09-road-grouping-tool-spec.md) — tool input / selection / cards
- [11](11-road-grouping-tool-roadmap.md) — T1 implements this system
- [08](08-association-tool-first-ux.md) — world-draw-first product direction
- [06](06-labels-vs-vanilla-sync-and-harmony.md) — H1 remains future; do not conflate with this overlay
