# [0072] LBL-03-ADR-placement-job-vanilla-ADR-membership

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/18-label-takeover-sprint-roadmap.md LBL-03
RefsLibrary/2026090907_custom-road-name-aggregation/15-adr-citywide-name-label-takeover.md
RefsLibrary/2026090907_custom-road-name-aggregation/05-adr-name-aggregates-and-vanilla.md
RefsLibrary/2026090907_custom-road-name-aggregation/14-vanilla-road-label-pipeline.md

## User Story

> Acting as **a player with ADR citywide name labels**, I want **label positions that follow S1 Aggregate or S2 ADR name-aggregate ownership**, so that I **names appear along roads for both ungrouped and Road-Grouping-associated stretches without double labels**.

---

## Background

Third task of epic adr-road-labels — largest placement piece. Dual membership: S1 = vanilla Aggregate (unassociated segments) for citywide coverage; S2 = ADR name-aggregate (associated / Road Grouping) when present. Place LabelPosition (and related placement buffers) along nets using ADR bake from LBL-02. Do not rewrite AggregateSystem membership. Phase-1 parity placement (spacing/height match vanilla feel).

---

## Definition of Ready (DoR)

- [ ] LBL-02 ADR TMP bake complete — LabelExtents available
- [ ] ACT-05/ACT-06 Road Grouping membership available for S2 owners (archived 0067/0068 preferred)
- [ ] RefsLibrary 15 S1/S2 pick/placement rules and 18 LBL-03 catalog agreed
- [ ] LBL-01 shutdown already live so vanilla placement is not competing

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Placement job/system walks nets and writes LabelPosition (and any required orientation/scale fields) for name labels
- [ ] S1: unassociated segments use vanilla Aggregate as name owner for placement identity
- [ ] S2: associated segments use ADR name-aggregate owner when Road Grouping / ADR association exists
- [ ] Owner switch S1↔S2 updates or rebuilds placement without leftover duplicate labels on same stretch
- [ ] Consumes LBL-02 LabelExtents so positions account for string width
- [ ] Smoke: mixed city (some grouped, some not) shows placements for both owners without double-stacking
- [ ] No AggregateSystem Burst patches
- [ ] Compile + load smoke on a dogfood save with Road Grouping associations
- [ ] Placement uses ~100f spacing, redundancy, m_MaxScale parity with vanilla AddLabels / UpdateLabelPositionsJob
- [ ] LabelPosition buffers written on the pick owner entity (S1 Aggregate or S2 ADR name-aggregate)
- [ ] Burst preferred for placement job

---

## Implementation Notes

1. Prefer ADR Burst/IJobEntity or managed system patterned on AggregateMeshSystem UpdateLabelPositions — reimplement, do not Prefix Burst AggregateSystem
2. Resolve owner: if ADR association → S2 name-aggregate entity; else S1 Aggregate entity
3. Query AggregateElement / edge membership the same sources Road Grouping and AggregateSystem already use — read-only
4. Spacing / curve sampling: match vanilla name label density for phase-1; document any intentional deltas
5. Invalidate on net geometry change, rename, association change
6. Keep arrow positions entirely in vanilla AggregateMeshSystem
7. Port/adapt UpdateLabelPositionsJob / AddLabels (100f spacing, redundancy, m_MaxScale)
8. Dual source: unassociated → AggregateElement on Aggregate (S1); associated → ADRNameAggregateElement on ADR aggregate (S2)
9. Write LabelPosition on pick owner entity
10. Shared owner-resolution helper for LBL-05 pick
11. Schedule after net/association systems; invalidate on geometry/association/rename
12. Keep arrow positions in vanilla AggregateMeshSystem

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Double labels or gaps at S1/S2 boundaries | high | Owner identity tests: same stretch S1-only, S2-only, transition after associate/disassociate; assert single label family |
| Stale positions after geometry or membership edits | medium | Schedule after net/association systems; use change versions like vanilla |

---

## Related Tasks

### Depends on

- [0071] LBL-02-ADR-TMP-bake-LabelExtents-materials
- [0067] ACT-05-Road-Grouping-ADR-mutations
- [0068] ACT-06-Membership-maintenance-dogfood

### Is dependent for

- [0073] LBL-04-ADR-name-mesh-batch-DrawMesh
- [0074] LBL-05-Forked-pick-SIP-Aggregate-section-parity
- [0075] LBL-06-Integration-dogfood-game-update-checklist
### Is related to

- [0066] ACT-04-Prefer-ADR-in-name-address-SIP

### Groups with

- LBL-01-Shutdown-vanilla-name-path
- LBL-02-ADR-TMP-bake-LabelExtents-materials
- LBL-04-ADR-name-mesh-batch-DrawMesh
- LBL-05-Forked-pick-SIP-Aggregate-section-parity
- LBL-06-Integration-dogfood-game-update-checklist

---

## Other Notes

Epic adr-road-labels. Size L. Depends on LBL-02 [0071]. Unblocks LBL-04 mesh and LBL-05 pick.
