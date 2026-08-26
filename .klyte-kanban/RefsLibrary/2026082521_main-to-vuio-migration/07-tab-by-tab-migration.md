# 07 — Tab-by-tab migration plan

Ordered implementation guide for the big-bang port. Target: finish all rows before enabling EUIS hard cut.

## Target folder layout

```
_Frontends/UI/k45_adr_vuio/src/
  index.tsx                          # + GamePanelType + gamePanelComponents + UniversalModMenu
  mods/                              # UNCHANGED (selection / editor wiring; Editor Portal stays here only)
  tool/                              # UNCHANGED
  components/                        # UNCHANGED contextual UI
  settings/                          # NEW
    AdrUniversalModButton.tsx        # toggleGamePanel(AdrSettingsPanelId)
    AdrSettingsPanel.tsx             # GamePanel component (onClose + centerLeftLayout)
    AdrSettingsPanelId.ts            # or export const from panel file
    AdrSettingsNav.tsx               # optional: wrap ListWithContentTab
    translate.ts                     # OR import shared helper that uses K45::ADR.main
    tabs/
      namesets/
        CityNamesetLibraryCmp.tsx
        Nameset*.tsx                 # ported fileManagement/* (no CategoryCmp / categorizeFiles)
      overrides/
        OverrideSettingsCmp.tsx
        OverrideCitizenTab.tsx
        OverrideRoadsDistrictsTab.tsx
        AddressFormatTab.tsx
        VehiclePlateControllerComponent.tsx
        VehicleSerialControllerComponent.tsx
      roadPrefix/
        RoadPrefixCmp.tsx
      region/
        RegionEditor.tsx
        MapDiv.tsx
        subpages/
          HighwayRegisterManagement.tsx
          RegionCitiesManagement.tsx
          _GenericListing.tsx
    styles/                          # CSS modules for migrated features
      …
```

Shared services: prefer **Path A** — inline under `settings/` or `src/adr/` after `main` is gone ([08](08-shared-libs.md)). Path B keeps `@klyte45/adr-commons` only if retargeted off EUIS.

Suggested new vuio-commons widgets (if needed globally):

- `FormLine` / expose multistate `Checkbox` / `ErrorBoundary` / color input — see [04](04-widget-conversion-map.md)

---

## Milestone order

Work left-to-right; later milestones may start scaffolding earlier, but **do not ship / cut EUIS** until M5 is green.

### M0 — Shell + UMM (no feature tabs)

| Task | Source | Target |
|------|--------|--------|
| Typings `UniversalModMenu` | modding.d.ts | vuio + vuio-commons |
| Extend `GamePanelType` + `gamePanelComponents` | VS/WE pattern; TransportOverview host | `index.tsx` + `AdrSettingsPanel.tsx` |
| UMM button → `toggleGamePanel` | — | `AdrUniversalModButton.tsx` |
| Empty panel + `centerLeftLayout` + city gate | `root.component.tsx` | `AdrSettingsPanel.tsx` sized **70vh × 80vw** (tunable later) |
| Side list for 4 tabs (placeholders) | — | nav using `ListWithContentTab` |

Exit: Mods glyph + UMM button open/close ADR **GamePanel** (not Portal); gate message; panel closes when another overview opens.

### M1 — Overrides (forms-heavy, no map)

| Source under `main/src/components/overrides/` | Notes |
|-----------------------------------------------|-------|
| `OverrideSettingsCmp.tsx` | Replace `Cs2SideTabs` with nested **`ListWithContentTab`** (H2 → `{ section }`; 14 tabs on left; content on right) |
| `OverrideCitizenTab.tsx` | Selects + checkboxes + number inputs |
| `OverrideRoadsDistrictsTab.tsx` | Per-district list |
| `AddressFormatTab.tsx` | Pattern preview |
| `VehiclePlateControllerComponent.tsx` | MultiUI bindings from **vuio-commons** |
| `VehicleSerialControllerComponent.tsx` | Same |
| `vehiclePlateController.scss` | → CSS module |

Exit: all 14 subtabs editable; bindings persist; nameset dropdowns update on city nameset change.

### M2 — Namesets (screen machine + flat list)

| Source under `main/src/components/fileManagement/` | Replacement notes |
|----------------------------------------------------|-------------------|
| `CityNamesetLibraryCmp.tsx` | Keep screen enum; **DEFAULT** = `ListWithContentTab` (or equivalent) with **flat raw names** on the left — no hierarchy |
| `NamesetLibrarySelectorCmp.tsx` | **`FilePickerDialog`** + disk `generateDataProvider`; keep only ADR confirm flow after pick if needed |
| `NamesetGitHubSelectorCmp.tsx` | **Same `FilePickerDialog`** + GitHub `generateDataProvider`; on listing/rate-limit failure → **error modal** with Open GitHub (`goToRepository`) + Cancel; verify XHR from game UI |
| `NamesetImportingCmp.tsx` / `NamesetEditorCmp.tsx` / `NamesetViewingCmp.tsx` / `NamesetDeletingCmp.tsx` | String prompts → `StringInputDialog` / overrides; browse/pick → `FilePickerDialog` only |
| `NamesetCategoryCmp.tsx` | **Do not port** — hierarchy not used |
| `NamesetGitHubCategoryCmp.tsx` / `NamesetGitHubLineViewer.tsx` | Drop browse UI in favor of GitHub `FilePickerDialog` |
| `NamesetLineViewer.tsx` | Fold into list selection + actions toolbar / right pane |
| `NamesetWordsContainer.tsx` | `GameScrollComponent` → `EditorScrollable` |
| `utility/categorizeFiles.tsx` | **Do not port** — flat `listCityNamesets()` array is enough |

Exit: full CRUD + disk library + GitHub import + export; city library shows **raw names** flat on the left.

### M3 — Road prefix

| Source | Notes |
|--------|-------|
| `RoadPrefixCmp.tsx` | Needs game **`Checkbox` + `multistate`** exposed in vuio-commons first (map `null`↔`undefined` if needed) |
| `styles/roadPrefixRuleEditor.scss` | CSS module |
| Defaults save/load UI | If user picks an XML path on disk, use **`FilePickerDialog`** (`allowedExtensions` for `.xml`); do not invent a picker |

Exit: rule reorder, flag tri-states, save/load defaults XML.

### M4 — Region (list editors without map polish)

| Source | Notes |
|--------|-------|
| `RegionEditor.tsx` shell | Replace `react-tabs` with vanilla tab strip |
| `HighwayRegisterManagement.tsx` | Listing + form |
| `RegionCitiesManagement.tsx` | Color input port |
| `_GenericListing.tsx` | Scrollable list |

Exit: highways + cities editable; map can be stubbed with “select on map” disabled if needed **only during development** — not for cutover.

### M5 — Region map (parity gate for big-bang)

| Source | Notes |
|--------|-------|
| `MapDiv.tsx` | Highest risk |
| `map.scss`, `regionEditor.scss`, overlays | CSS modules; Gameface SVG/mask quirks |
| `utility/svgutils.ts` | Port helper |

**Must work before cutting EUIS:**

- [ ] Pan / zoom
- [ ] Double-click point selection feeds highway start ref / city tools
- [ ] Outside connection markers
- [ ] Highway / rail / urban overlays
- [ ] Neighborhood sector overlays (None/Land/Water/Air)
- [ ] Reload + coordinate HUD
- [ ] Highlight selected highway path

### M6 — Hard cut (same sprint after M5)

See [09-euis-decommission.md](09-euis-decommission.md).

---

## Porting rules (apply to every file)

1. Replace `@klyte45/euis-components` imports per [04](04-widget-conversion-map.md)
2. Replace `#utility/translate` with settings translate helper that still uses **`K45::ADR.main[...]`**
3. Keep `@klyte45/adr-commons` service calls unchanged
4. Prefer CSS modules; no global EUIS form SCSS import
5. Do not bundle `react` / `react-dom` / `react-tabs` / `react-select` into VUIO (externals / replace)
6. Fix listener cleanup on every screen

## Suggested PR / commit slicing (implementation sprint)

1. Shell + UMM + empty tabs
2. Overrides
3. Namesets
4. Road prefix (+ expose multistate `Checkbox` in vuio-commons)
5. Region lists
6. Region map
7. EUIS decommission + delete `main`

Big-bang means these land before a player-facing release that removes EUIS — not that they must be one git commit.

## Explicit non-goals during port

- Do not move selection-info / tool / editor into `settings/`
- Do not rename `k45::adr.*` binders
- Do not rename i18n keys in this sprint ([10](10-i18n-and-assets.md))
