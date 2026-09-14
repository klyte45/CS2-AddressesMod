**End time:** 2026-09-13 02:50 -0300
**Start time:** 2026-09-13 01:41 -0300
# [0079] HW-03b-Road-Grouping-tool-options-parity

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/20-highway-reattach-on-adr-aggregates.md §D2
RefsLibrary/2026090907_custom-road-name-aggregation/21-highway-reattach-roadmap.md HW-03b
_Frontends/UI/k45_adr_vuio/src/tool/RoadGroupingToolOptions.tsx
BelzontAdr/Controllers/AdrRoadGroupingController.cs

## User Story

> Acting as **a player using the Road Grouping tool**, I want **to set custom name, seed, and highway settings on the selected ADR aggregate without opening SIP**, so that I **membership and naming/highway setup stay in one tool workflow**.

---

## Background

Fourth task. Expand RoadGroupingToolPanel: replace selected #Index display with custom name input; mount same SeedManagement + HighwayAggregateSettings as SIP against SelectedAggregation. Keep show-number-cards and pick-another. One write path with HW-02.

---

## Definition of Ready (DoR)

- [ ] HW-02 setters usable for SelectedAggregation entity
- [ ] HW-03 SIP highway + seed UI available to reuse or mirror
- [ ] Tool coerces selection to ADR after fork (ACT-05 behavior)

---

## Acceptance Criteria / Definition of Done (DoD)

- [x] Primary tool label is no longer Aggregation # / entity Index
- [x] Custom name text input binds to selected ADR aggregate NameSystem custom name
- [x] Empty/cleared custom name falls back to nameset/highway/generator naming
- [x] SeedManagement controls appear for ADR selection (parity with SIP)
- [x] HighwayAggregateSettings appear for ADR selection (parity with SIP)
- [x] Edits from tool and SIP stay consistent on the same entity
- [x] Existing actions remain: show number cards, pick another
- [x] i18n pending entries for any new tool strings
- [x] npm build + playtest tool panel with ADR selection

---

## Implementation Notes

1. Replace body string with custom name field + shared sections
2. Wire AdrRoadGroupingController binders or reuse SIP binders with SelectedAggregation
3. Before fork / non-ADR: keep pick prompt only — no highway block
4. Do not add highway controls into overlay cards — tool options panel only
5. RoadGroupingToolPanel: custom name StringInputField via get/setAggregateCustomName; SeedManagementOptions + HighwayAggregateSettings reuse SIP components against SelectedAggregation.
6. SeedManagementOptions seed ops target response.entityValue (optional seedEntity override) instead of SIP selectedEntity.
7. getEntityOptions: TryAppendAggregateSeedOption also runs under custom-name early return so tool/SIP keep seed for Aggregate/ADR hosts.
8. i18n pending BelzontAdr/i18n/road_grouping_tool_options_entries.txt (customName). npm run build OK.
9. Closed after tool parity implementation: custom name + SeedManagement + HighwayAggregateSettings on SelectedAggregation; seed ops via entityValue; pending i18n road_grouping_tool_options_entries.txt; non-road seed gate deferred to NET-04 (0086).

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Duplicate binder state between tool and SIP | medium | Single backend setters; UI reads from same bindings or refresh on select |
| Custom name fights highway name branch | medium | HW-04 documents priority: custom name wins when set |

---

## Related Tasks

### Depends on

- [0077] HW-02-SIP-backend-isAdrAggregate-highway-bindings
- [0078] HW-03-VUIO-Addresses-SIP-highway-section

### Is dependent for

- [0084] HW-08-Smoke-acceptance-checklist
### Is related to

- [0067] ACT-05-Road-Grouping-ADR-mutations
- [0085] HW-03c-Gate-road-seeds-namesets-to-roads-only
### Groups with

- HW-03-VUIO-Addresses-SIP-highway-section
- HW-04-Naming-highway-override-branch

---

---

## Other Notes

Epic adr-hw-reattach. Size M. Order 4. Supersedes earlier SIP-only settings decision. Status N = ready for sprint after review.
