**End time:** 2026-09-11 14:26 -0300
**Start time:** 2026-09-11 13:21 -0300
# [0062] ACT-02-Markers-visual-only-WE-placeholders

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/12-activation-change-map.md §C
RefsLibrary/2026090907_custom-road-name-aggregation/13-activation-sprint-roadmap.md ACT-02

## User Story

> Acting as **a Cities: Skylines 2 player using road markers**, I want **route and mileage stripped from marker runtime/UI while pylons and WE placeholders remain**, so that I **markers stay placeable as visual props without depending on Aggregate highway host data**.

---

## Background

Workstream 2. After Aggregate highway host is gone, markers must not carry route/mileage identity. Keep pylons/visuals; WE layouts keep formula tags but use Feature-visible placeholders until highway reattach.

---

## Definition of Ready (DoR)

- [ ] ACT-01 complete (or Aggregate highway writers already gone)
- [ ] RefsLibrary 12 §C and 13 ACT-02 catalog agreed

---

## Acceptance Criteria / Definition of Done (DoD)

- [x] ADRHighwayMarkerData no longer serializes or uses routeDataIndex, routeDirection, overrideMileage, newMileage, reverseMileageCounting (displayInformation + numericCustomParam1/2 kept as visuals)
- [x] Pylon fields kept: pylonCount, pylonSpacing, pylonMaterial, pylonFormat, pylonHeight; version bump discards stripped highway fields on migrate
- [x] AdrHighwayRoutesSystem marker dirty pipeline applies visual fields only (pylons + display/custom params); no Aggregate coupling
- [x] AddressesToolOptions / RoadMarkSettings / AdrHighwayRoutesSystem.ts drop route/mileage UI and bindings; keep display/pylon UI
- [x] K45_ADR_HighwayMarker.wedefault.xml keeps live displayInformation formulae (visual); no route/mileage formulae to placeholder
- [x] RoadMarkerInfoBridge and TempRoadMarkerTooltip no longer resolve route names or mileage-driven address hacks
- [x] Prefab ADRRoadMarkerObject / K45_ADR_HighwayMarker remains as visual prop

---

## Implementation Notes

1. Strip highway identity fields: routeDataIndex, routeDirection, overrideMileage, newMileage, reverseMileageCounting
2. Keep visual fields: displayInformation, numericCustomParam1/2, pylonCount, pylonSpacing, pylonMaterial, pylonFormat, pylonHeight
3. Bump ADRHighwayMarkerData CURRENT_VERSION; on deserialize discard stripped highway fields while preserving display/custom params
4. Remove Tool_/InfoPanel_ route/mileage binders; keep display/custom + pylon binders
5. WE: keep live displayInformation formulae
6. Gut RoadMarkerInfoBridge route resolution and TempRoadMarkerTooltip mileage coupling

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| WE layouts look empty until highway reattach | low | Keep formula XML tags + commented old formulae for later restore |

---

## Related Tasks

### Depends on

- [0061] ACT-01-Strip-Aggregate-highway-host

### Is dependent for



## Other Notes

Depends on ACT-01. Highway reattach + WE formula restore is a later epic.
