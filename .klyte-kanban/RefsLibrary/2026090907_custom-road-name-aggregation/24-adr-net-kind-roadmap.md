# ADR net-kind — implementation roadmap & catalog

*Added: 2026-09-13 · Design: [23](23-adr-net-kind-gate-labels-seeds.md) · Research: [22](22-game-net-edge-kinds-and-aggregation.md) · Related: highway [21](21-highway-reattach-roadmap.md), labels [18](18-label-takeover-sprint-roadmap.md)*

## Goal

Ordered implementation slices for prefab-first **net kind**, **cross-kind membership gate**, **per-kind NetNameData**, and **road-only seeds/namesets**.

Board items are **catalog only** here. Do **not** create NET-* tasks via MCP until explicitly requested. Existing sprint spill **0085** moves to **backlog** and is reshaped as the **NET-04** seed slice (depends on NET-01…03 conceptually).

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
| 3 | NET-03 | *(create when asked)* | Per-kind ADR NetNameData prefabs + label ensure | M | NET-01, LBL-02+ | Non-road S2 labels resolve materials; no vanilla AggregateNet mutation |
| 4 | NET-04 | **0085** (backlog) | Road-only seeds / namesets / SIP DTO | M | NET-01 (prefer NET-02) | `GetAggregateName` road path only for `RoadData` kind; no TrackData fake; seed UI only for Road; **not** `HasComponent<Road>` |
| 5 | NET-05 | *(create when asked)* | Playtest matrix + smoke notes | S | NET-02…04 | Street without edge `Road` still Road kind; street↔highway mix OK; pathway/track reject; labels on non-road if in scope |

### NET-04 / board **0085**

Former in-sprint **HW-03c** was too narrow (seed-only) and incorrectly leaned on edge `Road`. After docs:

- **Location:** backlog (out of Sprint 014 until NET-01…03 ready to pull with it)
- **Role:** implements road-only seed/nameset/SIP gating (= NET-04)
- **DoD must require** prefab `RoadData` / `AdrNetKind.Road`, mix-gate awareness, and coordination with NetNameData work (NET-03) even if NET-03 is a separate board item

## Suggested pull order

```text
NET-01 → NET-02 → NET-03
                ↘ NET-04 (0085)
                     → NET-05
```

May land NET-04 after NET-01 alone if seed bugs block playtest, but membership mix bugs remain until NET-02.

## Relation to Sprint 014 (highway reattach)

| Board | Note |
|-------|------|
| **0079** HW-03b | Stays in sprint; seed UI remains; kind gate deferred |
| **0085** | **Backlog**; not Sprint 014 DoD for HW-08 |
| **0080–0084** | Highway work; when touching naming, respect Road kind once NET-01/04 land |

## Success criteria (when NET catalog ships)

- [ ] Prefab-`RoadData` edges without edge `Road` classify as Road and can use seeds
- [ ] Street ↔ highway ADR membership allowed
- [ ] Road ↔ track/pathway/waterway/taxiway membership rejected
- [ ] Non-road aggregates do not get road namesets / seed dice
- [ ] Non-road S2 labels work when NetNameData kind prefabs exist
- [ ] Vanilla AggregateSystem unchanged
