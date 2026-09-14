# [0088] NET-02-Membership-Road-Grouping-mix-gate

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/23-adr-net-kind-gate-labels-seeds.md
RefsLibrary/2026090907_custom-road-name-aggregation/24-adr-net-kind-roadmap.md (NET-02)
BelzontAdr/Systems/AdrNameAggregateMembershipSystem.cs
BelzontAdr/Tools/AdrRoadGroupingTool.cs

## User Story

> Acting as **a player using Road Grouping**, I want **ADR membership to reject cross-kind nets while still allowing street and highway roads together**, so that I **ADR associations stay coherent for naming, seeds, and labels without fighting AggregateSystem**.

---

## Background

Second task of epic adr-net-kind. Enforce exact AdrNetKind match on membership add/steal and Road Grouping tool gestures. Soft-fail with clear UX feedback — no silent coerce. Street↔highway stays allowed (both Road). Vanilla AggregateSystem unchanged.

---

## Definition of Ready (DoR)

- [ ] NET-01 [0087] classifier + stored kind landed
- [ ] Road Grouping mutation path (ACT-05) and membership CRUD understood
- [ ] RefsLibrary 23 membership/tool table agreed

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] AddMemberCore (and steal-in) rejects when edge kind ≠ stored aggregate kind
- [ ] Create/fork with multiple seed edges requires all seeds share one kind
- [ ] Road Grouping tool soft-fails / no-ops on cross-kind attach with player-visible feedback
- [ ] Street↔highway membership still allowed (both AdrNetKind.Road)
- [ ] Vanilla AggregateSystem / AggregateType join rules unchanged
- [ ] Compile + playtest: road+pathway reject; street+highway accept

---

## Implementation Notes

1. Gate AdrNameAggregateMembershipSystem AddMember / steal paths with TryGetNetKind vs stored kind
2. Tool: AdrRoadGroupingTool respect membership reject; toast/log/overlay feedback — no silent success
3. Do not invent TrainTram super-kind; exact enum equality only
4. Fence/pipe/power remain out of scope (TryGetNetKind fails → reject)
5. Catalog: 24-adr-net-kind-roadmap.md NET-02

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Pre-existing cross-kind ADR mixes from before gate | medium | NET-05 detect; refuse further adds; optional repair later |
| Tool UX silent no-op confuses players | medium | Soft-fail DoD; mirror other tool reject feedback patterns |

---

## Related Tasks

### Depends on

- [0087] NET-01-Classifier-stored-kind-on-ADRNameAggregate

### Is dependent for

- [0086] NET-04-Road-only-seeds-namesets-prefab-RoadData

### Is related to

- [0067] ACT-05-Road-Grouping-ADR-mutations

### Groups with

- NET-01-Classifier-stored-kind-on-ADRNameAggregate
- NET-03-Per-kind-ADR-NetNameData-outline-by-kind
- NET-04-Road-only-seeds-namesets-prefab-RoadData
- NET-05-Playtest-matrix-smoke-notes

## Other Notes

Epic adr-net-kind. Size M. Order 2. Depends NET-01 [0087].
