# Label extensions — text, images, multi-content, look (feasibility)

*Added: 2026-09-11 · Architecture: [15](15-adr-citywide-name-label-takeover.md) · Pipeline: [14](14-vanilla-road-label-pipeline.md)*

## Purpose

Document **what is possible** after (or beside) a parity citywide name-label takeover. **Product choices for highway shields, shield+name layout, and WE shipping strategy are deferred to later research** — this doc is feasibility only. Phase-1 implementation remains **parity-only** ([15](15-adr-citywide-name-label-takeover.md)).

## Longer / richer text

**Feasible under ADR ownership.**

Vanilla already feeds arbitrary strings through TMP (`sizeDelta` 250×100, `fontSize` 200, center). Extents come from TMP `lineExtents` — **no hard character cap** in `AggregateMeshSystem`. Multiline / rich markup is TMP-capable if ADR sets TMP properties intentionally.

Caveat: player-typed custom names must **escape** rich-text if ADR enables tags for shields.

## Images via TMP sprite atlas

**Feasible** — preferred technical path for inline icons in the **same** road-label mesh pipeline.

### Why it fits the vanilla bake path

`UpdateLabelVertices` already:

1. Walks **every** `TMP_MeshInfo` from `GetTextInfo` (one entry per material / atlas).
2. Clones a `Material` per distinct `mainTexture` and calls `OverlayRenderSystem.CopyFontAtlasParameters`.
3. Stores quads in `LabelVertex` with UVs/colors like glyphs.

Vanilla road labels never assign a TMP **Sprite Asset**, but the multi-atlas walk is already there. Wiring `spriteAsset` + tags such as `<sprite name="…">` can emit shield quads into the same batched draw.

### Dynamic / live atlas texture (ADR TMP side)

**Feasible** for ADR’s **own** label sprite sheet:

| Change type | Cost |
|-------------|------|
| In-place pixel upload, **stable** UV layout | GPU upload; draw samples current texels — “live” for art updates |
| Add/resize sprites (layout change) | Update `TMP_SpriteAsset` UV tables; re-bake labels that reference them |
| Replace `mainTexture` **object identity** | Invalidate material cache (vanilla matches on `mainTexture` ref); dirty meshes |

Preferred product shape for a later epic: one ADR-owned **shields atlas** `Texture2D` as Sprite Asset sheet; many shields as UV rects.

### Caveats

- Sprite materials may need a clone path that does not assume SDF font params only.  
- Sprite size affects `LabelExtents` / `m_MaxScale` / pick bounds.  
- Fallback if net-name shader fights sprites: separate overlay quads / existing highway markers / WE.

## Multiple label contents on one aggregation

**Vanilla:** one content per aggregate — one string bake → one extents → many `LabelPosition`s that **repeat the same content**.

**ADR:** multiple contents are **possible**:

| Model | Description |
|-------|-------------|
| **A. Multi-recipe** | Several recipes on one owner: `{ string, extents, vertices, positions[], placementRules }` — e.g. shield-alone vs name-alone with different density |
| **B. Satellite entities** | Child label entities tied to same membership; pick still resolves to Aggregate / ADR aggregate |
| **C. Combined inline** | Single string `<sprite> Name` — one placement stream (not two labels) |

**Deferred product choice:** later research decides combined vs multi-recipe. Document that **A** is the real answer to “shield alone + name alone”; **C** is the cheapest first step if/when shields enter labels.

Extra cost vs single content: roughly ×N TMP bakes + placement passes; draw can still batch if materials are shared.

## Look customization (distance, width, density)

Vanilla has **no** simple “max render distance” or “max characters” setting. Real knobs:

| Desired control | Vanilla mechanism | ADR customization (later) |
|-----------------|-------------------|---------------------------|
| Fade / stop with camera distance | Pick/cull: `scale = sqrt(dist * 0.0001)` vs `LabelPosition.m_MaxScale` (~95% threshold) | Multiplier or replace formula in ADR pick (+ any shader-side scale) |
| Label height vs road | `m_MaxScale = roadWidth*0.5 / textHalfHeight` | Tunable scale factor |
| Label width (pixels) | TMP layout → `LabelExtents.m_Bounds` | `fontSize` / `sizeDelta` / truncate |
| Placement density | `AddLabels` spacing unit `100f` along length/√width | Tunable spacing |
| Color | `NetNameData.m_Color` / `m_SelectedColor` | ADR palette / per-road override |
| Underground | Dual materials + `m_IsUnderground` | Keep parity |

Phase-1: **do not** ship these knobs — parity only.

## WE / BelzontWE shield sync

### Settled: no live shared `Texture2D`

**Rejected** sharing a live GPU texture / handle between ADR TMP labels and BelzontWE (leak / lifetime / VT ownership risks; WE already leaks VT slots on `EnsureAtlasDeleted` by design). Do **not** request a live-share bridge for this feature family.

### Feasible: copy / re-register

| Piece | Today |
|-------|--------|
| ADR stub | [`WEImageManagementBridge`](../../../BelzontAdr/WriteEverywhere/Bridge/WEImageManagementBridge.cs) — file `RegisterImageAtlas` / `CheckImageAtlasExists` / `EnsureAtlasDeleted` |
| ADR registration | [`Adr_WEIntegrationSystem`](../../../BelzontAdr/WriteEverywhere/Adr_WEIntegrationSystem.cs) — PNG folders under `WriteEverywhere/.Resources/atlases/` |
| Real WE (extra) | `RegisterImageAtlasFromMemrory` — image **bytes** → WE packs **owned** atlas (often BC7 + VT). ADR stub does not declare it yet (reverse-patch gap). |

**Pattern:** same art source → ADR TMP atlas (independent) + WE atlas via register (PNG or memory bytes). On highway entry / shield-set change: `EnsureAtlasDeleted` + re-register.

### Overhead when shields change only on highway entry creation

Acceptable **one-off hitch**, not per-frame:

| Step | Rough cost |
|------|------------|
| `EnsureAtlasDeleted` | milliseconds |
| Cold re-register (decode/pack + BC7 + VT) | ~100 ms–~1 s main thread |
| Warm BC7 cache hit | tens–low hundreds of ms |

Gotchas: always delete before re-register (`DuplicateNameException`); bridge `onCompleteLoading` means **enqueued**, not ready; brief WE sprite fallback window; prefer checksums that reflect **pixel** content (memory-path length-only checksum is weak).

**Product when/how to sync WE** remains deferred; this section only proves the cost is tolerable for rare updates.

## Phaseing reminder

```text
Phase-1 (label epic): citywide name parity — [15] / [16]
Later research:       shields in labels? multi-recipe? WE register product path?
Later implementation: TMP sprites / knobs / multi-recipe as chosen by that research
Never (this family):  live shared Texture2D with BelzontWE
```

## Relation

- [14](14-vanilla-road-label-pipeline.md) — TMP / material multi-atlas walk  
- [15](15-adr-citywide-name-label-takeover.md) — parity scope  
- [16](16-label-takeover-effort-and-risk.md) — parity effort only  
- Highway marker / WE resources — existing ADR WE integration (visual markers; separate from road name labels)  
