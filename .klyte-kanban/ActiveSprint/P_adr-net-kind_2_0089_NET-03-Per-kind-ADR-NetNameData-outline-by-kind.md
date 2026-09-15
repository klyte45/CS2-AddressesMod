**Start time:** 2026-09-14 21:11 -0300
# [0089] NET-03-Per-kind-ADR-NetNameData-outline-by-kind

**Developed by:** Auto <auto@kwyt.com.br>
## Reference

RefsLibrary/2026090907_custom-road-name-aggregation/22-game-net-edge-kinds-and-aggregation.md
RefsLibrary/2026090907_custom-road-name-aggregation/23-adr-net-kind-gate-labels-seeds.md
RefsLibrary/2026090907_custom-road-name-aggregation/24-adr-net-kind-roadmap.md (NET-03)
BelzontAdr/Systems/AdrRoadNameLabelMeshSystem.cs

## User Story

> Acting as **a player viewing world road name labels**, I want **non-road ADR aggregates to show name labels with correct materials and per-kind outlines**, so that I **citywide ADR name labels work for tracks, pathways, and waterways with kind-distinct outlines**.

---

## Background

Third task of epic adr-net-kind. Non-road AggregateNets often lack NetLabel/NetNameData so S2 world labels break. Create ADR-owned NetNameData prefab entities for every kind except Road; face #FFFFFFC0; outline-by-kind (Subway #004400 former ADR green; Road #333333; Pathway #666666; Train #331A00; Tram #330033; Waterway #002244; Taxiway #33001A). Generalize m_IsAdrStyle toward kind outline. Do not mutate vanilla AggregateNet assets.

---

## Definition of Ready (DoR)

- [ ] NET-01 [0087] stored kind available on ADR aggregates
- [ ] LBL-02+ label mesh/ensure path shipped (archived 0071+)
- [ ] RefsLibrary 23 NetNameData strategy and color table locked

---

## Acceptance Criteria / Definition of Done (DoD)

- [ ] ADR-owned prefab entities with NetNameData exist for Pathway, Train, Tram, Subway, Waterway, Taxiway
- [ ] Face/text color #FFFFFFC0 on NetNameData for ADR kind prefabs
- [ ] Outline colors match locked table including Road #333333 and Subway #004400
- [ ] EnsureRoadNameLabelComponents (or successor) resolves PrefabRef to a prefab with NetNameData; outline follows stored AdrNetKind
- [ ] m_IsAdrStyle generalized to per-kind outline lookup — Road no longer forced to former single green
- [ ] No mutation of vanilla AggregateNet assets
- [ ] Compile + smoke: non-road S2 labels draw; Road S2 outline is #333333

---

## Implementation Notes

1. Extend AdrRoadNameLabelMeshSystem Ensure path and material clone outline selection
2. Create/register ADR AggregateNet-like entities with NetNameData for non-road kinds only
3. Prefer vanilla AggregateNet PrefabRef for Road when it already has NetNameData; still apply Road outline #333333 on ADR-style clones
4. Do not invent BelzontWE live shared Texture2D
5. Catalog: 24-adr-net-kind-roadmap.md NET-03

---

## Risk Assessment

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Wrong PrefabRef breaks road label look | medium | Prefer vanilla AggregateNet when it has NetNameData; outline still by kind |
| Accidentally mutate vanilla AggregateNet | low | DoD: no vanilla asset writes; ADR-owned entities only |

---

## Related Tasks

### Depends on

- [0087] NET-01-Classifier-stored-kind-on-ADRNameAggregate
- [0071] LBL-02-ADR-TMP-bake-LabelExtents-materials

### Is dependent for



### Is related to

- [0072] LBL-03-ADR-placement-job-vanilla-ADR-membership
- [0073] LBL-04-ADR-name-mesh-batch-DrawMesh

### Groups with

- NET-01-Classifier-stored-kind-on-ADRNameAggregate
- NET-02-Membership-Road-Grouping-mix-gate
- NET-04-Road-only-seeds-namesets-prefab-RoadData
- NET-05-Playtest-matrix-smoke-notes

## Other Notes

Epic adr-net-kind. Size M. Order 3. Depends NET-01; prefer LBL mesh path done.
