# [0082] HW-06-WE-formula-restore-ADR-resolve

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/20-highway-reattach-on-adr-aggregates.md §E WE
RefsLibrary/2026090907_custom-road-name-aggregation/21-highway-reattach-roadmap.md HW-06
BelzontAdr/Bridge/RoadMarkerInfoBridge.cs
Archive ACT-02 [0062] WE placeholders

## User Story

> Acting as **a player placing visual highway markers on ADR-associated roads**, I want **WE formulae to show route/mileage/direction from the owning ADR aggregate**, so that I **markers display live highway context without storing route on the marker**.

---

## Background

Seventh task. Restore WE formulae from ACT-02 Feature placeholders / XML comments. Resolve highway context: marker → attached edge → ADRNameAggregated → ADRNameAggregateHighwayData → registry. Update RoadMarkerInfoBridge. Markers remain visual-only components (no route fields returned to ADRHighwayMarkerData).

---

## Definition of Ready (DoR)

- [ ] HW-01 component present
- [ ] ACT-02 placeholders / commented formulae locateable in WE defaults
- [ ] WE integration path for highway marker understood

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] WE default formulae restored for route/mileage/direction (or equivalent Feature vars) reading ADR aggregate context
- [ ] Resolve path documented and implemented: attached net → ADRNameAggregated → ADRNameAggregateHighwayData + registry
- [ ] RoadMarkerInfoBridge no longer returns only defaults for route/direction when ADR context exists (or WE-only path documented if bridge unused)
- [ ] ADRHighwayMarkerData still has no serialized route/mileage fields
- [ ] Marker SIP/tool remain visual-only (pylons + display/custom)
- [ ] Playtest marker on ADR highway road shows registry/mileage/direction-driven WE text
- [ ] Compile + WE smoke

---

## Implementation Notes

1. Find K45_ADR_HighwayMarker.wedefault.xml ACT-02 comments for old formulae
2. Adr_WEIntegration / bridge: supply vars from ADR highway component
3. If edge has no ADR association, safe placeholders / empty — do not invent Aggregate host
4. Keep pylons and displayInformation behavior
5. Scope note: routeDirection is for WE formulae on visual highway markers (this task), not citywide world name labels. HW-04 labels stay prefix-suffix + name/custom only. Highway shields / direction on name labels remain deferred (RefsLibrary 17).
6. Labels scope (deferred, not this task): world highway name labels will also show routeDirection using per-direction format strings in AdrModData (one text field per RouteDirection except NONE). Empty ModData → fall back to existing i18n RouteDirection.{NAME}. See RefsLibrary 17.

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| WE vars missing when marker attached before ADR fork | medium | Fallback placeholders; refresh on membership dirty |
| Bridge API consumers expect marker-local routeDataIndex | low | Resolve via attachment; update Obsolete comment |

---

## Related Tasks

### Depends on

- [0076] HW-01-ADRNameAggregateHighwayData-component-lifecycle

### Is dependent for

- [0084] HW-08-Smoke-acceptance-checklist
### Is related to

- [0062] ACT-02-Markers-visual-only-WE-placeholders

### Groups with

- HW-04-Naming-highway-override-branch
- HW-05-Mileage-addressing-authored-fields

---

## Other Notes

Epic adr-hw-reattach. Size L. Order 7. Status N = ready for sprint after review.
