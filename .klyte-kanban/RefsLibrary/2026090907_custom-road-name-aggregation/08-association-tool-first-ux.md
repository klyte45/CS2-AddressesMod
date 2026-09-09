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

### Thin C# contract — what it actually is

Not “all backend.” A **minimal data + resolve API** so the tool and names have something real to talk to:

| In thin contract | Out (later epics) |
|------------------|-------------------|
| `ADRNameAggregate` tag entity | Full gap-fill / remap matrix ([07](07-membership-maintenance-edge-cases.md)) |
| `ADRNameAggregateElement` buffer (`m_Edge` + `preventPropagation` + `numberOverride*`) | Highway migrate |
| `ADRNameAggregated` on edges | World **name** labels + forked pick |
| Serialize those types | SIP / DefaultTool H1 |
| Create / destroy aggregate; add/remove member; set flags (C# API the tool calls) | Rich tool modes / polish |
| NameSystem + GetAddress prefer ADR when linked | |

Enough that: tool mutations persist, street name + house numbers follow ADR for associated edges.

## World-drawing-first (valid learning path)

If the unfamiliar risk is **world overlay drawing**, it is reasonable to **start there before** the thin contract — with a **stub data source**:

1. **Scaffold** association `ToolBaseSystem` / `IBelzontToolSystem` + toolbar entry + net-only raycast.  
2. **Draw overlays from vanilla only** — e.g. hover/focus edge → highlight that edge’s vanilla `AggregateElement` chain; optional second color for “focused” vs “hover.”  
3. **Node handle positions** at edge endpoints (draw only; clicks can no-op or log).  
4. **Then** add thin contract and switch overlay input from vanilla Aggregate → `ADRNameAggregateElement` (+ keep vanilla context layer).

That does **not** replace the thin contract; it **reorders** learning: drawing tech → wire real membership → mutations → maintenance.

Do **not** invent a permanent parallel “preview-only membership” format — stub against vanilla Aggregates, then replace the data source.

### Sequencing (updated)

```text
0a. (Optional learning) Tool scaffold + world overlays from vanilla AggregateElement
0b. Thin C# contract (types + serialize + member APIs + NameSystem/GetAddress)
1. Association tool — wire overlays to ADR membership; mutations + flags; node controls
2. Membership maintenance (remap + gap-fill); dogfood via tool
3. Highway migrate onto ADR aggregates
4. World name labels + forked RaycastLabelsJob (not tool overlay)
5. Cleanup fixedAggregation / ADREdgeData

Not in this wave: SIP / DefaultTool H1
```

### What changes vs earlier epic list ([05](05-research-gaps-and-next.md))

| Earlier item | Impact |
|--------------|--------|
| Epic: Player association UX | **Promoted to first playable epic**; concrete form = **net-only tool + world draw + node controls** |
| Epic: ADR name aggregation | Still first **code**, but **minimal** — enough for tool commands + name/number resolve |
| Polish: SIP stats + H1 | **Out of scope this wave** (research kept in [06](06-labels-vs-vanilla-sync-and-harmony.md) for later) |
| Epic / spike: ADR labels | Still after tool; do **not** confuse tool overlay with AggregateMesh-style name labels |
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
| World overlay systems are non-trivial | Start with edge coloring via existing highlight/batch patterns or simple line overlays; node handles in a second tool iteration |
| Tool + maintenance + labels all draw nets | Clear layer ownership: tool preview vs persistent labels (no DefaultTool H1 this wave) |
| Node UX ambiguous | Spike one gesture (click adjacent edge from endpoint) before drag gizmos |
| Thin backend must exist first | Optional: world-draw spike on vanilla Aggregates first; thin contract before real membership edits |

## Open questions (tool-specific) — settled

Superseded by **Road Grouping** specs [09](09-road-grouping-tool-spec.md), [10](10-road-grouping-overlay-render-system.md), [11](11-road-grouping-tool-roadmap.md). Product/code name is **Road Grouping** (`AdrRoadGrouping*`); this file keeps its historical filename.

| Former open question | Settled decision |
|----------------------|------------------|
| Gestures | Plain LMB = initial select only; **Ctrl+LMB** on **segment** = toggle (log stub); **LMB-drag** on **end node** = endpoint expand/shrink (log stub); RMB = cancel |
| Overlay tech | Dedicated `OverlayRenderSystem.Buffer` system — **not** `Highlighted`. Belonging **#00FF00**, non-belonging **#006600** |
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
- [11](11-road-grouping-tool-roadmap.md) — implementation roadmap  
