# ADR net-kind gate, labels, and seeds

*Added: 2026-09-13 · Research: [22](22-game-net-edge-kinds-and-aggregation.md) · Roadmap: [24](24-adr-net-kind-roadmap.md) · Highway: [20](20-highway-reattach-on-adr-aggregates.md) · Labels: [15](15-adr-citywide-name-label-takeover.md)*

## Goal

Lock product/design for:

1. **`AdrNetKind`** on ADR name-aggregates (prefab-first classifier)
2. **No cross-kind membership** in ADR / Road Grouping
3. **Per-kind `NetNameData`** for S2 world labels when vanilla AggregateNet lacks `NetLabel`
4. **Road nameset / seed UI only** for vehicle-road aggregates (`RoadData`)

This doc is **design only**. Catalog: [24](24-adr-net-kind-roadmap.md). Board: backlog **[0086] NET-04** (continuation of Z-overflowed **0085**); full NET-* board create only when explicitly requested.

## Locked decisions

| Topic | Choice |
|-------|--------|
| Classifier | **Prefab `*Data` first** (`RoadData`, `TrackData`, `PathwayData`, `WaterwayData`, `TaxiwayData`) |
| Dual Train\|Tram | Maps to **`AdrNetKind.Train`** (not a separate kind) |
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
| NetNameData | **ADR-owned** prefabs for **all kinds except Road** (Road already has vanilla `NetLabel`); differentiate by **outline** (face `#FFFFFFC0`); Subway inherits former ADR `#004400`; Road outline `#333333`; see color table below |
| Fence / pipe / power | Out of ADR grouping scope |

## `AdrNetKind` (proposed)

```text
None = 0
Road
Pathway
Train
Tram
Subway
Waterway
Taxiway
```

Compatibility = **exact enum equality** (no “any track”). There is **no** `TrainTram` kind.

**Track mapping:** `TrackTypes.Train` alone → `Train`. `TrackTypes.Tram` alone → `Tram`. `TrackTypes.Subway` alone → `Subway`. **`Train|Tram` (prefab and/or both edge tags) → `Train`** (settled — not a distinct case).

### Resolve from edge

1. Read `PrefabRef` on edge.
2. If prefab has `RoadData` → `Road` (ignore presence/absence of edge `Road`).
3. Else if `PathwayData` → `Pathway`.
4. Else if `TrackData` → map `m_TrackType`: Subway → `Subway`; Tram-only → `Tram`; Train or `Train|Tram` → `Train`.
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

**Approach** (same differentiation method as today’s ADR name labels: **outline color** on material clones, not different face glyphs):

1. Create/register **ADR AggregateNet-like prefab entities with `NetNameData` for every kind that lacks vanilla `NetLabel`** — i.e. **all kinds except Road** (Pathway, Train, Tram, Subway, Waterway, Taxiway). Do **not** mutate vanilla AggregateNet assets.
2. **Face / text color** for all kinds: **`#FFFFFFC0`** (RGBA) on `NetNameData.m_Color` / selected as appropriate.
3. **Outline** (`_OutlineColor` on ADR-style material clones — same pattern as current `m_IsAdrStyle` / `#004400` in `AdrRoadNameLabelMeshSystem`): **per `AdrNetKind`**, not a single ADR green.

| AdrNetKind | Outline | Notes |
|------------|---------|--------|
| **Subway** | `#004400` | **Former** single ADR outline (today’s `AdrOutlineColor`) |
| **Road** | `#333333` | Road usually already has vanilla `NetNameData`; still use this outline for ADR Road / S2 differentiation (replace the old one-size `#004400` for roads) |
| **Pathway** | `#666666` | ADR kind prefab + NetNameData required |
| **Train** | `#331A00` | Includes dual `Train\|Tram` → Train |
| **Tram** | `#330033` | Pure tram only |
| **Waterway** | `#002244` | ADR kind prefab + NetNameData required |
| **Taxiway** | `#33001A` | ADR kind prefab + NetNameData required |

4. `EnsureRoadNameLabelComponents` (rename later if needed):
   - Keep owner **`PrefabRef` on a real AggregateNet with `PrefabData`** (required for edit/highlight `CreationDefinition` — synthetic NetNameData-only entities hard-crash PrefabSystem).
   - ADR-owned NetNameData entities are **color metadata only** — never PrefabRef targets.
   - AggregateNets without `NetLabel`/`NetNameData` (train/pathway/…): borrow **`LabelMaterial`** index from baked AggregateNet name materials; outline still follows stored `AdrNetKind`.
   - Selection **Temp** Aggregates from unlabeled AggregateNets must still receive Label* (Ensure and/or CreationDefinition prefab with NetNameData) because the ADR original is `Hidden` on hover.
5. Generalize `m_IsAdrStyle` (bool) toward **per-kind outline** (enum / color lookup keyed by `AdrNetKind`) so Subway keeps dark green and Road uses `#333333`.

**NET-03** owns creating the non-road NetNameData prefabs + wiring outline-by-kind (shipped; see crash/hover notes above).

**NET-06** citywide bulk convert (settings): see [26](26-bulk-citywide-adr-aggregation.md).

## Seeds / namesets / highway

| Kind | Seed / nameset UI | `GetAggregateName` road path | Highway SIP/tool |
|------|-------------------|------------------------------|------------------|
| Road | Yes (existing road nameset) | Yes — **no** TrackData→RoadData fake | Yes (HW-* when ADR) |
| Pathway / Track* / Waterway / Taxiway | No road seed | Return vanilla (shall-run-original) | No |

SIP `TryAppendAggregateSeedOption`: set `hasCustomNameList` only when kind is Road **and** road nameset resolves. Prefer omitting misleading road-seed block for non-roads (custom name still available). Highway SIP/tool option only for Road kind.

Replace any `HasComponent<Road>` gates with prefab-`RoadData` / stored `AdrNetKind` (NET-04).

**NET-04 naming:** `GetAggregateName` applies road namesets/prefixes only for `AdrNetKind.Road`; drops TrackData→RoadData fake. Non-road aggregates use ADR locale defaults `K45::ADR.main[netKind.*]` (vanilla AggregateNet assets have no usable player-facing translation) — pending merge from `BelzontAdr/i18n/netKind_entries.txt`.

## Touch map

| Area | Change |
|------|--------|
| New helper | `AdrNetKind` + `TryGetNetKind(Entity edge)` |
| `ADRNameAggregate` (+ serialize) | Stored kind field |
| `AdrNameAggregateMembershipSystem` | Gate `AddMemberCore` / create / fork |
| `AdrRoadGroupingTool` | Respect membership reject; UX feedback |
| `AdrNameSystemOverrides.GetAggregateName` | Road kind only; drop TrackData fake for road namesets |
| `AdrSelectionInfoPanelSystem` | Seed DTO / member walks via kind or `RoadData` |
| `AdrRoadNameLabelMeshSystem` | Non-road NetNameData prefabs; face `#FFFFFFC0`; outline-by-kind (Subway `#004400`, Road `#333333`, …); ensure PrefabRef |
| Frontends | Seed hidden when backend says no custom list / no road aggregation seed |

## Relation to other epics

| Epic / task | Relation |
|-------------|----------|
| HW-03b **0079** | Tool seed UI stays; kind gate filters when seed appears |
| Backlog **0086** NET-04 (was HW-03c **0085** Z) | Road-only seeds after NET-01…03; deferred from Sprint 014 |
| HW-04…HW-08 | Highway naming/addressing only meaningful for `AdrNetKind.Road` |
| Labels LBL-* | Extend ensure path; non-road labels need NET-03 NetNameData |

## Risks

| Risk | Mitigation |
|------|------------|
| Dual Train\|Tram edges | Classified as **Train**; may mix with pure Train aggregates |
| Existing ADR mixes already cross-kind | NET playtest: detect / refuse further adds; optional repair later |
| Wrong PrefabRef breaks road label look | Prefer vanilla AggregateNet when it has NetNameData |
| Merge conflict with HW-04 in `AdrNameSystemOverrides` | Land kind early-out before highway branch |
