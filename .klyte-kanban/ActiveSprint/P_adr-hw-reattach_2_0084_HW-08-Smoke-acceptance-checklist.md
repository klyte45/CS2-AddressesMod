**Start time:** 2026-09-14 17:43 -0300
# [0084] HW-08-Smoke-acceptance-checklist

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/21-highway-reattach-roadmap.md Acceptance checklist
RefsLibrary/2026090907_custom-road-name-aggregation/20-highway-reattach-on-adr-aggregates.md

## User Story

> Acting as **an ADR maintainer dogfooding highway reattach**, I want **an end-to-end green checklist covering SIP, tool, naming, mileage, WE, and regions**, so that I **the single sprint can close with proven acceptance from RefsLibrary 21**.

---

## Background

Ninth and closing task of epic adr-hw-reattach. Integration QA against 21 acceptance checklist. Prefer fixing blockers in owning HW tasks; new scope → new tasks. Update 21 board IDs / status when sprint closes. All HW-01…HW-08 in one sprint.

---

## Definition of Ready (DoR)

- [ ] HW-01…HW-07 DoD met (or accepted with documented waivers)
- [ ] Dogfood city with ADR Road Grouping + highway registry entries
- [ ] RefsLibrary 21 acceptance list agreed

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Vanilla Aggregate Addresses SIP: seed only — no highway controls
- [ ] ADR SIP: seed + highway route/direction/mileage/reverse
- [ ] Road Grouping tool ADR selection: custom name (not entity Index) + seed + highway parity with SIP
- [ ] SIP and tool edits stay consistent on the same ADR entity
- [ ] Registry highway assign changes road naming for ADR-linked edges
- [ ] Mileage override + reverse change house-number/mileage along ADR chain
- [ ] Markers remain visual-only; WE shows route/mileage/direction from owning ADR aggregate
- [ ] Region map/registry hover reflects ADR highwayDataId
- [ ] default highwayDataId behaves as non-highway; empty ADR teardown removes highway component
- [ ] No Aggregate highway cache / marker↔Aggregate sync jobs revived
- [ ] i18n pending entries only (no agent i18n.csv edits)
- [ ] All HW-01…HW-08 completed in the same sprint
- [ ] Playtest notes attached or linked; known gaps listed not silent
- [ ] No save-migration work for old Aggregate/marker highway identity (beta — out of scope; manual re-assign OK)

---

## Implementation Notes

1. Treat as integration QA — fix blockers in owning HW-*
2. Script: vanilla Aggregate SIP → ADR fork via tool → set custom name/seed/highway → check labels/addresses/cards → place marker WE → Region map hover
3. Update 21 Board IDs section when closing
4. Do not expand into highway shields (RefsLibrary 17)
5. Do not open follow-up tasks for Aggregate/marker highway save migration — beta; out of scope for this feature.
6. Integration QA only — no new product code for HW-08. Dogfood against RefsLibrary 21 acceptance checklist.
7. Known gap / waiver: [0085] HW-03c cancelled (Z) → net-kind road-only seeds deferred to [0086] NET-04 / RefsLibrary 22–24. Do not block HW-08 on 0085; list as known gap if seed-on-non-road still observed.
8. Dogfood script: (1) Vanilla Aggregate SIP → seed only, no highway. (2) Road Grouping fork ADR → custom name + seed + highway parity with SIP; edits sync. (3) Registry highway → ADR road naming. (4) Mileage override + reverse → house numbers/cards. (5) Marker visual-only; WE ORIGINAL shows route/mileage/direction from ADR. (6) Region map Reload → hover registry highlights ADR segments only. (7) Clear highwayDataId → non-highway; remove last ADR membership → highway component gone. (8) Confirm no Aggregate cache / marker sync revival; no save-migration work.
9. Attach playtest notes here when done; check DoD items; update RefsLibrary 21 board IDs when sprint closes.

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Cross-task bugs spanning SIP/tool/WE/regions | medium | Scripted checklist; fix in owning task before closing HW-08 |
| Sprint closes with undocumented gaps | low | Known-gaps list required in DoD |

---

## Related Tasks

### Depends on

- [0076] HW-01-ADRNameAggregateHighwayData-component-lifecycle
- [0077] HW-02-SIP-backend-isAdrAggregate-highway-bindings
- [0078] HW-03-VUIO-Addresses-SIP-highway-section
- [0079] HW-03b-Road-Grouping-tool-options-parity
- [0080] HW-04-Naming-highway-override-branch
- [0081] HW-05-Mileage-addressing-authored-fields
- [0082] HW-06-WE-formula-restore-ADR-resolve
- [0083] HW-07-Regions-map-ADR-highwayDataId
- [0085] HW-03c-Gate-road-seeds-namesets-to-roads-only
### Is dependent for



### Is related to

- [0075] LBL-06-Integration-dogfood-game-update-checklist
- [0068] ACT-06-Membership-maintenance-dogfood
- [0086] NET-04-Road-only-seeds-namesets-prefab-RoadData
### Groups with

- HW-01-ADRNameAggregateHighwayData-component-lifecycle
- HW-02-SIP-backend-isAdrAggregate-highway-bindings
- HW-03-VUIO-Addresses-SIP-highway-section
- HW-03b-Road-Grouping-tool-options-parity
- HW-04-Naming-highway-override-branch
- HW-05-Mileage-addressing-authored-fields
- HW-06-WE-formula-restore-ADR-resolve
- HW-07-Regions-map-ADR-highwayDataId

---

---

## Other Notes

Epic adr-hw-reattach. Size S. Order 9. Sprint exit gate. Status N = ready for sprint after review.
