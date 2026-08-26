# 06 — Behaviors and bindings

Behaviors that must be reviewed when leaving the EUIS iframe for the game UI module.

## Binding model (unchanged contracts)

| Layer | Mechanism |
|-------|-----------|
| RPC | `engine.call("k45::adr.<system>.<method>", …)` via adr-commons |
| Push | `engine.on` / `engine.off` (namesets updated, citywide reloaded, districts changed) |
| Two-way UI | MultiUIValueBinding (`prefix?` / `prefix!` / `prefix->`) for plates/serials |
| C# | `IBelzontBindable` systems; EUIS bridge prefixes already `k45::adr.*` |

**Do not** change C# call names for the UI host migration.

### Service map (keep)

| Service | Prefix / responsibility |
|---------|-------------------------|
| `AdrMainService` | `isCityOrEditorLoaded`, `atob` |
| `NamingRulesService` | Citywide get/set; road prefix defaults file |
| `NamesetService` | Nameset CRUD / list / export / events |
| `DistrictRelativeService` | Districts + per-district road nameset |
| `RegionService` | Map/region/neighborhood |
| `HighwayRoutesService` | Highway registry |
| `GitHubAddressesFilesService` | GitHub tree/blob + rate limit |
| Plate/Serial UI | `k45::adr.vehiclePlate.*` / `vehicleSerial.*` |

---

## Lifecycle & listeners (critical)

EUIS unmounted the whole microfrontend. VUIO settings panel can hide/show repeatedly.

| Behavior today | Review requirement |
|----------------|--------------------|
| `NamesetService.doOnCityNamesetsUpdated` in `CityNamesetLibraryCmp` / `OverrideSettingsCmp` | Subscribe in `useEffect`; **always** `off*` on cleanup |
| `NamingRulesService.onCityDataReloaded` | Same |
| `DistrictRelativeService.onDistrictChanged` | Same |
| Plate/serial MultiUI subscriptions | Dispose bindings when leaving plate tab or closing panel (match vuio-commons dispose API) |
| GitHub in-flight requests | Cancel or ignore stale responses when screen changes |

Anti-pattern to fix during port: registering `doOn*` at render body without cleanup (current `CityNamesetLibraryCmp` pattern is fragile — fix while migrating).

---

## City / editor loaded gate

- Call `AdrMainService.isCityOrEditorLoaded()` when panel opens / becomes visible
- Show `loadACityWarningH2` instead of tabs when false
- UMM may be reachable from menu scenes — do not assume a city is loaded
- Re-validate after load if user leaves panel open across scene transitions (if detectable)

---

## MultiUIValueBinding (plates / serials)

1. Switch imports from euis-components → vuio-commons (`MultiUIValueBindingTools.InitializeBindings`)
2. Confirm constructor object `_prefix` pattern still matches C# controllers
3. Retest **each** plate type (Road/Rail/Water/Air) and **each** serial type (8 service vehicles)
4. Preview fields must update live when digits/flags change

---

## GitHub nameset import

Today: XHR to GitHub API + `AdrMainService`/`main.atob` for blob decode + open-repo helper + custom tree UI (`NamesetGitHubSelectorCmp`). Rate-limit today shows inline error text (`githubListing.errorLoadingRateLimit` / `errorLoadingMsg`) plus a separate “visit repository” button on the selector screen.

**UI rule:** GitHub folder navigation and `.txt` selection use the **same** vuio-commons **`FilePickerDialog`** as disk import. Supply a `generateDataProvider` that calls `GitHubAddressesFilesService.listAtTreePoint` and maps trees/blobs to `DataProviderItem`. After confirm, keep existing blob fetch + parse + import screen.

### Fetch / rate-limit error modal (required)

When listing fails (HTTP errors, free-tier rate limits, `GitHubResponseContainer.success === false`):

| Element | Behavior |
|---------|----------|
| Host | The **FilePickerDialog** itself becomes / shows a **blocking error modal** (not a silent empty list) |
| Message | Rate-limit: `githubListing.errorLoadingRateLimit` + `resetTime` when present; else `githubListing.errorLoadingMsg` |
| Button 1 | **Open GitHub in browser** → `GitHubAddressesFilesService.goToRepository()` |
| Button 2 | **Cancel** → close dialog and abort import (`setIsActive(false)`) |

Do not rely on toast-only or inline-only error text without those two actions.

Review:

| Topic | Question to verify in VUIO |
|-------|----------------------------|
| Network | Does game Cohtml allow GitHub HTTPS from UI module context? (Expect yes; confirm) |
| Rate limits | Error modal with Open-repo + Cancel appears on limit / listing failure |
| CORS / headers | Same as EUIS or stricter? |
| `atob` bridge | Still required? Keep C# binder |
| Dev logging | `process.env.NODE_ENV` may differ under VUIO webpack — check define plugin |
| Provider mapping | Tree → `directory: true`; blob `.txt` → selectable file; nested navigation via folder clicks |
| Provider errors | Provider must surface failure (throw or structured error) so the dialog can enter error-modal mode |

Retest:

- [ ] Open GitHub import via `FilePickerDialog`
- [ ] Navigate nested folders; pick `.txt`; import succeeds
- [ ] Force / wait for rate-limit: **error modal** with **Open GitHub** + **Cancel** only
- [ ] Open GitHub opens browser/repo; Cancel closes picker without import
- [ ] No leftover EUIS GitHub tree components in the shipped UI


---

## Disk library / file IO

Nameset disk library and road-prefix XML defaults go through C# binders (list dir, open folder, reload, save/load). Host change should be transparent.

**UI rule:** any interactive browse/pick — **disk or GitHub** — uses **`FilePickerDialog`** from `@klyte45/vuio-commons` (disk/`BaseFileService` or GitHub-backed `generateDataProvider`). Do not port EUIS custom folder/tree browsers for picking.

Retest:

- [ ] Open / browse disk library via `FilePickerDialog`
- [ ] Open / browse GitHub library via **same** `FilePickerDialog` + GitHub provider
- [ ] Reload library list
- [ ] Import to city
- [ ] Export from city
- [ ] Save/load `DefaultRoadPrefixRules.xml` (picker if path selection is UI-driven)

---

## Region map (`MapDiv`) — highest risk

Behaviors to retest after CSS-module + React host change:

| Behavior | Pass criteria |
|----------|---------------|
| Pan | Drag moves viewport; no stuck pointer |
| Zoom | Wheel / buttons; clamp works |
| Double-click select | Writes selection used by highway start ref |
| Outside connections | Markers render at correct coords |
| Overlays | Highway / rail / urban toggles |
| Neighborhood sectors | None / Land / Water / Air |
| Reload | Refreshes terrain/water layers |
| Coordinate HUD | Updates with cursor/selection |
| Highway highlight | Selected route path visible |
| Performance | Acceptable FPS with overlays on |

Treat map as **release blocker** for EUIS removal ([07](07-tab-by-tab-migration.md) M5).

---

## Persistence expectations (no change)

| Data | Persistence |
|------|-------------|
| Citywide settings / namesets / regions / highways | C# + savegame / serializable defaults |
| Road prefix defaults file | Mod settings folder XML |
| Vehicle plate/serial | Bound C# properties |
| Library namesets | Disk folder |

UI must not invent a second settings store.

---

## State management

Still: local React `useState` / `useEffect` / `useMemo` only. No Redux.

Screen enums for namesets stay. Tab indices for overrides/region stay as local state inside the settings panel.

Optional: rely on `useGamePanelState(AdrSettingsPanelId)` / `activeGamePanel$` for open state — no custom Portal store.

---

## Cohtml / Gameface quirks to re-verify

- Tooltip attributes (`data-tooltip`) vs vanilla tooltip components
- Focus traps inside `Panel`
- Scroll nesting (list + form + map)
- Input blur commit patterns (ellipsis inputs)
- SVG mask icons for tinted UMM/ADR icons

---

## Regression: existing VUIO surfaces

After adding settings:

- [ ] Selected-info ADR sections still appear for stations / districts / vehicles / marks / sources
- [ ] EditorBindings panel still works
- [ ] Road-marker tool options still inject

Settings code must not break `index.tsx` extend/append order for those hooks.

---

## Test matrix (minimum)

| Area | Cases |
|------|-------|
| Shell | Open/close; gate; reopen after city load |
| Namesets | Create/edit/view/delete; disk import; GitHub import; export; **flat raw-name list** (no folder tree) |
| Overrides | Each subtab save; district override; address preview; all plates/serials |
| Road prefix | Add/reorder/delete; tri-state flags; defaults save/load |
| Region | Map interactions above; highway CRUD; city CRUD + color |
| Hard cut | Mod boots without EUIS installed |
