# Game.Net edge kinds and aggregation

*Added: 2026-09-13 · Game.dll build GUID `d124bec16c0748da9ebf48366d9c1d68` · Design follow-up: [23](23-adr-net-kind-gate-labels-seeds.md) · Roadmap: [24](24-adr-net-kind-roadmap.md) · Labels: [14](14-vanilla-road-label-pipeline.md)–[15](15-adr-citywide-name-label-takeover.md)*

## Purpose

Document how Cities Skylines II classifies aggregatable nets (roads, tracks, waterways, pathways, …), how vanilla aggregation decides who may join, why **`Game.Net.Road` is an unreliable road detector**, and why non-road AggregateNets often lack **`NetNameData`** for world name labels.

## Playtest fact: streets/avenues without `Game.Net.Road`

**Observed in play:** real street/avenue edges can have **no** `Game.Net.Road` component at runtime.

Therefore:

- “Has `Game.Net.Road`” ≠ “is a vehicle road”
- “Missing `Game.Net.Road`” ≠ “non-road”
- ADR must **not** gate naming, seeds, membership, or SIP on `HasComponent<Road>(edge)`

Aggregate **host** entities never carry `Road` either — only edges/nodes (when present). Easy to mis-read Aggregate selection as “no Road.”

## Prefab authoring vs runtime

### `RoadPrefab` declares `Road` + `RoadData`

`Game.Prefabs.RoadPrefab`:

- Always adds prefab component **`RoadData`**
- Declares edge/node archetype **`Game.Net.Road`** when the archetype includes `Edge` / `Node` — **independent of** `m_HighwayRules`
- `m_HighwayRules` only changes extras (e.g. `ConnectedBuilding` when `!m_HighwayRules` and no zone block)

Prefab **intent** is `RoadPrefab ⇒ Road + RoadData`. **Runtime diverges** (playtest). Known Managed remove path: `ApplyNetSystem.UpdateComponent(Road)` strips `Road` when the temp entity lacks it (replace/upgrade sync). Full root-cause inventory of every no-`Road` street is optional follow-up; it does not restore tag gating.

### Highway is a prefab flag, not an edge tag

`NetInitializeSystem` when initializing `RoadData`:

| Prefab field | Effect |
|--------------|--------|
| `RoadType.Normal` | `NetData.m_RequiredLayers \|= Layer.Road` |
| `RoadType.PublicTransport` | `Layer.PublicTransportRoad` (still `RoadPrefab` + `RoadData`) |
| `m_HighwayRules` | `RoadData.m_Flags \|= RoadFlags.UseHighwayRules` (`0x10`) |

Street / avenue / highway / bus-road share the **same vehicle-road prefab family**. Highway ≠ “has `Game.Net.Road`”.

`Game.Net.Road` is a **runtime traffic-flow** component (duration/distance fields), not an empty marker like `TrainTrack`.

## Prefab-first kind inventory

Primary signal: **`PrefabRef` → prefab `*Data`**. Edge tags are secondary / corroborating.

| Kind | Prefab discriminator | Edge tag (`Game.Net`) | Notes |
|------|----------------------|----------------------|-------|
| Road (vehicle) | **`RoadData`** | `Road` **optional / unreliable** | Includes highway (`UseHighwayRules`) and `RoadType.PublicTransport` |
| Pathway | **`PathwayData`** | **None** | Tags alone cannot see pathways |
| Train | **`TrackData`** + `TrackTypes.Train` | `TrainTrack` | Tags also toggled by `LaneReferencesSystem` from lane composition |
| Tram | **`TrackData`** + `TrackTypes.Tram` | `TramTrack` | Same dynamic tag path |
| Subway | **`TrackData`** + `TrackTypes.Subway` | `SubwayTrack` | |
| Waterway | **`WaterwayData`** | `Waterway` | |
| Taxiway | **`TaxiwayData`** | `Taxiway` | |
| Out of scope | Fence / Pipeline / PowerLine prefabs | — | Not ADR grouping / label targets unless later proven |

`TrackTypes`: `Train=1`, `Tram=2`, `Subway=4`. Prefab `Train|Tram` initially adds **no** track tag in `TrackPrefab.AddTrackType`; lanes may later add **both** `TrainTrack` and `TramTrack`.

`NetData.m_RequiredLayers` (`Layer.Road`, `Layer.TrainTrack`, …) matches tool raycast masks; useful corroboration, not the ADR classifier.

### Prefab class → archetype tags (Managed)

| Prefab class | Prefab data | Edge/node tag when declared |
|--------------|-------------|-----------------------------|
| `RoadPrefab` | `RoadData` | `Road` (declared; may be absent at runtime) |
| `PathwayPrefab` | `PathwayData` | *(none)* |
| `TrackPrefab` | `TrackData` | `TrainTrack` / `TramTrack` / `SubwayTrack` by `m_TrackType` |
| `WaterwayPrefab` | `WaterwayData` | `Waterway` |
| `TaxiwayPrefab` | `TaxiwayData` | `Taxiway` |

## Vanilla aggregation mix rule

`Game.Net.AggregateSystem` joins edges when they share the same **`NetGeometryData.m_AggregateType`** (entity of an `AggregateNetPrefab`), via `GetAggregateType(edge)` → `m_PrefabGeometryData[prefabRef].m_AggregateType`.

- Mix gate is **AggregateNet prefab identity**, not edge tags.
- Different road assets may use **different** AggregateNet prefabs → vanilla keeps separate Aggregates even though both are vehicle roads.
- Nets without `m_AggregateType` are not vanilla-aggregated.

ADR’s parallel membership today (`AdrNameAggregateMembershipSystem.AddMemberCore`) has **no** kind or AggregateType check — Road Grouping can mix kinds vanilla would never put together.

## NetNameData / NetLabel gap

World name labels need **`NetNameData` on the AggregateNet prefab**:

- `Game.Prefabs.NetLabel` (on `AggregateNetPrefab`) → `GetPrefabComponents` adds `NetNameData`; `Initialize` writes colors.
- Aggregate instances get `LabelMaterial` / `LabelExtents` / `LabelPosition` / `LabelVertex` from `NetLabel.GetArchetypeComponents`.

Road AggregateNets usually ship with `NetLabel`. Many non-road AggregateNets do **not** → no `NetNameData` → ADR S2 label ensure that only copies vanilla Aggregate `PrefabRef` cannot resolve materials/colors.

ADR today: `AdrRoadNameLabelMeshSystem.EnsureRoadNameLabelComponents` attaches Label* buffers and copies `PrefabRef` from a vanilla Aggregate source. If that AggregateNet lacks `NetNameData`, labels break for that kind.

## Existing ADR checks that are inadequate

These treat edge `Game.Net.Road` as authoritative and will miss playtest streets/avenues without the component:

| Location | Pattern |
|----------|---------|
| `AdrSelectionInfoPanelSystem` | `HasComponent<Road>(e)` / skip members without `Road` |
| `AdrNameSystemOverrides` (address walk) | `!HasComponent<Road>(nextItem)` continue |
| Task **0085** early draft notes | Suggested gate on `HasComponent<Road>(refRoad)` — **superseded** by prefab `RoadData` ([23](23-adr-net-kind-gate-labels-seeds.md) / [24](24-adr-net-kind-roadmap.md)) |

`GetAggregateName` already prefers prefab `RoadData`, but falls back to faking `TrackData` into `RoadData` — applying **road** namesets/prefixes to non-roads.

## Implications for ADR (summary)

| Concern | Implication |
|---------|-------------|
| Kind classification | Prefab `*Data` first; never sole reliance on edge `Road` |
| Street ↔ highway | Same vehicle-road family; may mix for naming |
| Cross-kind mix | Forbid (road vs track vs pathway vs waterway vs taxiway) |
| Seeds / road namesets | Only for prefab-`RoadData` aggregates |
| Labels | Per-kind `NetNameData` (ADR-owned AggregateNet-like prefabs) when vanilla AggregateNet lacks `NetLabel` |

Design: [23](23-adr-net-kind-gate-labels-seeds.md). Implementation catalog: [24](24-adr-net-kind-roadmap.md).
