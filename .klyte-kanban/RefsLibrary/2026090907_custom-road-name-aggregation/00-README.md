# Custom road-name aggregation — research pack

*Researched: 2026-09-09 · Game.dll build GUID `b5fd0b9582554f3c83207d8d5d7a19a0` (kcs2m backend cache)*

Reference pack for a future feature: **player-customizable road name aggregation**, implemented as an **ADR-owned parallel system** that does **not** override vanilla `AggregateSystem` / `Aggregated` / `Aggregate` lifecycle.

This pack is **documentation only**. It does not ship product code or create Kanban tasks/sprints.

## Scope

| In scope | Out of scope |
|----------|--------------|
| Vanilla Aggregation audit (components, systems, consumers) | Implementing ADR components / systems / UI |
| Verdict: display / naming / tooling vs traffic simulation | Harmony-patching or disabling `AggregateSystem` |
| Current ADR touchpoints on vanilla aggregates | Editing `i18n.csv` / locale CSVs |
| Parallel-system design notes (name + house numbers; highway migrate) | Kanban sprint/task creation |
| Gaps and follow-up implementation planning inputs | Frontend research via game UI dumps |
| Labels-vs-Aggregation-sync discussion + Harmony viability for leftover UI | Implementing those Harmony patches |

## Confirmed decisions

| Decision | Choice |
|----------|--------|
| Strategy | **Parallel ADR aggregations** — leave vanilla Aggregation alone; new ADR components/entities for mod-managed associations |
| Address scope | ADR name-aggregates control **both road name and house-number sequencing** for associated edges |
| Highway data | **Migrate** `ADRHighwayAggregationData` (+ cache/dirty) from vanilla `Aggregate` to **ADR-owned** aggregate entities |
| World labels (when ADR-associated) | **ADR-owned** thin labels + suppress conflicting vanilla labels — not Aggregation post-sync |
| Label pick (click ADR labels) | **Fork** vanilla `RaycastLabelsJob` (~150 lines) as ADR Burst job; schedule via managed `RaycastSystem` hook — do not Harmony-patch the Burst job body |
| Membership per edge | **`preventPropagation`** (default false) allows non-contiguous ADR; gap-fill only between propagating endpoints (live net apply only — **not** on load). **`numberOverrideActive`** (default false) + **`numberOverride`** (`int`, highway-marker style) |
| Initial UX | **Net-only association tool** (world overlays + node controls). **SIP out of scope** for this implementation wave. See [08](08-association-tool-first-ux.md) |
| Pack depth | Analysis + design notes — **no** implementation backlog / Kanban board work in this pack |
| Research method | kcs2m backend MCP for vanilla; in-repo C# for ADR |

## Reading order

1. [01-vanilla-aggregation-overview.md](01-vanilla-aggregation-overview.md) — components + `AggregateSystem` rules + tool pipeline
2. [02-consumer-audit.md](02-consumer-audit.md) — who reads aggregates; **display-vs-sim verdict**
3. [03-adr-current-touchpoints.md](03-adr-current-touchpoints.md) — how ADR hangs data off vanilla aggregates today
4. [04-parallel-system-design-notes.md](04-parallel-system-design-notes.md) — recommended ADR-owned design
5. [05-research-gaps-and-next.md](05-research-gaps-and-next.md) — remaining gaps + follow-up pack inputs
6. [06-labels-vs-vanilla-sync-and-harmony.md](06-labels-vs-vanilla-sync-and-harmony.md) — ADR labels vs Aggregation sync; unattended branches; Harmony viability
7. [07-membership-maintenance-edge-cases.md](07-membership-maintenance-edge-cases.md) — net remap + gap-fill continuity + edge-case matrix
8. [08-association-tool-first-ux.md](08-association-tool-first-ux.md) — **start UX with net-only association tool** + world draw + node controls; impact on epic order

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

## Related packs

- [2026040409_address-format-customization](../2026040409_address-format-customization/01_game-address-format-research.md) — addresses already resolve through the **aggregate** entity via `BuildingUtils.GetAddress`
