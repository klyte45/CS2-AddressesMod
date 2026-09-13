# ADR aggregation activation — sprint roadmap & Kanban catalog

*Added: 2026-09-10 · Updated: 2026-09-10 (ACT-03 split) · Companion to [12](12-activation-change-map.md)*

## Goal

Define **one** implementation sprint for full ADR aggregation activation and a **Kanban catalog** (epic + ordered tasks). Board items are created via MCP when explicitly requested (not from this doc alone).

Road Grouping **T0–T3** (tool, overlay, stubs, number cards) is already shipped — see [11](11-road-grouping-tool-roadmap.md). Activation replaces former T4/T5 “thin contract” with full architecture + mutations.

## Sprint

| Field | Value |
|-------|--------|
| Suggested name | **ADR aggregation activation** |
| Epic slug | `adr-activation` |
| Scope | Strip Aggregate highway host; markers visual-only; ADR components → membership service → `ForkFromVanilla`; name/address retarget; Road Grouping real mutations; membership maintenance |
| Depends | Road Grouping T0–T3 shipped |
| Acceptance | All workstream tasks below done; ADR-linked edges name/number via ADR; tool writes membership; Aggregate highway components gone; markers visual-only with WE placeholders |

### Workstreams (ordered tasks inside the sprint)

| Order | Workstream | Scope | Depends | Acceptance sketch |
|-------|------------|-------|---------|-------------------|
| 1 | Highway strip | [12](12-activation-change-map.md) §B | — | Aggregate highway struct fields removed; old saved data ignored on load; matching frontend Aggregate-host fields/bindings removed when applicable; registry kept |
| 2 | Markers visual-only | [12](12-activation-change-map.md) §C | 1 | No route/mileage in runtime/UI; WE `[Feature]` placeholders + comments; pylons kept |
| 3 | ADR components + serialize | [12](12-activation-change-map.md) §A1 | 1 | `ADRNameAggregate` / `Element` / `Aggregated` exist, serialize, registered |
| 4 | Membership service CRUD | [12](12-activation-change-map.md) §A2 | 3 | Create/destroy/add/remove/flags/teardown empty |
| 5 | ForkFromVanilla + ADR seed | [12](12-activation-change-map.md) §A3 | 4 | Seed copy + vanilla `Redraw` + full buffer copy then edit; `AdrNamesetSystem` seeds ADR |
| 6 | Name/address/SIP | [12](12-activation-change-map.md) §D | 5 | ADR-linked edges resolve via ADR buffer; split naming after fork |
| 7 | Road Grouping mutations | [12](12-activation-change-map.md) §E | 5–6 | Toggle/drag write membership; overlays/cards from ADR; fork on first edit |
| 8 | Membership maintenance | [07](07-membership-maintenance-edge-cases.md) | 7 | Gap-fill/remap dogfood via tool |

```text
1 ──► 2
 │
 └──► 3a ──► 3b ──► 3c ──► 4 ──► 5 ──► 6
```

**Later backlog (after this sprint):** highway reattach via Addresses SIP + Road Grouping tool options ([20](20-highway-reattach-on-adr-aggregates.md) / [21](21-highway-reattach-roadmap.md)); world name labels + forked pick; `ADREdgeData` / `fixedAggregation` cleanup; SIP RoadSection / H1.

---

## Kanban catalog

Suggested epic label (human): **`ADR aggregation activation`**  
Suggested epic slug: **`adr-activation`**

Suggested board sprint: **one** sprint with the same name, containing all `ACT-*` tasks below.

Each task links to [12](12-activation-change-map.md).

### Epic

| Field | Value |
|-------|--------|
| Label | `ADR aggregation activation` |
| Slug | `adr-activation` |
| Summary | Activate ADR-owned Aggregate/Element/Aggregated; strip Aggregate highway host; markers visual-only; fork-from-vanilla with seed rules; wire Road Grouping mutations; then membership maintenance |

### Tasks (all in the single sprint)

| ID (suggested) | Board ID | Title | Order | Depends | Description |
|----------------|----------|-------|-------|---------|-------------|
| ACT-01 | **0061** | Strip Aggregate highway host | 1 | — | **Remove** unused Aggregate highway fields from the structs (`ADRHighwayAggregationData.highwayDataId`; `DirtyHwId`; `CacheData.startDistanceOverrideKm` / `reverseCounting`) — prefer delete the types. On load, **ignore** previously saved Aggregate highway data (no migrate). Gut Aggregate-touching jobs in `AdrHighwayRoutesSystem` / `2B`. Update consumers (NameSystem highway branch, GetAddress cache, regions, Road Grouping numbering). **Frontend:** remove VUIO/DTO/bindings for Aggregate-hosted highway Data/Cache when applicable (e.g. RegionEditor Aggregate-linked `highwayId`); keep highway **registry** UI. Marker route/mileage UI → ACT-02. See [12](12-activation-change-map.md) §B. |
| ACT-02 | **0062** | Markers visual-only + WE placeholders | 2 | ACT-01 / 0061 | Strip from `ADRHighwayMarkerData`: `routeDataIndex`, `routeDirection`, `displayInformation`, `numericCustomParam1/2`, `overrideMileage`, `newMileage`, `reverseMileageCounting`. **Keep** `pylonCount`, `pylonSpacing`, `pylonMaterial`, `pylonFormat`, `pylonHeight`. Strip tool options / SIP / TS bindings. WE: keep formula tags; `[Feature Name]` placeholders + XML comments. Gut bridge/tooltip route/mileage. See [12](12-activation-change-map.md) §C. |
| ACT-03a | **0063** | ADR name-aggregate components + serialize | 3 | ACT-01 / 0061 | Add `ADRNameAggregate` (tag), `ADRNameAggregateElement` buffer (`m_Edge`, `preventPropagation`, `numberOverrideActive`, `numberOverride`), `ADRNameAggregated` (edge→ADR link + mirrored flags). `ISerializable` + register like other ADR components. No gameplay yet. See [12](12-activation-change-map.md) §A1. |
| ACT-03b | **0064** | Membership service CRUD | 4 | ACT-03a / 0063 | Membership service: create/destroy ADR aggregate; add/remove/reorder members; set `preventPropagation` / `numberOverrideActive` / `numberOverride`; tear down empty. Does **not** include fork yet. See [12](12-activation-change-map.md) §A2. |
| ACT-03c | **0065** | ForkFromVanilla + ADR seeding | 5 | ACT-03b / 0064 | `ForkFromVanilla`: copy vanilla `ADRRandomizationData` seed S → ADR; `Redraw()` vanilla → S2; full `AggregateElement` copy into ADR buffer then apply first edit; set `ADRNameAggregated` on members. `AdrNamesetSystem` also seeds new ADR aggregates. See [12](12-activation-change-map.md) §A3. |
| ACT-04 | **0066** | Prefer ADR in name/address/SIP | 6 | ACT-03c / 0065 | Retarget `AdrNameSystemOverrides`, `BuildingUtilsOverrides`, `AdrRoadAddressNumbering`, `AdrSelectionInfoPanelSystem` (+ seed UI). Split naming: vanilla seed S2 for non-ADR edges; ADR for linked members. See [12](12-activation-change-map.md) §D. |
| ACT-05 | **0067** | Road Grouping ADR mutations | 7 | ACT-03c / 0065, ACT-04 / 0066 | Tool: fork on first membership apply; read/write ADR buffer; retire log stubs. Controller cards + overlay belonging/remainder from ADR. Verify VUIO entity Index/Version. See [12](12-activation-change-map.md) §E. |
| ACT-06 | **0068** | Membership maintenance dogfood | 8 | ACT-05 / 0067 | Remap + gap-fill per [07](07-membership-maintenance-edge-cases.md); dogfood via Road Grouping tool. |

### Optional later backlog tasks (same or new epics)

| Title | Notes |
|-------|--------|
| Highway reattach via Addresses SIP + Road Grouping tool options | Catalog: [21](21-highway-reattach-roadmap.md) (HW-01…08; board **0076–0084**; epic `adr-hw-reattach`; **single sprint**, HW-07 required); design: [20](20-highway-reattach-on-adr-aggregates.md). SIP + tool: seed, highway, custom name for ADR; regions map; WE restore; no mileage cache |
| ADR world name labels + forked RaycastLabelsJob | Catalog: [18](18-label-takeover-sprint-roadmap.md) (LBL-01…06; Sprint 013). Arch: [15](15-adr-citywide-name-label-takeover.md). Archive pick notes: [06](06-labels-vs-vanilla-sync-and-harmony.md) |
| Cleanup `ADREdgeData.fixedAggregation` | Delete or leave inert — [03](03-adr-current-touchpoints.md) |
| SIP RoadSection / DefaultTool H1 | **In label epic:** SIP Aggregate-gated sections → [18](18-label-takeover-sprint-roadmap.md) **LBL-05**; DefaultTool H1 prefer LBL-05 / spill **LBL-08**. Design archive: [06](06-labels-vs-vanilla-sync-and-harmony.md) |

### Suggested board sprint

| Board sprint | Tasks |
|--------------|-------|
| **Sprint 012 — ADR aggregation activation** | 0061…0068 (ACT-01, ACT-02, ACT-03a, ACT-03b, ACT-03c, ACT-04, ACT-05, ACT-06) |

### Superseded backlog

Former Road Grouping **T4** / **T5** thin-contract tasks **0058** / **0059** were **cancelled (C)** when activation tasks were registered.

---

## Relation

- [12](12-activation-change-map.md) — file-level change map (sprint tags = workstream order; §A split into A1–A3)
- [11](11-road-grouping-tool-roadmap.md) — T0–T3 shipped; T4/T5 superseded by this activation sprint
- [04](04-parallel-system-design-notes.md) — component design
- [07](07-membership-maintenance-edge-cases.md) — ACT-06 rules
- [08](08-association-tool-first-ux.md) — tool-first UX; full activation (not thin contract)
