# 02 — Feature inventory

Complete feature matrix of `_Frontends/UI/main/` that must reach parity inside `k45_adr_vuio` before EUIS is removed.

## Root shell

Source: `main/src/root.component.tsx`

| # | Tab key | Translate key | Component | Notes |
|---|---------|---------------|-----------|-------|
| 1 | Namesets | `namesetManagement.title` | `CityNamesetLibraryCmp` | Screen state machine |
| 2 | Overrides | `overrideSettings.title` | `OverrideSettingsCmp` | Nested **`ListWithContentTab`** (subtabs + sections) |
| 3 | Road prefix | `roadPrefixSettings.title` | `RoadPrefixCmp` | Rule list + editor |
| 4 | Region | `regionSettings.title` | `RegionEditor` | Map + 2 data tabs |

Gate: `AdrMainService.isCityOrEditorLoaded()` → warning `loadACityWarningH2`.

---

## Tab 1 — Nameset library

Source folder: `main/src/components/fileManagement/`

### Screens (`CityNamesetLibraryCmp` enum)

| Screen | Component | User actions |
|--------|-----------|--------------|
| `DEFAULT` | Flat list of city namesets | Import library, Import GitHub, Create; select row → view/edit/delete/export actions |
| `NAMESET_IMPORT_LIB` | `NamesetLibrarySelectorCmp` | Browse disk library; pick nameset — **`FilePickerDialog`** + disk provider |
| `NAMESET_IMPORT_GITHUB` | `NamesetGitHubSelectorCmp` | Browse GitHub via **`FilePickerDialog`** + GitHub provider; on fetch/rate-limit errors → **modal with Open GitHub + Cancel** |
| `IMPORTING_NAMESET` | `NamesetImportingCmp` | Confirm path/name then import |
| `EDIT_NAMESET` | `NamesetEditorCmp` | Edit name path + values textarea |
| `VIEWING_NAMESET` | `NamesetViewingCmp` | Read-only word list |
| `DELETE_CONFIRM` | `NamesetDeletingCmp` | Confirm delete |
| `AWAITING_ACTION` | inline loading | Async GitHub/disk ops |

### Supporting components

| File | Purpose |
|------|---------|
| `NamesetCategoryCmp.tsx` | **Do not port** — city namesets are a **flat raw-name list** on the left (`ListWithContentTab`), not a hierarchy |
| `NamesetLineViewer.tsx` | Row actions may fold into list selection + right-pane / toolbar actions |
| `NamesetGitHubCategoryCmp.tsx` / `NamesetGitHubLineViewer.tsx` | **Superseded for browse/pick** by GitHub-backed `FilePickerDialog` |
| `NamesetWordsContainer.tsx` | Scrollable dual-value word list (detail pane) |

### City nameset list UX (VUIO simplification)

- **Left:** flat list of nameset **raw names** (full path/name string as stored — no folder tree / `K45HierarchyMenu`)
- **Right:** content for the selected nameset (view/edit) or empty state
- Prefer `ListWithContentTab` / `ListWithPreviewTab`
- **Do not** port `categorizeFiles` / `NamesetCategoryCmp` / `EuisTreeView` for this screen — simplifies data handling (plain array from `NamesetService.listCityNamesets`)

### Utilities / services

| Piece | Role |
|-------|------|
| `utility/categorizeFiles.tsx` | **Drop** for VUIO city library (hierarchy not used) |
| `NamesetService` | CRUD / list / export / change events |
| `GitHubAddressesFilesService` | GitHub API + `atob` bridge |

---

## Tab 2 — Override settings

Source folder: `main/src/components/overrides/`

Host today: `OverrideSettingsCmp` with `Cs2SideTabs` + H2 headers.

**VUIO:** nest a second **`ListWithContentTab`** inside the Overrides root-tab body:

- **Left:** subtab entries + non-selectable `{ section: … }` headers (Vehicle identifiers / Road vehicle serial)
- **Right:** active subtab content (`OverrideCitizenTab`, plates, serials, …)

Do not port `Cs2SideTabs`.

### Subtabs

| Tab id | UI component | Settings |
|--------|--------------|----------|
| `Citizen` | `OverrideCitizenTab` | Male/female/surname/dog namesets; surname-first; max given/surnames (1–5) |
| `RoadsDistricts` | `OverrideRoadsDistrictsTab` | District/road namesets; road→station/cargo flags; per-district road nameset |
| `AddressFormat` | `AddressFormatTab` | Pattern `{number}` `{street}` `{district}` `{brand}` + live preview |
| *(H2)* Vehicle identifiers | — | Section header only |
| `RoadPlates` / `RailPlates` / `WaterPlates` / `AirPlates` | `VehiclePlateControllerComponent` | Digit alphabet, local/car/random, month offset, serial increment, preview |
| *(H2)* Road vehicle serial | — | Section header only |
| `BusSerial` / `TaxiSerial` / `PoliceSerial` / `FiretruckSerial` / `AmbulanceSerial` / `GarbageSerial` / `PostalSerial` / `DeathcareSerial` | `VehicleSerialControllerComponent` | Building city-id position; digit sources |

### Shared host behaviors

- Load city namesets → select options (`defaultSetting`, `defaultSettingRoadByDistrict`)
- Subscribe `NamesetService.doOnCityNamesetsUpdated`
- Load `NamingRulesService.getCurrentCitywideSettings` + `onCityDataReloaded`
- Load districts via `DistrictRelativeService` + change events
- Plates/serials use MultiUIValueBinding (`InitializeBindings`)

---

## Tab 3 — Road prefix rules

Source: `main/src/components/roadPrefix/RoadPrefixCmp.tsx`  
Styles: `styles/roadPrefixRuleEditor.scss`

| Feature | Detail |
|---------|--------|
| Fallback format | Pattern must include `{name}` |
| Rule list | Ordered; add / reorder / delete |
| Per-rule editor | Pattern, min/max speed, road flags (tri-state), bridge/elevated, car lanes, width |
| Defaults IO | Save/load XML (`DefaultRoadPrefixRules.xml`) via NamingRulesService |

Uses road-prefix flags that today go through EUIS `Cs2TriCheckbox`. **VUIO:** use game `Checkbox` with `multistate` (already in `game-ui/common/input/toggle/checkbox`; statistics panel uses it). vuio-commons currently types only boolean `checked` — **expose multistate** rather than inventing a new control.

---

## Tab 4 — Region editor

Source folder: `main/src/components/region/`

### Layout

| Area | Component | Features |
|------|-----------|----------|
| Left | `MapDiv` + map SCSS | Zoom/pan, double-click point select, outside-connection markers, highway/rail/urban overlays, neighborhood sectors (None/Land/Water/Air), reload, coordinate HUD |
| Right | `react-tabs` panels | Highway register + Region cities |

### Data tabs

| Tab | Component | Features |
|-----|-----------|----------|
| Highway register | `HighwayRegisterManagement` | List/add/edit (prefix, suffix, name, start ref from map); map highlight paths |
| Region cities | `RegionCitiesManagement` | Filter land/water/air; name, azimuth span, reachability, map color |

Shared: `_GenericListing.tsx`  
Services: `RegionService`, `HighwayRoutesService`  
Styles: `regionEditor.scss`, `map.scss`, `tabCommons.scss`, `HighwayListingTab.scss`, `RegionCitiesManagement.scss`

**Highest risk** for big-bang parity — see testing checklist in [06](06-behaviors-and-bindings.md) and [07](07-tab-by-tab-migration.md).

---

## Not in `main` (already VUIO — do not rebuild as settings tabs)

| Surface | Location |
|---------|----------|
| Selected-info ADR sections | `AddressesInfoOptionsComponent` + children |
| Editor floating panel | `EditorBindings` |
| Road-marker tool options | `AddressesToolOptions` |

These stay as-is. Settings migration is additive.

---

## Parity checklist (acceptance)

- [ ] All 4 root tabs present and gated
- [ ] All 8 nameset screens reachable with same actions
- [ ] All 14 override content tabs + 2 H2 headers
- [ ] Road prefix rules CRUD + defaults XML
- [ ] Region map interactions + highway + cities editors
- [ ] No intentional feature drop for big-bang cutover
