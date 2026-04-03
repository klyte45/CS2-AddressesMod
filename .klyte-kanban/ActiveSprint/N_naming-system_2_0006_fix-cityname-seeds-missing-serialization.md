# [0006] fix-cityname-seeds-missing-serialization

**Developed by:** 

## User Story

> Acting as **a city planner**, I want **the CityNameSeeds value to persist across game saves**, so that I **maintain consistent name generation for roads or districts after reloading the city**.

---

## Background

AdrCitywideSettings.cs defines `public long CityNameSeeds { get; set; } = new System.Random().NextLong()` but `CityNameSeeds` is absent from both Serialize and Deserialize methods. The value is re-randomized on every load, potentially causing name regeneration.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] CityNameSeeds is added to the Serialize method after bumping CURRENT_VERSION to 1
- [ ] The Deserialize method reads CityNameSeeds when version >= 1 and falls back to a random value for v0 saves
- [ ] Existing v0 saves load without throwing exceptions

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


