# Research gaps and next steps

*Follow-up inputs after this documentation pack. No Kanban tasks created here.*

## Settled by this pack

| Topic | Outcome |
|-------|---------|
| Display vs simulation | Aggregation does **not** drive traffic sim; it drives naming, addressing, labels, SIP road panel, tools, one achievement ([02](02-consumer-audit.md)) |
| Strategy | Parallel ADR system; do not override `AggregateSystem` |
| Address scope | ADR association controls **name + house numbers** |
| Highway data | **Activation:** strip Aggregate host; markers visual-only; registry kept. **Later:** reattach via tool onto ADR ([12](12-activation-change-map.md)) |
| World labels | **ADR-owned** draw + suppress vanilla on associated edges ([06](06-labels-vs-vanilla-sync-and-harmony.md)) |
| Label pick | **Fork** `RaycastLabelsJob` (~150 lines); schedule via managed `RaycastSystem` hook — not Burst Harmony |
| House-number chain order | **Not a research gap** — vanilla AggregateElement order is already rebuilt by Validate/Combine and is not a stable product guarantee; ADR already improves numbering (existing `GetAddress` / highway-cache overrides). Parallel ADR should keep using the same practical approach |
| Highway migration edge cases | **Not a research gap** — mod is in **beta**; breaking highway-on-vanilla Aggregate saves / odd orphan cases is acceptable. Prefer a straightforward migrate when cheap; do not spike empty-aggregate / multi-aggregate / compat-mode edge matrices as blockers |
| SIP road statistics / SIP chrome / DefaultTool H1 | **Out of scope for this implementation** — research in [06](06-labels-vs-vanilla-sync-and-harmony.md) remains valid for a **later** wave; do not plan RoadSection / TitleSection / H1 highlight epics now. Association editing is **tool-only** ([08](08-association-tool-first-ux.md)) |
| Selection highlight (H1) | Documented for later; **not** part of current delivery. Tool uses **ADR-owned overlays**, not DefaultTool H1 |
| Membership fields | **Settled** — per member: `preventPropagation` (default false); `numberOverrideActive` (default false) + `numberOverride` (`int`). Gap-fill only between propagating endpoints; **no** gap-fill on load. A7 split defaults to gap-fill. See [04](04-parallel-system-design-notes.md) / [07](07-membership-maintenance-edge-cases.md) |
| Initial UX | **Settled: net-only Road Grouping tool** — world drawing + node controls. **No SIP work in this wave.** T0–T3 shipped; activation sprint ACT-01…ACT-06 in [12](12-activation-change-map.md) / [13](13-activation-sprint-roadmap.md) |
| Activation sequencing | **Settled:** full ADR components (not thin contract); fork/seed rules; strip highway Aggregate host first |

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

**Needed for implementation:** Remap `ADRNameAggregated` via Temp original→new map; drop orphans; run gap-fill; teardown empty ADR aggregates; dirty names / tool overlays / highway cache.

### 3. `ADREdgeData` cleanup

Dead `fixedAggregation` (and possibly unused `ADREdgeData` entirely) — schedule delete or wire only if still useful for width/speed/highway flags. Out of scope for this pack’s code changes.

### 4. Non-Game.dll assemblies

Consumer audit focused on **Game.dll**. If other Managed DLLs reference Aggregation, a follow-up `kcs2m_backend_search` without `dlls` filter (or other DLLs) would close that loop. Low priority given Game.dll owns Net/UI/Tools.

## Suggested shape of a follow-up implementation pack / Kanban epic

**Use the activation catalog** — do not invent a thin-contract epic:

- Change map: [12](12-activation-change-map.md)
- One activation sprint + documented Kanban tasks ACT-01…ACT-06 (ACT-03 split into 03a/03b/03c): [13](13-activation-sprint-roadmap.md)
- Tool-first context: [08](08-association-tool-first-ux.md)

Order: strip Aggregate highway (ACT-01) → markers visual-only (ACT-02) → ADR components (ACT-03a) → membership CRUD (ACT-03b) → ForkFromVanilla + seed (ACT-03c) → name/address (ACT-04) → Road Grouping mutations (ACT-05) → membership maintenance (ACT-06).

**Later (after ACT-06):** highway reattach via tool; ADR labels + forked pick; `fixedAggregation` cleanup.

**Explicitly deferred:** SIP `RoadSection` / `TitleSection` / ImageSystem / DefaultTool H1 — keep [06](06-labels-vs-vanilla-sync-and-harmony.md) as future reference only.

Do **not** create Kanban board items from this research pack unless explicitly asked. Catalog text lives in [13](13-activation-sprint-roadmap.md).

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
