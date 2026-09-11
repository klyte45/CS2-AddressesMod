# Custom road-name aggregation — research pack

*Researched: 2026-09-09 · Activation planning: 2026-09-10 · Game.dll build GUID `b5fd0b9582554f3c83207d8d5d7a19a0` (kcs2m backend cache)*

Reference pack for **player-customizable road name aggregation**, implemented as an **ADR-owned parallel system** that does **not** override vanilla `AggregateSystem` / `Aggregated` / `Aggregate` lifecycle.

This pack is **documentation only**. It does not ship product code. Kanban tasks for activation are **catalogued in [13](13-activation-sprint-roadmap.md)** for manual import — this pack does not create board items.

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
| Activation change-map + single-sprint Kanban catalog ([12](12-activation-change-map.md)–[13](13-activation-sprint-roadmap.md)) | |

## Confirmed decisions

| Decision | Choice |
|----------|--------|
| Strategy | **Parallel ADR aggregations** — leave vanilla Aggregation alone; new ADR components/entities for mod-managed associations |
| Address scope | ADR name-aggregates control **both road name and house-number sequencing** for associated edges |
| Highway data (activation) | **Remove** Aggregate-hosted `ADRHighway*` now; markers **visual-only**; registry kept; **reattach via tool later** (not migrate-in-place this wave). See [12](12-activation-change-map.md) |
| World labels (when ADR-associated) | **ADR-owned** thin labels + suppress conflicting vanilla labels — not Aggregation post-sync (**later epic**) |
| Label pick (click ADR labels) | **Fork** vanilla `RaycastLabelsJob` (~150 lines) as ADR Burst job; schedule via managed `RaycastSystem` hook — do not Harmony-patch the Burst job body (**later epic**) |
| Membership per edge | **`preventPropagation`** (default false); **`numberOverrideActive`** + **`numberOverride`** (`int`). Gap-fill only between propagating endpoints; **no** gap-fill on load |
| Fork / seed (activation) | First membership edit forks ADR: copy vanilla seed → ADR; `Redraw` vanilla; full `AggregateElement` copy then apply edit; split naming after fork. See [12](12-activation-change-map.md) |
| Initial UX | **Net-only Road Grouping tool** (world overlays + node controls). **SIP out of scope** for this implementation wave. Spec: [09](09-road-grouping-tool-spec.md)–[11](11-road-grouping-tool-roadmap.md); activation: [12](12-activation-change-map.md)–[13](13-activation-sprint-roadmap.md) |
| Tool naming | Product/code: **Road Grouping** (`AdrRoadGrouping*`). Doc [08](08-association-tool-first-ux.md) keeps historical “association” filename |
| Pack depth | Analysis + design + tool specs + **activation change-map / single-sprint catalog** — board creation only if separately requested |
| Research method | kcs2m backend MCP for vanilla; in-repo C# for ADR; BelzontWE for tool scaffold |

## Reading order

1. [01-vanilla-aggregation-overview.md](01-vanilla-aggregation-overview.md) — components + `AggregateSystem` rules + tool pipeline
2. [02-consumer-audit.md](02-consumer-audit.md) — who reads aggregates; **display-vs-sim verdict**
3. [03-adr-current-touchpoints.md](03-adr-current-touchpoints.md) — how ADR hangs data off vanilla aggregates today (+ activation strip notes)
4. [04-parallel-system-design-notes.md](04-parallel-system-design-notes.md) — recommended ADR-owned design + fork/seed activation rules
5. [05-research-gaps-and-next.md](05-research-gaps-and-next.md) — remaining gaps; points to activation docs
6. [06-labels-vs-vanilla-sync-and-harmony.md](06-labels-vs-vanilla-sync-and-harmony.md) — ADR labels vs Aggregation sync; unattended branches; Harmony viability
7. [07-membership-maintenance-edge-cases.md](07-membership-maintenance-edge-cases.md) — net remap + gap-fill continuity + edge-case matrix
8. [08-association-tool-first-ux.md](08-association-tool-first-ux.md) — tool-first UX; **full activation** (not thin contract)
9. [09-road-grouping-tool-spec.md](09-road-grouping-tool-spec.md) — **Road Grouping** tool contract
10. [10-road-grouping-overlay-render-system.md](10-road-grouping-overlay-render-system.md) — opaque route-ribbon layer
11. [11-road-grouping-tool-roadmap.md](11-road-grouping-tool-roadmap.md) — T0–T3 shipped; T4/T5 → activation ACT-03…ACT-06
12. [12-activation-change-map.md](12-activation-change-map.md) — **file-level activation change map** (sections A–E)
13. [13-activation-sprint-roadmap.md](13-activation-sprint-roadmap.md) — **one activation sprint + Kanban task catalog** (document only)

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

## Related packs

- [2026040409_address-format-customization](../2026040409_address-format-customization/01_game-address-format-research.md) — addresses already resolve through the **aggregate** entity via `BuildingUtils.GetAddress`
