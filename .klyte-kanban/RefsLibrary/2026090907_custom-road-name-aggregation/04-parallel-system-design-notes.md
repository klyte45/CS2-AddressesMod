# Parallel ADR name-aggregation — design notes

*Design recommendations for a later implementation. Not product code.*

## Goals

- Let players define **custom road name associations** (which edges share one street name and one house-number sequence).
- Avoid fighting vanilla `AggregateSystem` Validate/Combine/Create.
- Keep **labels, SIP addresses, and building numbers** consistent when an edge is ADR-associated.
- **Migrate** highway aggregation data off vanilla `Aggregate` onto ADR-owned aggregates.

## Non-goals

- Harmony-replace or disable `Game.Net.AggregateSystem`.
- Mutate vanilla `Aggregated.m_Aggregate` / `AggregateElement` for custom membership.
- Change traffic simulation / pathfinding.

## Recommended component sketch (names illustrative)

Use ADR-prefixed types so they never collide with vanilla:

| Type | Kind | Role |
|------|------|------|
| `ADRNameAggregate` | Tag on ADR aggregate entity | Marks ADR-owned name-aggregate (parallel to vanilla `Aggregate`) |
| `ADRNameAggregateElement` | Buffer on ADR aggregate | Ordered membership entries (edge + per-member flags) — source of truth for name + numbering walk |
| `ADRNameAggregated` | Component on edge | Link to ADR aggregate when associated; absent ⇒ fall back to vanilla. Mirrors membership fields needed for edge queries |
| Existing | On ADR aggregate | `ADRRandomizationData`, custom name storage (via vanilla NameSystem custom-name APIs if applicable), **`ADRHighwayAggregationData` / Dirty / Cache** after migration |

Do **not** revive `ADREdgeData.fixedAggregation` as the link field; prefer an explicit `ADRNameAggregated` (or rename clearly) so edge geometry metadata stays separate from association.

### Membership fields (settled)

Per member (on **`ADRNameAggregateElement`** and mirrored on **`ADRNameAggregated`** where useful for queries). Pattern matches highway markers (`overrideMileage` + `newMileage`):

| Field | Type | Default | Role |
|-------|------|---------|------|
| `m_Edge` | `Entity` | — | Member edge |
| `preventPropagation` | `bool` | **`false`** | When **true**, this member **does not propagate** gap-fill: ADR may stay **non-contiguous** on a vanilla Aggregate; gap-fill must not treat this edge as an endpoint that absorbs interstitial neighbors across a hole |
| `numberOverrideActive` | `bool` | **`false`** | When **false**, ignore `numberOverride` (computed numbering). When **true**, use `numberOverride` like marker mileage override |
| `numberOverride` | number (`int` — house number) | `0` | House-number override; only applied if `numberOverrideActive` |

**Propagation:** Gap-fill only runs between consecutive ADR members on the same vanilla Aggregate when **both** endpoints have `preventPropagation == false`. A member with `preventPropagation == true` is still in the name/number list, but holes beside it are intentional. See [07](07-membership-maintenance-edge-cases.md).

**Number override:** Same shape as `ADRHighwayMarkerData.overrideMileage` + `newMileage` — boolean gate + stored value; default inactive.

### Coexistence

```text
                    ┌── vanilla AggregateSystem (unchanged)
Edge ──Aggregated──►│   Aggregate + AggregateElement   → default labels / SIP road panel
                    │
                    └── ADRNameAggregated (optional)
                            │
                            ▼
                      ADRNameAggregate + ADRNameAggregateElement
                            │
                            ├── NameSystem / ADR GetAggregateName
                            ├── BuildingUtils.GetAddress (ADR patch)
                            └── ADRHighwayAggregationData (migrated)
```

Vanilla aggregates continue to exist for every aggregatable net. ADR association is an **overlay**: when present, naming/addressing prefer ADR; when absent, behavior matches today.

## Resolution order (confirmed)

### Road name (`GetName` / `GetAggregateName`)

1. If entity is / resolves to an **ADR name-aggregate** (or edge has `ADRNameAggregated`): use that entity’s custom name / highway name / nameset+prefix rules (same logic as today’s `GetAggregateName`, but keyed by ADR buffer + ADR highway data).
2. Else if vanilla `Aggregate`: existing ADR + vanilla path.
3. Else: vanilla fallback.

### House numbers + address road entity (`GetAddress`)

When the building’s road edge has a valid `ADRNameAggregated`:

1. Walk **`ADRNameAggregateElement`** (not vanilla `AggregateElement`) for distance/number.
2. If the walk hits a member with **`numberOverrideActive`**, apply **`numberOverride`** (same idea as highway marker `overrideMileage` + `newMileage`).
3. Return **`road` = ADR name-aggregate entity** so `NameSystem` loads the matching street name.

When the edge has no ADR association: keep current vanilla / ADR highway-numbering behavior on the vanilla aggregate.

This is the confirmed **name + number** scope — avoids labels saying street A while addresses say street B / wrong number sequence.

## Membership lifecycle (ADR system)

Vanilla will still merge/split its own aggregates; ADR must maintain its own membership independently:

| Event | ADR response |
|-------|----------------|
| Player assigns edges to a custom association | Create/update ADR aggregate; set `ADRNameAggregated` on edges; fill ordered element buffer; mark labels dirty |
| Player removes association | Clear `ADRNameAggregated`; destroy empty ADR aggregates; dirty labels |
| Edge Deleted | Remove from ADR buffer; clear link; split/orphan ADR aggregates as needed |
| Edge Created / geometry Updated | If linked, refresh ADR chain membership as needed (same class of churn vanilla already has) |
| Net upgrade / replace (temp apply) | Remap ADR links from original → new edge entities (tool apply is a known pain point; see [05](05-research-gaps-and-next.md)) |

Fill `ADRNameAggregateElement` with a practical ordered walk (geometry / association-time). **Do not treat perfect permanent house-number order as a research blocker:** vanilla AggregateElement order is rebuilt by Validate/Combine and is not a stable guarantee either; ADR already improves numbering via existing `GetAddress` / highway-cache overrides — keep that bar, do not invent a stricter product requirement.

## Highway data migration (confirmed)

Today: `ADRHighwayAggregationData` (+ Dirty + Cache) live on **vanilla** `Aggregate`.

Target: same components live on **`ADRNameAggregate`** entities.

### Suggested migration path (implementation later)

1. On load / first run after feature: for each vanilla Aggregate with non-default `highwayDataId`, ensure an ADR name-aggregate exists covering the same `AggregateElement` edges (or create 1:1 ADR aggregate mirroring current membership).
2. Move (or copy-then-remove) highway + cache components onto the ADR entity; set `ADRNameAggregated` on those edges.
3. Update `AdrHighwayRoutesSystem` / `2B` / markers / SIP / region queries to key off ADR types.
4. Optional simple save migrate when cheap; **beta allows breakage** on orphans / conflicting markers / cities that never get association — do not treat those edge matrices as blockers.

After cutover, vanilla Aggregate for highways is only the game’s default label/SIP target; **ADR** owns route id and mileage cache.

## Labels and label pick (planned path)

Do **not** fight vanilla Aggregation for label geometry. For ADR-associated edges:

1. **Suppress** conflicting vanilla road-name labels on those edges / covered vanilla aggregates.
2. **Draw** ADR labels along `ADRNameAggregateElement` (thin layer — not a full `AggregateMeshSystem` clone). Prefer writing vanilla-shaped `LabelPosition` / `LabelExtents` (or equivalent ADR-owned copies of that contract) on the **ADR name-aggregate** entity so hit-testing can reuse the known algorithm.
3. **Pick:** fork `Game.Net.RaycastJobs.RaycastLabelsJob` (~150 lines, Burst) into an ADR job that resolves `m_Owner` to the ADR aggregate; schedule it from a **managed** Harmony hook on `RaycastSystem` (do not patch the Burst job body). See [06](06-labels-vs-vanilla-sync-and-harmony.md).

**Stability note:** the vanilla label-raycast job is a leaf (edge → aggregate → label curve hit-test only). Structurally it is less likely to churn than Aggregation / tools; still re-diff on game updates.

Continue ADR’s refresh pattern: mark label-related entities `Updated` when ADR membership or names change (`AdrMainSystem.ResetRoadsCache`). Do **not** depend on reflecting into `AggregateMeshSystem.OnDictionaryChanged`.

Address text remains correct via GetAddress / NameSystem patches independently of label draw/pick.

## SIP / selection

- Vanilla SIP `RoadSection` / selection highlight still key off vanilla `Aggregate` element lists unless Harmony remaps (polish — [06](06-labels-vs-vanilla-sync-and-harmony.md): S2 sums/highlights all ADR elements across vanilla Aggregates; S1 excludes ADR-associated edges from vanilla Aggregate sums **and** glow).
- Clicking an **ADR** label should select the **ADR name-aggregate** (selection model **S2** when using the forked raycast job).
- ADR SIP (`RoadAggregation` / seed UI): if edge has ADR association → ADR name-aggregate; else vanilla aggregate.
- Station road-reference options should list ADR aggregates when present.

## Serialization

- ADR link + element buffer + ADR aggregate tag must be `ISerializable` (or equivalent mod serialization) like other ADR components.
- Do not rely on vanilla `AggregatedSystem` to repair ADR links.
- After load, **do not** run gap-fill repair — membership as serialized is authoritative ([07](07-membership-maintenance-edge-cases.md)).

## Membership maintenance

Net apply remaps edge ids; **gap-fill** keeps ADR spans contiguous within each vanilla Aggregate. Edge-case matrix: [07](07-membership-maintenance-edge-cases.md).

## Testing checklist (for a future implementation sprint)

- [ ] Custom-associated edges share one name in SIP title / address format
- [ ] House numbers increase along ADR element order, not vanilla split
- [ ] Vanilla-only roads unchanged when ADR association absent
- [ ] Net delete / replace remaps ADR links; gap-fill between remaining members on same vanilla Aggregate ([07](07-membership-maintenance-edge-cases.md))
- [ ] Highway markers / mileage still work after migration (happy path)
- [ ] Optional: old save with highway-on-vanilla Aggregate migrates when cheap (beta may break odd cases)
- [ ] Pathfinding / traffic unchanged (smoke)
- [ ] Vanilla labels suppressed on ADR-associated edges; ADR labels drawn along ADR element order
- [ ] Clicking ADR labels selects ADR name-aggregate (forked `RaycastLabelsJob` + `RaycastSystem` schedule hook)
- [ ] SIP RoadSection stats follow ADR group when polish Harmony is in (optional v1)
