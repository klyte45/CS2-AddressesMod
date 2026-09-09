# Vanilla Aggregation — consumer audit

*Source: Game.dll regex search + context via kcs2m · build GUID `b5fd0b9582554f3c83207d8d5d7a19a0`*

## Verdict (short)

**Aggregation is not a traffic-simulation topology.** No pathfinding, vehicle AI, citizen AI, or economy system uses `Aggregate` / `Aggregated` / `AggregateElement` to decide movement or production.

It **is more than pure “floating labels”**: it is the game’s **road identity** for naming, addressing, SIP road selection stats, label/arrow rendering, net-tool temp entities, serialization repair, and one achievement. Changing which edges share an aggregate **does** change house numbers and street names, and SIP “road” selection targets the aggregate entity.

For ADR’s feature goal (“custom name association without breaking sim”), that means:

- Safe to leave vanilla Aggregation running for default nets.
- Unsafe to ignore address/name consumers when introducing a parallel association — labels and addresses must stay consistent ([00-README.md](00-README.md) confirmed decision: **name + house numbers** follow ADR aggregates).

## Consumer table

| Area | File / system | Uses | Impact if membership changes |
|------|---------------|------|------------------------------|
| **Net maintenance** | `Game.Net.AggregateSystem` | Creates/validates/combines/splits | Ownership rewrite (source of prior override pain) |
| **Serialization** | `Game.Serialization.AggregatedSystem` | Edge ← buffer ownership after load | Save/load consistency only |
| **Tools** | `GenerateEdgesSystem`, `GenerateAggregatesSystem`, `ApplyNetSystem`, `DefaultToolSystem`, `ToolClearSystem` | Temp/preview aggregates while editing nets | Tooling only |
| **Rendering** | `AggregateMeshSystem`, `AggregateRenderSystem` | Label + arrow meshes along element chain | Visual labels/arrows |
| **Addressing** | `BuildingUtils.GetAddress` | Walks element buffer; returns aggregate as `road` | **House numbers + address road entity** |
| **Naming** | `Game.UI.NameSystem` | Road name for addresses from aggregate | **Displayed street names in addresses** |
| **SIP / UI** | `RoadSection` | Visible when selected entity is Aggregate; walks `AggregateElement` and reads **edge** `Curve` / `Road` / `NetCondition` / prefab upkeep | SIP road panel — **grouping only**; stats are segment-level (ADR can re-sum over ADR elements) |
| **Tools / highlight** | `DefaultToolSystem` → `GenerateAggregatesSystem` | Copies `AggregateElement` onto `CreationDefinition`; adds `Highlighted` (+ `BatchesUpdated`) per `m_Edge` | Selection glow — same edge-list membership as SIP for ADR S1/S2 |
| **SIP / UI** | `TitleSection` | `HasComponent<Aggregate>` → title type `"RoadName"` | SIP title chrome |
| **SIP / UI** | `ActionsSection`, `TrafficRoutesSystem` | Aggregate selectable for traffic-routes actions | Tool UX |
| **Raycast** | `RaycastJobs` / `RaycastSystem` | `TypeMask.Labels` → resolve edge to aggregate | Selecting labels hits aggregate |
| **Tooltip** | `RaycastNameTooltipSystem` | (pattern includes Aggregate among nameable owners) | Hover names |
| **Icons** | `ImageSystem` | Prefab icon from first `AggregateElement` edge | UI thumbnail |
| **Infoview** | `TrafficInfoviewUISystem` | Query includes `Aggregated` on road edges; job reads `Road` flow only | Query filter convenience — **not** aggregate membership logic |
| **Achievements** | `AchievementTriggerSystem` | Measures aggregate curve length for an achievement | Meta / not sim |
| **Simulation (false positive)** | `CityProductionCapacityCalculationSystem.AggregateProductionCapacityJob` | Job name means “sum capacities” | **Unrelated** to `Game.Net.Aggregate` |

## Namespaces with **no** Aggregation consumers (sampled)

Searches for `Aggregate` / `Aggregated` / `AggregateElement` / `m_Aggregate` in Game.dll did **not** show real consumers under typical sim paths such as:

- Pathfinding / route finding
- Vehicle / public transport simulation systems (beyond UI tools)
- Citizen / household behavior
- Zoning / building placement simulation (buildings *use* aggregate only for **address display** via `GetAddress`)

Absence of hits is not a formal proof of every assembly forever, but within **Game.dll** the consumer set is clearly presentation + tooling + addressing + one achievement.

## Display / naming vs simulation — detailed verdict

| Concern | Affected by Aggregation? | Notes |
|---------|--------------------------|-------|
| Car/pedestrian pathfinding | **No** | Uses net graph (edges/nodes/lanes) |
| Traffic lights / lanes / composition | **No** | Edge-level components |
| Building placement / zoning | **No** (placement); address **text** yes | `BorderDistrict` etc. live on edges |
| Road name labels in world | **Yes** | Mesh/render systems |
| Building addresses (street + number) | **Yes** | `GetAddress` + `NameSystem` |
| SIP “road” selection & stats | **Yes** | Aggregate entity + element walk |
| Net tool preview/apply | **Yes** | Temp aggregates |
| Economy / production | **No** | False-positive job name only |

## What a parallel ADR system must still honor

Even if vanilla Aggregation stays untouched:

1. **Name resolution** for entities that today are vanilla aggregates (`NameSystem.GetName` / ADR `GetAggregateName`).
2. **`BuildingUtils.GetAddress`** (or ADR’s existing Harmony override) so house numbers follow the **ADR** ordered edge list when an edge is ADR-associated.
3. **Label refresh** so world labels match the name ADR assigns (ADR already bumps `LabelMaterial` `Updated`).
4. **SIP mapping** — ADR already maps edge/`Aggregated` → `RoadAggregation`; custom associations may need ADR selection to prefer ADR aggregate entities for naming/seed UI while vanilla SIP still sees vanilla aggregates for length/flow (acceptable split, document in design notes).

## Prior override failure (contextual)

Fighting `AggregateSystem` Validate/Combine means every road edit can undo custom `Aggregated` links. That is why this feature’s strategy is **parallel components**, not Harmony replacement of Aggregation itself.
