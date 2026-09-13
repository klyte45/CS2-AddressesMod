# ADR road name labels — game-update re-diff checklist

*Added: 2026-09-12 · LBL-06 [0075] · Baseline Game.dll GUID **`d124bec16c0748da9ebf48366d9c1d68`** · Architecture: [15](15-adr-citywide-name-label-takeover.md) · Pipeline: [14](14-vanilla-road-label-pipeline.md) · Effort: [16](16-label-takeover-effort-and-risk.md)*

## Purpose

After a Cities Skylines II Managed update, re-diff the surfaces ADR’s citywide **name-label takeover** depends on. Confirm kcs2m GUID first; then walk this list. Prefer MCP (`kcs2m_backend_search` / `kcs2m_backend_context`) — do not browse Managed dumps by hand.

## How to use

1. Note new GUID from `kcs2m_backend_*` / boot.config.  
2. For each row: search the listed symbols; compare signatures and call sites to ADR code paths.  
3. Smoke the dogfood script at the bottom.  
4. File new gaps as new tasks if they appear — Sprint 014 spills (LBL-07…09) were discarded.

## Surfaces

| # | Vanilla surface | ADR relies on | ADR code | Re-verify |
|---|-----------------|---------------|----------|-----------|
| 1 | `AggregateMeshSystem` name half (`UpdateLabelVertices`, `UpdateLabelPositions` / `AddLabels`, `FillNameMeshData`, prefab `NetNameData.m_MaterialIndex`) | Shutdown so vanilla never bakes/draws **names**; ADR forks bake/place/fill | `AdrRoadNameLabelOverrides`; `AdrRoadNameLabelMeshSystem`; `AdrRoadNameLabelPlacementJobs`; `AdrRoadNameLabelMeshFillJobs` | Prefix/early-out still hits name paths only; arrow half untouched |
| 2 | `AggregateRenderSystem.Render` name `DrawMesh` loop | Prefix skips vanilla name draw; ADR draws names | `AdrRoadNameLabelOverrides`; `AdrRoadNameLabelRenderSystem` | `hideOverlay`, cameras, surface + underground submeshes; `requireNetArrows` arrow branch still vanilla |
| 3 | `LabelVertex` / `LabelExtents` / `LabelPosition` / `LabelMaterial` layout (`IEmptySerializable`) | Runtime-only bake; SIP focus uses `LabelPosition` | Same as #1; SIP uses vanilla `SelectedInfoUISystem.GetAggregatePosition` | Struct fields/order; empty-serialize contract |
| 4 | `NetLabel` / `NetNameData` / AggregateNet prefab materials | Material index + TMP atlas clone + face/outline colors | `AdrRoadNameLabelMeshSystem` (`m_IsAdrStyle` ADR `#004400` outline vs vanilla black) | `_FaceColor` / `_OutlineColor`; `CopyFontAtlasParameters` |
| 5 | `Game.Net.RaycastJobs.RaycastLabelsJob` + `RaycastSystem` Labels schedule | Burst **clone** (not patched body); managed schedule hook | `AdrRaycastLabelsJob` / `AdrRaycastLabelsJobs`; `AdrRoadNameLabelPickOverrides` PerformRaycast **transpiler** | Owner = `ADRNameAggregated` else `Aggregated`; Areas district job untouched; IL log rule if transpiler changes |
| 6 | `DefaultToolSystem.CreateDefinitionsJob` AggregateElement copy + `GenerateAggregatesSystem` Highlighted | H1: rewrite definition membership before GenerateAggregates | `AdrAggregateHighlightRewriteSystem` (`UpdateBefore` GenerateAggregates, Modification1) | S1 remainder (skip `ADRNameAggregated`); S2 fill from `ADRNameAggregateElement`; Temp Aggregate still gets Select/Hidden |
| 7 | SIP `RoadSection` AggregateElement walk | S2 walk ADR buffer; S1 remainder filter | `AdrRoadNameLabelPickOverrides` + `AdrNameAggregateMembershipHelper` | Per-edge length/flow/wear math parity with [06](06-labels-vs-vanilla-sync-and-harmony.md) |
| 8 | `TitleSection.GetVirtualKeyboardLocaleKey` Aggregate → `"RoadName"` | ADR → `"RoadName"` | Pick overrides Prefix | Locale key only |
| 9 | `ImageSystem.GetInstanceIcon` Aggregate path | S2 icon via first ADR edge prefab | Pick overrides Prefix | Icon non-null for S2 |
| 10 | `ActionsSection` / `TrafficRoutesSystem` Aggregate gate + `FillTargetMapJob` AggregateElement | S2: `hasTrafficRoutes`; mirror `AggregateElement` + brief `Aggregate` tag for live routes | Pick overrides Prefix/Postfix | Routes appear for ADR selection; Aggregate tag not left stuck |
| 11 | `SelectedInfoUISystem` `LabelPosition` → `GetAggregatePosition` | S2 entity must carry `LabelPosition` | Placement on ADR owners; no Harmony needed if buffer present | Camera/focus on S2 select |
| 12 | Temp Aggregate label remap (`Temp.m_Original`, Select color, Hidden original) | Labels live on Temp Aggregate when original Hidden | Placement `includeAdrEdges` on Select/Create/Delete/Modify/Replace; bake uses `GetRenderedLabelName(original)` | S2 select keeps labels; selected color |

## Explicit non-surfaces (do not chase)

| Surface | Why |
|---------|-----|
| `AggregateSystem` Burst Validate/Combine | Out of epic scope |
| District / Areas label jobs | Untouched |
| Arrow mesh/draw | Must remain vanilla |
| Selection highlight tint (green) | Cancelled — vanilla `Highlighted` blue is fine |

## Known gaps (optional spill)

**None planned.** Sprint 014 drafts discarded after dogfood:

| Former spill | Disposition |
|--------------|-------------|
| LBL-07 Temp / net-tool label parity | Cancelled — symptoms not observed |
| LBL-08 H1 highlight membership | Shipped in LBL-05 |
| LBL-09 Mass-dirty performance | Cancelled — not needed |

## Dogfood script (LBL-06)

Baseline GUID: `d124bec16c0748da9ebf48366d9c1d68`.

1. **Load** dogfood city with ADR + Road Grouping associations.  
2. **Citywide names:** S1 (unassociated) + S2 (ADR) stretches visible; no vanilla name double-draw; arrows OK in net tools that require them.  
3. **Pick:** click S1 label → Aggregate SIP; click S2 → ADR aggregate SIP (stats, RoadName rename key, icon, focus).  
4. **Traffic routes:** enable on S2 selection — live paths over ADR edges.  
5. **Look:** ADR name outline dark green `#004400`; vanilla Aggregate names black outline.  
6. **Dirty:** locale change; nameset / custom rename; Road Grouping fork/edit/disassociate — labels refresh without permanent gaps/duplicates.  
7. **Temp:** select S2 (labels stay); place/upgrade net on an associated stretch if available — no stuck/dupe name meshes.  
8. **Tunnels / underground** view — underground name materials still readable.

## Relation

- [14](14-vanilla-road-label-pipeline.md) · [15](15-adr-citywide-name-label-takeover.md) · [16](16-label-takeover-effort-and-risk.md) · [18](18-label-takeover-sprint-roadmap.md) · [06](06-labels-vs-vanilla-sync-and-harmony.md)
