# [0073] LBL-04-ADR-name-mesh-batch-DrawMesh

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/18-label-takeover-sprint-roadmap.md LBL-04
RefsLibrary/2026090907_custom-road-name-aggregation/14-vanilla-road-label-pipeline.md
RefsLibrary/2026090907_custom-road-name-aggregation/15-adr-citywide-name-label-takeover.md
RefsLibrary/2026090907_custom-road-name-aggregation/16-label-takeover-effort-and-risk.md

## User Story

> Acting as **a player with ADR loaded**, I want **ADR to batch and draw road name meshes citywide**, so that I **road names are visible again after vanilla name DrawMesh is shut down**.

---

## Background

Fourth task of epic adr-road-labels. Build name meshes from LBL-02 vertices + LBL-03 positions and DrawMesh them from an ADR render path, replacing the AggregateRenderSystem name loop killed in LBL-01. Arrows stay on vanilla AggregateRenderSystem. Phase-1: parity material/shader/queue — no look knobs product work.

---

## Definition of Ready (DoR)

- [ ] LBL-03 placement writes LabelPosition for S1/S2
- [ ] LBL-02 bake supplies LabelVertex/LabelMaterial
- [ ] LBL-01 has disabled vanilla name DrawMesh so ADR is sole name drawer
- [ ] RefsLibrary 14 mesh→draw and 15 ADR render section agreed

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] ADR mesh builder fills name mesh buffers from LabelVertex + LabelPosition (FillNameMeshData equivalent)
- [ ] ADR render path issues DrawMesh (or equivalent) for name labels each frame/camera pass that vanilla used
- [ ] Respect hideOverlay / camera / LOD filters comparable to vanilla AggregateRenderSystem name loop
- [ ] Arrow DrawMesh path remains exclusively vanilla (LBL-01 allow-list still green)
- [ ] Smoke: after LBL-01+02+03, road names visible again citywide with S1 and S2 owners
- [ ] No obvious mesh leaks (create/destroy or update buffers on invalidate)
- [ ] Compile + visual smoke on dogfood save

---

## Implementation Notes

1. Mirror AggregateMeshSystem.FillNameMeshData + AggregateRenderSystem name GetNameMesh/DrawMesh loop as ADR systems
2. Prefer Graphics.DrawMesh / CommandBuffer patterns already used by game for NetLabel — match layer and material
3. Batch by material where possible; avoid per-label DrawMesh storm if vanilla batched
4. Invalidate mesh when bake or placement invalidates
5. Do not resurrect vanilla name Render loop — ADR owns names after LBL-01
6. Look knobs (distance, width) deferred — 17 feasibility only
7. Port FillNameMeshData warp/batch from AggregateMeshSystem; ADR render mirrors name half of AggregateRenderSystem (hideOverlay, Game/SceneView cameras, surface + underground submeshes)
8. Do not draw arrows — leave AggregateRenderSystem arrow branch alone
9. Match NetLabel layer/material; batch by material where vanilla batched
10. Invalidate mesh when bake or placement invalidates
11. Look knobs deferred — see 17

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Wrong layer/culling — names invisible or always-on-top | medium | Match vanilla render layer/material; A/B screenshot vs pre-LBL-01 if available |
| Mesh/buffer leak over long play sessions | low | Dispose/rebuild mesh on invalidate; track mesh count in debug |

---

## Related Tasks

### Depends on

- [0071] LBL-02-ADR-TMP-bake-LabelExtents-materials
- LBL-03-ADR-placement-job-vanilla-ADR-membership
- [0072] LBL-03-ADR-placement-job-vanilla-ADR-membership
### Is dependent for

- [0075] LBL-06-Integration-dogfood-game-update-checklist
### Is related to

- [0070] LBL-01-Shutdown-vanilla-name-path

### Groups with

- LBL-01-Shutdown-vanilla-name-path
- LBL-02-ADR-TMP-bake-LabelExtents-materials
- LBL-03-ADR-placement-job-vanilla-ADR-membership
- LBL-05-Forked-pick-SIP-Aggregate-section-parity
- LBL-06-Integration-dogfood-game-update-checklist
- [0070] LBL-01-Shutdown-vanilla-name-path
- [0071] LBL-02-ADR-TMP-bake-LabelExtents-materials
- [0072] LBL-03-ADR-placement-job-vanilla-ADR-membership
- [0074] LBL-05-Forked-pick-SIP-Aggregate-section-parity
- [0075] LBL-06-Integration-dogfood-game-update-checklist

---

## Other Notes

Epic adr-road-labels. Size M. Depends on LBL-03 [0072 expected]. Next: LBL-05 pick in parallel-ish; LBL-06 integration.
