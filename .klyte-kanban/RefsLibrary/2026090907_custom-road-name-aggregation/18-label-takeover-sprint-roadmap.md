# Road name-label takeover — implementation roadmap & Kanban catalog

*Added: 2026-09-12 · Updated: 2026-09-12 (board IDs 0070–0075 in backlog) · Companion to [14](14-vanilla-road-label-pipeline.md)–[17](17-label-extensions-text-images-look.md) · Architecture: [15](15-adr-citywide-name-label-takeover.md) · Effort: [16](16-label-takeover-effort-and-risk.md) · SIP gates: [06](06-labels-vs-vanilla-sync-and-harmony.md)*

## Goal

Define the **implementation roadmap** for **citywide ADR road name-label takeover** (parity-only phase-1): draft epic, ordered tasks, and possible sprint planning. Board items are created via MCP when explicitly requested (not from this doc alone).

This is the catalog that replaces the vague “ADR world name labels + forked RaycastLabelsJob” backlog line in [13](13-activation-sprint-roadmap.md).

## Prerequisites

| Depends on | Why |
|------------|-----|
| Activation **ACT-03a…ACT-04** (at least) | ADR name-aggregate entities + NameSystem/GetAddress preference so S2 placement/pick have a real owner |
| Prefer **ACT-05 / ACT-06** done | Membership edits + maintenance dirty labels correctly; tool dogfood exists |
| Research pack [14](14-vanilla-road-label-pipeline.md)–[17](17-label-extensions-text-images-look.md) | Architecture / effort locked |

Do **not** start LBL-* before ADR components + name preference exist; otherwise citywide labels cannot resolve S2 owners.

## Scope (phase-1)

| In | Out |
|----|-----|
| Always-on shutdown of vanilla **name** mesh/draw | Touching `AggregateSystem` / arrows / district labels |
| ADR TMP bake + placement + mesh + draw (vanilla parity look) | Player-facing distance/width knobs |
| Forked `RaycastLabelsJob` + `RaycastSystem` schedule; S1/S2 owners | TMP shield sprites / multi-recipe / WE register product path ([17](17-label-extensions-text-images-look.md) later research) |
| **SIP Aggregate parity:** every SIP section that activates for vanilla `Aggregate` must also activate / work for **ADR name-aggregate** (S2) selection | BelzontWE live shared `Texture2D` (rejected) |
| Dirty/`Updated` refresh; Temp + underground parity | Achievements / infoview filter noise |
| Integration / update-diff checklist | |
| **Traffic-routes** live path for S2 (ActionsSection + `TrafficRoutesSystem`) | |
| ADR name material outline `#004400` | |

## Recommended sprint plan

Prefer **one primary sprint** for parity (matches [16](16-label-takeover-effort-and-risk.md) “~1 sprint, may spill”). Optional harden sprint was drafted; **no longer needed** after dogfood.

| Field | Value |
|-------|--------|
| Suggested epic label | **ADR road name-label takeover** |
| Suggested epic slug | `adr-road-labels` |
| Suggested primary sprint | **Sprint 013 — ADR road name labels (parity)** |
| Optional spill sprint | ~~Sprint 014~~ — **cancelled** (LBL-07…09 discarded) |
| Effort band | ~1.3–1.7× Road Grouping T0–T3; ~0.5–0.65× activation sprint ([16](16-label-takeover-effort-and-risk.md)) |

### Sprint 013 — parity (all LBL-01…LBL-06)

| Order | ID | Board | Title | Size | Depends | Outcome |
|-------|-----|-------|-------|------|---------|---------|
| 1 | LBL-01 | **0070** | Shutdown vanilla name path | S | ACT-04+ | Vanilla names never bake/draw; arrows still work |
| 2 | LBL-02 | **0071** | ADR TMP bake + extents + materials | M | LBL-01 | Strings → `LabelVertex` / `LabelExtents` on label owners |
| 3 | LBL-03 | **0072** | ADR placement job (dual membership) | L | LBL-02, ACT-03a+ | `LabelPosition` along vanilla or ADR element chains |
| 4 | LBL-04 | **0073** | ADR mesh batch + DrawMesh | M | LBL-02, LBL-03 | Citywide name meshes visible at parity |
| 5 | LBL-05 | **0074** | Forked pick + SIP Aggregate parity | L | LBL-03, ACT-04 | Click → S1/S2; SIP Aggregate gates + TrafficRoutes for ADR; outline `#004400` |
| 6 | LBL-06 | **0075** | Integration dogfood + update checklist | M | LBL-04, LBL-05 | Load/Temp/underground/locale dirty; SIP on S2; GUID re-diff notes |

```text
ACT-04+ (prereq)
    │
    ▼
LBL-01 ──► LBL-02 ──► LBL-03 ──► LBL-04 ──► LBL-06
                 │         │
                 │         └──► LBL-05 ──────┘
                 └── (materials shared with mesh)
```

### Sprint 014 — harden (discarded)

Optional spill tasks were drafted; **all discarded after dogfood** (2026-09-12):

| ID | Title | Disposition |
|----|-------|-------------|
| LBL-07 | Temp / net-tool label parity | **Cancelled** — stuck/dupe/missing Temp labels not observed; LBL-03…05 Temp paths sufficient |
| LBL-08 | DefaultTool H1 highlight membership | **Shipped in LBL-05** — never opened as spill |
| LBL-09 | Mass-dirty performance | **Cancelled** — not needed |

Historical note (LBL-07): would have chased place/upgrade/replace/cancel ghost or stuck name meshes beyond Select Temp parity. Re-open only if those symptoms appear later.

### Later (not this epic)

| Theme | Doc | When |
|-------|-----|------|
| TMP sprite shields / multi-recipe / look knobs / WE copy-register | [17](17-label-extensions-text-images-look.md) | After later **product** research |
| Highway reattach via Addresses SIP + tool options | [20](20-highway-reattach-on-adr-aggregates.md) / [21](21-highway-reattach-roadmap.md) | Independent of labels |

---

## Kanban catalog

Epic label: **`ADR road name-label takeover`**  
Epic slug: **`adr-road-labels`**

Board IDs assigned **2026-09-12** in backlog (`N`, epic `adr-road-labels`) — ready for Sprint 013 after review. Do not `sprint init` until review.

### Epic

| Field | Value |
|-------|--------|
| Label | `ADR road name-label takeover` |
| Slug | `adr-road-labels` |
| Summary | Always-on citywide ADR ownership of road **name** labels: disable vanilla name mesh/draw; ADR TMP bake, placement, mesh, draw; forked label pick with S1/S2; SIP sections that activate on vanilla Aggregate also available for ADR name-aggregate; parity look only |
| Depends | Activation ACT-03a…ACT-04 (prefer ACT-06 complete) |
| Acceptance | Vanilla road names gone; ADR labels on all aggregatable roads; arrows OK; click selects correct Aggregate / ADR aggregate; SIP Aggregate-gated sections work for S2; locale/name dirty refreshes; no AggregateSystem patches |

### Tasks (Sprint 013)

| ID | Board ID | Title | Order | Depends | Description |
|----|----------|-------|-------|---------|-------------|
| LBL-01 | **0070** | Shutdown vanilla name path | 1 | ACT-04+ [0066] | Managed Harmony: Prefix/early-out `AggregateMeshSystem` **name** half (`UpdateLabelVertices` / `UpdateLabelPositions` / `FillNameMeshData`) while keeping arrow updates; Prefix `AggregateRenderSystem.Render` name `DrawMesh` loop; leave `requireNetArrows` arrow branch. Always-on with ADR load. Verify arrows + tools still work. See [15](15-adr-citywide-name-label-takeover.md). |
| LBL-02 | **0071** | ADR TMP bake + LabelExtents + materials | 2 | LBL-01 [0070] | Port vanilla `UpdateLabelVertices` pattern: `NameSystem.GetRenderedLabelName` (or ADR-resolved entity) → TMP `GetTextInfo` with parity layout (`sizeDelta` 250×100, `fontSize` 200, center); fill `LabelVertex`; clone materials + `CopyFontAtlasParameters`; write `LabelExtents`. Wire dirty via `Updated` (extend `AdrMainSystem.ResetRoadsCache` pattern to ADR owners as needed). Cache string changes. See [14](14-vanilla-road-label-pipeline.md). |
| LBL-03 | **0072** | ADR placement job (vanilla + ADR membership) | 3 | LBL-02 [0071], ACT-03a+ | Port/adapt `UpdateLabelPositionsJob` / `AddLabels` (`100f` spacing, redundancy, `m_MaxScale`). Dual source: unassociated → vanilla `AggregateElement` on Aggregate; associated → `ADRNameAggregateElement` on ADR aggregate. Write vanilla-compatible `LabelPosition` buffers on the **pick owner** entity (S1/S2). Burst preferred. See [15](15-adr-citywide-name-label-takeover.md). |
| LBL-04 | **0073** | ADR name mesh batch + DrawMesh | 4 | LBL-02 [0071], LBL-03 [0072] | Port `FillNameMeshData` warp/batch; ADR render system mirrors name half of `AggregateRenderSystem` (`hideOverlay`, Game/SceneView cameras, surface + underground submeshes). Do not draw arrows. Citywide visible names at parity. |
| LBL-05 | **0074** | Forked pick + SIP Aggregate section parity | 5 | LBL-03 [0072], ACT-04 [0066] | **Pick:** Burst clone of `Game.Net.RaycastJobs.RaycastLabelsJob` (~150 lines); resolve `m_Owner` S1 vs S2; schedule from managed Harmony on `RaycastSystem` when `TypeMask.Labels`. Do **not** patch Burst job body. **SIP:** audit every InGame SIP section / gate that requires vanilla `Aggregate` and/or `AggregateElement` (at minimum `RoadSection`, `TitleSection.GetVirtualKeyboardLocaleKey`, Aggregate-path `ImageSystem` / icon — full list via Game.dll search at implement time). Managed Harmony so **ADR name-aggregate (S2)** shows the same sections and data as a vanilla Aggregate selection: `RoadSection` walk **`ADRNameAggregateElement`** with same per-edge math as [06](06-labels-vs-vanilla-sync-and-harmony.md); S1 remainder filtering when a vanilla Aggregate is still selected. Title locale `"RoadName"` for ADR entity. Ensure `SelectedInfoUISystem.GetAggregatePosition` works for S2 `LabelPosition`. **Traffic routes (required):** `ActionsSection.hasTrafficRoutes` for S2 + live `TrafficRoutesSystem` path (mirror `AggregateElement` from ADR + accept S2 like Aggregate for `FillTargetMapJob`). Prefer shipping **DefaultTool H1** highlight list job in this task when cheap (same membership helper as RoadSection); otherwise open **LBL-08**. **Look (cheap):** ADR name materials `_OutlineColor` `#004400` (ADR-style clones only). See [06](06-labels-vs-vanilla-sync-and-harmony.md) / [15](15-adr-citywide-name-label-takeover.md). |
| LBL-06 | **0075** | Integration dogfood + game-update checklist | 6 | LBL-04 [0073], LBL-05 [0074] | Dogfood + written re-diff checklist [19](19-game-update-re-diff-checklist.md). Sprint 014 spills (LBL-07…09) discarded. |

### Optional spill tasks (Sprint 014)

**None.** LBL-07 / LBL-09 cancelled; LBL-08 shipped inside LBL-05. See Sprint 014 table above.

### Later (post-parity / post–highway-reattach) — not boarded yet

| Idea | Scope note |
|------|------------|
| Highway **direction** on world name labels | When `routeDirection != NONE`, append/include a direction fragment. Formats live in **`AdrModData`**: one string field per `RouteDirection` **except `NONE`**. Empty → i18n `RouteDirection.{NAME}`. See [17](17-label-extensions-text-images-look.md). Create board task only when explicitly requested. |

### Suggested board sprints

| Board sprint | Tasks |
|--------------|-------|
| **Sprint 013 — ADR road name labels (parity)** | LBL-01…LBL-06 |
| ~~Sprint 014~~ | Discarded |

---

## Suggested code locations (illustrative)

| Piece | Path (draft) |
|-------|----------------|
| Harmony shutdown | e.g. `BelzontAdr/Overrides/` or dedicated `AdrRoadNameLabelHarmony` |
| Mesh / bake / place | `BelzontAdr/Systems/AdrRoadNameLabelMeshSystem.cs` (name flexible) |
| Render | `BelzontAdr/Systems/AdrRoadNameLabelRenderSystem.cs` |
| Pick job | `BelzontAdr/Systems/AdrRaycastLabelsJob.cs` (or Jobs folder) |
| SIP Aggregate gates | Extend / add Harmony next to existing SIP overrides (`RoadSection`, `TitleSection`, icon path) |
| Dirty | Extend `AdrMainSystem` label `Updated` queries for ADR owners |

Exact names follow repo conventions at implementation time.

## Acceptance sketches (parity)

See also dogfood script in [19](19-game-update-re-diff-checklist.md).

- [ ] With ADR loaded, vanilla road **name** overlays are gone; **arrows** still appear when tools require them.
- [ ] All aggregatable roads show name labels at approximately vanilla density/scale/color/underground behavior.
- [ ] Unassociated: click label selects vanilla `Aggregate` (S1); SIP road sections match today.
- [ ] ADR-associated: click label selects ADR name-aggregate (S2); string matches ADR name preference.
- [ ] With S2 selected, every SIP section that appears for a vanilla Aggregate also appears and is usable (stats over ADR edge list, RoadName keyboard key, icon, focus position).
- [ ] Rename / locale / nameset change refreshes labels without requiring save/reload.
- [ ] Temp net edit does not leave stuck or duplicate name meshes (parity bar).
- [ ] No Harmony inside `AggregateSystem` or Burst `RaycastLabelsJob` body.
- [ ] Game-update re-diff checklist present ([19](19-game-update-re-diff-checklist.md)).

## Relation

- [14](14-vanilla-road-label-pipeline.md) — vanilla reference  
- [15](15-adr-citywide-name-label-takeover.md) — architecture / policy  
- [16](16-label-takeover-effort-and-risk.md) — effort bands / risk  
- [17](17-label-extensions-text-images-look.md) — deferred extensions  
- [19](19-game-update-re-diff-checklist.md) — post-update re-diff + dogfood script  
- [13](13-activation-sprint-roadmap.md) — activation prereq; this doc expands its labels backlog line  
- [06](06-labels-vs-vanilla-sync-and-harmony.md) — pick/SIP archive; thin-edge approach superseded for epic scope  
