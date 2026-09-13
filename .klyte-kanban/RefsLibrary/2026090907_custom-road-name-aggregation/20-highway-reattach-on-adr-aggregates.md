# Highway reattach on ADR name-aggregates

*Added: 2026-09-12 · Companion roadmap: [21](21-highway-reattach-roadmap.md) · Activation strip: [12](12-activation-change-map.md) ACT-01/ACT-02 · Parallel design: [04](04-parallel-system-design-notes.md)*

## Goal

Re-host **highway route association**, **mileage override**, and **route direction** on **ADR name-aggregates** (`ADRNameAggregate`), edited from the **Addresses SIP** and from **Road Grouping tool options** (same ADR field set). Vanilla aggregates keep today’s seed-only Addresses settings. Road markers stay **visual-only**. WE formulae restore and read the new ADR-hosted component (plus the highway registry).

This doc is **design only**. Implementation catalog: [21](21-highway-reattach-roadmap.md). Board tasks are created via MCP only when explicitly requested.

## Locked decisions

| Topic | Choice |
|-------|--------|
| Host | `ADRNameAggregate` only — never vanilla `Aggregate` |
| Authored data | One serializable component: **`ADRNameAggregateHighwayData`** |
| Cache | **None** — no `ADRHighwayAggregationCacheData` / bake jobs |
| Settings UX | Addresses SIP **and** Road Grouping tool options share ADR aggregate settings (seed + highway); SIP highway block for **ADR** selections only |
| Vanilla Aggregate SIP | Unchanged (seed / nameset only) |
| Markers | Visual-only (`ADRHighwayMarkerData` pylons + display/custom params) |
| WE | Restore formulae; resolve context from attached edge → `ADRNameAggregated` → `ADRNameAggregateHighwayData` (+ registry) |
| Registry | Keep `HighwayData` + Region register UI |
| Road Grouping tool options | When an ADR aggregate is selected: **custom name** input (replaces `Aggregation #Index`), **seed** controls (same as SIP), **highway** block (same as SIP). Membership gestures unchanged |
| Saves | **Out of scope:** no migration of pre-strip Aggregate- or marker-hosted highway identity. Mod is **beta** — old highway-on-Aggregate / marker-route saves may lose that association; players re-assign via SIP/tool |

## Why no cache

Former `ADRHighwayAggregationCacheData` was **derived**, not player-authored. `CalculateHighwayCacheData` scanned **markers** on the aggregate for the winning `overrideMileage` / `newMileage` / `reverseMileageCounting`, then baked:

- `startDistanceOverrideKm` — chain-start baseline so mileage at the marker matched `newMileage`
- `reverseCounting` — from that marker

The cache was removed when members or marker settings changed, then rebuilt. Address code read the cache instead of re-scanning markers.

With mileage/direction authored on the **ADR aggregate via SIP**, there is nothing to bake from markers. Store authored fields on `ADRNameAggregateHighwayData`; numbering and WE read them directly. Dirty names/addresses when SIP fields change (same pattern as other ADR rule changes). Do **not** revive `AdrHighwayRoutes2BSystem` or `CalculateHighwayCacheData`.

**Product change:** multi-marker mileage overrides along one route go away — **one** override set per ADR name-aggregate.

---

## A. Reuse inventory (keep)

| Asset | Role in reattach |
|-------|------------------|
| [`ADRNameAggregate`](../../../BelzontAdr/Components/ADRNameAggregate.cs) / [`ADRNameAggregateElement`](../../../BelzontAdr/Components/ADRNameAggregateElement.cs) / [`ADRNameAggregated`](../../../BelzontAdr/Components/ADRNameAggregated.cs) | Membership host; highway component lives on the ADR aggregate entity |
| [`AdrHighwayRoutesSystem.HighwayData`](../../../BelzontAdr/Systems/AdrHighwayRoutesSystem.cs) + serialize | Registry CRUD (`Id`, prefix, suffix, name, `refStartPoint`) |
| [`HighwayRegisterManagement.tsx`](../../../_Frontends/UI/k45_adr_vuio/src/settings/tabs/region/subpages/HighwayRegisterManagement.tsx) | Settings UI for registry entries |
| [`ADRHighwayMarkerData`](../../../BelzontAdr/Components/ADRHighwayMarkerData.cs) + dirty pipeline | Visual markers only (pylons, display, custom params) |
| [`AdrSelectionInfoPanelSystem`](../../../BelzontAdr/Systems/AdrSelectionInfoPanelSystem.cs) `RoadAggregation` | SIP entity options shell — extend DTO with ADR flag + highway fields |
| [`AddressesInfoOptionsComponent.tsx`](../../../_Frontends/UI/k45_adr_vuio/src/components/AddressesInfoOptionsComponent.tsx) / [`SeedManagementOptions.tsx`](../../../_Frontends/UI/k45_adr_vuio/src/components/SeedManagementOptions.tsx) | Shared seed UI; add ADR-only highway section sibling; reuse in tool options where practical |
| [`RoadGroupingToolOptions.tsx`](../../../_Frontends/UI/k45_adr_vuio/src/tool/RoadGroupingToolOptions.tsx) / [`AdrRoadGroupingController`](../../../BelzontAdr/Controllers/AdrRoadGroupingController.cs) | Expand tool panel beyond pick/clear; today shows `selected #Index` — replace with custom name + ADR settings |
| [`AdrRoadAddressNumbering`](../../../BelzontAdr/Utils/AdrRoadAddressNumbering.cs) | Re-plumb mileage/orientation from authored ADR highway fields |
| [`RoadMarkerInfoBridge`](../../../BelzontAdr/Bridge/RoadMarkerInfoBridge.cs) | Reimplement resolve path via ADR aggregate highway data (see WE / bridge below) |

Per-member house-number override (`numberOverrideActive` / `numberOverride` on `ADRNameAggregateElement`) stays as today — **not** highway km; pattern is analogous but separate.

---

## B. Gone / do not revive as-was

Removed in activation ACT-01 / ACT-02 (commits such as `e65d6fa`, `d3eae02`):

| Former piece | Why not revive as-was |
|--------------|------------------------|
| `ADRHighwayAggregationData` / `DirtyHwId` / `CacheData` on vanilla `Aggregate` | Wrong host; cache obsolete |
| Marker↔Aggregate filler / replicate / `CalculateHighwayCacheData` | Marker no longer owns route/mileage |
| `AdrHighwayRoutes2BSystem` | Cache invalidate on vanilla `Aggregated` churn |
| Marker fields `routeDataIndex`, `RouteDirection`, mileage override trio on `ADRHighwayMarkerData` | Settings move to ADR aggregate SIP |

Historical field map (for implementers reading old saves / WE comments):

| Old location | Old field | New location |
|--------------|-----------|--------------|
| Aggregate `ADRHighwayAggregationData` | `highwayDataId` | `ADRNameAggregateHighwayData.highwayDataId` |
| Aggregate cache | `startDistanceOverrideKm` | authored `mileageOverrideKm` when override active (chain-start baseline) |
| Aggregate cache | `reverseCounting` | `ADRNameAggregateHighwayData.reverseCounting` |
| Marker | `routeDataIndex` | `highwayDataId` on ADR aggregate |
| Marker | `RouteDirection` | `routeDirection` on ADR aggregate |
| Marker | `overrideMileage` / `newMileage` / `reverseMileageCounting` | `mileageOverrideActive` / `mileageOverrideKm` / `reverseCounting` |

---

## C. New component — `ADRNameAggregateHighwayData`

On entities that have `ADRNameAggregate`:

| Field | Type | Default | Notes |
|-------|------|---------|-------|
| `highwayDataId` | `Hash128` | `default` | Link to registry; `default` = not a highway route |
| `routeDirection` | enum (former marker `RouteDirection`) | `NONE` (or first enum value) | WE / display cardinals + INTERNAL/EXTERNAL |
| `mileageOverrideActive` | `bool` | `false` | When false, ignore `mileageOverrideKm` |
| `mileageOverrideKm` | `float` | `0` | When active: **chain-start** mileage baseline (former cache `startDistanceOverrideKm`). No marker-curve bake |
| `reverseCounting` | `bool` | `false` | Mileage growth orientation along the ADR element walk |

Serialization: `ISerializable` with version bump discipline like other ADR components. Prefer always present on ADR aggregates (defaults = inactive) so SIP does not special-case missing component; alternatively add on first SIP write — pick one in HW-01 and stick to it.

Optional empty dirty tag (`ADRNameAggregateHighwayDataDirty`) only if burst jobs need a query bit; otherwise SIP write + existing name-rules dirty (`OnChangedRoadNameGenerationRules` / equivalent) is enough.

### Lifecycle

- Create/destroy with ADR name-aggregate entity (membership service).
- Membership remap / gap-fill ([07](07-membership-maintenance-edge-cases.md)): highway data stays on the ADR entity; dirty naming/address/WE consumers.
- Empty ADR teardown: destroy highway component with the entity.
- `highwayDataId == default`: naming/address treat as non-highway; WE may still show direction/custom params from marker visual fields only.

### `RouteDirection` enum (restore from pre-ACT-02 marker)

`NONE`, `NORTH`, `NORTHEAST`, `EAST`, `SOUTHEAST`, `SOUTH`, `SOUTHWEST`, `WEST`, `NORTHWEST`, `INTERNAL`, `EXTERNAL` — keep names compatible with former WE formulae / bridge consumers where practical.

---

## D. Addresses SIP

Both vanilla Aggregate and ADR name-aggregate already resolve as `AdrEntityType.RoadAggregation` (edge → preferred ADR aggregate, or aggregate entity itself).

```text
Selected RoadAggregation
  ├─ always: SeedManagement (nameset / vanilla generator / seed dice)
  └─ only if isAdrAggregate: HighwayAggregateSettings
        ├─ highway registry picker (highwayDataId)
        ├─ routeDirection
        ├─ mileageOverrideActive + mileageOverrideKm
        └─ reverseCounting
```

### Backend

- Extend `getEntityOptions` / `AdrEntityData` (or sibling DTO) with **`isAdrAggregate: bool`** when `type == RoadAggregation`.
- When ADR: include current highway field values (and optionally registry list via existing `listHighwaysRegistered`).
- Bindings to get/set highway fields on the resolved ADR entity (prefer `AdrSelectionInfoPanelSystem` or `AdrHighwayRoutesSystem` — one binder surface; document choice in HW-02).
- Writes must dirty name/address consumers; no Aggregate host writes.

### Frontend

- [`AddressesInfoOptionsComponent.tsx`](../../../_Frontends/UI/k45_adr_vuio/src/components/AddressesInfoOptionsComponent.tsx): for `RoadAggregation`, keep seed UI; if `isAdrAggregate`, also mount a new `HighwayAggregateSettings` (or equivalent) section.
- Vanilla Aggregate path: no highway controls.
- Prefer **shared presentational pieces** (`SeedManagementOptions`, `HighwayAggregateSettings`) so SIP and Road Grouping tool stay in sync.
- i18n: pending `*_entries.txt` only (do not edit `i18n.csv`).

---

## D2. Road Grouping tool options

Today [`RoadGroupingToolPanel`](../../../_Frontends/UI/k45_adr_vuio/src/tool/RoadGroupingToolOptions.tsx) shows pick prompt or `selected #${Index}`, plus show-number-cards / pick-another actions.

When `SelectedAggregation` is an **ADR** name-aggregate, the tool options panel must expose the **same ADR aggregate settings as Addresses SIP**, plus a custom name field:

```text
Road Grouping tool options (ADR selection)
  ├─ Custom name text input  (replaces "Aggregation #Index" / entity Index display)
  ├─ SeedManagement          (same controls as SIP — nameset / vanilla generator / seed dice)
  ├─ HighwayAggregateSettings (same as SIP)
  └─ existing actions        (show number cards, pick another)
```

| Requirement | Detail |
|-------------|--------|
| Custom name | Editable field bound to the ADR aggregate’s **custom name** (vanilla `NameSystem` custom-name APIs already used elsewhere for aggregates). Empty / cleared → fall back to nameset / highway / generator naming like SIP. **Do not** show raw entity Index as the primary label |
| Seed | Reuse SIP seed bindings / UI against `SelectedAggregation` |
| Highway | Same fields as §D; only meaningful for ADR selection (tool already coerces focus to ADR after fork) |
| Vanilla-only selection | If the tool ever holds a non-ADR Aggregate before fork, keep pick/prompt UX only — no highway block; after first membership edit / fork, switch to the ADR panel above |

Bindings may live on `AdrRoadGroupingController` (tool-scoped) calling the same backend setters as SIP, or reuse SIP binders with the tool’s selected entity — pick one surface in HW-03b and avoid divergent write paths.

---

## E. Consumers to retarget

| Consumer | Required behavior |
|----------|-------------------|
| [`AdrNameSystemOverrides`](../../../BelzontAdr/Overrides/AdrNameSystemOverrides.cs) | If edge/ADR has non-default `highwayDataId`, highway name from registry (prefix/suffix/name rules as before ACT-01); honor custom name when set |
| [`AdrRoadAddressNumbering`](../../../BelzontAdr/Utils/AdrRoadAddressNumbering.cs) / [`BuildingUtilsOverrides`](../../../BelzontAdr/Overrides/BuildingUtilsOverrides.cs) | Prefer ADR walk; when highway override active, use `mileageOverrideKm` + `reverseCounting` as chain baseline/orientation |
| Road Grouping number cards ([`AdrRoadGroupingController`](../../../BelzontAdr/Controllers/AdrRoadGroupingController.cs)) | Same numbering helpers; tool options own name/seed/highway UI (not the cards) |
| [`AdrRegionsSystem`](../../../BelzontAdr/Systems/AdrRegionsSystem.cs) / Region map VUIO | **Required** (HW-07): class/hover by ADR `highwayDataId` |
| WE default XML + WE integration | Restore formulae from ACT-02 `[Feature]` placeholders + XML comments; resolve route/mileage/direction via attached net → `ADRNameAggregated.m_Aggregate` → `ADRNameAggregateHighwayData` + registry |
| [`RoadMarkerInfoBridge`](../../../BelzontAdr/Bridge/RoadMarkerInfoBridge.cs) | Stop returning defaults for route/direction when reimplemented: resolve from owning ADR aggregate of the marker’s attached edge (or document bridge deprecation if WE uses a different path) |
| Marker SIP / tool ([`RoadMarkSettings.tsx`](../../../_Frontends/UI/k45_adr_vuio/src/components/RoadMarkSettings.tsx), tool options) | Unchanged visual surface |

---

## F. Explicit non-goals

- Restoring marker-authored `routeDataIndex` / mileage / direction on `ADRHighwayMarkerData`
- **Save migration** of pre-strip Aggregate- or marker-hosted highway identity (**beta** — out of scope for this feature; re-assign manually)
- Highway shields / shield+name on world name labels ([17](17-label-extensions-text-images-look.md) — still deferred)
- Putting highway data back on vanilla `Aggregate`
- Replacing Road Grouping **membership** gestures with SIP (membership stays tool-first; settings are dual-surface)

---

## Relation to prior pack docs

| Doc | Status relative to this epic |
|-----|------------------------------|
| [03](03-adr-current-touchpoints.md) | Historical “highway on vanilla Aggregate” — superseded for runtime after ACT-01 |
| [04](04-parallel-system-design-notes.md) / [05](05-research-gaps-and-next.md) | “Reattach via Road Grouping tool” alone → **superseded**: reattach via **Addresses SIP + Road Grouping tool options** on ADR; see this doc + [21](21-highway-reattach-roadmap.md) |
| [12](12-activation-change-map.md) / [13](13-activation-sprint-roadmap.md) | ACT-01/02 strip remains; later backlog points here |
| [07](07-membership-maintenance-edge-cases.md) §G | Still valid: highway stays on ADR entity across remap/gap-fill (now authored component, not cache) |
