# ADR net-kind gate, labels, and seeds

*Added: 2026-09-13 · Research: [22](22-game-net-edge-kinds-and-aggregation.md) · Roadmap: [24](24-adr-net-kind-roadmap.md) · Highway: [20](20-highway-reattach-on-adr-aggregates.md) · Labels: [15](15-adr-citywide-name-label-takeover.md)*

## Goal

Lock product/design for:

1. **`AdrNetKind`** on ADR name-aggregates (prefab-first classifier)
2. **No cross-kind membership** in ADR / Road Grouping
3. **Per-kind `NetNameData`** for S2 world labels when vanilla AggregateNet lacks `NetLabel`
4. **Road nameset / seed UI only** for vehicle-road aggregates (`RoadData`)

This doc is **design only**. Catalog: [24](24-adr-net-kind-roadmap.md). Board: backlog task **0085** reshaped as NET-04 slice; full NET-* board create only when explicitly requested.

## Locked decisions

| Topic | Choice |
|-------|--------|
| Classifier | **Prefab `*Data` first** (`RoadData`, `TrackData`, `PathwayData`, `WaterwayData`, `TaxiwayData`) |
| Edge tags | Secondary / corroborating only; **`Game.Net.Road` unreliable** ([22](22-game-net-edge-kinds-and-aggregation.md)) |
| Street ↔ highway | **Same** `AdrNetKind.Road`; **may mix** |
| Public transport road | Stay under `AdrNetKind.Road` (`RoadType.PublicTransport` / `Layer.PublicTransportRoad`) |
| Cross-kind | **Reject** add/steal into wrong-kind ADR aggregate |
| Stored kind | Persist `AdrNetKind` on `ADRNameAggregate` at create/fork |
| Vanilla `AggregateSystem` | **Unchanged** |
| ADR vs vanilla AggregateType | ADR may join different road AggregateNet assets **within** `AdrNetKind.Road` when the player does |
| Seeds / namesets | Road nameset + road prefix + seed UX **only** for `AdrNetKind.Road` |
| Other kinds naming | Custom name + vanilla NameSystem this wave; per-kind namesets deferred |
| Highway (HW-*) | Road aggregates only; `UseHighwayRules` is subtype hint, not a separate kind |
| NetNameData | **ADR-owned** AggregateNet-like prefab entities (one per kind); do **not** mutate vanilla AggregateNet assets |
| Fence / pipe / power | Out of ADR grouping scope |

## `AdrNetKind` (proposed)

```text
None = 0
Road
Pathway
Train
Tram
Subway
TrainTram   // dual TrackTypes / both edge tags — exact fingerprint match
Waterway
Taxiway
```

Compatibility = **exact enum equality** (no “any track”). Dual Train+Tram is its own kind until product splits otherwise.

### Resolve from edge

1. Read `PrefabRef` on edge.
2. If prefab has `RoadData` → `Road` (ignore presence/absence of edge `Road`).
3. Else if `PathwayData` → `Pathway`.
4. Else if `TrackData` → map `m_TrackType` flags to Train / Tram / Subway / TrainTram.
5. Else if `WaterwayData` → `Waterway`.
6. Else if `TaxiwayData` → `Taxiway`.
7. Else → reject for ADR membership (out of scope).

Optional corroboration: edge tags, `NetData.m_RequiredLayers` — never override prefab `RoadData`.

### Persist on host

On first member / fork: set stored kind from that edge. Every later `AddMember` / tool attach / steal-in must match. Empty aggregate destroy clears kind with the entity.

Serialize with ADR aggregate (same pass as highway data / membership).

## Membership / Road Grouping

| Operation | Rule |
|-----------|------|
| Create / fork | Kind from seed edges; all seed edges must share one kind |
| Add member | Reject if edge kind ≠ stored kind |
| Steal from other ADR | Target and edge kinds must match; source leftover unchanged |
| Tool UX | Soft-fail / no-op with clear feedback (no silent coerce) |

Vanilla Aggregate remapping stays vanilla. ADR gate is parallel and **stricter on kind** while **looser than AggregateType** within `Road`.

## NetNameData strategy

Problem: S2 label owners need `PrefabRef` → AggregateNet with `NetNameData`. Non-road AggregateNets often lack `NetLabel` ([22](22-game-net-edge-kinds-and-aggregation.md)).

**Approach:**

1. At ADR label-system init (or first ensure), create/register **one ADR AggregateNet-like prefab entity per `AdrNetKind`** with `NetNameData` (+ colors; material index via existing `AdrRoadNameLabelMeshSystem` init path).
2. `EnsureRoadNameLabelComponents` (rename later if needed): if vanilla AggregateNet source has `NetNameData`, may keep copying that prefab for parity; if **missing**, point owner `PrefabRef` at the **ADR kind prefab**.
3. Do **not** inject `NetNameData` onto vanilla AggregateNet assets.

Road kind can continue using vanilla AggregateNet materials when present; non-roads use ADR kind prefabs.

## Seeds / namesets / highway

| Kind | Seed / nameset UI | `GetAggregateName` road path | Highway SIP/tool |
|------|-------------------|------------------------------|------------------|
| Road | Yes (existing road nameset) | Yes — **no** TrackData→RoadData fake | Yes (HW-* when ADR) |
| Pathway / Track* / Waterway / Taxiway | No road seed | Return vanilla (shall-run-original) | No |

SIP `TryAppendAggregateSeedOption`: set `hasCustomNameList` only when kind is Road **and** road nameset resolves. Prefer omitting misleading road-seed block for non-roads (custom name still available).

Replace any `HasComponent<Road>` gates with prefab-`RoadData` / stored `AdrNetKind`.

## Touch map

| Area | Change |
|------|--------|
| New helper | `AdrNetKind` + `TryGetNetKind(Entity edge)` |
| `ADRNameAggregate` (+ serialize) | Stored kind field |
| `AdrNameAggregateMembershipSystem` | Gate `AddMemberCore` / create / fork |
| `AdrRoadGroupingTool` | Respect membership reject; UX feedback |
| `AdrNameSystemOverrides.GetAggregateName` | Road kind only; drop TrackData fake for road namesets |
| `AdrSelectionInfoPanelSystem` | Seed DTO / member walks via kind or `RoadData` |
| `AdrRoadNameLabelMeshSystem` | Per-kind NetNameData prefabs + ensure PrefabRef |
| Frontends | Seed hidden when backend says no custom list / no road aggregation seed |

## Relation to other epics

| Epic / task | Relation |
|-------------|----------|
| HW-03b **0079** | Tool seed UI stays; kind gate filters when seed appears |
| Backlog **0085** (was HW-03c) | Absorbed as **NET-04** (road-only seeds) after NET-01…03; deferred from Sprint 014 |
| HW-04…HW-08 | Highway naming/addressing only meaningful for `AdrNetKind.Road` |
| Labels LBL-* | Extend ensure path; non-road labels need NET-03 NetNameData |

## Risks

| Risk | Mitigation |
|------|------------|
| Dual Train\|Tram edges | Distinct `TrainTram` kind; playtest; reject if policy changes |
| Existing ADR mixes already cross-kind | NET playtest: detect / refuse further adds; optional repair later |
| Wrong PrefabRef breaks road label look | Prefer vanilla AggregateNet when it has NetNameData |
| Merge conflict with HW-04 in `AdrNameSystemOverrides` | Land kind early-out before highway branch |
