# 10 — i18n and assets

## Confirmed i18n decision

**Keep `K45::ADR.main[...]` keys** during the migration. No CSV churn required for cutover.

Optional rename to `K45::ADR.settings[...]` is a **future follow-up only** (not part of big-bang).

## Current namespaces

| UI | Helper | Namespace |
|----|--------|-----------|
| EUIS `main` | `main/src/utility/translate.ts` | `K45::ADR.main[${key}]` |
| VUIO contextual | `k45_adr_vuio/src/utility/translate.ts` | `K45::ADR.vuio[${key}]` |

CSV source: `BelzontAdr/i18n/*.csv` (and any packaging that copies them).

## Settings UI translate helper

Ported settings code must **not** use the existing vuio helper (that would look up wrong keys).

Options (pick one in implementation):

1. **Dedicated** `settings/translate.ts` that wraps `K45::ADR.main[...]` (copy of main helper)
2. **Parameterized** helper `translate(ns, key)` — only if cleaning both call sites is cheap

Recommended: option 1 for minimal diff from current main components.

Missing-key tracking via `window.K45_MISSING_I18N` can remain.

## Keys that must keep working

All keys currently used under `main` — including but not limited to:

- Shell: `loadACityWarningH2`, root tab titles (`namesetManagement.title`, `overrideSettings.title`, `roadPrefixSettings.title`, `regionSettings.title`)
- Nameset flows: `cityNamesetsLibrary.*`, `namesetsLibrary.*`, `githubLibrary.*`, `githubListing.*` (rate-limit / error strings), `namesetsImport.*`, `namesetEditor.*`, `namesetDelete.*`, …
- GitHub error modal buttons: reuse `githubLibrary.visitRepository` (or add a dedicated Cancel key under `githubListing.*` if none exists)
- Overrides: `overrideSettings.*`, `vehicleSerial.*`, plate strings
- Road prefix: `roadPrefixSettings.*`
- Region: `regionSettings.*`, highway / city editor keys

**Do not** migrate these strings into `K45::ADR.vuio[...]` in this sprint.

## Future rename (follow-up, optional)

If later renaming to `K45::ADR.settings[...]`:

1. CSV: duplicate or move keys
2. Replace settings `translate.ts` namespace
3. Grep for `ADR.main[` leftovers in settings folder
4. Keep contextual `ADR.vuio` unchanged

## Icons and coui assets

| Asset | Usage | After migration |
|-------|--------|-----------------|
| `coui://adr.k45/UI/images/ADR.svg` | EUIS app icon; root menu | UMM button + panel header |
| `coui://adr.k45/UI/images/outsideConnections.svg` | Region tab icon | Keep path |
| `coui://uil/Standard/*.svg` | Tab / action icons | Keep Unified Icon Library paths |
| `Media/Glyphs/ParadoxMods.svg` | Vanilla UMM right-menu glyph | Game-owned; do not replace |

`_Frontends/UI/images/` (or packaged UI images) must still ship with the mod even after deleting `main` — VUIO settings still references ADR SVG paths.

## What dies with `main`

| Asset | Note |
|-------|------|
| `k45-adr-main.js` / `.css` | Stop packaging |
| EUIS icon registration URL | Replaced by UMM button using same SVG via coui |

## Checklist

- [ ] Settings `translate` still resolves `K45::ADR.main[...]`
- [ ] Contextual VUIO still uses `K45::ADR.vuio[...]`
- [ ] ADR.svg and outsideConnections.svg still packaged
- [ ] No new CSV required for cutover
- [ ] Changelog mentions Universal Mod Menu location for players who looked for EUIS app
