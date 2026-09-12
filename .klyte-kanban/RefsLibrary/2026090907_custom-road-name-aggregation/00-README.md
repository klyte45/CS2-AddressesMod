# Custom road-name aggregation — research pack

*Researched: 2026-09-09 · Activation planning: 2026-09-10 · Label takeover research: 2026-09-11 · Game.dll build GUID `d124bec16c0748da9ebf48366d9c1d68` (kcs2m backend cache; older docs may still cite `b5fd0b9582554f3c83207d8d5d7a19a0`)*

Reference pack for **player-customizable road name aggregation**, implemented as an **ADR-owned parallel system** that does **not** override vanilla `AggregateSystem` / `Aggregated` / `Aggregate` lifecycle.

This pack is **documentation only** for research/design. Activation Kanban tasks live in **Sprint 012** (`adr-activation`, board IDs **0061–0068**) — catalog detail in [13](13-activation-sprint-roadmap.md). Citywide **road name-label takeover** research: [14](14-vanilla-road-label-pipeline.md)–[17](17-label-extensions-text-images-look.md).

## Scope

| In scope | Out of scope |
|----------|--------------|
| Vanilla Aggregation audit (components, systems, consumers) | Implementing ADR components / systems / UI (follows [13](13-activation-sprint-roadmap.md)) |
| Verdict: display / naming / tooling vs traffic simulation | Harmony-patching or disabling `AggregateSystem` |
| Current ADR touchpoints on vanilla aggregates | Editing `i18n.csv` / locale CSVs |
| Parallel-system design notes (name + house numbers) | Creating Kanban board items via MCP from this pack |
| Gaps and follow-up implementation planning inputs | Frontend research via game UI dumps |
| Labels-vs-Aggregation-sync discussion + Harmony viability for leftover UI | Implementing those Harmony patches |
| Road Grouping tool / overlay specs + T0–T3 roadmap ([09](09-road-grouping-tool-spec.md)–[11](11-road-grouping-tool-roadmap.md)) | Shipping activation sprint code (see [12](12-activation-change-map.md)–[13](13-activation-sprint-roadmap.md)) |
| Activation change-map + single-sprint Kanban catalog ([12](12-activation-change-map.md)–[13](13-activation-sprint-roadmap.md)) | Taking over district/area labels or one-way arrows |
| Citywide road **name** label takeover research ([14](14-vanilla-road-label-pipeline.md)–[17](17-label-extensions-text-images-look.md)) | Implementing label takeover / BelzontWE live shared `Texture2D` |

## Confirmed decisions

| Decision | Choice |
|----------|--------|
| Strategy | **Parallel ADR aggregations** — leave vanilla Aggregation alone; new ADR components/entities for mod-managed associations |
| Address scope | ADR name-aggregates control **both road name and house-number sequencing** for associated edges |
| Highway data (activation) | **Remove** Aggregate-hosted `ADRHighway*` now; markers **visual-only**; registry kept; **reattach via tool later** (not migrate-in-place this wave). See [12](12-activation-change-map.md) |
| World labels (label epic) | **Citywide ADR ownership** of road **name** labels; shut down vanilla name mesh/draw; leave arrows + `AggregateSystem` alone. Always-on when ADR loads. Phase-1 **parity-only**. See [15](15-adr-citywide-name-label-takeover.md). *(Supersedes “thin labels on ADR edges only” in [06](06-labels-vs-vanilla-sync-and-harmony.md) for epic scope.)* |
| Label pick | **Fork** vanilla `RaycastLabelsJob` (~150 lines); schedule via managed `RaycastSystem` hook. **S1** (vanilla Aggregate) for unassociated roads; **S2** (ADR name-aggregate) when associated. See [15](15-adr-citywide-name-label-takeover.md) / [06](06-labels-vs-vanilla-sync-and-harmony.md) |
| Membership per edge | **`preventPropagation`** (default false); **`numberOverrideActive`** + **`numberOverride`** (`int`). Gap-fill only between propagating endpoints; **no** gap-fill on load |
| Fork / seed (activation) | First membership edit forks ADR: copy vanilla seed → ADR; `Redraw` vanilla; full `AggregateElement` copy then apply edit; split naming after fork. See [12](12-activation-change-map.md) |
| Initial UX | **Net-only Road Grouping tool** (world overlays + node controls). **SIP out of scope** for this implementation wave. Spec: [09](09-road-grouping-tool-spec.md)–[11](11-road-grouping-tool-roadmap.md); activation: [12](12-activation-change-map.md)–[13](13-activation-sprint-roadmap.md) |
| Tool naming | Product/code: **Road Grouping** (`AdrRoadGrouping*`). Doc [08](08-association-tool-first-ux.md) keeps historical “association” filename |
| Label extensions (shields / multi-recipe / look knobs) | **Feasibility** documented in [17](17-label-extensions-text-images-look.md); **product strategy deferred** to later research. WE sync = **copy/re-register only** (no live shared `Texture2D`) |
| Pack depth | Analysis + design + tool specs + activation catalog + **label takeover research (14–17)** |
| Research method | kcs2m backend MCP for vanilla; in-repo C# for ADR; BelzontWE for tool scaffold / WE atlas APIs |

## Reading order

1. [01-vanilla-aggregation-overview.md](01-vanilla-aggregation-overview.md) — components + `AggregateSystem` rules + tool pipeline
2. [02-consumer-audit.md](02-consumer-audit.md) — who reads aggregates; **display-vs-sim verdict**
3. [03-adr-current-touchpoints.md](03-adr-current-touchpoints.md) — how ADR hangs data off vanilla aggregates today (+ activation strip notes)
4. [04-parallel-system-design-notes.md](04-parallel-system-design-notes.md) — recommended ADR-owned design + fork/seed activation rules
5. [05-research-gaps-and-next.md](05-research-gaps-and-next.md) — remaining gaps; points to activation docs
6. [06-labels-vs-vanilla-sync-and-harmony.md](06-labels-vs-vanilla-sync-and-harmony.md) — labels vs Aggregation sync; SIP/H1 archive; thin-label sketch (**epic scope superseded by [15](15-adr-citywide-name-label-takeover.md)**)
7. [07-membership-maintenance-edge-cases.md](07-membership-maintenance-edge-cases.md) — net remap + gap-fill continuity + edge-case matrix
8. [08-association-tool-first-ux.md](08-association-tool-first-ux.md) — tool-first UX; **full activation** (not thin contract)
9. [09-road-grouping-tool-spec.md](09-road-grouping-tool-spec.md) — **Road Grouping** tool contract
10. [10-road-grouping-overlay-render-system.md](10-road-grouping-overlay-render-system.md) — opaque route-ribbon layer
11. [11-road-grouping-tool-roadmap.md](11-road-grouping-tool-roadmap.md) — T0–T3 shipped; T4/T5 → activation ACT-03a…ACT-06
12. [12-activation-change-map.md](12-activation-change-map.md) — **file-level activation change map** (sections A1–A3, B–E)
13. [13-activation-sprint-roadmap.md](13-activation-sprint-roadmap.md) — **one activation sprint + Kanban task catalog** (ACT-03 split into 03a/03b/03c)
14. [14-vanilla-road-label-pipeline.md](14-vanilla-road-label-pipeline.md) — vanilla TMP → placement → mesh → draw → pick
15. [15-adr-citywide-name-label-takeover.md](15-adr-citywide-name-label-takeover.md) — citywide ADR name-label architecture + shutdown levers
16. [16-label-takeover-effort-and-risk.md](16-label-takeover-effort-and-risk.md) — effort vs tool/activation; Harmony / churn risk
17. [17-label-extensions-text-images-look.md](17-label-extensions-text-images-look.md) — extension **feasibility** (text, TMP sprites, multi-recipe, WE copy/re-register, look knobs)

## Success criteria (this research pack)

- [x] Folder exists under `.klyte-kanban/RefsLibrary/2026090907_custom-road-name-aggregation/`
- [x] Vanilla Aggregation lifecycle and join rules documented from decompiled Game.dll
- [x] Consumer audit concludes whether Aggregation affects traffic simulation
- [x] ADR current usage (namesets, highways, SIP, dead `fixedAggregation`) documented
- [x] Parallel design documents name+number resolution and highway migration
- [x] Follow-up gaps listed for a later implementation / Kanban plan
- [x] Labels-vs-sync discussion and Harmony viability for leftover consumers documented ([06](06-labels-vs-vanilla-sync-and-harmony.md))
- [x] Membership maintenance / gap-fill edge-case matrix documented ([07](07-membership-maintenance-edge-cases.md))
- [x] Tool-first UX (net-only + world draw + node controls) and epic-order impact documented ([08](08-association-tool-first-ux.md))
- [x] Road Grouping tool specification documented ([09](09-road-grouping-tool-spec.md))
- [x] Road Grouping overlay render system documented ([10](10-road-grouping-overlay-render-system.md))
- [x] Road Grouping implementation roadmap T0–T5 documented ([11](11-road-grouping-tool-roadmap.md))
- [x] Activation change-map documented ([12](12-activation-change-map.md))
- [x] Activation sprint roadmap + Kanban catalog documented ([13](13-activation-sprint-roadmap.md))
- [x] Vanilla road name-label pipeline documented ([14](14-vanilla-road-label-pipeline.md))
- [x] Citywide ADR name-label takeover architecture documented ([15](15-adr-citywide-name-label-takeover.md))
- [x] Label takeover effort / risk documented ([16](16-label-takeover-effort-and-risk.md))
- [x] Label extension feasibility (text / images / multi-content / look / WE) documented ([17](17-label-extensions-text-images-look.md))

## Related packs

- [2026040409_address-format-customization](../2026040409_address-format-customization/01_game-address-format-research.md) — addresses already resolve through the **aggregate** entity via `BuildingUtils.GetAddress`
