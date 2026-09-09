# [0059] T5 Road Grouping real membership mutations via tool gestures

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/07-membership-maintenance-edge-cases.md
RefsLibrary/2026090907_custom-road-name-aggregation/11-road-grouping-tool-roadmap.md

## User Story

> Acting as **a Cities player**, I want **Ctrl+LMB and endpoint drag to change road-group membership**, so that I **so Road Grouping actually edits associations in the save**.

---

## Background

Phase T5 (deferred). Replace T2 log stubs with real add/remove and gap-fill per membership edge-case matrix. Depends on T4 and maintenance APIs.

---

## Definition of Ready (DoR)

- [ ] T4 rewire complete
- [ ] Membership maintenance / gap-fill APIs available (07)

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Ctrl+LMB toggle writes real membership add/remove
- [ ] Endpoint LMB-drag applies path as real add/remove segments
- [ ] Gap-fill and preventPropagation rules from 07 obeyed
- [ ] Logs demoted to debug or removed for happy path

---

## Implementation Notes

1. May split further into maintenance epic tasks; this task is the tool wiring closer
2. Follow 07 edge-case matrix for H2 and gap-fill

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Complex remap/gap-fill edge cases blow scope | high | Ship T2 logs first; implement mutations only after T4 + 07 APIs |

---

## Related Tasks

### Depends on

- [0056] T2 Road Grouping Ctrl+LMB toggle and endpoint drag log stubs
- [0058] T4 Road Grouping rewire selection/overlay to ADR thin contract

### Is dependent for



## Other Notes

Deferred (status L) and lower priority; often several maintenance tasks behind this closer.
