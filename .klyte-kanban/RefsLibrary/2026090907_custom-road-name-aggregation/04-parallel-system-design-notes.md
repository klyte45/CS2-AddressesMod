# Parallel ADR name-aggregation — design notes

*Design recommendations for a later implementation. Not product code.*

## Goals

- Let players define **custom road name associations** (which edges share one street name and one house-number sequence).
- Avoid fighting vanilla `AggregateSystem` Validate/Combine/Create.
- Keep **labels, SIP addresses, and building numbers** consistent when an edge is ADR-associated.
- **Activation wave:** strip Aggregate-hosted highway components; markers visual-only; introduce full ADR Aggregate/Element/Aggregated and fork-from-vanilla. Highway **reattach via Road Grouping tool later** (not migrate-in-place this wave). See [12](12-activation-change-map.md).

## Non-goals

- Harmony-replace or disable `Game.Net.AggregateSystem`.
- Mutate vanilla `Aggregated.m_Aggregate` / `AggregateElement` for custom membership.
- Change traffic simulation / pathfinding.
- World labels / SIP H1 in the activation wave ([13](13-activation-sprint-roadmap.md)).

## Activation fork / seed rules (settled 2026-09-10)

| Rule | Choice |
|------|--------|
| Fork trigger | First membership **edit** (not plain select) |
| Seed | Copy vanilla `ADRRandomizationData` → ADR; **`Redraw()`** vanilla Aggregate |
| Buffer init | Full vanilla `AggregateElement` copy, then apply edit |
| Naming after fork | Split: vanilla name (new seed) for non-ADR edges; ADR name for linked members |

Full file map: [12](12-activation-change-map.md). Sprints: [13](13-activation-sprint-roadmap.md).

## Recommended component sketch (names illustrative)

Use ADR-prefixed types so they never collide with vanilla:

| Type | Kind | Role |
|------|------|------|
| `ADRNameAggregate` | Tag on ADR aggregate entity | Marks ADR-owned name-aggregate (parallel to vanilla `Aggregate`) |
| `ADRNameAggregateElement` | Buffer on ADR aggregate | Ordered membership entries (edge + per-member flags) — source of truth for name + numbering walk |
| `ADRNameAggregated` | Component on edge | Link to ADR aggregate when associated; absent ⇒ fall back to vanilla. Mirrors membership fields needed for edge queries |
| Existing | On ADR aggregate | `ADRRandomizationData`, custom name storage (via vanilla NameSystem custom-name APIs if applicable). **`ADRHighwayAggregationData` / Dirty / Cache** — **not** on Aggregate in activation wave; reattach to ADR via tool in a **later** epic |

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

## Highway data (activation vs later)

**Today:** `ADRHighwayAggregationData` (+ Dirty + Cache) live on **vanilla** `Aggregate`.

**Activation wave ([12](12-activation-change-map.md) ACT-01 / ACT-02):** **Remove** Aggregate-hosted highway components. Markers become **visual-only** (strip route/mileage; keep pylons; WE `[Feature]` placeholders). **Keep** highway **registry** settings UI. Do **not** migrate highway onto ADR in this activation sprint.

**Later epic:** reattach route/mileage onto `ADRNameAggregate` via Road Grouping tool; restore WE formulae from placeholders. Beta breakage on old Aggregate highway saves is acceptable when stripping.

## Labels and label pick (planned path — after activation)

Do **not** fight vanilla Aggregation for label geometry. For ADR-associated edges:

1. **Suppress** conflicting vanilla road-name labels on those edges / covered vanilla aggregates.
2. **Draw** ADR labels along `ADRNameAggregateElement` (thin layer — not a full `AggregateMeshSystem` clone). Prefer writing vanilla-shaped `LabelPosition` / `LabelExtents` (or equivalent ADR-owned copies of that contract) on the **ADR name-aggregate** entity so hit-testing can reuse the known algorithm.
3. **Pick:** fork `Game.Net.RaycastJobs.RaycastLabelsJob` (~150 lines, Burst) into an ADR job that resolves `m_Owner` to the ADR aggregate; schedule it from a **managed** Harmony hook on `RaycastSystem` (do not patch the Burst job body). See [06](06-labels-vs-vanilla-sync-and-harmony.md).

**Stability note:** the vanilla label-raycast job is a leaf (edge → aggregate → label curve hit-test only). Structurally it is less likely to churn than Aggregation / tools; still re-diff on game updates.

Continue ADR’s refresh pattern: mark label-related entities `Updated` when ADR membership or names change (`AdrMainSystem.ResetRoadsCache`). Do **not** depend on reflecting into `AggregateMeshSystem.OnDictionaryChanged`.

Address text remains correct via GetAddress / NameSystem patches independently of label draw/pick.

## SIP / selection

**This implementation wave does not include SIP work** ([08](08-association-tool-first-ux.md)). No RoadSection / TitleSection / DefaultTool H1 / ADR SIP `RoadAggregation` panel.

Vanilla SIP may continue to show vanilla Aggregates for length/flow while ADR owns naming via NameSystem/GetAddress and the association **tool**. Revisit SIP coherence only in a later wave ([06](06-labels-vs-vanilla-sync-and-harmony.md)).

- Station road-reference options should list ADR aggregates when present (can follow tool/data; not SIP-panel work).

## Serialization

- ADR link + element buffer + ADR aggregate tag must be `ISerializable` (or equivalent mod serialization) like other ADR components.
- Do not rely on vanilla `AggregatedSystem` to repair ADR links.
- After load, **do not** run gap-fill repair — membership as serialized is authoritative ([07](07-membership-maintenance-edge-cases.md)).

## Membership maintenance

Net apply remaps edge ids; **gap-fill** keeps ADR spans contiguous within each vanilla Aggregate. Edge-case matrix: [07](07-membership-maintenance-edge-cases.md).

## Testing checklist (for a future implementation sprint)

- [ ] Custom-associated edges share one name in addresses / tool-focused name UI
- [ ] House numbers increase along ADR element order, not vanilla split
- [ ] Vanilla-only roads unchanged when ADR association absent
- [ ] Net delete / replace remaps ADR links; gap-fill between remaining members on same vanilla Aggregate ([07](07-membership-maintenance-edge-cases.md))
- [ ] Highway markers / mileage still work after migration (happy path)
- [ ] Optional: old save with highway-on-vanilla Aggregate migrates when cheap (beta may break odd cases)
- [ ] Pathfinding / traffic unchanged (smoke)
- [ ] Association **tool** overlays + node controls edit membership
- [ ] Vanilla labels suppressed on ADR-associated edges; ADR labels drawn along ADR element order
- [ ] Clicking ADR labels selects ADR name-aggregate (forked `RaycastLabelsJob` + `RaycastSystem` schedule hook)
- [ ] *(Later wave)* SIP RoadSection / H1 highlight — not required for this implementation
