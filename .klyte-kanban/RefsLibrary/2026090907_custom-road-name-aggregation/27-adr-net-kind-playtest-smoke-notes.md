# ADR net-kind — playtest matrix smoke notes (NET-05)

*Sprint 015 · Board **[0090]** · Catalog [24](24-adr-net-kind-roadmap.md) · Design [23](23-adr-net-kind-gate-labels-seeds.md)*  
*Recorded: 2026-09-14 · Covers NET-01…04 ([0087][0088][0089][0086])*

## Scope

Written smoke evidence for the locked adr-net-kind design. **No new product features** in this task.

| Board | Slice | Status when smoked |
|-------|-------|--------------------|
| **0087** | NET-01 classifier + stored kind | T |
| **0088** | NET-02 membership / tool mix gate | T |
| **0089** | NET-03 per-kind NetNameData + outlines | T |
| **0086** | NET-04 road-only seeds / namesets | T |

## Matrix results

| # | Scenario | Expected | Result | Notes |
|---|----------|----------|--------|-------|
| 1 | Street/avenue edge **without** `Game.Net.Road` but with prefab `RoadData` | Classifies as `AdrNetKind.Road`; road seed / nameset available | **Pass** | NET-01/04; seed no longer uses `HasComponent<Road>` |
| 2 | Street ↔ highway on same ADR aggregate | Membership allowed (both Road) | **Pass** | NET-02 |
| 3 | Road ↔ pathway / train / tram / subway / waterway / taxiway | Reject with soft-fail UX (notification + Info log) | **Pass** | NET-02; hover unsupported = red `#FF0000` |
| 4 | Endpoint drag that would mix kinds / fail soft filter | Atomic abort — no partial add/remove | **Pass** | Soft-fail hardened during NET-02 validation |
| 5 | Non-road ADR (e.g. train) | No road seed dice; no road nameset naming | **Pass** | NET-04; `hasCustomNameList` only for Road |
| 6 | Non-road ADR default display name | ADR locale `K45::ADR.main[netKind.*]` (not vanilla AggregateNet asset id) | **Pass** | NET-04; i18n merged (`netKind` entries) |
| 7 | Non-road S2 world name labels | Draw with borrowed `LabelMaterial`; outline by kind | **Pass** | NET-03 |
| 8 | Hover / select non-road ADR | Labels do not vanish (Temp Aggregate Ensure / labeled CreationDefinition prefab) | **Pass** | NET-03 follow-up fix |
| 9 | Edit / select train track ADR | No PrefabSystem hard-crash | **Pass** | PrefabRef stays real AggregateNet with `PrefabData` |
| 10 | ADR Road S2 label outline | `#333333` (not former single green) | **Pass** | NET-03 color table |
| 11 | Subway / other non-road outlines | Match locked table in [23](23-adr-net-kind-gate-labels-seeds.md) | **Pass** | NET-03 (Subway `#004400`, …) |
| 12 | Highway SIP / tool option | Only on Road ADR | **Pass** | NET-04 gate |
| 13 | Unset highway on ADR Road | Clearing works (zero Hash128 sentinel) | **Pass** | Fixed during sprint validation (pre-NET-04) |
| 14 | Vanilla `AggregateSystem` | Untouched by this epic (no Harmony on AggregateSystem for net-kind) | **Pass** | By construction / code review |

## Out of matrix (deferred)

| Item | Where |
|------|--------|
| Citywide bulk convert (kind checkboxes) | NET-06 **[0091]** / [26](26-bulk-citywide-adr-aggregation.md) |
| Road Grouping tooltip polish | [25](25-road-grouping-tooltips.md) (locked; TT-01…TT-06) |

## Success criteria map ([24](24-adr-net-kind-roadmap.md))

- [x] Prefab-`RoadData` edges without edge `Road` classify as Road and can use seeds — #1
- [x] Street ↔ highway ADR membership allowed — #2
- [x] Road ↔ track/pathway/waterway/taxiway membership rejected — #3–4
- [x] Non-road aggregates do not get road namesets / seed dice — #5–6
- [x] Non-road S2 labels + kind outlines — #7–11
- [x] ADR Road outline `#333333` — #10
- [x] Vanilla AggregateSystem unchanged — #14
- [ ] Citywide bulk convert — NET-06

## Follow-ups (optional)

None blocking epic close for NET-01…05. Product may still pull **[0091] NET-06** separately.
