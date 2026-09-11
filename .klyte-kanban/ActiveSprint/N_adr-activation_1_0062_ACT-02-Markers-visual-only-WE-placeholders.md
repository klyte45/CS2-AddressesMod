# [0062] ACT-02-Markers-visual-only-WE-placeholders

**Developed by:** 

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

- [ ] ADRHighwayMarkerData no longer serializes or uses routeDataIndex, routeDirection, displayInformation, numericCustomParam1, numericCustomParam2, overrideMileage, newMileage, reverseMileageCounting
- [ ] Pylon fields kept: pylonCount, pylonSpacing, pylonMaterial, pylonFormat, pylonHeight; version bump discards stripped fields on migrate
- [ ] AdrHighwayRoutesSystem marker dirty pipeline only applies remaining visual fields; no Aggregate coupling
- [ ] AddressesToolOptions / RoadMarkSettings / AdrHighwayRoutesSystem.ts drop route/mileage UI and bindings
- [ ] K45_ADR_HighwayMarker.wedefault.xml keeps formula tags; live formulae replaced with [Feature Name] placeholders; XML comments document old formulae
- [ ] RoadMarkerInfoBridge and TempRoadMarkerTooltip no longer resolve route names or mileage-driven address hacks
- [ ] Prefab ADRRoadMarkerObject / K45_ADR_HighwayMarker remains as visual prop

---

## Implementation Notes

1. Strip fields: routeDataIndex (Hash128), routeDirection (enum), displayInformation (enum), numericCustomParam1 (int), numericCustomParam2 (int), overrideMileage (bool), newMileage (float), reverseMileageCounting (bool)
2. Keep fields: pylonCount (byte), pylonSpacing (float), pylonMaterial (PylonMaterial), pylonFormat (PylonFormat), pylonHeight (float)
3. Bump ADRHighwayMarkerData CURRENT_VERSION; on deserialize discard stripped highway fields
4. Remove Tool_/InfoPanel_ route/mileage binders in AdrHighwayRoutesSystem.cs and .ts
5. WE: replace formulae with literals like [Route ID], [Mileage]; comment old formulae for future Aggregation reimpl
6. Gut RoadMarkerInfoBridge.DesconstructRoadMarkerData route resolution and TempRoadMarkerTooltip mileage coupling

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
