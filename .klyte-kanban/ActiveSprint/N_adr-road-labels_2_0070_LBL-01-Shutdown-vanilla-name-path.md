# [0070] LBL-01-Shutdown-vanilla-name-path

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/18-label-takeover-sprint-roadmap.md LBL-01
RefsLibrary/2026090907_custom-road-name-aggregation/15-adr-citywide-name-label-takeover.md
RefsLibrary/2026090907_custom-road-name-aggregation/14-vanilla-road-label-pipeline.md
RefsLibrary/2026090907_custom-road-name-aggregation/16-label-takeover-effort-and-risk.md

## User Story

> Acting as **a Cities: Skylines 2 player with ADR loaded**, I want **vanilla road name overlays to stop drawing while one-way arrows keep working**, so that I **ADR can own citywide road names without fighting AggregateSystem or breaking tool arrows**.

---

## Background

First task of epic adr-road-labels (Sprint 013 catalog). Always-on when ADR loads: disable only the vanilla NAME half of AggregateMeshSystem and the name DrawMesh loop in AggregateRenderSystem. Leave AggregateSystem membership and arrow mesh/render paths untouched. Prefer both shutdown levers so TMP/mesh work is not wasted and names never draw. Do not Harmony-patch Burst AggregateSystem jobs.

---

## Definition of Ready (DoR)

- [ ] Activation ACT-04 Prefer ADR in name/address complete (archived 0066) — ADR name-aggregates and naming preference exist
- [ ] Prefer ACT-05/ACT-06 Road Grouping membership shipped so later LBL tasks can dogfood S2 owners
- [ ] RefsLibrary 15 shutdown levers and 18 LBL-01 catalog agreed
- [ ] No open sprint required yet — task stays in backlog until review pulls Sprint 013

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] Managed Harmony Prefix/early-out skips AggregateMeshSystem name half: UpdateLabelVertices, UpdateLabelPositions, FillNameMeshData
- [ ] Arrow half of AggregateMeshSystem still runs (UpdateArrowMaterials / UpdateArrowPositions / FillArrowMeshData as needed)
- [ ] Managed Harmony Prefix prevents AggregateRenderSystem.Render name GetNameMesh/DrawMesh loop
- [ ] requireNetArrows arrow branch in AggregateRenderSystem still draws arrows
- [ ] With ADR loaded, world road NAME overlays are gone on a smoke city
- [ ] One-way arrows still appear when a tool requires net arrows
- [ ] Always-on with ADR load — no player-facing vanilla-label fallback toggle
- [ ] No patches to AggregateSystem Burst jobs
- [ ] Compile + smoke: load game with ADR, confirm no name labels and arrows still work in a net tool that needs them

---

## Implementation Notes

1. Target Game.Rendering.AggregateMeshSystem.OnUpdate name branch only — mirror 15 shutdown levers
2. Target Game.Rendering.AggregateRenderSystem.Render name loop; leave early-return arrow path intact
3. Place Harmony under BelzontAdr/Overrides or AdrRoadNameLabelHarmony — follow existing override patterns
4. Keep Prefab/material init for arrows if required; do not tear down NetLabel archetype components yet (ADR bake will reuse Label* contract)
5. Verify hideOverlay and camera filters are unchanged for arrows
6. Document any game-update re-diff notes for these two managed methods (GUID d124bec1… baseline in 14/16)
7. Target Game.Rendering.AggregateMeshSystem.OnUpdate name branch only: early-out UpdateLabelVertices / UpdateLabelPositions / FillNameMeshData
8. Target AggregateRenderSystem.Render name GetNameMesh/DrawMesh loop; leave requireNetArrows arrow branch
9. Harmony under BelzontAdr/Overrides or AdrRoadNameLabelHarmony
10. Keep Prefab/material init for arrows; do not tear down NetLabel Label* components (ADR reuse)
11. GUID d124bec1… baseline in 14/16 for re-diff

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Accidentally disable arrow mesh/render while killing names | medium | Explicit allow-list of arrow methods; playtest requireNetArrows tool after change |
| OnUpdate structure churn on game update breaks Prefix | medium | Thin Prefix; re-diff AggregateMeshSystem.OnUpdate / AggregateRenderSystem.Render on GUID change |

---

## Related Tasks

### Depends on

- [0066] ACT-04-Prefer-ADR-in-name-address-SIP

### Is dependent for

- [0071] LBL-02-ADR-TMP-bake-LabelExtents-materials
- [0075] LBL-06-Integration-dogfood-game-update-checklist
### Is related to

- [0067] ACT-05-Road-Grouping-ADR-mutations
- [0068] ACT-06-Membership-maintenance-dogfood

### Groups with

- LBL-02-ADR-TMP-bake-LabelExtents-materials
- LBL-03-ADR-placement-job-vanilla-ADR-membership
- LBL-04-ADR-name-mesh-batch-DrawMesh
- LBL-05-Forked-pick-SIP-Aggregate-section-parity
- LBL-06-Integration-dogfood-game-update-checklist

---

## Other Notes

Epic adr-road-labels. Size S. Backlog N — ready for Sprint 013 after review. Next: LBL-02 TMP bake.
