# Road Grouping overlay render system

*Added: 2026-09-09 · Companion to [09](09-road-grouping-tool-spec.md) · Game.dll build GUID `b5fd0b9582554f3c83207d8d5d7a19a0`*

## Verdict

**Use the transport-route GPU path** (`RouteBufferSystem` SegmentData packing + `RouteRenderSystem` mesh / material / `DrawMeshInstancedIndirect`), not `OverlayRenderSystem.Buffer.DrawCurve`. Overlay absolute/projected curves cannot match elevated continuity the way route ribbons do.

Class remains `AdrRoadGroupingOverlayRenderSystem` (name historical); implementation mirrors route rendering.

## Why not Overlay / Highlighted

| Approach | Fit for Road Grouping? |
|----------|------------------------|
| `Highlighted` + `BatchesUpdated` | Soft wash; wrong ownership |
| `OverlayRenderSystem.DrawCurve` (Projected) | Terrain decal — bridges paint on grass |
| `OverlayRenderSystem.DrawCurve` (absolute) | FitQuad seams → gaps/overlaps at edge joins |
| **Route SegmentData + route material/mesh** | Same elevated ribbon + `AddNode` blobs as transport lines |

## System placement

| Item | Choice |
|------|--------|
| Class | `AdrRoadGroupingOverlayRenderSystem` |
| Phase | `SystemUpdatePhase.Rendering` (`UpdateAt`) — builds segment list |
| Draw | `RenderPipelineManager.beginContextRendering` (same as `RouteRenderSystem`) |
| Material | Clone of `RouteConfigurationPrefab.m_CarPathVisualization.m_Material` (belonging / preview / remove) |
| Mesh | Exact copy of `RouteRenderSystem.EnsureMesh` (68 verts / 192 indices) |
| Gate | Tool active + selected Aggregate → belonging ribbons + end handles + drag add/remove / non-member hover; no selection + hover Aggregate → pick-preview wash |

## Packing (mirror `RouteBufferSystem`)

1. Walk `AggregateElement` edges; read raw `Curve.m_Bezier`.
2. **Orient** for **house-number growth** (same as `BuildingUtilsOverrides.GetAddress_Override`): topology `IsInverted` XOR `isInverseAggregate` (chain ends vs highway `refStartPoint` / zero marker) XOR `ADRHighwayAggregationCacheData.reverseCounting`.
3. **Snap** oriented `a`/`d` to Edge `Node.m_Position` (Start→End or End→Start when inverted) so joins share exact node centers.
4. Skip near-zero length; `AddSegment` with continuous distance offsets and opacity from tangent/position continuity.
5. Match consecutive `m_SizeFactor` + `m_DividedOpacity` at joints; then `math.saturate` all opacities.
6. Absolute Overlay **circles** at node height: half-size pads on degree ≥ 2 joints; full-size drag handles on AggregateElement **chain termini** when selected (always — including loops / non–dead-end ends).

## GPU upload (must match Game)

- `SegmentData` is an **unmarked** sequential struct (same fields as private `RouteBufferSystem.SegmentData`).
- Buffer: `new ComputeBuffer(length, UnsafeUtility.SizeOf<SegmentData>())` — Game uses `sizeof(SegmentData)`; **do not hardcode 112**.
- Upload: `ConvertExistingDataToNativeArray` from list ptr + `SetData` (same as `RouteBufferSystem.GetBuffer`).
- Wrong stride → neon “vertex soup” across the map.

## Colors

Synced to `AdrRoadGroupingOverlayRenderSystem` constants:

| Layer | Color |
|-------|-------|
| Belonging (selected, base) | **#008800**, alpha 1, size × 1.333 (route style) |
| Highlights (end handles idle, endpoint-drag would-add) | **#00BB00**, same ribbon/DrawCircle path |
| End-handle hover | **#00FF00** (brighter/larger circle) |
| Hovered non-member (when selected) | **#004400**, same draw path, no end handles |
| Pick preview (hover, no selection) | **#004400**, same draw path, no end handles |
| Endpoint-drag would-remove | **#FF8800**, dedicated remove material/buffer |
| Remainder (non-belonging in same Aggregate context when subset exists) | **#004400** |

## Relation

- [09](09-road-grouping-tool-spec.md) — tool input / selection / cards
- [11](11-road-grouping-tool-roadmap.md) — T1 implements this system
