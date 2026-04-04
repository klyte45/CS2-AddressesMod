**End time:** 2026-04-03 18:46 -0300
**Start time:** 2026-04-03 18:45 -0300
# [0006] fix-cityname-seeds-missing-serialization

**Developed by:** claude-sonnet-4-6@kwyt.com.br
## User Story

> Acting as **a city planner**, I want **the CityNameSeeds value to persist across game saves**, so that I **maintain consistent name generation for roads or districts after reloading the city**.

---

## Background

AdrCitywideSettings.cs defines `public long CityNameSeeds { get; set; } = new System.Random().NextLong()` but `CityNameSeeds` is absent from both Serialize and Deserialize methods. The value is re-randomized on every load, potentially causing name regeneration.

ATTENTION: Only compile the project using the solution file, as explained on the kanban README! This project was compiling before, so if it stopped it was something you have done!
---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [x] CityNameSeeds is added to the Serialize method after bumping CURRENT_VERSION to 1
- [x] The Deserialize method reads CityNameSeeds when version >= 1 and falls back to a random value for v0 saves
- [x] Existing v0 saves load without throwing exceptions

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


