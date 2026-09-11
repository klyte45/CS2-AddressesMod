# [0067] ACT-05-Road-Grouping-ADR-mutations

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/12-activation-change-map.md §E
RefsLibrary/2026090907_custom-road-name-aggregation/09-road-grouping-tool-spec.md
RefsLibrary/2026090907_custom-road-name-aggregation/13-activation-sprint-roadmap.md ACT-05

## User Story

> Acting as **a Cities: Skylines 2 player using Road Grouping**, I want **Ctrl+LMB and endpoint-drag to write real ADR membership with overlays and cards reflecting ADR**, so that I **I can build custom shared street names and number sequences from the world tool**.

---

## Background

Workstream 5. Wire shipped Road Grouping T0-T3 to real ADR membership. Discover via vanilla Aggregated; on first would-add/remove apply call ForkFromVanilla and set SelectedAggregation to ADR; thereafter read/write ADR buffer. Retire T2 log stubs. Supersedes backlog thin-contract T4/T5 (0058/0059).

---

## Definition of Ready (DoR)

- [ ] ACT-03c ForkFromVanilla + ACT-04 naming preference done
- [ ] Road Grouping T0-T3 shipped and stable
- [ ] RefsLibrary 12 §E and 09 tool spec agreed

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] AdrRoadGroupingTool discovers selection via vanilla Aggregated but membership source is ADR after fork
- [ ] First Ctrl+LMB apply or endpoint-drag release that would change membership calls ForkFromVanilla; SelectedAggregation becomes ADR entity
- [ ] Subsequent toggle/drag writes ADR membership via ACT-03b APIs; log stubs retired
- [ ] Fixed pin remains session behavior (or later flag) without mutating vanilla Aggregate membership
- [ ] AdrRoadGroupingController segment cards built from ADRNameAggregateElement; invalidate on real membership change
- [ ] Overlay: belonging = ADR members; remainder = vanilla Aggregate edges not in ADR; empty-selection pick-preview unchanged
- [ ] VUIO entity Index/Version still valid after fork (opaque entity identity)
- [ ] Vanilla Aggregate membership list is never mutated by the tool

---

## Implementation Notes

1. AdrRoadGroupingTool: replace stub logs with membership service calls
2. On first edit: ForkFromVanilla → SelectedAggregation = ADR
3. Controller cards from ADR elements; refresh on membership events
4. Overlay belonging/remainder from ADR vs vanilla Aggregate set difference
5. Verify VUIO Index/Version after entity swap on fork
6. Cancel/supersede backlog 0058 and 0059 if still open

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| VUIO holds stale Index/Version after fork | medium | Keep selectedAggregation Entity opaque; re-bind Index/Version after fork; add VUIO smoke check |
| Accidentally mutate vanilla Aggregate membership | low | Code review: tool never writes Aggregated.m_Aggregate / AggregateElement |

---

## Related Tasks

### Depends on

- [0065] ACT-03c-ForkFromVanilla-ADR-seeding
- [0066] ACT-04-Prefer-ADR-in-name-address-SIP

### Is dependent for



### Is related to

- [0058] T4-Road-Grouping-rewire-selectionoverlay-to-ADR-thin-contract
- [0059] T5-Road-Grouping-real-membership-mutations-via-tool-gestures

## Other Notes

Depends on ACT-03c and ACT-04. Maintenance gap-fill is ACT-06.
