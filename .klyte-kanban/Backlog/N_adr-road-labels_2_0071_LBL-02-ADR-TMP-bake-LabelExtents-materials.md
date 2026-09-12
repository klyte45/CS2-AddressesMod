# [0071] LBL-02-ADR-TMP-bake-LabelExtents-materials

**Developed by:** 

## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/18-label-takeover-sprint-roadmap.md LBL-02
RefsLibrary/2026090907_custom-road-name-aggregation/14-vanilla-road-label-pipeline.md
RefsLibrary/2026090907_custom-road-name-aggregation/15-adr-citywide-name-label-takeover.md
RefsLibrary/2026090907_custom-road-name-aggregation/17-label-extensions-text-images-look.md

## User Story

> Acting as **ADR label pipeline**, I want **TMP-baked LabelVertex/LabelExtents/LabelMaterial for each road-name owner**, so that I **later placement and draw tasks can render ADR-owned road names with vanilla-compatible geometry**.

---

## Background

Second task of epic adr-road-labels. After LBL-01 shuts down vanilla name TMP/mesh, ADR must bake road-name label geometry into the same LabelVertex / LabelExtents / LabelMaterial contract AggregateMeshSystem used, so placement and mesh tasks can consume it. Prefer TMP FontAsset + Material from NetLabelPrefab (GUID d124bec1…). Phase-1 is text-only parity — no multi-recipe or WE image product work.

---

## Definition of Ready (DoR)

- [ ] LBL-01 Shutdown vanilla name path complete — vanilla no longer owns name TMP bake
- [ ] RefsLibrary 14 TMP→LabelVertex pipeline and 15 ADR bake section agreed
- [ ] NetLabelPrefab / FontAsset access pattern understood via kcs2m (no Managed folder browse)
- [ ] Activation naming preference (ACT-04 / 0066) available for string source

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] ADR system (or service) produces Buffer/DynamicBuffer LabelVertex for road-name label entities (or owner-keyed buffers ADR owns)
- [ ] LabelExtents written consistently with baked glyph bounds
- [ ] LabelMaterial / material references resolve to NetLabelPrefab (or documented ADR copy) TMP Material + FontAsset
- [ ] Bake invalidates/rebuilds when GetRenderedLabelName / ADR display name changes for an owner aggregate
- [ ] No live shared Texture2D with WE — copy/re-register only if any sprite path is touched (phase-1 text-only preferred)
- [ ] Unit/smoke: sample aggregate name string bakes non-empty vertices and sane extents
- [ ] Compile + load: no TMP/FontAsset null refs in bake path logs
- [ ] Parity layout: sizeDelta 250×100, fontSize 200, center alignment matching vanilla UpdateLabelVertices
- [ ] Clone materials + CopyFontAtlasParameters as vanilla
- [ ] Wire dirty via Updated; extend AdrMainSystem.ResetRoadsCache pattern to ADR owners as needed; cache string changes

---

## Implementation Notes

1. Mirror AggregateMeshSystem UpdateLabelVertices / extents write patterns — prefer reimplement over Harmony-inject into vanilla
2. Reuse Game.Rendering Label* component types where possible so pick/mesh stay compatible
3. String source: NameSystem.GetRenderedLabelName or ADR equivalent for the owner Aggregate / ADR name-aggregate
4. Cache FontAsset/Material from NetLabelPrefab; avoid per-frame TMP allocations
5. Invalidate on rename, locale change, and aggregate owner change events already used by ADR naming
6. Defer TMP sprites / multi-recipe / look knobs — see 17 feasibility only
7. Port vanilla UpdateLabelVertices: NameSystem.GetRenderedLabelName (or ADR-resolved entity) → TMP GetTextInfo → LabelVertex
8. Write LabelExtents; LabelMaterial from NetLabelPrefab FontAsset/Material
9. Parity: sizeDelta 250×100, fontSize 200, center
10. Dirty via Updated / ResetRoadsCache extension for ADR owners
11. Phase-1 text-only — no TMP sprites / multi-recipe / WE product path

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| LabelVertex layout mismatch breaks mesh/pick later | medium | Match vanilla LabelVertex layout byte-for-byte; compare extents against a captured vanilla sample before LBL-01 permanently kills bake |
| FontAsset/Material lifetime leaks or dispose crashes | low | Hold FontAsset reference for mod lifetime; never dispose shared TMP assets |

---

## Related Tasks

### Depends on

- [0070] LBL-01-Shutdown-vanilla-name-path
- [0066] ACT-04-Prefer-ADR-in-name-address-SIP

### Is dependent for

- [0072] LBL-03-ADR-placement-job-vanilla-ADR-membership
- [0073] LBL-04-ADR-name-mesh-batch-DrawMesh
- [0075] LBL-06-Integration-dogfood-game-update-checklist
### Is related to

- [0067] ACT-05-Road-Grouping-ADR-mutations

### Groups with

- LBL-01-Shutdown-vanilla-name-path
- LBL-03-ADR-placement-job-vanilla-ADR-membership
- LBL-04-ADR-name-mesh-batch-DrawMesh
- LBL-05-Forked-pick-SIP-Aggregate-section-parity
- LBL-06-Integration-dogfood-game-update-checklist

---

## Other Notes

Epic adr-road-labels. Size M. Depends on LBL-01 [0070]. Next: LBL-03 placement.
