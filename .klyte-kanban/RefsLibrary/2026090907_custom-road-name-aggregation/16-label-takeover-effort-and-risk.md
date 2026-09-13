# Label takeover — effort and risk

*Added: 2026-09-11 · Architecture: [15](15-adr-citywide-name-label-takeover.md) · Pipeline: [14](14-vanilla-road-label-pipeline.md)*

## Purpose

Size the **parity-only, citywide name-label takeover** epic relative to existing ADR work (Road Grouping T0–T3, activation sprint), and list Harmony surface / update-churn risks.

**Out of this estimate:** TMP shield sprites, multi-recipe labels, look knobs, WE atlas product sync — feasibility only in [17](17-label-extensions-text-images-look.md).

## Work bands (parity epic)

| Slice | Size | Notes |
|-------|------|-------|
| Vanilla name shutdown | **S** | Managed Prefix / early-out on mesh name half + render name loop; keep arrows |
| ADR TMP → `LabelVertex` + extents | **M** | Port `UpdateLabelVertices` pattern; material atlas clone via `CopyFontAtlasParameters` |
| ADR placement along membership | **L** | Port/adapt `UpdateLabelPositionsJob` / `AddLabels` (`100f` spacing, redundancy / `m_MaxScale`); dual source lists (vanilla vs ADR elements) |
| ADR batched mesh + `Graphics.DrawMesh` | **M** | Mirror `FillNameMeshData` + name draw loop |
| Forked pick + `RaycastSystem` hook | **M** | ~150-line leaf job; S1/S2 owner policy |
| SIP Aggregate section parity (with pick) | **M–L** | `RoadSection` / `TitleSection` / icon / focus — see LBL-05 in [18](18-label-takeover-sprint-roadmap.md); design [06](06-labels-vs-vanilla-sync-and-harmony.md) |
| Citywide membership policy + dirty | **M** | Always-on wiring; `Updated` refresh; Temp / underground parity |
| Integration / game-update re-diff | **M** | Checklist against GUID; temp nets; load |

**Overall:** one solid **post-activation** epic. Larger than “thin suppress + draw on ADR edges only” ([06](06-labels-vs-vanilla-sync-and-harmony.md)), far smaller than fighting `AggregateSystem`.

## Comparison to existing feature work

| Baseline | Relative size of parity label takeover |
|----------|----------------------------------------|
| Road Grouping **T0–T3** (tool + overlay + cards) | **~1.3–1.7×** — not twice the tool |
| ACT-05 / ACT-06 alone (mutations + maintenance) | **~2–2.5×** — rendering subsystem vs CRUD wiring |
| Full activation sprint ACT-01…ACT-06 | **~0.5–0.65×** — labels are later backlog in [13](13-activation-sprint-roadmap.md) |
| Thin ADR-edge labels only (old [06] sketch) | Citywide parity is **~2–3×** that thin path |

Existing research ([14](14-vanilla-road-label-pipeline.md)–[15](15-adr-citywide-name-label-takeover.md), prior [01](01-vanilla-aggregation-overview.md)/[06](06-labels-vs-vanilla-sync-and-harmony.md)) cuts **planning risk**, not raw port volume: `AggregateMeshSystem` name path remains a large fork surface.

### Why not “2× the tool”?

- T0–T3 was **new ADR UX** (owned design: tool, overlays, VUIO).
- Label takeover is **porting a mature vanilla renderer** — less product invention, more parity on every road.
- Activation sprint is broader (highway strip, components, fork/seed, names, mutations, maintenance). Labels sit beside that, not inside it.

## Suggested task split (4–6 tasks)

Full Kanban catalog + sprint planning: **[18-label-takeover-sprint-roadmap.md](18-label-takeover-sprint-roadmap.md)** (LBL-01…LBL-06; Sprint 014 spills discarded).

1. Shutdown vanilla name path + ADR render hook → **LBL-01**  
2. TMP bake + material/atlas parity → **LBL-02**  
3. Placement job (vanilla AggregateElement + ADR element sources) → **LBL-03**  
4. Mesh batching + draw → **LBL-04**  
5. Forked pick + S1/S2 owner policy + **SIP Aggregate section parity** → **LBL-05**  
6. Integration testing / update-diff checklist → **LBL-06**  

Calendar feel: **~1 sprint** (suggested **Sprint 013**) for someone familiar with the codebase; may spill to **Sprint 014** if placement + Temp-net edge cases fight back.

## Harmony / intervention surface

| Target | Approach | Risk |
|--------|----------|------|
| `AggregateMeshSystem.OnUpdate` name half | Managed Prefix / early-out | Medium — method may grow; re-diff on updates |
| `AggregateRenderSystem.Render` name loop | Managed Prefix | Low–medium — small file today (~105 lines) |
| `RaycastSystem` schedule site | Managed Prefix/Postfix to schedule ADR job | Medium — schedule graph churn |
| Forked `RaycastLabelsJob` | Copy Burst job; do **not** patch body | Low structural churn; still re-diff `LabelPosition` contract |
| `AggregateSystem` Burst | **Never** for this epic | N/A |

## Update-churn risk

| Surface | Likelihood of churn | Mitigation |
|---------|---------------------|------------|
| `LabelPosition` / `LabelExtents` layout | Low–medium | Keep vanilla-compatible layouts for pick/SIP focus |
| TMP bake constants / material clone | Low | Parity copy; document GUID |
| Placement `AddLabels` math | Medium | Own fork; re-diff on game updates |
| `RaycastSystem` Labels schedule | Medium | Thin managed hook only |
| Aggregate membership Validate/Combine | High if fought — **out of scope** | Leave vanilla |

Re-verify against current kcs2m GUID (`d124bec16c0748da9ebf48366d9c1d68` as of this doc; older pack used `b5fd0b95…`). Full surface table + dogfood script: [19](19-game-update-re-diff-checklist.md).

## What existing docs already cover (reduces discovery)

| Doc | Saves |
|-----|-------|
| [01](01-vanilla-aggregation-overview.md) | Membership vs display |
| [06](06-labels-vs-vanilla-sync-and-harmony.md) | Fork pick, S1/S2, SIP later |
| [14](14-vanilla-road-label-pipeline.md) | Full name pipeline |
| [15](15-adr-citywide-name-label-takeover.md) | Shutdown + always-on + dual placement sources |
| [11](11-road-grouping-tool-roadmap.md) / [13](13-activation-sprint-roadmap.md) | Sequencing: after ACT-06 |
| [19](19-game-update-re-diff-checklist.md) | Post-update re-diff + dogfood (LBL-06) |

## Risks / gotchas

- **Arrow coupling:** mesh system is shared — must disable **name** paths only.  
- **SIP focus:** `SelectedInfoUISystem.GetAggregatePosition` reads `LabelPosition` — ADR must keep positions on the entity SIP selects (S1 Aggregate or S2 ADR).  
- **Temp aggregates:** vanilla skips / remaps label strings for temps — parity needs the same branches.  
- **Always-on:** no fallback if ADR label system fails — ship behind solid dirty/load tests.  
- **Performance:** citywide TMP bake on mass `Updated` (locale / nameset change) — match vanilla `m_LabelQuery` batching, do not invent worse full-city rebuilds.

## Relation

- [15](15-adr-citywide-name-label-takeover.md) — architecture  
- [17](17-label-extensions-text-images-look.md) — deferred extension cost (not in parity bands)  
- [19](19-game-update-re-diff-checklist.md) — game-update checklist  
- [13](13-activation-sprint-roadmap.md) — “ADR world name labels + forked RaycastLabelsJob” later backlog  
