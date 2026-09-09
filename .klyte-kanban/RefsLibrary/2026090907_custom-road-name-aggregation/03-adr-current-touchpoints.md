# ADR current Aggregation touchpoints

*In-repo C# under `BelzontAdr/` · 2026-09-09*

ADR does **not** create, merge, or override vanilla `Aggregate` / `Aggregated` / `AggregateElement`. It **attaches data** to vanilla aggregate entities and patches **naming / addressing** consumers.

## Component map (today)

| ADR type | Lives on | Purpose |
|----------|----------|---------|
| `ADRHighwayAggregationData` | Vanilla `Aggregate` entity | `highwayDataId` (Hash128) linking to highway registry |
| `ADRHighwayAggregationDataDirtyHwId` | Same | Dirty flag to replicate marker / clear cache |
| `ADRHighwayAggregationCacheData` | Same | Mileage: `startDistanceOverrideKm`, `reverseCounting` |
| `ADRRandomizationData` | New `Aggregate` (among other entity types) | Seed for nameset-based road names |
| `ADREntityStationRef` / manual building refs | Aggregate / building | Station naming locked to a road aggregate |
| `ADREdgeData.fixedAggregation` | Intended for edges | **Declared + serialized; never read/written elsewhere** |

## Systems / overrides

### Highway pipeline

- [`AdrHighwayRoutesSystem`](../../../BelzontAdr/Systems/AdrHighwayRoutesSystem.cs) — fill `ADRHighwayAggregationData` on aggregates missing it (scan markers on `AggregateElement` edges); replicate highway id to markers; compute `ADRHighwayAggregationCacheData`; sync marker tool writes onto `Aggregated.m_Aggregate`.
- [`AdrHighwayRoutes2BSystem`](../../../BelzontAdr/Systems/AdrHighwayRoutes2BSystem.cs) — on non-temp edge `Aggregated` Created/Updated/Deleted → drop cache + dirty highway id on the vanilla aggregate.

### Naming

- [`AdrNamesetSystem`](../../../BelzontAdr/Systems/AdrNamesetSystem.cs) — seeds `ADRRandomizationData` on new `Aggregate` entities.
- [`AdrNameSystemOverrides`](../../../BelzontAdr/Overrides/AdrNameSystemOverrides.cs) — Harmony on `NameSystem`:
  - `GetAggregateName`: custom name → vanilla; else highway name if `ADRHighwayAggregationData` set; else nameset + road-prefix from `FindReferenceRoad` on `AggregateElement` buffer.
  - Building / stop addresses use `BuildingUtils.GetAddress` then format with that road name.
- [`BuildingUtilsOverrides`](../../../BelzontAdr/Overrides/BuildingUtilsOverrides.cs) — replaces numbering using aggregate buffer + highway cache when applicable.

### Selection / region / refresh

- [`AdrSelectionInfoPanelSystem`](../../../BelzontAdr/Systems/AdrSelectionInfoPanelSystem.cs) — edge with `Aggregated` or entity with `Aggregate` → `AdrEntityType.RoadAggregation`; nameset UI via reference road.
- [`AdrRegionsSystem`](../../../BelzontAdr/Systems/AdrRegionsSystem.cs) — groups edges by `Aggregated.m_Aggregate` for region map; exposes `highwayId` from ADR highway data.
- [`AdrMainSystem`](../../../BelzontAdr/Systems/AdrMainSystem.cs) — `FindReferenceRoad` (pick edge closer to origin by `EdgeGeometry` bounds); `ResetRoadsCache` marks `LabelMaterial` `Updated` (trace log still mentions abandoned `AggregateMeshSystem.OnDictionaryChanged` reflection).

## `ADREdgeData.fixedAggregation`

```csharp
// BelzontAdr/Components/ADREdgeData.cs
public Entity fixedAggregation;
```

- Serialized with edge data version 0.
- **No** `AddComponent` / `SetComponent` / reads of `ADREdgeData` found elsewhere in the repo.
- Treat as **abandoned stub** for “pin edge to fixed aggregate.” Parallel design should **replace** this with ADR-owned link components rather than reviving the field as-is (see [04](04-parallel-system-design-notes.md)).

## What ADR does **not** do today

- No Harmony on `AggregateSystem`, `AggregateMeshSystem`, or `GenerateAggregatesSystem`.
- No UI to merge/split vanilla aggregates or set `fixedAggregation`.
- No separate ADR aggregate entity type — highway and street identity are always the vanilla aggregate entity.

## Implications for the parallel feature

| Current behavior | Parallel-system consequence |
|------------------|----------------------------|
| Highway data on vanilla Aggregate | **Confirmed:** migrate to ADR-owned aggregates |
| Namesets / seeds on Aggregate | Move or dual-key: ADR aggregate becomes naming identity when present |
| `GetAggregateName` / GetAddress patches assume vanilla Aggregate entity | Must resolve **ADR aggregate first**, then fall back to vanilla |
| Region / SIP group by `Aggregated.m_Aggregate` | Need ADR membership lookup for custom associations; vanilla grouping remains for default roads |
| `fixedAggregation` dead | Do not wire; delete or leave inert until cutover |
