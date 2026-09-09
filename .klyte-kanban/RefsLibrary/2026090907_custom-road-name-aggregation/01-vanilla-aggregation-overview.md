# Vanilla Aggregation — overview

*Source: Game.dll decompile via kcs2m · build GUID `b5fd0b9582554f3c83207d8d5d7a19a0`*

## Purpose

Vanilla Aggregation groups contiguous net **edges** that share the same **aggregate type** into a single logical “road” entity. That entity is the unit for:

- World-space **road name labels** and one-way **arrows**
- **House-number / address** identity (`BuildingUtils.GetAddress` → aggregate as `road`)
- Selection-info **Road** panel stats (length, flow, wear) when an aggregate is selected

It does **not** redefine the traffic graph. Pathfinding and vehicle AI use edges/nodes/lanes; aggregates are a naming and presentation overlay maintained by net tooling.

## Core types

| Type | Namespace | Role |
|------|-----------|------|
| `Aggregate` | `Game.Net` | Empty tag on the aggregate **entity** (`IEmptySerializable`, size 1) |
| `Aggregated` | `Game.Net` | On each **edge**: `Entity m_Aggregate` → owning aggregate (or null/default when detached) |
| `AggregateElement` | `Game.Net` | Buffer on the aggregate entity: ordered `Entity m_Edge` list (`ISerializable`) |
| `AggregateNetPrefab` | `Game.Prefabs` | Prefab that defines aggregate archetype (`Aggregate` + `AggregateElement` + Created/Updated) |
| `AggregateNetData` | `Game.Prefabs` | On prefab entity: `EntityArchetype m_Archetype` used when creating aggregates |
| `NetGeometryData.m_AggregateType` | `Game.Prefabs` | On edge net geometry prefab: which `AggregateNetPrefab` applies (null ⇒ no aggregation) |

`AggregateNetPrefab.LateInitialize` builds the instance archetype with `Aggregate`, buffers from components (including `AggregateElement`), plus `Created` / `Updated`.

## Systems (lifecycle)

### `Game.Net.AggregateSystem`

Runs on edges with `Aggregated` that are `Created` / `Updated` / `Deleted` (`ModificationBarrier2B`).

High-level job (`UpdateAgregatesJob`):

1. Collect aggregates touched by modified edges (and temp-edge special cases).
2. For each touched aggregate: **`ValidateAggregate`** — clear element buffer, keep edges that still match aggregate prefab type, detach mismatched edges (`Aggregated = default`), delete empty aggregates, rebuild ordered element chain from a seed edge via `AddElements` / `GetBestConnectionEdge`, mark `Updated`.
3. **`CombineAggregate`** / **`TryCombine`** — if adjacent aggregate ends meet `ShouldCombine`, merge buffers and retarget `Aggregated.m_Aggregate`; delete emptied aggregates.
4. For edges with empty/null aggregate: **`CreateAggregate`** — walk connections with matching `GetAggregateType`, create entity from `AggregateNetData.m_Archetype`, fill `AggregateElement`, set each edge’s `Aggregated`.

#### Join / walk rules (important)

- **Same aggregate type only:** `GetAggregateType(edge)` = `NetGeometryData.m_AggregateType` of the edge’s `PrefabRef`. Edges with different types never share an aggregate.
- **Best connection at a node:** among connected edges of the same prefab type, pick the geometrically “best” continuation (tangent/chord scoring in `GetBestConnectionEdge`). Mutual best-connection is required before chaining (`AddElements` while-loop).
- **Combine only at ends:** `ShouldCombine` requires the neighboring edge to be the **first or last** element of the other aggregate (and matching Temp-ness).
- **Aggressive rebuild:** any edge create/update/delete that touches an aggregate can clear and re-walk membership. Custom membership on vanilla components is unstable under normal net edits.

### Tooling companions

| System | Role |
|--------|------|
| `GenerateEdgesSystem` | When creating/editing edges with `m_AggregateType != Null`, attaches/propagates `Aggregated` |
| `GenerateAggregatesSystem` | Materializes temp aggregate definitions (`CreationDefinition` + `AggregateElement`) |
| `ApplyNetSystem` | Apply-time handling of temp aggregates on edges |
| `DefaultToolSystem` | Copies `AggregateElement` buffers when duplicating selection temps |
| `ToolClearSystem` | Clears temp aggregate-related buffers |
| `Game.Serialization.AggregatedSystem` | After load: rewrites each edge’s `Aggregated.m_Aggregate` from buffer ownership |

## Rendering

| System | Role |
|--------|------|
| `AggregateMeshSystem` | Builds label positions / mesh data along `AggregateElement` chains; also arrow positions for one-way indication |
| `AggregateRenderSystem` | Draws name meshes and arrow meshes produced by the mesh system |

Label **geometry** follows the ordered edge chain (splits at complex nodes / breaks). The **name string** for an aggregate comes from the UI naming path (`NameSystem` / custom names), not from simulation. ADR already refreshes labels by marking `LabelMaterial` entities `Updated` rather than calling `AggregateMeshSystem.OnDictionaryChanged` via reflection ([`AdrMainSystem.ResetRoadsCache`](../../../BelzontAdr/Systems/AdrMainSystem.cs)).

## Address numbering (vanilla)

`BuildingUtils.GetAddress(entityManager, building, edge, curvePos, out road, out number)`:

1. Requires `Aggregated` on the building’s road edge and a live `AggregateElement` buffer on `m_Aggregate`.
2. Walks the buffer in order, accumulating curve length (+ width/angle and roundabout adjustments).
3. Sets `road = component.m_Aggregate` (the **aggregate entity**, not the raw edge).
4. Sets `number` from distance along the chain and side of the road.

`NameSystem.GetSpawnableBuildingName` / transport stop naming then resolve the **road name** from that aggregate entity.

## Mental model

```text
NetGeometryPrefab.m_AggregateType ──► AggregateNetPrefab
                                            │
Edge ──Aggregated.m_Aggregate───────────────┼──► Aggregate entity
                                            │         │
                                            │         └── AggregateElement[] = ordered edges
                                            │
                                            ├── AggregateMeshSystem (labels / arrows)
                                            └── BuildingUtils.GetAddress (road + house #)
```

## Implication for custom association

Vanilla will **always** keep rewriting `Aggregated` / `AggregateElement` for nets that have an aggregate type. A custom “force these edges to share one name” feature must **not** fight that rewrite. Use **parallel ADR components** for membership and names, and patch **consumers** that matter for naming (`NameSystem`, `GetAddress`, optionally label refresh) — see [04-parallel-system-design-notes.md](04-parallel-system-design-notes.md).
