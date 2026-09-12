# [0074] LBL-05-Forked-pick-SIP-Aggregate-section-parity

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/18-label-takeover-sprint-roadmap.md LBL-05
RefsLibrary/2026090907_custom-road-name-aggregation/15-adr-citywide-name-label-takeover.md
RefsLibrary/2026090907_custom-road-name-aggregation/06-sip-and-tooling-gaps.md
RefsLibrary/2026090907_custom-road-name-aggregation/14-vanilla-road-label-pipeline.md

## User Story

> Acting as **a player clicking road name labels**, I want **pick and SIP to resolve S1 Aggregate or S2 ADR name-aggregate correctly**, so that I **I can select a road by its name label and get the same SIP usefulness for ADR-associated roads as for vanilla aggregates**.

---

## Background

Fifth task of epic adr-road-labels. Fork RaycastLabelsJob / label pick so clicks resolve S1 Aggregate or S2 ADR name-aggregate owners. Expand scope (locked): SIP sections that currently gate on vanilla Aggregate must also open/work for ADR S2 owners — RoadSection, TitleSection, aggregate icon, DefaultTool H1 preferred in this task; spill leftover SIP surface to optional LBL-08 if needed. Prefer DefaultTool H1 in LBL-05.

---

## Definition of Ready (DoR)

- [ ] LBL-03 placement produces pickable LabelPosition geometry
- [ ] ACT-04 Prefer ADR in name/address/SIP (0066) baseline for naming — extend Aggregate gates to ADR
- [ ] RefsLibrary 06/15 SIP Aggregate gates and 18 LBL-05 expanded scope agreed
- [ ] kcs2m research of RaycastJobs.RaycastLabelsJob and SIP Aggregate checks available

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Label raycast/pick hits ADR-placed name labels (after LBL-03 positions exist)
- [ ] Pick result entity is S1 Aggregate or S2 ADR name-aggregate per membership rules (same as placement)
- [ ] Clicking an S1 label selects/opens the same Aggregate-oriented SIP path as vanilla
- [ ] Clicking an S2 label selects the ADR name-aggregate and SIP sections that gated on Aggregate still appear/work (RoadSection, TitleSection, icon at minimum)
- [ ] DefaultTool H1 hover/tooltip path updated for ADR owners OR explicit spill note filed to LBL-08 with remaining gaps listed
- [ ] No broken pick on arrows-only or non-label net hits
- [ ] Smoke: pick S1 and S2 labels in dogfood city; SIP shows correct name/sections
- [ ] Compile + playtest

---

## Implementation Notes

1. Fork or Prefix Managed pick entry that feeds RaycastLabelsJob — prefer ADR-owned label buffer if vanilla name labels are empty post-LBL-01
2. Map hit → owner entity using S1/S2 rules identical to LBL-03
3. SIP: find sections/components that require Aggregate; accept ADR name-aggregate as Aggregate-equivalent for those UI gates (H1 DefaultTool preferred here)
4. Do not break building/address SIP from ACT-04
5. If SIP surface is too large mid-task, finish pick+core sections and document LBL-08 leftovers in otherNotes/relatedTasks
6. Arrows remain non-name pick targets as vanilla

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Pick owner disagrees with placement owner (wrong SIP) | high | Shared owner-resolution helper with LBL-03; tests for S1-only, S2-only, boundary clicks |
| SIP Aggregate gates missed — empty or wrong sections for S2 | medium | Checklist from 06; spill to LBL-08 rather than infinite SIP chase in LBL-05 |

---

## Related Tasks

### Depends on

- [0072] LBL-03-ADR-placement-job-vanilla-ADR-membership
- [0066] ACT-04-Prefer-ADR-in-name-address-SIP

### Is dependent for



### Is related to

- [0067] ACT-05-Road-Grouping-ADR-mutations
- [0073] LBL-04-ADR-name-mesh-batch-DrawMesh

### Groups with

- LBL-01-Shutdown-vanilla-name-path
- LBL-02-ADR-TMP-bake-LabelExtents-materials
- LBL-03-ADR-placement-job-vanilla-ADR-membership
- LBL-04-ADR-name-mesh-batch-DrawMesh
- LBL-06-Integration-dogfood-game-update-checklist

## Other Notes

Epic adr-road-labels. Size L. Depends on LBL-03 [0072] and ACT-04 [0066]. Feeds LBL-06. Optional spill: LBL-08.
