**Start time:** 2026-09-12 22:59 -0300
# [0075] LBL-06-Integration-dogfood-game-update-checklist

**Developed by:** Leandro Klyte (klyte@kwyt.com.br)
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/18-label-takeover-sprint-roadmap.md LBL-06
RefsLibrary/2026090907_custom-road-name-aggregation/16-label-takeover-effort-and-risk.md
RefsLibrary/2026090907_custom-road-name-aggregation/15-adr-citywide-name-label-takeover.md
RefsLibrary/2026090907_custom-road-name-aggregation/14-vanilla-road-label-pipeline.md

## User Story

> Acting as **ADR maintainer / player dogfooding Road Grouping**, I want **an end-to-end green path and a written game-update checklist**, so that I **phase-1 label takeover is proven in a real city and future game updates have a concrete re-diff list**.

---

## Background

Sixth and closing task of epic adr-road-labels phase-1 (Sprint 013 catalog). End-to-end dogfood: LBL-01…05 together on a real city — shutdown, bake, dual-membership placement, draw, pick+SIP. Produce a game-update checklist for AggregateMeshSystem / AggregateRenderSystem / RaycastLabelsJob / NetLabelPrefab GUID d124bec1… so future patches are re-diffable. Phase-1 parity only — no shields/multi-recipe/look knobs product.

---

## Definition of Ready (DoR)

- [ ] LBL-01…LBL-05 DoD met (or accepted with documented waivers)
- [ ] Dogfood city with Road Grouping associations available
- [ ] RefsLibrary 16 risk + 18 Sprint 013 exit criteria agreed

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Dogfood save: citywide names visible via ADR (S1 + S2 stretches) after full chain LBL-01…05
- [ ] Vanilla name path remains off; arrows still work in requireNetArrows tools
- [ ] Rename / associate / disassociate refresh labels without duplicates or permanent gaps
- [ ] Pick S1 and S2 opens correct SIP (core Aggregate-gated sections from LBL-05)
- [ ] Written game-update checklist in RefsLibrary (or task notes promoted to 18/16 appendix) covering: AggregateMeshSystem name half, AggregateRenderSystem name loop, RaycastLabelsJob, NetLabelPrefab GUID d124bec1…, Label* component layout
- [ ] Known gaps listed (optional LBL-07/08/09) — not silent
- [ ] No player-facing vanilla-label fallback toggle
- [ ] Compile + extended playtest session notes attached or linked
- [ ] Dogfood: locale change, nameset/custom rename, Temp place/upgrade, tunnels, ADR fork/edit via Road Grouping refresh labels
- [ ] Full SIP panel on S2 vs vanilla Aggregate: stats, title rename key, icon, focus position
- [ ] P0 parity holes fixed in owning LBL or LBL-07…09 opened for spill — not silent

---

## Implementation Notes

1. Treat as integration QA + docs task — prefer fixing only blockers found in dogfood; new features → new tasks
2. Checklist format: file/method, what ADR relies on, how to re-verify after game update
3. Capture before/after notes for S1 vs S2 visual and pick
4. If LBL-05 spilled SIP to LBL-08, verify core path still and mark spill in checklist
5. Update 18 Board IDs / status when sprint closes
6. Integration QA + docs — fix only dogfood blockers; new features → new tasks
7. Checklist: AggregateMeshSystem name half, AggregateRenderSystem name loop, RaycastLabelsJob, RaycastSystem Labels schedule, NetLabelPrefab GUID d124bec1…, Label* layout, SIP Aggregate gates
8. Capture S1 vs S2 visual + pick + SIP notes
9. If LBL-05 spilled H1/SIP to LBL-08, verify core path and mark spill in checklist
10. Update 18 Board IDs / status when sprint closes
11. Added RefsLibrary 19-game-update-re-diff-checklist.md (GUID d124bec1… surfaces 1–12 + dogfood script + known gaps). Linked from 16/18. H1 shipped in LBL-05 — no LBL-08 membership spill. Green selection tint cancelled. Optional spills: LBL-07 Temp deeper parity; LBL-09 mass dirty perf if hitch. Stopped for user extended playtest vs dogfood script in 19.

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Integration-only bugs spanning bake/place/draw/pick | medium | Scripted dogfood checklist; fix blockers in owning LBL task before closing LBL-06 |
| Update checklist goes stale or never written | low | Checklist owned in RefsLibrary; revisit on each game GUID bump |

---

## Related Tasks

### Depends on

- [0070] LBL-01-Shutdown-vanilla-name-path
- [0071] LBL-02-ADR-TMP-bake-LabelExtents-materials
- [0072] LBL-03-ADR-placement-job-vanilla-ADR-membership
- [0073] LBL-04-ADR-name-mesh-batch-DrawMesh
- [0074] LBL-05-Forked-pick-SIP-Aggregate-section-parity

### Is dependent for



### Is related to

- [0066] ACT-04-Prefer-ADR-in-name-address-SIP
- [0067] ACT-05-Road-Grouping-ADR-mutations
- [0068] ACT-06-Membership-maintenance-dogfood

### Groups with

- LBL-01-Shutdown-vanilla-name-path
- LBL-02-ADR-TMP-bake-LabelExtents-materials
- LBL-03-ADR-placement-job-vanilla-ADR-membership
- LBL-04-ADR-name-mesh-batch-DrawMesh
- LBL-05-Forked-pick-SIP-Aggregate-section-parity

## Other Notes

Epic adr-road-labels. Size M. Sprint 013 exit gate. Optional follow-ups LBL-07–09 stay out of phase-1 DoD.
