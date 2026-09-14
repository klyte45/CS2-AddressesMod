**Start time:** 2026-09-14 17:22 -0300
# [0083] HW-07-Regions-map-ADR-highwayDataId

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/20-highway-reattach-on-adr-aggregates.md §E Regions
RefsLibrary/2026090907_custom-road-name-aggregation/21-highway-reattach-roadmap.md HW-07
BelzontAdr/Systems/AdrRegionsSystem.cs
_Frontends/UI/k45_adr_vuio/src/settings/tabs/region/RegionEditor.tsx
Archive ACT-01 [0061] regions strip notes

## User Story

> Acting as **a player using the Region map / highway registry overlay**, I want **roads linked to registered highways via ADR aggregates to show up by highway id again**, so that I **registry associations are visible citywide after the ACT-01 Aggregate strip**.

---

## Background

Eighth task — REQUIRED in the same sprint (not optional). After ACT-01, AdrRegionsSystem stopped exposing Aggregate highwayId. Re-plumb Region map / hover / class using ADRNameAggregateHighwayData.highwayDataId on ADR-linked edges. Keep HighwayRegisterManagement registry UI.

---

## Definition of Ready (DoR)

- [ ] HW-01 component present
- [ ] Region editor + listHighways / curve overlay paths understood
- [ ] RefsLibrary 21 marks HW-07 required

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] AdrRegionsSystem (or successor) exposes highway association from ADR highwayDataId for linked edges
- [ ] Region map VUIO can class/hover by registry highway id for ADR-associated roads
- [ ] Roads without ADR highway id remain non-associated (net classification alone does not fake registry membership)
- [ ] Registry CRUD UI still works
- [ ] No Aggregate-hosted highwayDataId revived
- [ ] Compile + playtest Region map hover/class after assigning highway in SIP/tool

---

## Implementation Notes

1. Prefer ADRNameAggregated → ADRNameAggregateHighwayData over Aggregated → Aggregate
2. Restore any frontend bindings removed in ACT-01 that only need ADR-sourced ids
3. Coordinate dirty/refresh when SIP/tool changes highwayDataId
4. AdrRegionsSystem.AggregationData.highwayId restored: ResolveAdrHighwayId walks group edges via ADRNameAggregated → ADRNameAggregateHighwayData.highwayDataId (first HasHighwayRoute). Net-only highways stay default/empty — no Aggregate-hosted id.
5. RegionEditor paths again apply hwId_${highwayId} class for HighwayRegisterManagement hover highlight (.hwId_hover).
6. Map refresh after SIP/tool highway assign: use existing Region map Reload (listHighways re-reads ADR ids).
7. Awaiting user compile + playtest: assign registry highway on ADR SIP/tool, reload Region map, hover registry list entry highlights associated roads.

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Performance scanning all edges for highway ids | low | Reuse existing region curve grouping; only swap id source |
| Confusion between net isHighway flag and registry association | medium | UI copy / hover shows registry name when highwayDataId set |

---

## Related Tasks

### Depends on

- [0076] HW-01-ADRNameAggregateHighwayData-component-lifecycle

### Is dependent for

- [0084] HW-08-Smoke-acceptance-checklist
### Is related to

- [0061] ACT-01-Strip-Aggregate-highway-host

### Groups with

- HW-02-SIP-backend-isAdrAggregate-highway-bindings
- HW-03-VUIO-Addresses-SIP-highway-section

---

## Other Notes

Epic adr-hw-reattach. Size S. Order 8. Required — same sprint as HW-01…HW-08. Status N = ready for sprint after review.
