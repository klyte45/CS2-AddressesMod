**Start time:** 2026-09-13 14:40 -0300
# [0080] HW-04-Naming-highway-override-branch

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/20-highway-reattach-on-adr-aggregates.md §E
RefsLibrary/2026090907_custom-road-name-aggregation/21-highway-reattach-roadmap.md HW-04
BelzontAdr/Overrides/AdrNameSystemOverrides.cs
Archive ACT-01 [0061] stripped Aggregate highway name branch

## User Story

> Acting as **a player who assigned a registered highway to an ADR grouping**, I want **road names to follow the highway registry naming rules**, so that I **highway routes show consistent prefix/suffix/name without Aggregate-hosted data**.

---

## Background

Fifth task. Restore AdrNameSystemOverrides highway name branch keyed by ADRNameAggregateHighwayData.highwayDataId + HighwayData registry (not vanilla Aggregate). Custom name from SIP/tool wins when set (same priority as existing custom-name rules).

---

## Definition of Ready (DoR)

- [ ] HW-01 component present
- [ ] Highway registry CRUD still works
- [ ] ACT-04 Prefer ADR naming baseline understood

---

## Acceptance Criteria / Definition of Done (DoD)

- [x] Non-default highwayDataId on ADR → registry highway naming for ADR-linked edges
- [x] default highwayDataId → no highway name branch (nameset/vanilla/custom as today)
- [x] Custom name when set wins over highway/nameset (document/test priority)
- [x] Vanilla Aggregate-only roads unchanged
- [x] Name dirty/refresh after SIP/tool highway or custom-name change
- [x] Compile + playtest rename/highway assign

---

## Implementation Notes

1. Reintroduce GetAggregateName-style highway branch on ADR entity path only
2. Resolve via ADRNameAggregated / ADRNameAggregate — never Aggregate ADRHighwayAggregationData
3. Use AdrHighwayRoutesSystem.TryGetHighwayData
4. Coordinate with HW-03b custom name field
5. Restored highway naming in AdrNameSystemOverrides keyed by ADRNameAggregate + ADRNameAggregateHighwayData.highwayDataId + AdrHighwayRoutesSystem.TryGetHighwayData (not Aggregate).
6. GetAggregateName priority documented: (1) custom name → original NameSystem path, (2) non-default highwayDataId → registry hwData.name, (3) nameset + road-prefix.
7. GetRenderedLabelName: ADR highway shows yellow prefix-suffix; custom name wins name line when set (pre-ACT-01 parity).
8. TryGetAdrHighwayRegistryName helper; m_adrHighwayRoutesSystem wired in DoPatches. Dirty already via DirtyAdrAggregateConsumers → MarkRoadsDirty (HW-02).
9. Awaiting user solution compile + playtest rename/highway assign before DoD check / T.

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Highway name overrides custom name unexpectedly | medium | Explicit priority in DoD and code comments |
| Stale labels after highway assign | medium | Dirty via same path as nameset seed change; labels epic already citywide |

---

## Related Tasks

### Depends on

- [0076] HW-01-ADRNameAggregateHighwayData-component-lifecycle

### Is dependent for

- [0084] HW-08-Smoke-acceptance-checklist
### Is related to

- [0066] ACT-04-Prefer-ADR-in-name-address-SIP
- [0070] LBL-01-Shutdown-vanilla-name-path

### Groups with

- HW-05-Mileage-addressing-authored-fields

---

## Other Notes

Epic adr-hw-reattach. Size M. Order 5. Status N = ready for sprint after review.
