**Start time:** 2026-09-11 22:05 -0300
# [0067] ACT-05-Road-Grouping-ADR-mutations

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
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

- [ ] AdrRoadGroupingTool discovers via vanilla Aggregated; selection always resolves to ADR (existing link or TryEnsureAdrForEdge / TryForkFromVanillaSeed on click) — never keeps vanilla Aggregate selected
- [ ] Membership edits (Ctrl+toggle / endpoint-drag) write ADR via membership APIs; vanilla AggregateElement is never mutated
- [ ] Fork seed snapshots AggregateElement edges before structural changes so full vanilla remainder is copied (not a single-edge ADR)
- [ ] Custom-name transfer on fork is deferred to MainLoop (NameSystem EndFrameBarrier)
- [ ] Fixed pin remains session-only
- [ ] AdrRoadGroupingController segment cards from ADRNameAggregateElement; invalidate on membership change
- [ ] Overlay: belonging = ADR members when selected; pick-preview prefers ADR membership when hovering ADR-linked edges; no dark vanilla-remainder wash while editing ADR
- [ ] Steal: adding an ADR-owned edge moves it to the focused ADR and resorts both aggregates
- [ ] VUIO entity Index/Version valid after fork

---

## Implementation Notes

1. Select materializes ADR via TryEnsureAdrForEdge / TryForkFromVanillaSeed
2. Snapshot vanilla edges before CreateEntity/EnsureNameDirty when seeding
3. Deferred SetCustomName queue on AdrNameAggregateMembershipSystem MainLoop
4. Overlay pick-preview ADR-aware; removed BuildVanillaRemainderOverlay from edit path
5. AddMember steals from other ADR aggregates and resorts source+target

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
