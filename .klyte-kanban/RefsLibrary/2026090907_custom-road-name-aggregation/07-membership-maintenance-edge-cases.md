# ADR name-aggregate membership maintenance — edge cases

*Added: 2026-09-09 · Expands [05](05-research-gaps-and-next.md) § Net replace / remapping · Game.dll build GUID `b5fd0b9582554f3c83207d8d5d7a19a0`*

## Purpose

Define how `ADRNameAggregated` / `ADRNameAggregateElement` stay coherent when the net changes, and enumerate **all practical cases** to implement against.

Vanilla Aggregation still owns geometry join/split. ADR only maintains **parallel membership**.

## Vanilla remap notes (MCP)


| Stage                 | Behavior relevant to ADR                                                                                                                                                                                   |
| --------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `GenerateEdgesSystem` | Temp edge can copy `Aggregated` from `definitionData.m_Original` when `m_AggregateType != Null`                                                                                                            |
| `ApplyNetSystem`      | On edges: if Aggregate pointed by `Aggregated` is **Temp**, clear edge `Aggregated` to default; `Replace`/`Combine` delete original + `Create` temp; other updates patch components onto `temp.m_Original` |
| `AggregateSystem`     | Rebuilds vanilla membership after edits (Validate / Combine) — **independent** of ADR                                                                                                                      |


ADR must **not** assume the edge entity id is stable under Replace, or that vanilla Aggregate membership equals ADR membership.

## Settled continuity rule — gap fill

**When membership is maintained after a member edge leaves the ADR list** (deleted, replaced by a new entity, or otherwise no longer a member), the ADR name-aggregate **auto-assigns gap edges** into its list when:

1. Gap edge **G** is **not** currently an ADR member of this aggregate, and
2. Along the **vanilla** `AggregateElement` order of **one** vanilla Aggregate, there exist members **A** and **B** of this same ADR aggregate such that **G** lies strictly between **A** and **B**, and
3. **A**, **G**, and **B** share that **same** vanilla Aggregate, and
4. **Both A and B have `preventPropagation == false`** (see membership fields below).

Then **G** (and every interstitial edge between A and B on that vanilla chain) is added to the ADR aggregate.

```text
Vanilla AggregateElement order:  … A · g1 · g2 · B …
ADR members before:              A, B   (g1/g2 missing — hole after delete/remap)
ADR members after gap-fill:      A, g1, g2, B   (only if A and B both allow propagation)
```

**Rationale:** Holes from net edits heal automatically **within** a propagating span. ADR may still be **non-contiguous** when members are flagged `preventPropagation`.

**Does not apply across different vanilla Aggregates** (no auto-bridge between two vanilla roads).

## Membership fields (settled)

Stored on each membership entry (`ADRNameAggregateElement`; mirror on `ADRNameAggregated` as needed). Highway-marker style for the number override (`overrideMileage` + `newMileage`):


| Field                  | Default | Meaning                                                                                                               |
| ---------------------- | ------- | --------------------------------------------------------------------------------------------------------------------- |
| `preventPropagation`   | `false` | **true** ⇒ edge is in the ADR list but **will not propagate** gap-fill (intentional non-contiguous / island boundary) |
| `numberOverrideActive` | `false` | **true** ⇒ use `numberOverride` instead of computed sequence value                                                    |
| `numberOverride`       | `0`     | Override **house number** (`int`); ignored unless `numberOverrideActive`                              |


New gap-filled edges: `preventPropagation = false`, `numberOverrideActive = false`.

On entity remap (replace): **copy** these flags from original membership to the successor edge.

## Remap of surviving members (entity identity)


| Event                                                              | ADR action                                                                                           |
| ------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------- |
| Edge **updated in place** (`Temp` → same `m_Original`)             | Keep `ADRNameAggregated`; refresh element buffer if needed                                           |
| Edge **replaced** (original Deleted, new entity Created from temp) | Map membership **original → new** via `Temp.m_Original` (or equivalent apply map); then run gap-fill |
| Edge **deleted** with no replacement                               | Remove from ADR element list + clear link; then run gap-fill on remaining members                    |
| Temp preview only (not applied)                                    | Prefer no permanent ADR writes; optional preview-only highlight/labels                               |


## Edge-case matrix

Use this as the implementation checklist. Columns: **Trigger** → **Expected ADR result**.

### A. Single-edge lifecycle


| ID  | Trigger                                                            | Expected                                                                                                                                                                                                 |
| --- | ------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A1  | Upgrade / modify in place (same entity)                            | Membership unchanged                                                                                                                                                                                     |
| A2  | Replace (new entity, old deleted)                                  | Remap link to new edge; gap-fill                                                                                                                                                                         |
| A3  | Delete member, no successor                                        | Drop member; gap-fill between remaining members on same vanilla Aggregate                                                                                                                                |
| A4  | Delete non-member adjacent to ADR span                             | No ADR change (unless later gap-fill from other edits)                                                                                                                                                   |
| A5  | Create brand-new edge (no original)                                | **Not** auto-added unless it becomes a gap between two existing ADR members on the same vanilla Aggregate (A6)                                                                                           |
| A6  | New edge appears between two ADR members on same vanilla Aggregate | Gap-fill adds it                                                                                                                                                                                         |
| A7  | Split one edge into two (node insert)                              | Add **both** halves only if both successors are known replacements of the member. Often that map is missing after apply — **default to gap-fill** (drop dead edge / remap what Temp provides, then fill between propagating members) |
| A8  | Merge / combine two edges                                          | Surviving edge keeps membership if either side was member (prefer union); drop deleted; gap-fill                                                                                                         |


### B. Vanilla Aggregate topology (ADR spans one vanilla Aggregate)


| ID  | Trigger                                                                        | Expected                                                                                                                                                          |
| --- | ------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| B1  | Hole in middle of ADR span (member deleted)                                    | Gap-fill interstitial edges                                                                                                                                       |
| B2  | Member removed from **end** of ADR span (no ADR members on both sides)         | Drop only; **no** auto-grow beyond former span                                                                                                                    |
| B3  | Two disjoint ADR islands on same vanilla Aggregate (`A…A` then gap then `B…B`) | Gap-fill **joins** them only if the facing endpoints both have `preventPropagation == false`. To keep islands, set `preventPropagation` on the boundary member(s) |
| B4  | Vanilla Aggregate splits into two Aggregates                                   | Each ADR member keeps ADR link; element buffer may now reference edges on two vanilla Aggregates (allowed by design); no forced split of ADR entity               |
| B5  | Vanilla Aggregates combine                                                     | Membership unchanged; gap-fill may add newly interstitial edges if they fall between **propagating** ADR members on the **new** combined vanilla order            |
| B6  | Vanilla Validate rebuilds element order                                        | ADR element order may be refreshed to a practical walk; gap-fill uses **current** vanilla order                                                                   |
| B7  | Member with `preventPropagation == true` between two spans                     | Acts as non-contiguous membership; no gap-fill across that member as a propagating endpoint                                                                       |


**B3 settled:** Non-contiguous ADR on one vanilla Aggregate is supported via `**preventPropagation`**. Without that flag on boundaries, net-driven gap-fill will heal holes between propagating members.

### C. Multi–vanilla-Aggregate ADR (by design)


| ID  | Trigger                                                                 | Expected                                                                  |
| --- | ----------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| C1  | ADR members on vanilla Agg X and Agg Y                                  | Allowed; gap-fill runs **per** vanilla Aggregate independently            |
| C2  | Edge between X and Y (connector) not in either Aggregate’s element list | Not gap-filled (fails “same vanilla aggregation”)                         |
| C3  | Player associates edges from three vanilla Aggregates                   | Membership is explicit union; gap-fill only within each Aggregate’s chain |


### D. Prefab / type changes


| ID  | Trigger                                                                       | Expected                                                                                                                                       |
| --- | ----------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| D1  | Road upgrade same AggregateNet type                                           | Remap + gap-fill                                                                                                                               |
| D2  | Replace with prefab that has **no** `m_AggregateType`                         | Edge may lose vanilla `Aggregated`; keep ADR membership if still an Edge; gap-fill using whatever vanilla Aggregate still exists for neighbors |
| D3  | Replace with different road type that joins a **different** vanilla Aggregate | Remap entity; gap-fill on **new** Aggregate’s order; may leave old Aggregate without this edge (drop from old gaps automatically)              |


### E. Empty / degenerate ADR aggregates


| ID  | Trigger                                                       | Expected                                                                                                                              |
| --- | ------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| E1  | Last member deleted                                           | Destroy or mark empty ADR aggregate; clear highway data per migrate rules; hide SIP/labels                                            |
| E2  | All members remapped to zero surviving edges                  | Same as E1                                                                                                                            |
| E3  | Gap-fill would add edges but ADR aggregate was player-cleared | Only run gap-fill when maintenance is triggered by net remap of an **existing** non-empty ADR aggregate — not when player unbound all |


### F. Temp / tool / cancel


| ID  | Trigger                                        | Expected                                                                          |
| --- | ---------------------------------------------- | --------------------------------------------------------------------------------- |
| F1  | Hover replace preview                          | Do not commit ADR membership; optional visual only                                |
| F2  | Apply commit                                   | Remap + gap-fill once                                                             |
| F3  | Cancel / ToolClear                             | No permanent ADR change                                                           |
| F4  | Selection highlight H1 CreationDefinition list | Uses **current** ADR membership (post-gap-fill); not a separate membership writer |


### G. Concurrency with highway / names


| ID  | Trigger                                         | Expected                                                             |
| --- | ----------------------------------------------- | -------------------------------------------------------------------- |
| G1  | Member remap with highway data on ADR aggregate | Highway stays on ADR entity; mileage cache dirty                     |
| G2  | Gap-fill adds edges under highway ADR           | New edges inherit ADR association (name + numbers + highway context) |
| G3  | Custom name on ADR aggregate                    | Unchanged by membership remap                                        |


### H. Explicit player membership edits (not net-driven)


| ID  | Trigger                                               | Expected                                                                                                                                                                  |
| --- | ----------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| H1  | Player **adds** edge to ADR                           | Add edge (`preventPropagation` / number override as UI sets; defaults false/false); gap-fill between new edge and other **propagating** members on same vanilla Aggregate |
| H2  | Player **removes** edge from ADR                      | Remove edge; do not gap-fill that edge back. Remaining holes heal only between propagating endpoints                                                                      |
| H3  | Player sets `preventPropagation` on a member          | Stops gap-fill using that edge as a propagating endpoint; allows lasting non-contiguous membership                                                                        |
| H4  | Player sets `numberOverrideActive` + `numberOverride` | GetAddress / numbering uses override for that member when active; inactive ⇒ computed sequence                                                                            |


## Recommended maintenance pipeline

```text
On applied net change affecting edges (not on load):
  1. Build old→new edge map from Temp / Deleted / Created
  2. Remap ADRNameAggregated + rewrite ADRNameAggregateElement (copy preventPropagation + number override fields)
  3. Drop deleted edges with no successor
  4. Gap-fill per vanilla Aggregate between consecutive members where BOTH have preventPropagation == false
  5. If empty → teardown ADR aggregate
  6. Dirty names / labels / highway cache / SIP

On load / deserialize:
  Trust saved membership — no gap-fill repair
```

## Settled answers (was open questions)

1. **A7 split:** Ideal “add both halves” only when **both** successors are known replacements of the member. In practice original membership often will not survive apply — **default to gap-fill** after dropping the dead edge / remapping whatever `Temp` still provides.
2. **Load / deserialize:** **No** gap-fill repair on load — trust membership as saved.
3. **`numberOverride`:** **`int`** (house number).

## Relation

- [04](04-parallel-system-design-notes.md) — component model  
- [05](05-research-gaps-and-next.md) — points here for remapping  
- [06](06-labels-vs-vanilla-sync-and-harmony.md) — highlight H1 consumes post-maintenance membership

