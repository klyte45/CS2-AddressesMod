# Vanilla road name-label pipeline

*Added: 2026-09-11 · Game.dll build GUID `d124bec16c0748da9ebf48366d9c1d68` (kcs2m backend cache)*  
*Earlier pack docs cited GUID `b5fd0b9582554f3c83207d8d5d7a19a0` — re-verify citations against the current GUID when porting.*

## Purpose

Document the **end-to-end vanilla road name label** path: prefab archetype → string bake (TMP) → placement along aggregate chains → batched mesh → draw → raycast pick. This is the subsystem ADR would take over for **names only** ([15](15-adr-citywide-name-label-takeover.md)). Membership stays in `AggregateSystem` ([01](01-vanilla-aggregation-overview.md)).

## Separation: membership vs labels vs arrows

| Concern | Owner systems | Components |
|---------|---------------|------------|
| Contiguous edge groups | `Game.Net.AggregateSystem` | `Aggregate`, `Aggregated`, `AggregateElement` |
| Name label bake / place / mesh | `Game.Rendering.AggregateMeshSystem` (name half) | `LabelMaterial`, `LabelExtents`, `LabelPosition`, `LabelVertex` |
| Name draw | `Game.Rendering.AggregateRenderSystem` (name loop) | Consumes mesh data from mesh system |
| One-way arrows | Same mesh/render systems (arrow half) | `ArrowMaterial`, `ArrowPosition`, … |
| Label click | `Game.Net.RaycastJobs.RaycastLabelsJob` via `RaycastSystem` | Reads `Aggregated` → aggregate’s `LabelPosition` / `LabelExtents` |

Names and arrows share `AggregateMeshSystem` / `AggregateRenderSystem`, but `AggregateRenderSystem.Render` already branches: when `ToolSystem.activeTool.requireNetArrows`, it draws **arrows only** and **returns without drawing names**. Name-only takeover is therefore structurally clean.

## Prefab → archetype

`Game.Prefabs.NetLabel` (component on `AggregateNetPrefab`):

| Field / hook | Role |
|--------------|------|
| `m_NameMaterial` | Source material for name glyphs |
| `m_NameColor` / `m_SelectedNameColor` | Written into `NetNameData` |
| `GetPrefabComponents` | Adds `NetNameData` |
| `GetArchetypeComponents` | Adds `LabelMaterial`, `LabelExtents`, `LabelPosition`, `LabelVertex` on **aggregate instances** |

`Game.Prefabs.NetNameData` (on prefab entity): `m_Color`, `m_SelectedColor`, `m_MaterialIndex` (assigned when mesh system initializes prefab materials).

## Per-aggregate label data

| Type | Kind | Role |
|------|------|------|
| `Game.Net.LabelMaterial` | Shared component | `m_Index` → material bucket in mesh system |
| `Game.Net.LabelExtents` | Component | `Bounds2 m_Bounds` from TMP line extents (text half-size in TMP space) |
| `Game.Net.LabelPosition` | Buffer | Placement samples along the road chain |
| `Game.Net.LabelVertex` | Buffer | Glyph (or multi-atlas) quads in TMP local space |

### `LabelPosition` fields

| Field | Role |
|-------|------|
| `m_Curve` | `Bezier4x3` — usually the edge curve where the label sits |
| `m_ElementIndex` | Index into aggregate element walk |
| `m_HalfLength` | Half of host edge length (for pick / warp) |
| `m_MaxScale` | Max camera-distance scale before label is culled / clamped |
| `m_IsUnderground` | Tunnel → underground material variant |

## String source

`Game.UI.NameSystem.GetRenderedLabelName(Entity)`:

1. Custom name if present (`TryGetCustomName`).
2. Else localization dictionary for `GetId(entity)`.
3. Else raw id string.

`AggregateMeshSystem.UpdateLabelVertices` calls this on the aggregate entity (or `Temp.m_Original` when applicable). ADR already influences names via NameSystem hooks; citywide takeover still uses this (or ADR equivalent) as the **string input** to TMP.

## Bake pipeline (`UpdateLabelVertices`)

Managed path inside `AggregateMeshSystem` (not Burst):

1. Get shared TMP via `OverlayRenderSystem.GetTextMesh()`.
2. Hardcoded layout for road names: `rectTransform.sizeDelta = (250, 100)`, `fontSize = 200`, `alignment = Center`.
3. For each updated aggregate with `LabelMaterial`:
   - Resolve `NetNameData` from `PrefabRef`.
   - Cache string per entity (`m_CachedLabels`); skip if unchanged.
   - `textMesh.GetTextInfo(renderedLabelName)`.
   - Copy **all** `TMP_MeshInfo` vertex blocks into `LabelVertex` (position, UV0, UV1, color, material indices).
   - For each distinct `mainTexture`, clone material from `m_OriginalMaterial` and `CopyFontAtlasParameters` (mainTexture + SDF floats).
   - Union TMP `lineInfo[].lineExtents` into `LabelExtents.m_Bounds`.

There is **no** hard character-count cap in this system; width is driven by TMP layout box + font size + string.

## Placement pipeline (`UpdateLabelPositionsJob`)

Burst `IJobChunk` on aggregates with `LabelExtents` + `AggregateElement` + `LabelPosition`:

1. Clear `LabelPosition` buffer.
2. Walk ordered `AggregateElement` edges; **split** label segments at complex nodes (`ConnectedEdge` length > 2) or topology breaks.
3. Per contiguous span, `AddLabels`:
   - Accumulate width-normalized length: `Σ (curve.Length / √max(1, width))`.
   - Spacing unit constant **`100f`** → how many labels along the span.
   - Pick best edges for samples; set `m_MaxScale = (roadWidth * 0.5) / textHalfHeight` from `LabelExtents`.
   - Redundancy pass scales down `m_MaxScale` for clustered samples (×0.25 / ×0.5).

Placement is **geometry + extents**, independent of the glyph string except via extents height.

## Mesh fill + draw

1. `FillNameMeshData` — Burst/job path warps `LabelVertex` quads along each `LabelPosition` curve, packs into `Mesh.MeshDataArray` per material bucket (submeshes for surface vs underground).
2. `AggregateRenderSystem.Render` (SRP `beginContextRendering`):
   - Skip if `RenderingSystem.hideOverlay`.
   - If tool needs net arrows → draw arrow meshes only, **return**.
   - Else loop name materials → `Graphics.DrawMesh` identity matrix, no shadows.

## Pick pipeline

`Game.Common.RaycastSystem` schedules `Game.Net.RaycastJobs.RaycastLabelsJob` (`IJobParallelForDefer`) when `TypeMask.Labels` is set:

1. Edge candidate → `Aggregated.m_Aggregate`.
2. Load `LabelExtents` + `LabelPosition` on that aggregate.
3. Camera-distance scale: `num = max(sqrt(distance * 0.0001), 0.01)`; skip if `num >= m_MaxScale * 0.95`.
4. Build oriented quads from extents × scale along curve; intersect ray; `m_Owner = aggregate`.

Job body is ~150 lines, leaf-isolated (no Aggregation maintenance). Prefer **fork + schedule**, not Harmony inside Burst ([06](06-labels-vs-vanilla-sync-and-harmony.md)).

## Refresh / dirty

- Locale dictionary change → `OnDictionaryChanged` → `AddComponent<Updated>(m_LabelQuery)`.
- ADR already dirties labels the same way: `AdrMainSystem.ResetRoadsCache` adds `Updated` to entities with `LabelMaterial` (does **not** reflect into `OnDictionaryChanged`).

## Other consumers of label geometry

| Consumer | Use |
|----------|-----|
| `SelectedInfoUISystem.GetAggregatePosition` | SIP camera / focus uses `LabelPosition` buffer midpoints |
| Area labels (`AreaBufferSystem`) | Twin TMP→mesh pattern for districts — **out of scope** for road takeover |

## Mental model

```text
AggregateNetPrefab + NetLabel
        │
        ▼
Aggregate entity + LabelMaterial / Extents / Position / Vertex
        │
        ├── NameSystem.GetRenderedLabelName ──► TMP GetTextInfo ──► LabelVertex + LabelExtents
        ├── AggregateElement chain ──► UpdateLabelPositionsJob ──► LabelPosition[]
        ├── FillNameMeshData ──► Mesh + materials
        ├── AggregateRenderSystem ──► Graphics.DrawMesh (names)
        └── RaycastLabelsJob ──► pick Aggregate
```

## Implication for ADR

Vanilla label generation is a **stable, self-contained rendering overlay** on top of Aggregation membership. Taking over **names** without touching `AggregateSystem` or arrows is feasible; see [15](15-adr-citywide-name-label-takeover.md).

## Relation

- [01](01-vanilla-aggregation-overview.md) — membership lifecycle
- [06](06-labels-vs-vanilla-sync-and-harmony.md) — earlier “thin ADR labels” sketch (superseded for epic scope by [15](15-adr-citywide-name-label-takeover.md))
- [15](15-adr-citywide-name-label-takeover.md) — citywide ADR ownership architecture
- [16](16-label-takeover-effort-and-risk.md) — effort / risk
- [17](17-label-extensions-text-images-look.md) — extension feasibility
