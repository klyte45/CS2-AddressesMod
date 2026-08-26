# 05 — Styles conversion

## Today (`main`)

- Global SCSS extracted to `k45-adr-main.css` (EUIS isolates per-app CSS)
- Entry pull-in: `src/styles/main.scss` (+ `@klyte45/euis-components` form styles)
- Feature SCSS files co-located or under `styles/`
- Heavy use of game CSS variables (`--dialogButtonYesColor`, `--accentColor*`, `--menuPanel1`, …)
- Gameface scrollable containers (`gameface-scrollable-container`)
- Masked icons via `maskImage: url(...)`
- Units: CS2 **`rem`** (not browser rem semantics)

## Tomorrow (`k45_adr_vuio`)

- Webpack CSS modules (auto) + Sass; locals camelCase; `localIdentName: [local]_[hash:base64:3]`
- Prefer vanilla theme classes resolved via `VanillaComponentResolver` / module SCSS (`themeToggleLine`, editor item classes, etc.)
- Shared tweaks live in `vuio-commons` (e.g. `common.scss`, `buttonsStyles.scss`)
- Bundle advertises CSS via `CSSPresencePlugin` (`hasCSS`)

## SCSS inventory to rewrite

| Source file | Used by | Conversion notes |
|-------------|---------|------------------|
| `main/src/styles/main.scss` | Global buttons / layout | **Do not import wholesale.** Re-home only needed button/layout rules as CSS modules under `settings/styles/` or use vuio-commons button styles |
| `main/src/styles/roadPrefixRuleEditor.scss` | Road prefix | → `settings/styles/roadPrefixRuleEditor.module.scss` |
| `main/src/styles/wordContainer.scss` | Nameset word list | → CSS module next to `NamesetWordsContainer` |
| `main/src/components/overrides/vehiclePlateController.scss` | Plates | → CSS module |
| `main/src/components/region/regionEditor.scss` | Region shell + tabs | Strip `react-tabs__*` selectors; rewrite for new tab strip |
| `main/src/components/region/map.scss` | MapDiv | Highest visual risk; keep structure, modularize class names |
| `main/src/components/region/subpages/tabCommons.scss` | Shared listing tabs | Merge into region modules |
| `main/src/components/region/subpages/HighwayListingTab.scss` | Highways | CSS module |
| `main/src/components/region/subpages/RegionCitiesManagement.scss` | Cities | CSS module |

## Rules for implementers

1. **No global class leakage** into game UI — always CSS modules for custom ADR settings styles.
2. **Prefer game variables** over hard-coded colors. When unsure, query via `kcs2m_css_lookup` (property name) against game `index.css`.
3. **Keep rem units** for most internal layout (match existing VUIO). Exception: settings panel outer size uses viewport units — **`width: 80vw; height: 70vh`** (see [03](03-shell-and-umm.md)); revisit if Gameface handles `vh`/`vw` poorly.
4. **Do not** import `euis-components/src/styles/cs2-form-style.scss` into VUIO.
5. **react-select** container rules in `main.scss` die with react-select — replace with vanilla dropdown styling.
6. **react-tabs** selectors in `regionEditor.scss` must be rewritten for the new tab control.
7. Prefer `centerLeftLayout` / GamePanel layout classes over absolute positioning. Inline styles are fine for one-off gaps inside the panel, but reusable spacing goes in modules.

## Collision / specificity risks

| Risk | Mitigation |
|------|------------|
| Class names like `.selected` matching game | Scope under ADR module class root (`k45_adr_settings`) |
| Gameface incomplete CSS | Avoid unsupported shorthand; test scroll/overflow |
| Font-fit / ellipsis | Reuse vuio-commons patterns that already work in editor |
| Map SVG overlays | Test mask + pointer events after modularization |

## Suggested root class convention

Wrap settings panel content:

```tsx
<div className={styles.adrSettingsRoot}>
  …
</div>
```

Nest feature modules under that root in SCSS for safer overrides.

## Validation checklist

- [ ] Settings panel does not alter selected-info / tool option styling
- [ ] Road prefix tri-checkboxes readable in light/dark if applicable
- [ ] Map pan/zoom cursors and overlay colors match previous behavior
- [ ] Word list scroll performance acceptable with large namesets
- [ ] No missing CSS module imports (blank unstyled panels)
- [ ] Production bundle sets `hasCSS` correctly
