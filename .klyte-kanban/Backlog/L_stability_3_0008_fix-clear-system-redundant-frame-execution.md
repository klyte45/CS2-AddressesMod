# [0008] fix-clear-system-redundant-frame-execution

**Developed by:** 

## User Story

> Acting as **a developer**, I want **AdrClearSystem to only run its logic when there are entities to process**, so that I **reduce unnecessary ECS overhead caused by an always-running system with no entity guard**.

---

## Background

AdrClearSystem.OnUpdate calls EntityManager.DestroyEntity on every game frame with no RequireForUpdate guard. The query may routinely be empty, but the system still wakes up every frame. Additionally, if this runs during loading transitions, it could destroy entities prematurely.

---

## Definition of Ready (DoR)



---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] RequireForUpdate(m_ClearQuery) is added to OnCreate
- [ ] The system only executes OnUpdate when the query has matching entities
- [ ] No regression in entity cleanup behavior during normal gameplay

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


