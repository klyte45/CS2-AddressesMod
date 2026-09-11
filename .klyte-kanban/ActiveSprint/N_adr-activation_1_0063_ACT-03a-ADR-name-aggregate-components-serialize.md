# [0063] ACT-03a-ADR-name-aggregate-components-serialize

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/12-activation-change-map.md §A1
RefsLibrary/2026090907_custom-road-name-aggregation/04-parallel-system-design-notes.md
RefsLibrary/2026090907_custom-road-name-aggregation/13-activation-sprint-roadmap.md ACT-03a

## User Story

> Acting as **a Cities: Skylines 2 mod developer**, I want **ADRNameAggregate, ADRNameAggregateElement, and ADRNameAggregated defined, serialized, and registered**, so that I **later membership, fork, naming, and tool tasks have a stable ADR data contract**.

---

## Background

Workstream 3a (split from former ACT-03). Introduce ADR-owned name-aggregate ECS types only — no membership gameplay, no fork. Seed remains on co-located ADRRandomizationData, not fields on the tag. Do not revive ADREdgeData.fixedAggregation.

---

## Definition of Ready (DoR)

- [ ] ACT-01 complete or Aggregate highway host no longer blocking parallel types
- [ ] RefsLibrary 04 membership fields settled; 12 §A1 catalog agreed

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] ADRNameAggregate tag component exists on ADR aggregate entities
- [ ] ADRNameAggregateElement buffer entry fields exist: m_Edge (Entity), preventPropagation (bool default false), numberOverrideActive (bool default false), numberOverride (int default 0)
- [ ] ADRNameAggregated edge component exists with link to ADR aggregate entity and mirrored flags where useful for queries
- [ ] All three implement ISerializable and are registered like other ADR components
- [ ] Save/load round-trip of empty/populated buffers works in a minimal smoke test
- [ ] No Road Grouping mutation or naming preference yet (those are later ACT tasks)

---

## Implementation Notes

1. New file BelzontAdr/Components/ADRNameAggregate.cs — tag only (parallel to vanilla Aggregate)
2. New ADRNameAggregateElement buffer: m_Edge, preventPropagation, numberOverrideActive, numberOverride
3. New ADRNameAggregated on edge: ADR aggregate Entity link + mirror preventPropagation / numberOverrideActive / numberOverride as useful
4. ISerializable + component registration matching existing ADR patterns
5. Do not use ADREdgeData.fixedAggregation as the link field
6. No create/destroy service yet — that is ACT-03b

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Serialization versioning mistakes on new buffers | low | Register and version like existing ADR ISerializable components; smoke round-trip early |

---

## Related Tasks

### Depends on

- [0061] ACT-01-Strip-Aggregate-highway-host

### Is dependent for



### Groups with

- ACT-03b
- ACT-03c

## Other Notes

First of three ACT-03 split tasks. Depends on ACT-01 only (can proceed in parallel with ACT-02).
