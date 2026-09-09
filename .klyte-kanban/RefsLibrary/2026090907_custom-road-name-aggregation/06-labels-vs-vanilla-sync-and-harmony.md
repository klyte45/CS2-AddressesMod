# Labels ownership, unattended branches, and Harmony viability

*Added: 2026-09-09 · Discussion follow-up to [04](04-parallel-system-design-notes.md) / [05](05-research-gaps-and-next.md) · Game.dll build GUID `b5fd0b9582554f3c83207d8d5d7a19a0`*

## Purpose

Capture the design discussion after the initial research pack:

1. Prefer **owning world labels for ADR-associated segments** over forcing custom membership into vanilla Aggregation.
2. ADR already (or can easily) own **name** and **house-number** resolution for the parallel system.
3. List **remaining unattended vanilla branches** (e.g. SIP traffic stats).
4. Assess **Harmony viability** for those branches — without patching Burst `AggregateSystem` jobs.
5. Lock the **label pick** plan: fork the isolated `RaycastLabelsJob` rather than Harmony-patching Burst or skipping pick parity.

## Recommended stack (planned path for next steps)

| Layer | Owner | Notes |
|-------|--------|------|
| Membership source of truth | **ADR** parallel components | Do not rewrite vanilla `Aggregated` / `AggregateElement` for custom joins |
| Street name | **ADR** (`NameSystem` hooks → ADR aggregate) | Already in ADR’s control |
| House numbers / address `road` | **ADR** (`BuildingUtils.GetAddress` hooks) | Confirmed: follow ADR element order |
| World labels on ADR-associated edges | **ADR** (draw + suppress vanilla on those edges) | Thin layer — not full `AggregateMeshSystem` clone |
| Label click → select | **ADR forked** `RaycastLabelsJob` + managed `RaycastSystem` schedule hook | Prefer selection model **S2** (ADR aggregate entity) |
| Vanilla Aggregation | Unchanged | Default roads, tools, unassociated segments |

**Compared to post-forcing vanilla aggregates:** owning ADR labels for associated segments is **easier** than fighting `ValidateAggregate`, and fits existing name/number patches.

## What parallel + name/number already covers

When an edge belongs to a custom ADR name-aggregate:

- SIP / tooltip / address **strings** can resolve via ADR name getter.
- Building **house numbers** can walk ADR ordered edges.
- Highway route id / mileage can live on the ADR aggregate after migration ([00-README.md](00-README.md)).

These do **not** require Harmony on Aggregation maintenance.

## Unattended branches (still vanilla Aggregate identity)

If ADR owns membership + name + numbers + labels + forked pick, these may still need polish:

| Branch | Vanilla entry | Player-visible effect if unpatched | Plan |
|--------|---------------|-------------------------------------|------|
| **SIP road statistics** | `RoadSection.OnUpdate` / `OnProcess` | Length, volume, flow, wear over **vanilla** elements only | Harmony over **ADR edge list** (validated: data on edges — below) |
| **Selection highlight** | `DefaultToolSystem` → `GenerateAggregatesSystem` (`Highlighted` on each `AggregateElement.m_Edge`) | Full vanilla Aggregate glow, including edges also in ADR | **Settled H1:** ADR list job rewrites CreationDefinition buffer before GenerateAggregates; same membership as SIP |
| **SIP title locale key** | `TitleSection.GetVirtualKeyboardLocaleKey` | `"RoadName"` only if entity has vanilla `Aggregate` | Easy Harmony if S2 |
| **SIP icon** | `ImageSystem` via `AggregateElement` | Miss on ADR-only entity | Low-priority Harmony |
| **Traffic routes tool** | `TrafficRoutesSystem` / `ActionsSection` | Still vanilla grouping | Optional later |
| **Achievements / net temps / infoview filter** | various | Negligible for naming feature | Leave |

**Label click** is **not** left unattended on the planned path — see next section.

## Label pick — fork `RaycastLabelsJob` (planned)

### Why fork, not Harmony-inside-Burst

- Vanilla job is `[BurstCompile]` `IJobParallelForDefer` — poor in-place Harmony target.
- Job body is **small (~150 lines)**: ~30 lines fields/`Execute`, ~120 lines `CheckAggregate` (curve / quad hit-test).
- **Leaf / isolated:** only uses edge candidates, `Aggregated`, `LabelExtents`, `LabelPosition`, camera right, result accumulator. No pathfinding, Aggregation maintenance, or net tools.
- **Structural judgment:** less likely to churn on updates than `AggregateSystem` or broad `RaycastSystem` rewrites. Still re-diff on game updates (especially `LabelPosition` contract and schedule site in `RaycastSystem`). Evidence is structural (single decompile snapshot), not multi-version history.

### Planned ADR implementation shape

1. ADR aggregate (or ADR label entity) carries vanilla-compatible **`LabelPosition` / `LabelExtents`** (or ADR duplicates of that contract).
2. ADR owns a **Burst clone** of `Game.Net.RaycastJobs.RaycastLabelsJob` that:
   - Resolves label owner via ADR association (e.g. edge → `ADRNameAggregated`, or iterate ADR aggregates with label buffers).
   - Sets `RaycastResult.m_Owner` / hit entity to the **ADR name-aggregate**.
3. Schedule the clone from a **managed** Harmony Prefix/Postfix on `Game.Common.RaycastSystem` when `TypeMask.Labels` is requested (same frame plumbing as vanilla’s `jobData19.Schedule(...)`).
4. Suppress vanilla labels on ADR-covered edges so players do not click a leftover vanilla label and get the wrong selection.

### Vanilla flow (reference)

```text
RaycastSystem
  → edges list
  → RaycastLabelsJob.Execute
       → Aggregated.m_Aggregate
       → LabelPosition / LabelExtents on that aggregate
       → m_Owner = vanilla Aggregate
```

ADR fork keeps the hit math; changes ownership resolution and schedule input so ADR-drawn labels are pickable.

## Approach comparison (short)

| Approach | Pros | Cons |
|----------|------|------|
| **A. ADR labels + name/number + forked pick** | No AggregateSystem fight; click selects ADR street | Need suppress/draw + RaycastSystem schedule hook |
| **B. Post-job force vanilla merges** | Stats/raycast “free” when merge sticks | Validate thrash; fails for arbitrary groups |
| **C. Harmony only on SIP consumers** | Fixes stats without labels ownership | Does not fix world labels / pick alone |
| **A + selective C** | Full naming UX + polish stats | Recommended next-steps shape |

## Harmony viability by branch

Viability assumes **managed** Harmony Prefix/Postfix (ADR already does this for `NameSystem` / `GetAddress`).

### High viability (managed UI)

#### `Game.UI.InGame.RoadSection` — traffic / condition stats (**validated**)

**Verdict:** User hypothesis confirmed. Aggregation is only a **membership list**; stats live on **edges** (segments). ADR can re-sum the same edge components over `ADRNameAggregateElement` and match the road the player sees as the ADR association.

**Vanilla `OnProcess` (Game.dll):** for each `AggregateElement.m_Edge`:

| Stat | Source component (on **edge**, not Aggregate) |
|------|-----------------------------------------------|
| Length | `Curve.m_Length` |
| Volume / flow | `Road` (`m_TrafficFlowDistance0/1`, `NetUtils.GetTrafficFlowSpeed`) — then **average** by `roadElementCount` |
| Wear / condition | `NetCondition.m_Wear` (min/max + average transform) |
| Upkeep | `PrefabRef` → `PlaceableNetData.m_DefaultUpkeepCost` |

Visibility (`OnUpdate`): `HasComponent<Aggregate> && HasComponent<AggregateElement>` on `selectedEntity`.

**Harmony plan (managed Prefix — preferred with selection S2):**

1. **`RoadSection.OnUpdate` Prefix**  
   - If `selectedEntity` has ADR name-aggregate + `ADRNameAggregateElement` buffer → set `visible = true`, skip or force-after vanilla.  
   - Else leave vanilla gate (unassociated roads).

2. **`RoadSection.OnProcess` Prefix**  
   - **S2 — selected is ADR name-aggregate:** skip original; walk **all** `ADRNameAggregateElement` edges with the same per-edge math (ADR groups may span **multiple** vanilla Aggregates — that is intended).  
   - **S1 — selected is still a vanilla `Aggregate`:** do **not** expand to the ADR group. Walk vanilla `AggregateElement`, but **skip every edge that also belongs to an ADR name-aggregate** (`ADRNameAggregated` / equivalent). Those edges are counted only when the ADR aggregate is selected (S2). Leftover vanilla-only edges keep coherent “remainder” stats for that vanilla Aggregate.  
   - If a vanilla Aggregate’s buffer is entirely ADR-associated after filtering, stats may be empty / section hide — acceptable; player should use the ADR selection for the associated street.

3. **Do not** invent aggregate-level traffic storage — sim already writes `Road` / `NetCondition` on edges; dual identity is **list choice**, not data ownership.

4. **Companion (thin):** `TitleSection.GetVirtualKeyboardLocaleKey` Prefix — treat ADR name-aggregate like `Aggregate` → `"RoadName"`. `ImageSystem` / traffic-routes remain optional later.

**Design constraint (S1):** By design one ADR name-aggregate can hold edges from **several** vanilla Aggregates. Therefore S1 must **exclude** ADR-associated edges from vanilla Aggregate sums — never double-count them into both vanilla and ADR SIP, and never treat “selected vanilla Aggregate” as “sum the ADR super-set.”

**Shared membership rule (SIP + highlight):** The edge set used for `RoadSection` and for selection **highlight** must be the **same** for a given selection model (next subsection).

**Implementation notes:** copy vanilla float math exactly (including `/ roadElementCount` and wear → percent transforms) so ADR-associated SIP numbers are comparable to vanilla for the same edge set. No Burst; no AggregateSystem touch.

**Verdict:** **Needed for coherent SIP** (S2 full ADR sum + S1 filtered vanilla remainder); high viability; schedule as polish epic with labels/pick (or immediately after).

#### Selection highlight — same edge sets as SIP (**validated path**)

Vanilla glow when a road Aggregate is selected:

1. `DefaultToolSystem` copies the selected entity’s `AggregateElement` buffer onto a `CreationDefinition` (+ `Updated`).
2. `GenerateAggregatesSystem.CreateAggregates` walks that buffer and, for each `m_Edge`, adds `Highlighted` + `BatchesUpdated`.

**Product rule (must match SIP):**

| Selection | Edges that receive `Highlighted` |
|-----------|----------------------------------|
| **S1** — vanilla `Aggregate` | Only edges in that Aggregate that are **not** ADR-associated (vanilla-only remainder) |
| **S2** — ADR name-aggregate | **All** edges in `ADRNameAggregateElement` (may span multiple vanilla Aggregates) |

**Harmony / ADR plan — settled: list job only (H1):**

Do **not** replace entire Burst `DefaultToolSystem.CreateDefinitionsJob` — it builds definitions for edges, nodes, routes, icons, etc.; the AggregateElement copy is only a small branch.

**Do** own a focused ADR job that is the **source of truth for the highlight membership list** (same S1/S2 rules as `RoadSection`):

```text
CreateDefinitionsJob (vanilla)
  → ADR list job rewrites CreationDefinition AggregateElement
       S1: vanilla-only remainder
       S2: full ADR members
  → GenerateAggregatesSystem (vanilla) applies Highlighted + BatchesUpdated
```

**H1 is enough** — leave GenerateAggregates as the highlighter. Do **not** pursue H2 (ADR applying `Highlighted` itself / suppressing CreateAggregates). Schedule the list job from managed update order between DefaultTool definitions and GenerateAggregates; do not patch Burst job bodies.

Clear/remove highlight on deselection via existing tool clear paths; do not leave ADR edges stuck Highlighted when switching back to S1 remainder selection.

**Verdict:** Same membership helper as RoadSection; implement together so stats and glow never disagree.

### Planned non-Harmony path (Burst leaf — clone)

#### `Game.Net.RaycastJobs.RaycastLabelsJob`

- **Do not** Harmony-patch the Burst body.
- **Do** fork + schedule (above). Part of the **ADR labels epic**, not “leave unattended.”

### Avoid

#### `AggregateSystem` / `AggregateMeshSystem` Burst jobs

- Out of scope as primary association or label-generation mechanism.

#### Achievements / serialization aggregate repair / net tool temps

- Leave unattended for this feature.

## Suggested product layering

```text
Must for feature:
  ADR membership + NameSystem + GetAddress
  + ADR labels (draw + suppress vanilla)
  + forked RaycastLabelsJob + RaycastSystem schedule hook (pick → ADR aggregate)

Should for polish:
  *(deferred — not this wave)* SIP RoadSection / TitleSection / DefaultTool H1
Nice later:
  Traffic-routes affordance over ADR groups
  Optional constrained vanilla post-sync ONLY for geometry-legal same-type chains

Never:
  Burst AggregateSystem rewrite as primary association mechanism
  Harmony inside RaycastLabelsJob Burst body
```

**Scope note (2026-09-09):** SIP stats/chrome and DefaultTool H1 are **out of the current implementation**. Deliver association via **tool overlays** + NameSystem/GetAddress + (later) world labels. Keep the RoadSection/H1 design above as archive for a future wave.

## Selection model

| Model | How selection works | SIP stats + highlight membership | Fits forked pick? |
|-------|---------------------|----------------------------------|-------------------|
| **S1 — Select vanilla Aggregate** | Default tool still picks vanilla aggregate | Vanilla `AggregateElement` **minus** edges that also belong to an ADR name-aggregate | Weak for ADR streets — pick lands on vanilla |
| **S2 — Select ADR aggregate entity** | ADR label pick / SIP sets selected = ADR entity | **All** `ADRNameAggregateElement` edges (may cross many vanilla Aggregates) | **Preferred** with forked `RaycastLabelsJob` |

**Why S1 excludes ADR edges:** An ADR association can legally mix segments from **various** vanilla Aggregations. Counting or **highlighting** those edges again while a vanilla Aggregate is selected would mis-attribute length/flow/wear and show the wrong glow (and double-count vs the ADR SIP/highlight). ADR-owned edges belong only to the ADR aggregate’s stats **and** highlight.

With the planned fork, default to **S2** for ADR-associated roads. Unassociated roads keep vanilla selection unchanged. S1 filtering still matters whenever vanilla Aggregate remains selectable (edge click, tools, leftover aggregates).

## Relation to prior pack docs

- [02-consumer-audit.md](02-consumer-audit.md) — full consumer list.
- [04-parallel-system-design-notes.md](04-parallel-system-design-notes.md) — components + **labels/pick planned path**.
- [05-research-gaps-and-next.md](05-research-gaps-and-next.md) — implementation spikes/epics including forked raycast.

## Open implementation choices (later plan)

1. Exact suppress rule: per-edge vs entire vanilla aggregate when any edge is ADR-associated.
2. Whether ADR reuses vanilla `LabelPosition`/`LabelExtents` types on ADR entities or ADR-named duplicates of the same layout.
3. SIP stats + DefaultTool H1: **out of current implementation** — revisit only in a later wave.
4. How aggressively to share schedule dependency with vanilla label raycast (same `JobHandle` chain vs independent).
