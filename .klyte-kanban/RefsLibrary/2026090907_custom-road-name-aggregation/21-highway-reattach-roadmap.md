# Highway reattach — implementation roadmap & Kanban catalog

*Added: 2026-09-12 · Updated: 2026-09-14 (Sprint 014 closed — HW-01…HW-08 delivered; HW-03c/0085 cancelled → NET-04) · Design: [20](20-highway-reattach-on-adr-aggregates.md) · Activation strip: [12](12-activation-change-map.md) ACT-01/ACT-02 · Membership: [07](07-membership-maintenance-edge-cases.md)*

## Goal

Define the **implementation roadmap** for re-hosting highway route / mileage / direction on **ADR name-aggregates**, edited from the **Addresses SIP** and **Road Grouping tool options** (shared ADR field set + custom name), with WE formulae restored from ADR-hosted data.

Board epic **`adr-hw-reattach`** delivered in **Sprint 014** (IDs **0076–0084**; **0085** cancelled → [0086] NET-04).

## Prerequisites

| Depends on | Why |
|------------|-----|
| Activation **ACT-01** / **ACT-02** | Aggregate highway host stripped; markers visual-only; registry kept |
| Activation **ACT-03a…ACT-04** (at least) | ADR name-aggregate entities + naming/address preference so SIP and consumers have a real host |
| Prefer **ACT-05 / ACT-06** done | Membership edits + maintenance exist; tool `SelectedAggregation` coerces to ADR; SIP resolves `RoadAggregation` |

Do **not** start HW-* before ADR components exist; highway data has nowhere to live.

## Scope

| In | Out |
|----|-----|
| `ADRNameAggregateHighwayData` on ADR aggregates | Highway fields on vanilla `Aggregate` |
| Addresses SIP highway block for ADR only | Marker route/mileage authorship |
| Road Grouping tool options: custom name + seed + highway (parity with ADR SIP) | **Save migration** (pre-ACT-01 Aggregate/marker highway) — **beta; out of scope** |
| Naming + addressing + Road Grouping cards from authored fields | Highway shields on world name labels ([17](17-label-extensions-text-images-look.md)) |
| WE formula restore + resolve via ADR aggregate | Separate mileage cache / `CalculateHighwayCacheData` / `AdrHighwayRoutes2BSystem` |
| Regions map / registry hover by ADR `highwayDataId` (HW-07) | Replacing membership gestures with SIP |
| Smoke / acceptance (HW-08) | |

## Recommended epic / sprint

| Field | Value |
|-------|--------|
| Epic label | **ADR highway reattach** |
| Epic slug | `adr-hw-reattach` (≤15 chars for board) |
| Sprint plan | **Sprint 014** closed 2026-09-14 — HW-01…HW-08 done; HW-03c (**0085**) cancelled → NET-04 |
| Effort band | Sized to fit a single sprint end-to-end |
| Board IDs | **0076–0084** |

## Task catalog

| Order | ID | Board | Title | Size | Depends | Outcome |
|-------|-----|-------|-------|------|---------|---------|
| 1 | HW-01 | **0076** | Component + serialize on `ADRNameAggregate`; create/destroy with membership | M | ACT-03a+ | `ADRNameAggregateHighwayData` present with defaults; torn down with empty ADR aggregates |
| 2 | HW-02 | **0077** | SIP backend: `isAdrAggregate` + get/set highway (+ shared setters for tool) | M | HW-01 / 0076 | DTO distinguishes ADR vs vanilla; fields read/write on ADR entity; dirty naming rules |
| 3 | HW-03 | **0078** | VUIO Addresses SIP highway section (ADR-only) + i18n `*_entries.txt` | M | HW-02 / 0077 | Seed UI for both; highway picker / direction / mileage / reverse only for ADR; shared components preferred |
| 4 | HW-03b | **0079** | Road Grouping tool options: custom name + seed + highway parity | M | HW-02 / 0077, HW-03 / 0078 | Replace `Aggregation #Index` with custom name input; mount seed + highway against `SelectedAggregation` |
| 5 | HW-04 | **0080** | Naming override branch (`AdrNameSystemOverrides`) | M | HW-01 / 0076 | Non-default `highwayDataId` → registry highway name; custom name when set |
| 6 | HW-05 | **0081** | Mileage/addressing + Road Grouping cards use authored fields | M | HW-01 / 0076 | Override + reverse honored; no marker/cache scan |
| 7 | HW-06 | **0082** | WE formula restore + bridge resolve via ADR aggregate | L | HW-01 / 0076 | Markers show route/mileage/direction from owning ADR aggregate + registry |
| 8 | HW-07 | **0083** | Regions map / registry hover by ADR `highwayDataId` | S | HW-01 / 0076 | Region map classes/hovers by ADR highway id |
| 9 | HW-08 | **0084** | Smoke / acceptance checklist | S | HW-03…HW-07 / 0078–0083 | Dogfood matrix below green |

```text
ACT-03a…ACT-04+ (prereq)
    │
    ▼
HW-01 ──► HW-02 ──► HW-03 ──► HW-03b
    │
    ├──► HW-04
    ├──► HW-05
    ├──► HW-06
    └──► HW-07
              │
              ▼
           HW-08
```

### HW-01 — Component + lifecycle

- Add [`ADRNameAggregateHighwayData`](20-highway-reattach-on-adr-aggregates.md) (`highwayDataId`, `routeDirection`, `mileageOverrideActive`, `mileageOverrideMeters`, `reverseCounting`).
- `ISerializable`; register like other ADR components.
- Attach on ADR aggregate create; destroy with entity; defaults = inactive / no route.
- No cache struct; no `AdrHighwayRoutes2BSystem`.

### HW-02 — SIP / shared backend

- Extend `AdrSelectionInfoPanelSystem` / `AdrEntityData`: `isAdrAggregate` when `RoadAggregation`.
- Bindings get/set highway fields on resolved ADR entity.
- Keep setters usable from tool (same entity target) so HW-03b does not fork write logic.
- On write: dirty name/address consumers (same family as seed redraw).
- Custom-name get/set via existing NameSystem custom-name path (used by SIP/tool).

### HW-03 — SIP frontend

- Branch in `AddressesInfoOptionsComponent` (or sibling): ADR-only `HighwayAggregateSettings`.
- Prefer extractable shared UI for seed + highway so tool can import the same pieces.
- Registry list from existing `listHighwaysRegistered` / save APIs.
- i18n via pending `*_entries.txt` only.

### HW-03b — Road Grouping tool options parity

- In [`RoadGroupingToolOptions.tsx`](../../../_Frontends/UI/k45_adr_vuio/src/tool/RoadGroupingToolOptions.tsx): when ADR selection is active, **remove** primary display of `selected #${Index}`.
- Add **custom name** text input bound to the selected ADR aggregate.
- Mount **SeedManagement** (same as SIP) and **HighwayAggregateSettings** (same as SIP) for that entity.
- Keep existing actions (number cards toggle, pick another).
- Wire via `AdrRoadGroupingController` and/or shared SIP binders — one write path with HW-02.
- i18n pending entries for tool labels as needed.

### HW-04 — Naming

- Restore highway name branch keyed by ADR `highwayDataId` + registry (not vanilla Aggregate).
- Custom name from SIP/tool wins when set (same priority as existing custom-name rules).

### HW-05 — Numbering

- `AdrRoadAddressNumbering` / `BuildingUtilsOverrides`: use authored mileage baseline + `reverseCounting`.
- Road Grouping cards inherit via shared helpers.

### HW-06 — WE + bridge

- Restore formulae from ACT-02 placeholders / XML comments on highway marker WE defaults.
- Resolve: marker → attached edge → `ADRNameAggregated` → `ADRNameAggregateHighwayData` → registry.
- Update [`RoadMarkerInfoBridge`](../../../BelzontAdr/Bridge/RoadMarkerInfoBridge.cs) accordingly (or document WE-only path if bridge stays unused).

### HW-07 — Regions map

- Expose ADR `highwayDataId` for Region map class/hover (and related VUIO) so registry association is visible again after ACT-01 strip.
- [`AdrRegionsSystem`](../../../BelzontAdr/Systems/AdrRegionsSystem.cs) + Region editor bindings: prefer ADR-linked edges’ highway id over net-classification-only paths where applicable.
- **Required** in this sprint — not deferrable.

### HW-08 — Acceptance

See checklist below.

## Acceptance checklist

- [x] Selecting a **vanilla** Aggregate (or edge without ADR) shows Addresses seed UI only — **no** highway controls
- [x] Selecting an **ADR** name-aggregate (or ADR-linked edge) shows seed UI **plus** highway route / direction / mileage override / reverse
- [x] Road Grouping tool with ADR selection shows **custom name** input (not entity Index / `Aggregation #`)
- [x] Tool options expose the **same seed + highway** controls as ADR Addresses SIP; edits from either surface stay consistent
- [x] Assigning a registry highway changes road naming for ADR-linked edges
- [x] Mileage override + reverse change house-number / mileage labels along the ADR chain
- [x] Road markers remain visual-only in marker tool + marker SIP; no route/mileage fields on marker component
- [x] WE on markers shows route / mileage / direction from the owning ADR aggregate’s highway data
- [x] Region map / registry hover reflects ADR `highwayDataId` for linked roads
- [x] Empty / default `highwayDataId` behaves as non-highway
- [x] Destroying last ADR membership tears down highway component with the aggregate
- [x] No revive of Aggregate-hosted highway cache or marker↔Aggregate sync jobs
- [x] No save-migration work for old Aggregate/marker highway identity (**beta** — out of scope; manual re-assign OK)
- [x] i18n pending entries only (no agent edits to `i18n.csv`)
- [x] All HW-01…HW-08 completed in the **same** sprint

*Sprint 014 dogfood 2026-09-14. Known gap: road-only seed gate (former HW-03c / **0085**) deferred to NET-04 **0086**.*

## Later (not this epic)

- Highway shields / multi-recipe world name labels ([17](17-label-extensions-text-images-look.md))
- *(Not planned: save migration of pre-ACT-01 Aggregate/marker highway — beta breakage accepted)*
