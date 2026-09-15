# Custom road-name aggregation — research pack

*Researched: 2026-09-09 · Activation planning: 2026-09-10 · Label takeover research: 2026-09-11 · Label sprint catalog: 2026-09-12 · Highway reattach design: 2026-09-12 · Net-kind typing: 2026-09-13 · Game.dll build GUID `d124bec16c0748da9ebf48366d9c1d68` (kcs2m backend cache; older docs may still cite `b5fd0b9582554f3c83207d8d5d7a19a0`)*

Reference pack for **player-customizable road name aggregation**, implemented as an **ADR-owned parallel system** that does **not** override vanilla `AggregateSystem` / `Aggregated` / `Aggregate` lifecycle.

This pack is **documentation only** for research/design. Activation Kanban tasks live in **Sprint 012** (`adr-activation`, board IDs **0061–0068**) — catalog detail in [13](13-activation-sprint-roadmap.md). Citywide **road name-label takeover** research: [14](14-vanilla-road-label-pipeline.md)–[17](17-label-extensions-text-images-look.md); implementation catalog: [18](18-label-takeover-sprint-roadmap.md). **Highway reattach** on ADR aggregates: [20](20-highway-reattach-on-adr-aggregates.md)–[21](21-highway-reattach-roadmap.md). **Net-kind mix gate / NetNameData:** [22](22-game-net-edge-kinds-and-aggregation.md)–[24](24-adr-net-kind-roadmap.md). **Citywide bulk ADR convert:** [26](26-bulk-citywide-adr-aggregation.md) (board **0091**).

## Scope

| In scope | Out of scope |
|----------|--------------|
| Vanilla Aggregation audit (components, systems, consumers) | Implementing ADR components / systems / UI (follows [13](13-activation-sprint-roadmap.md) / [18](18-label-takeover-sprint-roadmap.md)) |
| Verdict: display / naming / tooling vs traffic simulation | Harmony-patching or disabling `AggregateSystem` |
| Current ADR touchpoints on vanilla aggregates | Editing `i18n.csv` / locale CSVs |
| Parallel-system design notes (name + house numbers) | Creating Kanban board items via MCP from this pack |
| Gaps and follow-up implementation planning inputs | Frontend research via game UI dumps |
| Labels-vs-Aggregation-sync discussion + Harmony viability for leftover UI | Implementing those Harmony patches |
| Road Grouping tool / overlay specs + T0–T3 roadmap ([09](09-road-grouping-tool-spec.md)–[11](11-road-grouping-tool-roadmap.md)) | Shipping activation sprint code (see [12](12-activation-change-map.md)–[13](13-activation-sprint-roadmap.md)) |
| Activation change-map + single-sprint Kanban catalog ([12](12-activation-change-map.md)–[13](13-activation-sprint-roadmap.md)) | Taking over district/area labels or one-way arrows |
| Citywide road **name** label takeover research ([14](14-vanilla-road-label-pipeline.md)–[17](17-label-extensions-text-images-look.md)) | BelzontWE live shared `Texture2D` |
| Label takeover sprint / task catalog ([18](18-label-takeover-sprint-roadmap.md)) | Creating board items until explicitly requested |
| Highway reattach design + roadmap ([20](20-highway-reattach-on-adr-aggregates.md)–[21](21-highway-reattach-roadmap.md)) | Shipping HW-* product code until explicitly requested |
| Net-kind typing + mix gate + per-kind NetNameData ([22](22-game-net-edge-kinds-and-aggregation.md)–[24](24-adr-net-kind-roadmap.md)) | Creating NET-* board tasks until explicitly requested; mutating vanilla AggregateNet assets |
| Road Grouping tooltips requirements ([25](25-road-grouping-tooltips.md)) | Shipping tooltip C# until TT-* board tasks pulled |
| Citywide bulk ADR aggregation settings ([26](26-bulk-citywide-adr-aggregation.md)) | Shipping convert-all UI until board task pulled |
| Net-kind playtest smoke notes ([27](27-adr-net-kind-playtest-smoke-notes.md)) | Replacing notes without product ask |

## Confirmed decisions

| Decision | Choice |
|----------|--------|
| Strategy | **Parallel ADR aggregations** — leave vanilla Aggregation alone; new ADR components/entities for mod-managed associations |
| Address scope | ADR name-aggregates control **both road name and house-number sequencing** for associated edges |
| Highway data (activation) | **Remove** Aggregate-hosted `ADRHighway*` now; markers **visual-only**; registry kept; **not** migrate-in-place this wave. See [12](12-activation-change-map.md) |
| Highway data (reattach) | Re-host on **`ADRNameAggregate`** as `ADRNameAggregateHighwayData`; edit via **Addresses SIP** and **Road Grouping tool options** (seed + highway + custom name; ADR only); no mileage cache; WE reads new component. See [20](20-highway-reattach-on-adr-aggregates.md) / [21](21-highway-reattach-roadmap.md). *(Supersedes “reattach via Road Grouping tool” alone and “SIP-only settings”.)* |
| World labels (label epic) | **Citywide ADR ownership** of road **name** labels; shut down vanilla name mesh/draw; leave arrows + `AggregateSystem` alone. Always-on when ADR loads. Phase-1 **parity-only**. See [15](15-adr-citywide-name-label-takeover.md). *(Supersedes “thin labels on ADR edges only” in [06](06-labels-vs-vanilla-sync-and-harmony.md) for epic scope.)* |
| Label pick | **Fork** vanilla `RaycastLabelsJob` (~150 lines); schedule via managed `RaycastSystem` hook. **S1** (vanilla Aggregate) for unassociated roads; **S2** (ADR name-aggregate) when associated. See [15](15-adr-citywide-name-label-takeover.md) / [06](06-labels-vs-vanilla-sync-and-harmony.md) |
| SIP with label pick | SIP sections that activate on vanilla `Aggregate` must also work for ADR name-aggregate (S2) — catalog **LBL-05** in [18](18-label-takeover-sprint-roadmap.md) |
| Label implementation catalog | Draft tasks **LBL-01…LBL-06** (Sprint 014 spills discarded); epic `adr-road-labels`; suggested **Sprint 013**. See [18](18-label-takeover-sprint-roadmap.md) |
| Membership per edge | **`preventPropagation`** (default false); **`numberOverrideActive`** + **`numberOverride`** (`int`). Gap-fill only between propagating endpoints; **no** gap-fill on load |
| Fork / seed (activation) | First membership edit forks ADR: copy vanilla seed → ADR; `Redraw` vanilla; full `AggregateElement` copy then apply edit; split naming after fork. See [12](12-activation-change-map.md) |
| Initial UX | **Net-only Road Grouping tool** (world overlays + node controls). **SIP out of scope** for this implementation wave. Spec: [09](09-road-grouping-tool-spec.md)–[11](11-road-grouping-tool-roadmap.md); activation: [12](12-activation-change-map.md)–[13](13-activation-sprint-roadmap.md) |
| Tool naming | Product/code: **Road Grouping** (`AdrRoadGrouping*`). Doc [08](08-association-tool-first-ux.md) keeps historical “association” filename |
| Label extensions (shields / multi-recipe / look knobs) | **Feasibility** documented in [17](17-label-extensions-text-images-look.md); **product strategy deferred** to later research. WE sync = **copy/re-register only** (no live shared `Texture2D`) |
| Pack depth | Analysis + design + tool specs + activation catalog + label research (14–17) + label sprint catalog (18) + highway reattach (20–21) + **net-kind (22–24)** + tooltips requirements (25) + **bulk citywide ADR (26)** |
| Research method | kcs2m backend MCP for vanilla; in-repo C# for ADR; BelzontWE for tool scaffold / WE atlas APIs |
| Net kind classifier | **Prefab `*Data` first** (`RoadData` / `TrackData` / …); `Game.Net.Road` **unreliable** (streets/avenues may lack it). Dual `Train\|Tram` → **Train**. See [22](22-game-net-edge-kinds-and-aggregation.md) |
| ADR kind mix | Exact `AdrNetKind` match; street↔highway **allowed**; cross-kind **forbidden**. Per-kind ADR `NetNameData` (non-road) + outline-by-kind (face `#FFFFFFC0`; Subway `#004400`, Road `#333333`, …). Road seeds only for `RoadData`. See [23](23-adr-net-kind-gate-labels-seeds.md) / [24](24-adr-net-kind-roadmap.md) |
| Citywide bulk ADR convert | Settings button on **Roads and Districts** (above per-district road namesets); confirm dialog with **kind checkboxes**. See [26](26-bulk-citywide-adr-aggregation.md) |

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
18. [18-label-takeover-sprint-roadmap.md](18-label-takeover-sprint-roadmap.md) — **label takeover draft tasks + Sprint 013/014 planning**
19. [19-game-update-re-diff-checklist.md](19-game-update-re-diff-checklist.md) — Managed update re-diff for name-label takeover (LBL-06)
20. [20-highway-reattach-on-adr-aggregates.md](20-highway-reattach-on-adr-aggregates.md) — **highway rehost design** (component, SIP, WE, no-cache)
21. [21-highway-reattach-roadmap.md](21-highway-reattach-roadmap.md) — **highway reattach task catalog** (HW-01…HW-08)
22. [22-game-net-edge-kinds-and-aggregation.md](22-game-net-edge-kinds-and-aggregation.md) — Game.Net kinds, AggregateType, Road vs RoadData, NetNameData gap
23. [23-adr-net-kind-gate-labels-seeds.md](23-adr-net-kind-gate-labels-seeds.md) — ADR kind gate, per-kind labels, road-only seeds
24. [24-adr-net-kind-roadmap.md](24-adr-net-kind-roadmap.md) — NET-01…NET-06 catalog (NET-04 board **0086**; **0085** Z-overflowed)
25. [25-road-grouping-tooltips.md](25-road-grouping-tooltips.md) — **locked** Road Grouping tooltips (vanilla pipeline + TT-01…TT-06 coding catalog)
26. [26-bulk-citywide-adr-aggregation.md](26-bulk-citywide-adr-aggregation.md) — citywide “convert applicable segments to ADR” settings UX + kind checkboxes
27. [27-adr-net-kind-playtest-smoke-notes.md](27-adr-net-kind-playtest-smoke-notes.md) — NET-05 playtest matrix results (Sprint 015)

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
- [x] Label takeover sprint / task catalog documented ([18](18-label-takeover-sprint-roadmap.md))
- [x] Highway reattach on ADR aggregates designed ([20](20-highway-reattach-on-adr-aggregates.md))
- [x] Highway reattach sprint / task catalog documented ([21](21-highway-reattach-roadmap.md))
- [x] Game.Net edge kinds + AggregateType + NetNameData gap documented ([22](22-game-net-edge-kinds-and-aggregation.md))
- [x] ADR net-kind gate / labels / seeds designed ([23](23-adr-net-kind-gate-labels-seeds.md))
- [x] Net-kind implementation catalog documented ([24](24-adr-net-kind-roadmap.md))
- [x] Road Grouping tooltips requirements + coding catalog ([25](25-road-grouping-tooltips.md))
- [x] Citywide bulk ADR aggregation settings UX documented ([26](26-bulk-citywide-adr-aggregation.md))
- [x] Net-kind playtest smoke notes ([27](27-adr-net-kind-playtest-smoke-notes.md))

## Related packs

- [2026040409_address-format-customization](../2026040409_address-format-customization/01_game-address-format-research.md) — addresses already resolve through the **aggregate** entity via `BuildingUtils.GetAddress`
