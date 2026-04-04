**Start time:** 2026-04-04 01:01 -0300
# [0028] add-district-side-lookup-helper

**Developed by:** Claude Sonnet 4.6 (claude-sonnet-4-6@kwytco.com.br)
## User Story

> Acting as **a developer**, I want **a reusable method to get the district name for a building based on which side of the road it is on**, so that I **the {district} placeholder can be resolved correctly with side-aware geometry**.

---

## Background

To support the {district} placeholder, we need to determine which district a building is in based on road-side geometry. Uses Building.m_RoadEdge -> BorderDistrict -> side check via tangent/dot product. This helper will also be reusable for future features.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] GetBuildingSideDistrictName(Entity buildingEntity, NameSystem nameSystem) method exists on AdrMainSystem or a new static utility
- [ ] Uses Building.m_RoadEdge to find the street edge, then BorderDistrict for side-aware district lookup
- [ ] Returns null if no district found on either side
- [ ] Returns district name string when found (via NameSystem.GetRenderedLabelName or equivalent)
- [ ] Handles case where only one side has a district
- [ ] Build passes

---

## Implementation Notes



---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|

---

## Related Tasks

### Depends on



### Is dependent for


