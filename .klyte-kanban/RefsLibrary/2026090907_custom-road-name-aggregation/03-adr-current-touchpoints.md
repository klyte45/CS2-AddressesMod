# ADR current Aggregation touchpoints

*In-repo C# under `BelzontAdr/` · 2026-09-09 · Activation notes: 2026-09-10*

ADR does **not** create, merge, or override vanilla `Aggregate` / `Aggregated` / `AggregateElement`. Today it **attaches data** to vanilla aggregate entities and patches **naming / addressing** consumers. **Activation** ([12](12-activation-change-map.md)) will introduce ADR-owned Aggregate/Element/Aggregated and **strip** Aggregate-hosted highway state.

## Component map (today)

| ADR type | Lives on | Purpose | Activation fate |
|----------|----------|---------|-----------------|
| `ADRHighwayAggregationData` | Vanilla `Aggregate` entity | `highwayDataId` (Hash128) linking to highway registry | **Remove** (ACT-01) |
| `ADRHighwayAggregationDataDirtyHwId` | Same | Dirty flag to replicate marker / clear cache | **Remove** (ACT-01) |
| `ADRHighwayAggregationCacheData` | Same | Mileage: `startDistanceOverrideKm`, `reverseCounting` | **Remove** (ACT-01) |
| `ADRRandomizationData` | New `Aggregate` (among other entity types) | Seed for nameset-based road names | Keep on vanilla; **also** on ADR aggregates after fork (ACT-03c) |
| `ADREntityStationRef` / manual building refs | Aggregate / building | Station naming locked to a road aggregate | Prefer ADR when linked (ACT-04) |
| `ADREdgeData.fixedAggregation` | Intended for edges | **Declared + serialized; never read/written elsewhere** | Do not revive; use `ADRNameAggregated` |
| Road Grouping T0–T3 | Tool / overlay / cards | Vanilla Aggregate membership + stub logs | **Done**; wire to ADR in ACT-05 |

**Planned (not in repo yet):** `ADRNameAggregate`, `ADRNameAggregateElement`, `ADRNameAggregated` — see [04](04-parallel-system-design-notes.md) / [12](12-activation-change-map.md) §A.

## Systems / overrides

### Highway pipeline (to strip — ACT-01)

- [`AdrHighwayRoutesSystem`](../../../BelzontAdr/Systems/AdrHighwayRoutesSystem.cs) — fill `ADRHighwayAggregationData` on aggregates missing it (scan markers on `AggregateElement` edges); replicate highway id to markers; compute `ADRHighwayAggregationCacheData`; sync marker tool writes onto `Aggregated.m_Aggregate`. **Activation:** remove Aggregate jobs; **keep** highway registry CRUD.
- [`AdrHighwayRoutes2BSystem`](../../../BelzontAdr/Systems/AdrHighwayRoutes2BSystem.cs) — on non-temp edge `Aggregated` Created/Updated/Deleted → drop cache + dirty highway id on the vanilla aggregate. **Activation:** gut/remove.

### Markers (visual-only — ACT-02)

- `ADRHighwayMarkerData` currently holds route, mileage, pylons. **Activation:** strip route/mileage; keep pylons; WE formulae → `[Feature]` placeholders + comments.

### Naming

- [`AdrNamesetSystem`](../../../BelzontAdr/Systems/AdrNamesetSystem.cs) — seeds `ADRRandomizationData` on new `Aggregate` entities. **Activation:** also seed ADR aggregates (ACT-03c).
- [`AdrNameSystemOverrides`](../../../BelzontAdr/Overrides/AdrNameSystemOverrides.cs) — Harmony on `NameSystem`:
  - `GetAggregateName`: custom name → vanilla; else highway name if `ADRHighwayAggregationData` set; else nameset + road-prefix from `FindReferenceRoad` on `AggregateElement` buffer.
  - Building / stop addresses use `BuildingUtils.GetAddress` then format with that road name.
  - **Activation (ACT-04):** prefer ADR link; drop Aggregate highway name branch.
- [`BuildingUtilsOverrides`](../../../BelzontAdr/Overrides/BuildingUtilsOverrides.cs) — replaces numbering using aggregate buffer + highway cache when applicable. **Activation:** ADR buffer; no highway cache.

### Selection / region / refresh

- [`AdrSelectionInfoPanelSystem`](../../../BelzontAdr/Systems/AdrSelectionInfoPanelSystem.cs) — edge with `Aggregated` or entity with `Aggregate` → `AdrEntityType.RoadAggregation`; nameset UI via reference road. **Activation:** ADR entity when linked (ACT-04).
- [`AdrRegionsSystem`](../../../BelzontAdr/Systems/AdrRegionsSystem.cs) — groups edges by `Aggregated.m_Aggregate` for region map; exposes `highwayId` from ADR highway data. **Activation:** stop Aggregate highwayId until later tool epic.
- [`AdrMainSystem`](../../../BelzontAdr/Systems/AdrMainSystem.cs) — `FindReferenceRoad` (pick edge closer to origin by `EdgeGeometry` bounds); `ResetRoadsCache` marks `LabelMaterial` `Updated`.

### Road Grouping (shipped T0–T3)

- [`AdrRoadGroupingTool`](../../../BelzontAdr/Tools/AdrRoadGroupingTool.cs) / Controller / Overlay — vanilla Aggregate membership, stub mutations. **Activation ACT-05:** fork + real ADR writes.

## `ADREdgeData.fixedAggregation`

```csharp
// BelzontAdr/Components/ADREdgeData.cs
public Entity fixedAggregation;
```

- Serialized with edge data version 0.
- **No** `AddComponent` / `SetComponent` / reads of `ADREdgeData` found elsewhere in the repo.
- Treat as **abandoned stub**. Parallel design uses **`ADRNameAggregated`** (see [04](04-parallel-system-design-notes.md)).

## What ADR does **not** do today

- No Harmony on `AggregateSystem`, `AggregateMeshSystem`, or `GenerateAggregatesSystem`.
- No UI to merge/split vanilla aggregates or set `fixedAggregation`.
- No separate ADR aggregate entity type yet — highway and street identity are always the vanilla aggregate entity (**until ACT-03a…ACT-03c**).

## Implications for activation ([12](12-activation-change-map.md))

| Current behavior | Activation consequence |
|------------------|------------------------|
| Highway data on vanilla Aggregate | **Remove** Aggregate host (ACT-01); reattach via Addresses SIP later ([20](20-highway-reattach-on-adr-aggregates.md)) |
| Markers with route/mileage | **Visual-only** (ACT-02); WE placeholders |
| Namesets / seeds on Aggregate | Fork copies seed to ADR; vanilla `Redraw` (ACT-03c); types in ACT-03a; CRUD in ACT-03b |
| `GetAggregateName` / GetAddress assume vanilla Aggregate | Prefer ADR when linked (ACT-04); split naming after fork |
| Road Grouping on vanilla Aggregate | Fork + real mutations (ACT-05); maintenance (ACT-06) |
| `fixedAggregation` dead | Do not wire; use `ADRNameAggregated` |
