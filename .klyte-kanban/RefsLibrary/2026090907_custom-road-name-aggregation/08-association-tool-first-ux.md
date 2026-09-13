# Association tool-first UX — network tool + world drawing

*Added: 2026-09-09 · Product direction follow-up to [05](05-research-gaps-and-next.md) / [07](07-membership-maintenance-edge-cases.md) · Game.dll build GUID `b5fd0b9582554f3c83207d8d5d7a19a0`*

## Verdict

**Yes — start with a dedicated ADR tool that targets networks only.** That matches the domain (membership is edge-based, may span multiple vanilla Aggregates, supports non-contiguous members via `preventPropagation`) better than SIP-first or settings-first UX.

It **does** require **world drawing** beyond vanilla selection highlight (H1): the tool must show working membership / vanilla context while editing, and expose **node-level controls** to grow/shrink/split association spans. That is a deliberate scope increase for v1 UX, not a contradiction of earlier plans.

## Why a net-only tool fits

| Need from research pack | Tool-friendly? |
|-------------------------|----------------|
| Bind/unbind edges across **multiple** vanilla Aggregates | Click/paint nets in world — natural |
| Contiguous gap-fill vs `preventPropagation` islands | Visual spans + boundary flags at nodes/edges |
| `numberOverride` / `numberOverrideActive` | Inspect selected member edge in tool options / world affordance |
| S1 remainder vs S2 ADR membership | Tool can draw **both** overlays without relying on DefaultTool selection |
| Playtest remap/gap-fill early | Tool creates real membership without waiting for SIP or DefaultTool polish |

**SIP:** **Out of scope for this implementation wave** — no RoadSection / TitleSection / H1 selection work, no ADR SIP panel as editor or inspect surface. Vanilla SIP may still show vanilla Aggregates; that is accepted until a later wave. Association editing and feedback live in the **tool** (and later world name labels).

## Proposed tool shape

### Target

- Active tool: ADR **name-association** tool (`IBelzontToolSystem` / `ToolBaseSystem` pattern already in Commons).
- Raycast: **`TypeMask.Net` only** (edges/nodes) — no buildings, vehicles, labels-as-primary.
- Hover/click resolves to **edge** (and incident **nodes** for handles).

### Modes (illustrative — refine in implementation plan)

| Mode | Action |
|------|--------|
| Select / focus ADR aggregate | Click member edge → focus that `ADRNameAggregate`; click non-member → offer create or add |
| Add span | Add edge (and optional gap-fill between propagating endpoints on same vanilla Aggregate) |
| Remove member | Unbind edge; respect [07](07-membership-maintenance-edge-cases.md) H2 |
| Toggle `preventPropagation` | On member (or via node handle at island boundary) |
| Edit `numberOverride*` | On focused member edge (tool options panel + optional world badge) |

Exact mode set is an open product detail; the important lock is **tool owns association editing**.

### World drawing (required — not optional polish)

Vanilla **H1** (CreationDefinition → `GenerateAggregatesSystem` → `Highlighted`) only covers **selection glow** for DefaultTool. The association tool needs its **own** overlay while active:

| Layer | Purpose |
|-------|---------|
| **Vanilla Aggregate context** | Edges of the hovered/focused edge’s vanilla Aggregate (remainder-aware: optionally dim ADR-owned edges) |
| **ADR membership** | All edges in focused `ADRNameAggregateElement` (full S2 set, cross-vanilla) |
| **Hover candidate** | Edge under cursor + preview of add/remove/gap-fill |
| **Non-propagate members** | Distinct style (e.g. dashed / alternate color) so islands are visible |
| **Number override** | Optional badge on members with `numberOverrideActive` |

Prefer **ADR-owned overlay draw** (mesh/lines/icons) while the tool is active, rather than overloading vanilla `Highlighted` for edit preview. DefaultTool **H1** highlight is **not** in this wave.

### Node controls (required for “manipulating it”)

Nodes are the natural place to **extend or shrink** contiguous runs along a vanilla Aggregate chain:

| Control | Behavior (proposed) |
|---------|---------------------|
| Endpoint handle on a propagating span | Drag/click toward adjacent non-member → add edge(s) along vanilla chain (gap-fill rules) |
| Endpoint handle inward | Remove end member(s) from ADR list |
| Boundary at `preventPropagation` member | Show “break” affordance; toggling flag merges/splits islands under gap-fill rules |
| Node shared by two vanilla Aggregates | Only operate along the chain(s) that participate in the focused ADR aggregate |

Implementation note: node picks still use net raycast; handles are **tool overlays** at node positions, not vanilla Aggregate UI. Exact gesture (click adjacent edge vs drag handle) is an open UX choice.

## Impact on prior plan

### Full architecture activation (replaces “thin contract”)

Road Grouping T0–T3 is shipped. Next backend work is **full** ADR Aggregate/Element/Aggregated + fork/seed + real mutations — not a minimal thin contract. File map: [12](12-activation-change-map.md). One sprint + tasks: [13](13-activation-sprint-roadmap.md).

| In activation sprint (ACT-01…ACT-06, with ACT-03a/b/c) | Later epics |
|--------------------------------------|-------------|
| Strip Aggregate highway host; markers visual-only | Highway reattach via Addresses SIP ([20](20-highway-reattach-on-adr-aggregates.md)) |
| `ADRNameAggregate` / Element / Aggregated + serialize + ForkFromVanilla | World **name** labels + forked pick |
| NameSystem + GetAddress prefer ADR | SIP / DefaultTool H1 |
| Road Grouping real membership writes | |
| Membership maintenance / gap-fill ([07](07-membership-maintenance-edge-cases.md)) | |

### World-drawing-first — completed

Tool scaffold + overlays + stubs + number cards landed against vanilla Aggregate. **Do not** invent a permanent preview-only membership format — switch data source to ADR (ACT-03a…ACT-05).

### Sequencing (updated 2026-09-10)

```text
0a–0b / T0–T3. Road Grouping tool + overlays + stubs + cards (SHIPPED)
ACT-01. Strip Aggregate highway host
ACT-02. Markers visual-only + WE placeholders
ACT-03a. ADR name-aggregate components + serialize
ACT-03b. Membership service CRUD
ACT-03c. ForkFromVanilla + ADR seeding
ACT-04. NameSystem / GetAddress / SIP seed prefer ADR
ACT-05. Road Grouping mutations → ADR
ACT-06. Membership maintenance (remap + gap-fill)
Later: highway reattach via Addresses SIP ([20](20-highway-reattach-on-adr-aggregates.md) / [21](21-highway-reattach-roadmap.md)); world labels; fixedAggregation cleanup

Not in this wave: SIP / DefaultTool H1
```

### What changes vs earlier epic list ([05](05-research-gaps-and-next.md))

| Earlier item | Impact |
|--------------|--------|
| Epic: Player association UX | **Shipped** as Road Grouping T0–T3; mutations in ACT-05 |
| Epic: Thin ADR name aggregation | **Superseded** by full activation ACT-03a…ACT-03c + ACT-04 |
| Polish: SIP stats + H1 | **Out of scope this wave** (research kept in [06](06-labels-vs-vanilla-sync-and-harmony.md)) |
| Epic / spike: ADR labels | Still after activation; do **not** confuse tool overlay with AggregateMesh-style name labels |
| SIP as editor or inspect | **Excluded** from current implementation |

### Relationship to H1 highlight plan ([06](06-labels-vs-vanilla-sync-and-harmony.md))

- **H1** (DefaultTool CreationDefinition list rewrite) is **future work only** — not scheduled with the tool.
- **Tool overlays** = the only selection/edit visualization in this wave.
- Do not implement H1 to ship the tool; do not try to make H1 implement node handles.

### Relationship to S1 / S2

- Tool focuses an **ADR name-aggregate** for editing (S2-like).
- Overlays can still show vanilla Aggregate context so players see vanilla vs ADR ownership.
- Label pick → ADR aggregate remains a later labels epic; **SIP selection models are unused this wave**.

## Risks / cost

| Risk | Mitigation |
|------|------------|
| World overlay systems are non-trivial | Mitigated — T0–T3 shipped route-ribbon overlays |
| Tool + maintenance + labels all draw nets | Clear layer ownership: tool preview vs persistent labels (no DefaultTool H1 this wave) |
| Node UX ambiguous | Endpoint LMB-drag + fixed pin shipped as stubs; real writes in ACT-05 |
| Backend before mutations | Full ADR components in ACT-03a…ACT-03c (not thin contract); see [12](12-activation-change-map.md) |

## Open questions (tool-specific) — settled

Superseded by **Road Grouping** specs [09](09-road-grouping-tool-spec.md), [10](10-road-grouping-overlay-render-system.md), [11](11-road-grouping-tool-roadmap.md). Product/code name is **Road Grouping** (`AdrRoadGrouping*`); this file keeps its historical filename.

| Former open question | Settled decision |
|----------------------|------------------|
| Gestures | Plain LMB = initial select only; **Ctrl+LMB** on **segment** = toggle (log stub); **LMB-drag** on **end node** = endpoint expand/shrink (log stub); RMB = cancel |
| Overlay tech | Dedicated `OverlayRenderSystem.Buffer` / route-ribbon system — **not** `Highlighted`. Belonging base **#008800**, highlights / would-add **#00BB00**, non-belonging / pick preview **#004400**, would-remove **#FF8800**, end-handle hover **#00FF00** |
| Multi-select | **Single** focused aggregate/group per session |
| Toolbar / activation | WE-style `Select` / `enableTool` + ADR UI / optional keybind; VUIO tool-options when `K45_ADR_AdrRoadGroupingTool` active |
| Endpoint handles in first stub wave | **Yes** — end circles + LMB-drag path stubs in T1–T2 ([11](11-road-grouping-tool-roadmap.md)) |

## Relation

- [04](04-parallel-system-design-notes.md) — components / resolution  
- [05](05-research-gaps-and-next.md) — epic list (update reading: tool-first)  
- [06](06-labels-vs-vanilla-sync-and-harmony.md) — labels / future SIP+H1 reference (SIP **not** in this wave)  
- [07](07-membership-maintenance-edge-cases.md) — mutations the tool must obey  
- [09](09-road-grouping-tool-spec.md) — **Road Grouping** tool contract  
- [10](10-road-grouping-overlay-render-system.md) — overlay system  
- [11](11-road-grouping-tool-roadmap.md) — T0–T3 shipped; activation ACT-03a…ACT-06
- [12](12-activation-change-map.md) — activation file map
- [13](13-activation-sprint-roadmap.md) — one sprint + Kanban catalog (docs only)  
