**Start time:** 2026-09-12 21:41 -0300
# [0074] LBL-05-Forked-pick-SIP-Aggregate-section-parity

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/18-label-takeover-sprint-roadmap.md LBL-05
RefsLibrary/2026090907_custom-road-name-aggregation/15-adr-citywide-name-label-takeover.md
RefsLibrary/2026090907_custom-road-name-aggregation/06-labels-vs-vanilla-sync-and-harmony.md
RefsLibrary/2026090907_custom-road-name-aggregation/14-vanilla-road-label-pipeline.md

---

## User Story

> Acting as **a player clicking road name labels**, I want **pick and SIP to resolve S1 Aggregate or S2 ADR name-aggregate correctly**, so that I **I can select a road by its name label and get the same SIP usefulness for ADR-associated roads as for vanilla aggregates**.

---

## Background

Fifth task of epic adr-road-labels. Fork RaycastLabelsJob / label pick so clicks resolve S1 Aggregate or S2 ADR name-aggregate owners. Expanded locked scope: every SIP section that activates for vanilla Aggregate must also activate/work for ADR name-aggregate (S2). At minimum RoadSection, TitleSection.GetVirtualKeyboardLocaleKey ("RoadName"), Aggregate-path ImageSystem/icon, SelectedInfoUISystem.GetAggregatePosition for S2 LabelPosition. Prefer shipping DefaultTool H1 highlight list job here when cheap (same membership helper as RoadSection); else open LBL-08. Correct SIP gate doc is 06-labels-vs-vanilla-sync-and-harmony.md.

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
- [ ] Burst clone of Game.Net.RaycastJobs.RaycastLabelsJob (~150 lines); resolve m_Owner S1 vs S2; schedule from managed Harmony on RaycastSystem when TypeMask.Labels — do not patch Burst job body
- [ ] RoadSection walks ADRNameAggregateElement with same per-edge math as doc 06; S1 remainder filtering when vanilla Aggregate still selected
- [ ] Title locale key RoadName for ADR entity; Aggregate-path icon works for S2
- [ ] SelectedInfoUISystem.GetAggregatePosition works for S2 LabelPosition

---

## Implementation Notes

1. Fork or Prefix Managed pick entry that feeds RaycastLabelsJob — prefer ADR-owned label buffer if vanilla name labels are empty post-LBL-01
2. Map hit → owner entity using S1/S2 rules identical to LBL-03
3. SIP: find sections/components that require Aggregate; accept ADR name-aggregate as Aggregate-equivalent for those UI gates (H1 DefaultTool preferred here)
4. Do not break building/address SIP from ACT-04
5. If SIP surface is too large mid-task, finish pick+core sections and document LBL-08 leftovers in otherNotes/relatedTasks
6. Arrows remain non-name pick targets as vanilla
7. Burst clone RaycastLabelsJob; managed Harmony on RaycastSystem for TypeMask.Labels schedule — never patch Burst job body
8. Shared S1/S2 owner helper with LBL-03
9. SIP audit via Game.dll search at implement time: Aggregate / AggregateElement gates; Managed Harmony for ADR equivalence
10. RoadSection: ADRNameAggregateElement walk; S1 remainder when Aggregate selected
11. TitleSection RoadName key; ImageSystem Aggregate icon path; GetAggregatePosition for S2
12. Prefer DefaultTool H1 in this task; spill to LBL-08 with gap list
13. Do not break ACT-04 building/address SIP
14. Dogfood pre-H1: hovering/selecting a vanilla Aggregate (incl. via its name label) still Highlights edges that also have ADRNameAggregated — wrong glow until H1 S1 remainder filter (doc 06). Must fix in LBL-05 H1 or spill to LBL-08 with this gap listed.
15. When changing vanilla Aggregate selection behavior: highlight membership MUST use same S1 remainder / S2 full ADR sets as RoadSection (doc 06).

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

- [0075] LBL-06-Integration-dogfood-game-update-checklist
### Is related to

- [0067] ACT-05-Road-Grouping-ADR-mutations
- [0073] LBL-04-ADR-name-mesh-batch-DrawMesh
- RefsLibrary/2026090907_custom-road-name-aggregation/06-labels-vs-vanilla-sync-and-harmony.md
### Groups with

- LBL-01-Shutdown-vanilla-name-path
- LBL-02-ADR-TMP-bake-LabelExtents-materials
- LBL-03-ADR-placement-job-vanilla-ADR-membership
- LBL-04-ADR-name-mesh-batch-DrawMesh
- LBL-06-Integration-dogfood-game-update-checklist

---

---

## Other Notes

Epic adr-road-labels. Size L. Depends on LBL-03 [0072] and ACT-04 [0066]. Feeds LBL-06. Optional spill: LBL-08.
