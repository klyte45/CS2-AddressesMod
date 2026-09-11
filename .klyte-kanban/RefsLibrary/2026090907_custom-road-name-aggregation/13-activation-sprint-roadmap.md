# ADR aggregation activation — sprint roadmap & Kanban catalog

*Added: 2026-09-10 · Companion to [12](12-activation-change-map.md) · **Documentation only** — do not create Kanban board items from this file unless a separate explicit ask*

## Goal

Define **one** implementation sprint for full ADR aggregation activation and a **ready-to-import Kanban catalog** (epic + ordered tasks). Board creation is **out of scope** for this doc pass.

Road Grouping **T0–T3** (tool, overlay, stubs, number cards) is already shipped — see [11](11-road-grouping-tool-roadmap.md). Activation replaces former T4/T5 “thin contract” with full architecture + mutations.

## Sprint

| Field | Value |
|-------|--------|
| Suggested name | **ADR aggregation activation** |
| Scope | Strip Aggregate highway host; markers visual-only; ADR components + `ForkFromVanilla`; name/address retarget; Road Grouping real mutations; membership maintenance |
| Depends | Road Grouping T0–T3 shipped |
| Acceptance | All six workstream tasks below done; ADR-linked edges name/number via ADR; tool writes membership; Aggregate highway components gone; markers visual-only with WE placeholders |

### Workstreams (ordered tasks inside the sprint)

| Order | Workstream | Scope | Depends | Acceptance sketch |
|-------|------------|-------|---------|-------------------|
| 1 | Highway strip | [12](12-activation-change-map.md) §B | — | No Aggregate `ADRHighway*` written; registry kept |
| 2 | Markers visual-only | [12](12-activation-change-map.md) §C | 1 | No route/mileage in runtime/UI; WE `[Feature]` placeholders + comments; pylons kept |
| 3 | ADR components + fork | [12](12-activation-change-map.md) §A | 1 | Full buffer copy + seed copy + vanilla `Redraw`; save/load round-trip |
| 4 | Name/address/SIP | [12](12-activation-change-map.md) §D | 3 | ADR-linked edges resolve via ADR buffer; split naming after fork |
| 5 | Road Grouping mutations | [12](12-activation-change-map.md) §E | 3–4 | Toggle/drag write membership; overlays/cards from ADR; fork on first edit |
| 6 | Membership maintenance | [07](07-membership-maintenance-edge-cases.md) | 5 | Gap-fill/remap dogfood via tool |

```text
1 ──► 2
 │
 └──► 3 ──► 4 ──► 5 ──► 6
```

**Later backlog (after this sprint):** highway reattach via Road Grouping tool; world name labels + forked pick; `ADREdgeData` / `fixedAggregation` cleanup; SIP RoadSection / H1.

---

## Kanban catalog (document only)

Suggested epic label: **`ADR aggregation activation`**

Suggested board sprint: **one** sprint with the same name, containing all `ACT-*` tasks below.

Import manually or via a **separate** MCP session. Each task links to [12](12-activation-change-map.md).

### Epic

| Field | Value |
|-------|--------|
| Label | `ADR aggregation activation` |
| Summary | Activate ADR-owned Aggregate/Element/Aggregated; strip Aggregate highway host; markers visual-only; fork-from-vanilla with seed rules; wire Road Grouping mutations; then membership maintenance |

### Tasks (all in the single sprint)

| ID (suggested) | Title | Order | Depends | Description |
|----------------|-------|-------|---------|-------------|
| ACT-01 | Strip Aggregate highway host | 1 | — | Remove `ADRHighwayAggregationData` / Dirty / Cache and all Aggregate-touching jobs in `AdrHighwayRoutesSystem` / `2B`. Keep highway **registry**. Update consumers (NameSystem highway branch, GetAddress cache, regions, Road Grouping numbering). Beta: drop leftover components on load. See [12](12-activation-change-map.md) §B. |
| ACT-02 | Markers visual-only + WE placeholders | 2 | ACT-01 | Strip route/mileage/display fields from `ADRHighwayMarkerData` (keep pylons). Strip tool options / SIP / TS bindings. WE: keep formula tags; use `[Feature Name]` placeholders + XML comments for old formulae. Gut bridge/tooltip route/mileage. See [12](12-activation-change-map.md) §C. |
| ACT-03 | ADR components + ForkFromVanilla | 3 | ACT-01 | Add `ADRNameAggregate` / `ADRNameAggregateElement` / `ADRNameAggregated` + serialize. Membership service with create/destroy/add/remove/flags. `ForkFromVanilla`: copy seed S → ADR, `Redraw` vanilla, full `AggregateElement` copy then apply edit. Seed ADR via `AdrNamesetSystem`. See [12](12-activation-change-map.md) §A. |
| ACT-04 | Prefer ADR in name/address/SIP | 4 | ACT-03 | Retarget `AdrNameSystemOverrides`, `BuildingUtilsOverrides`, `AdrRoadAddressNumbering`, `AdrSelectionInfoPanelSystem` (+ seed UI). Split naming: vanilla seed S2 for non-ADR edges; ADR for linked members. See [12](12-activation-change-map.md) §D. |
| ACT-05 | Road Grouping ADR mutations | 5 | ACT-03, ACT-04 | Tool: fork on first membership apply; read/write ADR buffer; retire log stubs. Controller cards + overlay belonging/remainder from ADR. Verify VUIO entity Index/Version. See [12](12-activation-change-map.md) §E. |
| ACT-06 | Membership maintenance dogfood | 6 | ACT-05 | Remap + gap-fill per [07](07-membership-maintenance-edge-cases.md); dogfood via Road Grouping tool. |

### Optional later backlog tasks (same or new epics)

| Title | Notes |
|-------|--------|
| Highway reattach via Road Grouping tool | Re-host route/mileage on ADR aggregates; restore WE formulae from placeholders |
| ADR world name labels + forked RaycastLabelsJob | [06](06-labels-vs-vanilla-sync-and-harmony.md) |
| Cleanup `ADREdgeData.fixedAggregation` | Delete or leave inert — [03](03-adr-current-touchpoints.md) |
| SIP RoadSection / DefaultTool H1 | Explicitly deferred |

### Suggested board sprint

| Board sprint | Tasks |
|--------------|-------|
| **ADR aggregation activation** | ACT-01 … ACT-06 (all) |

---

## Relation

- [12](12-activation-change-map.md) — file-level change map (sprint tags = workstream order 1–6)
- [11](11-road-grouping-tool-roadmap.md) — T0–T3 shipped; T4/T5 superseded by this activation sprint
- [04](04-parallel-system-design-notes.md) — component design
- [07](07-membership-maintenance-edge-cases.md) — ACT-06 rules
- [08](08-association-tool-first-ux.md) — tool-first UX; full activation (not thin contract)
