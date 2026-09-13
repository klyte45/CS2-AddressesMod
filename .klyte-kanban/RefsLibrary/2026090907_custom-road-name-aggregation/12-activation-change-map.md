# ADR aggregation activation — change map

*Added: 2026-09-10 · File-level map for full ADR-owned road aggregation activation · No product code in this doc · Workstream tags: [13](13-activation-sprint-roadmap.md)*

Supersedes “thin contract” as the next backend step. Road Grouping T0–T3 is already solid enough to bind to **effective** ADR membership data. This map lists every project touchpoint that must change.

## Locked decisions

| Topic | Choice |
|-------|--------|
| Component shape | `ADRNameAggregate`, `ADRNameAggregateElement`, `ADRNameAggregated` (+ per-member flags — [04](04-parallel-system-design-notes.md)) |
| Fork trigger | **First membership change** (Ctrl+LMB apply / endpoint-drag release). Plain select does **not** create ADR |
| Seed rule | Copy vanilla `ADRRandomizationData` seed → ADR; **`Redraw()`** vanilla Aggregate |
| Fork buffer init | **Full** vanilla `AggregateElement` copy into ADR buffer, **then** apply first edit |
| Naming after fork | **Split:** non-ADR edges → vanilla Aggregate name (new seed); ADR-linked edges → ADR name (copied seed) |
| Highway on Aggregate | **Remove** now (not migrate in this wave): delete Aggregate highway struct fields/types; **ignore** previously saved Aggregate highway data on load |
| Markers | **Visual-only** runtime/UI (strip route/mileage); **keep** pylon fields; **keep** highway **registry** settings for later tool epic |
| WE layout | Keep formula XML tags; **`[Feature Name]`** placeholders + **XML comments** for old formulae |
| Out of this wave | World labels / forked pick; SIP H1; highway reattach ([20](20-highway-reattach-on-adr-aggregates.md) / [21](21-highway-reattach-roadmap.md)) |

```text
On first membership edit:
  ADRNameAggregate.seed = vanilla.seed (S)
  vanilla.Redraw() → S2
  ADRNameAggregateElement = full AggregateElement copy + apply edit
  ADRNameAggregated on member edges
```

---

## A. New core data — **ACT-03a / ACT-03b / ACT-03c**

Split so components, CRUD, and fork ship as separate reviewable tasks ([13](13-activation-sprint-roadmap.md)).

### A1. Components + serialize — **ACT-03a**

| File / deliverable | Current role | Required change | Task |
|--------------------|--------------|-----------------|--------|
| `BelzontAdr/Components/ADRNameAggregate.cs` (new) | — | Tag component on ADR aggregate entity (parallel to vanilla `Aggregate`). Seed lives on co-located `ADRRandomizationData`, not as fields on this tag. | ACT-03a |
| `BelzontAdr/Components/ADRNameAggregateElement.cs` (new) | — | Ordered buffer entry fields: `m_Edge` (`Entity`), `preventPropagation` (`bool`, default `false`), `numberOverrideActive` (`bool`, default `false`), `numberOverride` (`int`, default `0`) | ACT-03a |
| `BelzontAdr/Components/ADRNameAggregated.cs` (new) | — | On edge: link to ADR aggregate entity + mirrored flags as useful for queries (`preventPropagation`, `numberOverrideActive`, `numberOverride`) | ACT-03a |
| Serialization | — | `ISerializable` on all three; register like other ADR components | ACT-03a |
| [`ADREdgeData.cs`](../../../BelzontAdr/Components/ADREdgeData.cs) `fixedAggregation` | Dead stub | **Do not** use as link; leave inert or delete in later cleanup epic | — |

### A2. Membership service CRUD — **ACT-03b**

| File / deliverable | Current role | Required change | Task |
|--------------------|--------------|-----------------|--------|
| Membership service / system (new) | — | Create/destroy ADR aggregate entity; add/remove/reorder buffer members; set per-member flags; tear down empty aggregates; keep `ADRNameAggregated` in sync with buffer | ACT-03b |

Does **not** include `ForkFromVanilla` or nameset seeding (those are ACT-03c).

### A3. ForkFromVanilla + ADR seeding — **ACT-03c**

| File / deliverable | Current role | Required change | Task |
|--------------------|--------------|-----------------|--------|
| Membership service — `ForkFromVanilla` | — | On first membership edit: copy vanilla `ADRRandomizationData` seed **S** → ADR; call vanilla Aggregate `Redraw()` → **S2**; full vanilla `AggregateElement` copy into `ADRNameAggregateElement`, **then** apply the edit; set `ADRNameAggregated` on members | ACT-03c |
| [`AdrNamesetSystem.cs`](../../../BelzontAdr/Systems/AdrNamesetSystem.cs) | Seeds `ADRRandomizationData` on new vanilla `Aggregate` | Also seed new **ADR** aggregates | ACT-03c |

---

## B. Highway / Aggregate host — remove — **ACT-01**

| File | Current role | Required change | Task |
|------|--------------|-----------------|--------|
| [`ADRHighwayAggregationData.cs`](../../../BelzontAdr/Components/ADRHighwayAggregationData.cs) | `Data`, `DirtyHwId`, `CacheData` on vanilla Aggregate | **Remove** unused fields from the structs — prefer **delete** the three types (`ADRHighwayAggregationData.highwayDataId`; empty `DirtyHwId`; `CacheData.startDistanceOverrideKm` / `reverseCounting`). Do not leave those values as dead serialized members. | ACT-01 |
| [`AdrHighwayRoutesSystem.cs`](../../../BelzontAdr/Systems/AdrHighwayRoutesSystem.cs) | Aggregate filler / replicate / cache jobs; marker↔Aggregate sync | Remove Aggregate-touching jobs (`NewAggregateFiller`, `ReplicateAggregateSetting`, `CalculateHighwayCacheData`, `AggregationCacheEraser`, InfoPanel Aggregate writes). **Keep** `highwaysDataRegistry` CRUD + serialize | ACT-01 |
| [`AdrHighwayRoutes2BSystem.cs`](../../../BelzontAdr/Systems/AdrHighwayRoutes2BSystem.cs) | Dirties Aggregate highway cache on Aggregated C/U/D | Remove or gut | ACT-01 |
| [`AdrNameSystemOverrides.cs`](../../../BelzontAdr/Overrides/AdrNameSystemOverrides.cs) | Highway name branch via Aggregate Data | Drop Aggregate→highway name branch until later tool epic | ACT-01 |
| [`BuildingUtilsOverrides.cs`](../../../BelzontAdr/Overrides/BuildingUtilsOverrides.cs) | Mileage via Aggregate Cache + markers | Remove Aggregate highway cache / marker mileage from numbering | ACT-01 |
| [`AdrRoadAddressNumbering.cs`](../../../BelzontAdr/Utils/AdrRoadAddressNumbering.cs) | Shared numbering + highway helpers | Same strip | ACT-01 |
| [`AdrRegionsSystem.cs`](../../../BelzontAdr/Systems/AdrRegionsSystem.cs) | Exposes Aggregate `highwayId` | Stop reading removed component (default/empty until later) | ACT-01 |
| Road Grouping overlay / controller | Uses highway Data/Cache for orientation/numbering | Use ADR/vanilla walk **without** highway overrides | ACT-01 |
| [`AddressesCs2Mod.cs`](../../../BelzontAdr/AddressesCs2Mod.cs) | Registers 2B highway system | Drop registration if system emptied | ACT-01 |
| Region map VUIO ([`RegionEditor.tsx`](../../../_Frontends/UI/k45_adr_vuio/src/settings/tabs/region/RegionEditor.tsx) `highwayId`, related DTOs/bindings) | Map paths classed by Aggregate `highwayDataId` via `AdrRegionsSystem` | **Remove or empty** frontend fields/bindings that only exist for Aggregate-hosted highway Data/Cache when backend stops providing them. Keep registry hover UI that keys off registry ids. | ACT-01 |

Save migration: on load, **ignore** previously saved Aggregate highway component data (drop/skip leftover components). **Do not** migrate/convert old `highwayDataId` or cache values. Beta OK.

**Keep:** settings [`HighwayRegisterManagement.tsx`](../../../_Frontends/UI/k45_adr_vuio/src/settings/tabs/region/subpages/HighwayRegisterManagement.tsx) + registry APIs (orphaned from Aggregate until reattached via Addresses SIP later — [20](20-highway-reattach-on-adr-aggregates.md)). Marker route/mileage tool/SIP UI strip is **ACT-02**, not ACT-01.

---

## C. Road markers — visual only — **ACT-02**

| File | Current role | Required change | Task |
|------|--------------|-----------------|--------|
| [`ADRHighwayMarkerData.cs`](../../../BelzontAdr/Components/ADRHighwayMarkerData.cs) | Route, display, mileage, pylons | Strip/stop serializing: `routeDataIndex`, `routeDirection`, `displayInformation`, custom params, `overrideMileage` / `newMileage` / `reverseMileageCounting`. **Keep pylon/visual fields**. Bump version; discard highway fields on migrate | ACT-02 |
| `AdrHighwayRoutesSystem` marker dirty pipeline | Copies tool highway defaults; may write Aggregate | Only apply remaining visual fields; no Aggregate coupling | ACT-02 |
| Prefab `ADRRoadMarkerObject` / `K45_ADR_HighwayMarker` | Marker prop | Keep as visual prop | ACT-02 |
| [`AddressesToolOptions.tsx`](../../../_Frontends/UI/k45_adr_vuio/src/tool/AddressesToolOptions.tsx) | Route/mileage tool UI | Remove route/mileage sections; keep visual/pylon if relevant | ACT-02 |
| [`RoadMarkSettings.tsx`](../../../_Frontends/UI/k45_adr_vuio/src/components/RoadMarkSettings.tsx) | SIP marker settings | Same strip | ACT-02 |
| [`AdrHighwayRoutesSystem.ts`](../../../_Frontends/UI/k45_adr_vuio/src/service/AdrHighwayRoutesSystem.ts) | Tool_/InfoPanel_ bindings | Drop highway/mileage bindings that no longer exist | ACT-02 |
| `K45_ADR_HighwayMarker.wedefault.xml` (+ related WE) | Formulae on highway fields | **Keep** formula XML tags. Replace live formulae with **`[Feature Name]`** literals (e.g. `[Route ID]`, `[Mileage]`). **Add XML comments** documenting old formulae for future Aggregation reimpl | ACT-02 |
| [`RoadMarkerInfoBridge.cs`](../../../BelzontAdr/Bridge/RoadMarkerInfoBridge.cs) | Route name resolution | Stop resolving route names from markers | ACT-02 |
| [`TempRoadMarkerTooltip.cs`](../../../BelzontAdr/Tooltips/TempRoadMarkerTooltip.cs) | Mileage-driven address hacks | Remove mileage/route tooltip coupling | ACT-02 |

---

## D. Naming + addressing — prefer ADR — **ACT-04**

| File | Current role | Required change | Task |
|------|--------------|-----------------|--------|
| [`AdrNameSystemOverrides.cs`](../../../BelzontAdr/Overrides/AdrNameSystemOverrides.cs) | Names vanilla Aggregate | If `ADRNameAggregated` → name ADR entity; else vanilla. `FindReferenceRoad` for ADR buffer | ACT-04 |
| [`BuildingUtilsOverrides.cs`](../../../BelzontAdr/Overrides/BuildingUtilsOverrides.cs) | Walks vanilla `AggregateElement` | Walk `ADRNameAggregateElement` when linked; `road` = ADR entity; no highway cache | ACT-04 |
| [`AdrRoadAddressNumbering.cs`](../../../BelzontAdr/Utils/AdrRoadAddressNumbering.cs) | Road Grouping card ranges | Same membership resolution | ACT-04 |
| [`AdrSelectionInfoPanelSystem.cs`](../../../BelzontAdr/Systems/AdrSelectionInfoPanelSystem.cs) | RoadAggregation / seed on vanilla Aggregate | Target ADR entity when edge has ADR link | ACT-04 |
| [`SeedManagementOptions.tsx`](../../../_Frontends/UI/k45_adr_vuio/src/components/SeedManagementOptions.tsx) / [`AddressesInfoOptionsComponent.tsx`](../../../_Frontends/UI/k45_adr_vuio/src/components/AddressesInfoOptionsComponent.tsx) | Seed UI | Works if backend returns ADR as naming entity | ACT-04 |
| Station / road-options lists in name overrides | Vanilla Aggregate walk | Prefer ADR when present | ACT-04 |

---

## E. Road Grouping — ADR link + real mutations — **ACT-05** (maintenance **ACT-06**)

| File | Current role | Required change | Task |
|------|--------------|-----------------|--------|
| [`AdrRoadGroupingTool.cs`](../../../BelzontAdr/Tools/AdrRoadGroupingTool.cs) | Vanilla Aggregate membership; stub logs | Discover via vanilla `Aggregated`. On first would-add/remove apply: **`ForkFromVanilla`** → `SelectedAggregation` = ADR. Membership from ADR buffer. Toggle/drag **write** (retire stubs). Fixed pin = session (or later flag) | ACT-05 |
| [`AdrRoadGroupingController.cs`](../../../BelzontAdr/Controllers/AdrRoadGroupingController.cs) | Cards from vanilla AggregateElement | Cards from ADR elements; invalidate on real membership change | ACT-05 |
| [`AdrRoadGroupingOverlayRenderSystem.cs`](../../../BelzontAdr/Systems/AdrRoadGroupingOverlayRenderSystem.cs) | Vanilla Aggregate ribbons | Belonging = ADR members; remainder = vanilla Aggregate edges not in ADR; empty-selection pick-preview unchanged | ACT-05 |
| Road Grouping VUIO | Entity-opaque cards/options | Verify Index/Version after fork | ACT-05 |
| Membership maintenance system (new) | — | Remap + gap-fill per [07](07-membership-maintenance-edge-cases.md); dogfood via tool | ACT-06 |

Vanilla Aggregate **membership** is never mutated by the tool — only seed redraw on fork + ADR overlay membership.

---

## Sprint tag summary

Workstream order inside the **single** activation sprint ([13](13-activation-sprint-roadmap.md)):

| Order | Sections | Kanban task |
|-------|----------|-------------|
| 1 | B | ACT-01 |
| 2 | C | ACT-02 |
| 3 | A1 | ACT-03a |
| 4 | A2 | ACT-03b |
| 5 | A3 | ACT-03c |
| 6 | D | ACT-04 |
| 7 | E (tool mutations) | ACT-05 |
| 8 | E (maintenance / gap-fill) | ACT-06 |

Full sprint + Kanban catalog: [13](13-activation-sprint-roadmap.md).

## Relation

- [04](04-parallel-system-design-notes.md) — component design
- [07](07-membership-maintenance-edge-cases.md) — ACT-06 rules
- [09](09-road-grouping-tool-spec.md) / [11](11-road-grouping-tool-roadmap.md) — tool; T0–T3 shipped
- [03](03-adr-current-touchpoints.md) — pre-activation touchpoints (historical + planned strip)
