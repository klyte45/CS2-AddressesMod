# Road name-label takeover — implementation roadmap & Kanban catalog

*Added: 2026-09-12 · Updated: 2026-09-12 (LBL-05 + SIP Aggregate parity) · Companion to [14](14-vanilla-road-label-pipeline.md)–[17](17-label-extensions-text-images-look.md) · Architecture: [15](15-adr-citywide-name-label-takeover.md) · Effort: [16](16-label-takeover-effort-and-risk.md) · SIP gates: [06](06-labels-vs-vanilla-sync-and-harmony.md)*

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
| Dirty/`Updated` refresh; Temp + underground parity | Traffic-routes tool affordance (optional later; [06](06-labels-vs-vanilla-sync-and-harmony.md)) |
| Integration / update-diff checklist | Achievements / infoview filter noise |

## Recommended sprint plan

Prefer **one primary sprint** for parity (matches [16](16-label-takeover-effort-and-risk.md) “~1 sprint, may spill”). Optional **spill / harden** sprint if placement or Temp-net work overruns.

| Field | Value |
|-------|--------|
| Suggested epic label | **ADR road name-label takeover** |
| Suggested epic slug | `adr-road-labels` |
| Suggested primary sprint | **Sprint 013 — ADR road name labels (parity)** |
| Optional spill sprint | **Sprint 014 — ADR road name labels harden** (only if needed) |
| Effort band | ~1.3–1.7× Road Grouping T0–T3; ~0.5–0.65× activation sprint ([16](16-label-takeover-effort-and-risk.md)) |

### Sprint 013 — parity (all LBL-01…LBL-06)

| Order | ID | Title | Size | Depends | Outcome |
|-------|-----|-------|------|---------|---------|
| 1 | LBL-01 | Shutdown vanilla name path | S | ACT-04+ | Vanilla names never bake/draw; arrows still work |
| 2 | LBL-02 | ADR TMP bake + extents + materials | M | LBL-01 | Strings → `LabelVertex` / `LabelExtents` on label owners |
| 3 | LBL-03 | ADR placement job (dual membership) | L | LBL-02, ACT-03a+ | `LabelPosition` along vanilla or ADR element chains |
| 4 | LBL-04 | ADR mesh batch + DrawMesh | M | LBL-02, LBL-03 | Citywide name meshes visible at parity |
| 5 | LBL-05 | Forked pick + SIP Aggregate parity | L | LBL-03, ACT-04 | Click → S1/S2; SIP sections that gate on vanilla Aggregate also work for ADR |
| 6 | LBL-06 | Integration dogfood + update checklist | M | LBL-04, LBL-05 | Load/Temp/underground/locale dirty; SIP on S2; GUID re-diff notes |

```text
ACT-04+ (prereq)
    │
    ▼
LBL-01 ──► LBL-02 ──► LBL-03 ──► LBL-04 ──► LBL-06
                 │         │
                 │         └──► LBL-05 ──────┘
                 └── (materials shared with mesh)
```

### Sprint 014 — harden (optional spill)

Use only if Sprint 013 ships with known gaps. Draft tasks (create on board only if needed):

| ID | Title | Notes |
|----|-------|-------|
| LBL-07 | Temp / net-tool label parity | Match vanilla Temp original remap / clear branches |
| LBL-08 | DefaultTool H1 highlight membership | Same S1/S2 edge sets as SIP stats ([06](06-labels-vs-vanilla-sync-and-harmony.md)); spill if not finished in LBL-05 |
| LBL-09 | Performance pass on mass dirty | Locale / nameset change should batch like vanilla `m_LabelQuery` |

### Later (not this epic)

| Theme | Doc | When |
|-------|-----|------|
| TMP sprite shields / multi-recipe / look knobs / WE copy-register | [17](17-label-extensions-text-images-look.md) | After later **product** research |
| Traffic-routes tool over ADR groups | [06](06-labels-vs-vanilla-sync-and-harmony.md) | Optional polish |
| Highway reattach via tool | [13](13-activation-sprint-roadmap.md) | Independent of labels |

---

## Kanban catalog (draft)

Suggested epic label: **`ADR road name-label takeover`**  
Suggested epic slug: **`adr-road-labels`**

Board IDs are **TBD** — assign when creating tasks via MCP. Do not invent board numbers here.

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
| LBL-01 | *TBD* | Shutdown vanilla name path | 1 | ACT-04+ | Managed Harmony: Prefix/early-out `AggregateMeshSystem` **name** half (`UpdateLabelVertices` / `UpdateLabelPositions` / `FillNameMeshData`) while keeping arrow updates; Prefix `AggregateRenderSystem.Render` name `DrawMesh` loop; leave `requireNetArrows` arrow branch. Always-on with ADR load. Verify arrows + tools still work. See [15](15-adr-citywide-name-label-takeover.md). |
| LBL-02 | *TBD* | ADR TMP bake + LabelExtents + materials | 2 | LBL-01 | Port vanilla `UpdateLabelVertices` pattern: `NameSystem.GetRenderedLabelName` (or ADR-resolved entity) → TMP `GetTextInfo` with parity layout (`sizeDelta` 250×100, `fontSize` 200, center); fill `LabelVertex`; clone materials + `CopyFontAtlasParameters`; write `LabelExtents`. Wire dirty via `Updated` (extend `AdrMainSystem.ResetRoadsCache` pattern to ADR owners as needed). Cache string changes. See [14](14-vanilla-road-label-pipeline.md). |
| LBL-03 | *TBD* | ADR placement job (vanilla + ADR membership) | 3 | LBL-02, ACT-03a+ | Port/adapt `UpdateLabelPositionsJob` / `AddLabels` (`100f` spacing, redundancy, `m_MaxScale`). Dual source: unassociated → vanilla `AggregateElement` on Aggregate; associated → `ADRNameAggregateElement` on ADR aggregate. Write vanilla-compatible `LabelPosition` buffers on the **pick owner** entity (S1/S2). Burst preferred. See [15](15-adr-citywide-name-label-takeover.md). |
| LBL-04 | *TBD* | ADR name mesh batch + DrawMesh | 4 | LBL-02, LBL-03 | Port `FillNameMeshData` warp/batch; ADR render system mirrors name half of `AggregateRenderSystem` (`hideOverlay`, Game/SceneView cameras, surface + underground submeshes). Do not draw arrows. Citywide visible names at parity. |
| LBL-05 | *TBD* | Forked pick + SIP Aggregate section parity | 5 | LBL-03, ACT-04 | **Pick:** Burst clone of `Game.Net.RaycastJobs.RaycastLabelsJob` (~150 lines); resolve `m_Owner` S1 vs S2; schedule from managed Harmony on `RaycastSystem` when `TypeMask.Labels`. Do **not** patch Burst job body. **SIP:** audit every InGame SIP section / gate that requires vanilla `Aggregate` and/or `AggregateElement` (at minimum `RoadSection`, `TitleSection.GetVirtualKeyboardLocaleKey`, Aggregate-path `ImageSystem` / icon — full list via Game.dll search at implement time). Managed Harmony so **ADR name-aggregate (S2)** shows the same sections and data as a vanilla Aggregate selection: `RoadSection` walk **`ADRNameAggregateElement`** with same per-edge math as [06](06-labels-vs-vanilla-sync-and-harmony.md); S1 remainder filtering when a vanilla Aggregate is still selected. Title locale `"RoadName"` for ADR entity. Ensure `SelectedInfoUISystem.GetAggregatePosition` works for S2 `LabelPosition`. Prefer shipping **DefaultTool H1** highlight list job in this task when cheap (same membership helper as RoadSection); otherwise open **LBL-08**. See [06](06-labels-vs-vanilla-sync-and-harmony.md) / [15](15-adr-citywide-name-label-takeover.md). |
| LBL-06 | *TBD* | Integration dogfood + game-update checklist | 6 | LBL-04, LBL-05 | Dogfood: load city, locale change, nameset/custom rename, Temp place/upgrade, tunnels, ADR fork/edit via Road Grouping, pick + **full SIP panel on S2** (stats, title rename key, icon) vs vanilla Aggregate. Document re-diff checklist against kcs2m GUID for mesh/render/raycast/SIP surfaces ([16](16-label-takeover-effort-and-risk.md)). Fix P0 parity holes or open LBL-07…09 for spill. |

### Optional spill tasks (Sprint 014)

| ID | Board ID | Title | Depends | Description |
|----|----------|-------|---------|-------------|
| LBL-07 | *TBD* | Temp / net-tool label parity | LBL-06 | Close remaining Temp/original/clear gaps vs vanilla |
| LBL-08 | *TBD* | DefaultTool H1 highlight membership | LBL-06 | If not done in LBL-05: ADR list job rewrites CreationDefinition `AggregateElement` (S1 remainder / S2 full ADR) before `GenerateAggregatesSystem` ([06](06-labels-vs-vanilla-sync-and-harmony.md)) |
| LBL-09 | *TBD* | Mass-dirty performance | LBL-06 | Avoid worse-than-vanilla full-city rebuilds |

### Suggested board sprints

| Board sprint | Tasks |
|--------------|-------|
| **Sprint 013 — ADR road name labels (parity)** | LBL-01…LBL-06 |
| **Sprint 014 — ADR road name labels harden** *(optional)* | LBL-07…LBL-09 as needed |

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

- [ ] With ADR loaded, vanilla road **name** overlays are gone; **arrows** still appear when tools require them.
- [ ] All aggregatable roads show name labels at approximately vanilla density/scale/color/underground behavior.
- [ ] Unassociated: click label selects vanilla `Aggregate` (S1); SIP road sections match today.
- [ ] ADR-associated: click label selects ADR name-aggregate (S2); string matches ADR name preference.
- [ ] With S2 selected, every SIP section that appears for a vanilla Aggregate also appears and is usable (stats over ADR edge list, RoadName keyboard key, icon, focus position).
- [ ] Rename / locale / nameset change refreshes labels without requiring save/reload.
- [ ] Temp net edit does not leave stuck or duplicate name meshes (parity bar).
- [ ] No Harmony inside `AggregateSystem` or Burst `RaycastLabelsJob` body.

## Relation

- [14](14-vanilla-road-label-pipeline.md) — vanilla reference  
- [15](15-adr-citywide-name-label-takeover.md) — architecture / policy  
- [16](16-label-takeover-effort-and-risk.md) — effort bands / risk  
- [17](17-label-extensions-text-images-look.md) — deferred extensions  
- [13](13-activation-sprint-roadmap.md) — activation prereq; this doc expands its labels backlog line  
- [06](06-labels-vs-vanilla-sync-and-harmony.md) — pick/SIP archive; thin-edge approach superseded for epic scope  
