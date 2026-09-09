# Research gaps and next steps

*Follow-up inputs after this documentation pack. No Kanban tasks created here.*

## Settled by this pack

| Topic | Outcome |
|-------|---------|
| Display vs simulation | Aggregation does **not** drive traffic sim; it drives naming, addressing, labels, SIP road panel, tools, one achievement ([02](02-consumer-audit.md)) |
| Strategy | Parallel ADR system; do not override `AggregateSystem` |
| Address scope | ADR association controls **name + house numbers** |
| Highway data | **Migrate** to ADR-owned aggregates |
| World labels | **ADR-owned** draw + suppress vanilla on associated edges ([06](06-labels-vs-vanilla-sync-and-harmony.md)) |
| Label pick | **Fork** `RaycastLabelsJob` (~150 lines); schedule via managed `RaycastSystem` hook — not Burst Harmony |
| House-number chain order | **Not a research gap** — vanilla AggregateElement order is already rebuilt by Validate/Combine and is not a stable product guarantee; ADR already improves numbering (existing `GetAddress` / highway-cache overrides). Parallel ADR should keep using the same practical approach |
| Highway migration edge cases | **Not a research gap** — mod is in **beta**; breaking highway-on-vanilla Aggregate saves / odd orphan cases is acceptable. Prefer a straightforward migrate when cheap; do not spike empty-aggregate / multi-aggregate / compat-mode edge matrices as blockers |
| SIP road statistics | **Validated + Harmony planned** — stats are **edge-level**; S2 sums all ADR elements (may span many vanilla Aggregates); **S1** (vanilla Aggregate selected) **ignores** edges that also belong to an ADR name-aggregate so they are not double-counted. See [06](06-labels-vs-vanilla-sync-and-harmony.md) |
| Selection highlight | **Settled: H1 only** — ADR list job rewrites CreationDefinition `AggregateElement` before `GenerateAggregatesSystem` (same S1/S2 membership as SIP). Do not replace whole `CreateDefinitionsJob`; do not have ADR apply `Highlighted` itself (H2 out of scope). See [06](06-labels-vs-vanilla-sync-and-harmony.md) |
| Membership fields | **Settled** — per member: `preventPropagation` (default false); `numberOverrideActive` (default false) + `numberOverride` (`int`). Gap-fill only between propagating endpoints; **no** gap-fill on load. A7 split defaults to gap-fill. See [04](04-parallel-system-design-notes.md) / [07](07-membership-maintenance-edge-cases.md) |

## Remaining technical gaps (research / spikes)

### 1. World label placement + pick (ADR path)

Vanilla `AggregateMeshSystem` lays labels on **vanilla** `AggregateElement` chains. ADR owns strings via NameSystem; for associated edges ADR also owns **draw + pick**.

**Planned approach (see [06](06-labels-vs-vanilla-sync-and-harmony.md)):**

1. Suppress conflicting vanilla labels on ADR-covered edges.
2. Place thin ADR labels along `ADRNameAggregateElement` (reuse `LabelPosition` / `LabelExtents` contract on ADR aggregate where practical).
3. **Fork** `Game.Net.RaycastJobs.RaycastLabelsJob` into an ADR Burst job (`m_Owner` → ADR name-aggregate); schedule from managed Harmony on `RaycastSystem`. Do **not** patch the vanilla Burst job body.
4. Do **not** use Aggregation post-sync or `AggregateMeshSystem` Harmony as the primary fix.

**Stability:** job is a ~150-line leaf (edge → aggregate → curve hit-test). Structurally low churn vs Aggregation; still re-check on game updates (especially `LabelPosition` layout and `RaycastSystem` scheduling).

**Needed for implementation:** spike suppress + draw; then fork/schedule pick; verify selection model **S2** (click ADR label → ADR entity).

### 2. Net replace / temp apply remapping + membership maintenance

Vanilla: `GenerateEdgesSystem` can copy `Aggregated` from `Temp.m_Original`; `ApplyNetSystem` clears edge `Aggregated` when the Aggregate is Temp, and Replace deletes original + creates from temp. Vanilla `AggregateSystem` then rebuilds its own membership — **independent of ADR**.

**Settled continuity (gap-fill):** After a member leaves the ADR list (delete/replace/remap), auto-add any non-member edges that lie **between two still-member edges on the same vanilla Aggregate** when **both** endpoints have `preventPropagation == false`. Non-contiguous ADR is allowed via that membership flag. Per-member **`numberOverrideActive` (default false) + `numberOverride`** mirror highway marker override style. Full matrix: **[07-membership-maintenance-edge-cases.md](07-membership-maintenance-edge-cases.md)**; fields in [04](04-parallel-system-design-notes.md).

**Needed for implementation:** Remap `ADRNameAggregated` via Temp original→new map; drop orphans; run gap-fill; teardown empty ADR aggregates; dirty labels/highway/SIP.

### 3. `ADREdgeData` cleanup

Dead `fixedAggregation` (and possibly unused `ADREdgeData` entirely) — schedule delete or wire only if still useful for width/speed/highway flags. Out of scope for this pack’s code changes.

### 4. Non-Game.dll assemblies

Consumer audit focused on **Game.dll**. If other Managed DLLs reference Aggregation, a follow-up `kcs2m_backend_search` without `dlls` filter (or other DLLs) would close that loop. Low priority given Game.dll owns Net/UI/Tools.

## Suggested shape of a follow-up implementation pack / Kanban epic

When ready to leave documentation-only mode:

1. **Epic: ADR name aggregation** — components + systems + serialization + GetAddress/NameSystem resolution order + **membership maintenance / gap-fill** ([07](07-membership-maintenance-edge-cases.md)).
2. **Epic: Highway migrate** — move highway components; update routes/2B/region/SIP; optional simple save migrate (beta may break odd cases).
3. **Epic / spike: ADR labels** — suppress vanilla; draw along ADR elements; **fork `RaycastLabelsJob`** + `RaycastSystem` schedule hook; selection → ADR aggregate.
4. **Polish: SIP stats + highlight** — shared membership helper; Harmony `RoadSection` / thin `TitleSection` + ADR **highlight list job (H1)** before `GenerateAggregates` (S2 = all ADR edges; S1 = vanilla minus ADR-associated; [06](06-labels-vs-vanilla-sync-and-harmony.md)).
5. **Epic: Player association UX** — tool or SIP actions to bind/unbind edge sets; region map visualization.
6. **Cleanup:** remove or replace `fixedAggregation` stub.

Do **not** create these Kanban items from this research pack unless explicitly asked.

## MCP sources used

- `kcs2m_backend_dlls_list`, `kcs2m_backend_files_list` (pathFilter `Aggregat`)
- `kcs2m_backend_search` / `kcs2m_backend_context` on:
  - `Game/Net/AggregateSystem.cs`, `Aggregate.cs`, `Aggregated.cs`, `AggregateElement.cs`
  - `Game/Prefabs/AggregateNetPrefab.cs`, `AggregateNetData.cs`
  - `Game/Buildings/BuildingUtils.cs` (`GetAddress`)
  - `Game/Rendering/AggregateMeshSystem.cs`, `AggregateRenderSystem.cs`
  - `Game/UI/InGame/RoadSection.cs`, `TitleSection.cs`, `NameSystem.cs`
  - `Game/Tools/*`, `Game/Serialization/AggregatedSystem.cs`, achievements / traffic infoview

Build GUID cited throughout: `b5fd0b9582554f3c83207d8d5d7a19a0`.
