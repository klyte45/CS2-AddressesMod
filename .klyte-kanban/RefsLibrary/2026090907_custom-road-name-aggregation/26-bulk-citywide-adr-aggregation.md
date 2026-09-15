# Citywide bulk ADR aggregation (settings)

*Added: 2026-09-14 · Depends: [23](23-adr-net-kind-gate-labels-seeds.md) / [24](24-adr-net-kind-roadmap.md) NET-01…03 · UI: `_Frontends/UI/k45_adr_vuio/src/settings/tabs/overrides/OverrideRoadsDistrictsTab.tsx`*

## Goal

Give players a **one-shot settings action** that turns every **applicable** city edge into an ADR name-aggregation membership (fork/create as needed), with an explicit **kind filter** so they can convert e.g. only roads, or roads + tracks, without touching kinds they leave unchecked.

## Placement (UI)

**Roads and Districts** tab (`OverrideRoadsDistrictsTab`):

- Put the feature **button** immediately **above** the district-dependent road nameset block (the `<h2>` / `overrideSettings.perDistrictRoadsFile` section).
- Do **not** bury it under per-district controls.

## Confirmation dialog

Clicking the button opens a **confirmation dialog**:

| Element | Behavior |
|---------|----------|
| Body | Checkboxes — one per **aggregable** `AdrNetKind` the player may include |
| Default | Product decision at implement time (recommend: **Road** checked; other kinds unchecked — safer default) |
| Confirm | Runs bulk convert for **checked kinds only** |
| Cancel | No-op |

Checkbox labels should use existing / new i18n keys (pending `*_entries.txt`; do not edit `i18n.csv`).

Kinds to offer (align with [23](23-adr-net-kind-gate-labels-seeds.md) aggregable set):

- Road
- Pathway
- Train
- Tram
- Subway
- Waterway
- Taxiway

Omit non-aggregable nets (fence, pipeline, powerline, …).

## Backend behavior (sketch)

1. Scan city edges (or vanilla Aggregates → member edges) that classify via `AdrNetKindUtil.TryGetNetKind` to a **checked** kind.
2. Skip edges already on an ADR name-aggregate (`ADRNameAggregated`).
3. For remaining edges: apply the same fork/create rules as Road Grouping / membership (`TryForkFromVanilla` / `CreateAggregate` / kind gate) — **exact algorithm** TBD in implementation (prefer: convert **per vanilla Aggregate** when all members share a checked kind; otherwise per-edge / contiguous same-kind runs).
4. Respect NET-02 mix gate: never merge cross-kind.
5. Progress / result: toast or dialog summary (converted count / skipped / failed) — preferred for large cities.
6. Must be **safe to re-run** (idempotent for already-ADR edges).

## Out of scope

- Auto-run on load / save
- Converting non-aggregable kinds
- Changing district nameset files
- Replacing Road Grouping interactive editing

## Success criteria

- [ ] Button visible above per-district road nameset section on Roads and Districts
- [ ] Confirm dialog body lists kind checkboxes; only checked kinds convert
- [ ] Citywide pass creates/forks ADR aggregates without cross-kind membership
- [ ] Already-ADR edges skipped; re-run safe
- [ ] i18n via pending entries (no CSV edit by agent)
- [ ] Docs + board task linked

## Related board

Catalog id **NET-06** — board **[0091]** (Sprint 015). Depends NET-01…03 (kind + membership + labels).
