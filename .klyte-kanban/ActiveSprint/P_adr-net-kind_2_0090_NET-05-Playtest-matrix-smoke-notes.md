**Start time:** 2026-09-14 23:02 -0300
# [0090] NET-05-Playtest-matrix-smoke-notes

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/24-adr-net-kind-roadmap.md (NET-05 success criteria)
RefsLibrary/2026090907_custom-road-name-aggregation/23-adr-net-kind-gate-labels-seeds.md

## User Story

> Acting as **a Cities: Skylines 2 mod developer**, I want **a written playtest matrix proving net-kind gates, seeds, and labels behave as designed**, so that I **the adr-net-kind epic can close with confidence against the locked design**.

---

## Background

Final catalog task of epic adr-net-kind. Dogfood the playtest matrix from RefsLibrary 24 success criteria after NET-02…04 land. Document smoke notes; no new product features.

---

## Definition of Ready (DoR)

- [ ] NET-02 [0088], NET-03 [0089], NET-04 [0086] done or intentionally scoped for the matrix
- [ ] RefsLibrary 24 success criteria list available

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Street/avenue edges without edge Game.Net.Road classify as Road and can use seeds
- [ ] Street↔highway ADR membership allowed in playtest
- [ ] Road↔pathway/track/waterway/taxiway membership rejected with feedback
- [ ] Non-road aggregates do not show road seed dice / road namesets
- [ ] Non-road S2 labels work when NET-03 in scope; outlines match kind table
- [ ] ADR Road labels use outline #333333
- [ ] Vanilla AggregateSystem unchanged (verified by no Harmony on AggregateSystem for this epic)
- [ ] Written smoke notes attached to this task or RefsLibrary pointer

---

## Implementation Notes

1. Playtest matrix: street without Road; street+highway mix; pathway/track reject; seed UI on/off by kind; label outlines
2. Capture findings in task notes / short smoke doc — do not invent new board epics here
3. Catalog: 24-adr-net-kind-roadmap.md NET-05

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Matrix finds edge cases outside locked design | medium | List failures on this task; open follow-up only if product asks |

---

## Related Tasks

### Depends on

- [0088] NET-02-Membership-Road-Grouping-mix-gate
- [0089] NET-03-Per-kind-ADR-NetNameData-outline-by-kind
- [0086] NET-04-Road-only-seeds-namesets-prefab-RoadData

### Is dependent for



### Groups with

- NET-01-Classifier-stored-kind-on-ADRNameAggregate
- NET-02-Membership-Road-Grouping-mix-gate
- NET-03-Per-kind-ADR-NetNameData-outline-by-kind
- NET-04-Road-only-seeds-namesets-prefab-RoadData

## Other Notes

Epic adr-net-kind. Size S. Order 5. Pull last after NET-02…04.
