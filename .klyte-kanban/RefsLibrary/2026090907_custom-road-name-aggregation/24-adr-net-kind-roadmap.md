# ADR net-kind — implementation roadmap & catalog

*Added: 2026-09-13 · Design: [23](23-adr-net-kind-gate-labels-seeds.md) · Research: [22](22-game-net-edge-kinds-and-aggregation.md) · Related: highway [21](21-highway-reattach-roadmap.md), labels [18](18-label-takeover-sprint-roadmap.md)*

## Goal

Ordered implementation slices for prefab-first **net kind**, **cross-kind membership gate**, **per-kind NetNameData**, and **road-only seeds/namesets**.

Board items are **catalog only** here. Do **not** create NET-* tasks via MCP until explicitly requested (except **0086** NET-04 already on backlog as the deferred seed slice). Sprint spill **0085** is terminal **Z**; continuation is **0086**.

## Prerequisites

| Depends on | Why |
|------------|-----|
| Activation ACT-03a+ | ADR name-aggregates exist |
| Labels LBL-02+ (prefer done) | Label ensure / mesh path to extend for NetNameData |
| HW-03b optional | Tool seed surface already mounts; NET-04 gates it correctly |

## Scope

| In | Out |
|----|-----|
| `AdrNetKind` + prefab classifier | Splitting street vs highway kinds |
| Persist kind on ADR aggregate | Changing vanilla `AggregateSystem` |
| Membership / Road Grouping mix gate | Per-kind nameset libraries (beyond road) |
| ADR-owned NetNameData prefabs per kind | Mutating vanilla AggregateNet assets |
| Road-only seeds / `GetAggregateName` gate | Fence / pipeline / powerline grouping |
| Playtest matrix (street without edge `Road`, street↔highway mix, track reject) | Auto-creating full NET board epic this wave |

## Task catalog (draft)

| Order | ID | Board | Title | Size | Depends | Outcome |
|-------|-----|-------|-------|------|---------|---------|
| 1 | NET-01 | *(create when asked)* | Classifier + stored kind on `ADRNameAggregate` | M | ACT-03a+ | `TryGetNetKind` from prefab `*Data`; serialize kind; set on create/fork |
| 2 | NET-02 | *(create when asked)* | Membership + Road Grouping mix gate | M | NET-01 | `AddMemberCore` / tool reject cross-kind; UX feedback |
| 3 | NET-03 | **0089** T | Per-kind ADR NetNameData + outline-by-kind | M | NET-01, LBL-02+ | Create NetNameData for non-road kinds; face `#FFFFFFC0`; outlines: Subway `#004400`, Road `#333333`, Pathway `#666666`, Train `#331A00`, Tram `#330033`, Waterway `#002244`, Taxiway `#33001A`; generalize `m_IsAdrStyle` → kind outline; no vanilla AggregateNet mutation; PrefabRef stays real AggregateNet; LabelMaterial borrow + Temp Ensure for unlabeled kinds |
| 4 | NET-04 | **0086** P | Road-only seeds / namesets / SIP DTO | M | NET-01 (prefer NET-02) | `GetAggregateName` road path only for `RoadData` kind; no TrackData fake; seed UI only for Road; **not** `HasComponent<Road>` |
| 5 | NET-05 | **0090** P | Playtest matrix + smoke notes | S | NET-02…04 | Street without edge `Road` still Road kind; street↔highway mix OK; pathway/track reject; labels on non-road if in scope. Notes: [27](27-adr-net-kind-playtest-smoke-notes.md) |
| 6 | NET-06 | **0091** N | Citywide bulk ADR aggregation (kind checkboxes) | M | NET-01…03 | Settings button on Roads and Districts above per-district road namesets; confirm dialog with kind checkboxes; convert applicable segments to ADR. Spec: [26](26-bulk-citywide-adr-aggregation.md) |

### NET-04 / board **0086** (was **0085**)

Former in-sprint **HW-03c [0085]** was Z-overflowed mid-sprint (seed-only scope too narrow; edge-`Road` gating wrong). Continuation:

- **Board:** **0086** — backlog status **L** (`Backlog/L_adr-hw-reattach_2_0086_NET-04-Road-only-seeds-namesets-prefab-RoadData.md`)
- **Role:** implements road-only seed/nameset/SIP gating (= NET-04)
- **DoD:** prefab `RoadData` / `AdrNetKind.Road`, no `HasComponent<Road>` sole gate; coordinate with NET-01…03
- **0085:** remains terminal **Z** in ActiveSprint — do not pull; use **0086**

## Suggested pull order

```text
NET-01 → NET-02 → NET-03 → NET-06 (bulk citywide convert)
                ↘ NET-04 (0086)
                     → NET-05
```

May land NET-04 after NET-01 alone if seed bugs block playtest, but membership mix bugs remain until NET-02. NET-06 needs kind gate + labels (NET-01…03).

## Relation to Sprint 014 (highway reattach)

| Board | Note |
|-------|------|
| **0079** HW-03b | Stays in sprint; seed UI remains; kind gate deferred |
| **0085** | Terminal **Z** in ActiveSprint (overflowed); superseded by **0086** |
| **0086** NET-04 | **Backlog L**; not Sprint 014 DoD for HW-08 |
| **0080–0084** | Highway work; when touching naming, respect Road kind once NET-01/04 land |

## Success criteria (when NET catalog ships)

- [x] Prefab-`RoadData` edges without edge `Road` classify as Road and can use seeds — [27](27-adr-net-kind-playtest-smoke-notes.md)
- [x] Street ↔ highway ADR membership allowed — [27](27-adr-net-kind-playtest-smoke-notes.md)
- [x] Road ↔ track/pathway/waterway/taxiway membership rejected — [27](27-adr-net-kind-playtest-smoke-notes.md)
- [x] Non-road aggregates do not get road namesets / seed dice — [27](27-adr-net-kind-playtest-smoke-notes.md)
- [x] Non-road S2 labels work via ADR NetNameData prefabs; outlines match kind table in [23](23-adr-net-kind-gate-labels-seeds.md) (Subway = former `#004400`) — NET-03 **0089** / [27](27-adr-net-kind-playtest-smoke-notes.md)
- [x] ADR Road labels use outline `#333333` (not the old one-size green) — NET-03 **0089** / [27](27-adr-net-kind-playtest-smoke-notes.md)
- [x] Vanilla AggregateSystem unchanged — [27](27-adr-net-kind-playtest-smoke-notes.md)
- [ ] Citywide bulk convert (kind checkboxes) on Roads and Districts — NET-06 / [26](26-bulk-citywide-adr-aggregation.md)
