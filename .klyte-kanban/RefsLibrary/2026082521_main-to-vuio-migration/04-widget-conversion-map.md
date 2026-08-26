# 04 — Widget conversion map

EUIS widgets used by `main` → VUIO / vanilla replacements. Confirmed strategy: **rebuild with vanilla**, do not port `Cs2SideTabs` or `MainSideTabMenuComponent` as-is.

## Primary table

| From `@klyte45/euis-components` | Used in `main` for | Target replacement | Action |
|--------------------------------|--------------------|--------------------|--------|
| `DefaultPanelScreen` | Almost every sub-screen title/subtitle/buttons | **Not needed on VUIO** — compose with GamePanel / `Panel` / `ListWithContentTab` / dialogs / local headers + `ToolButton`s | **Drop** — do not port or recreate in vuio-commons |
| `MainSideTabMenuComponent` / `MenuItem` | Root 4-tab shell | **GamePanel** (`centerLeftLayout` + common `Panel`) + `ListWithContentTab` | Rebuild — see [03](03-shell-and-umm.md); **not** Editor `Portal` |
| `Cs2SideTabs` | Override 14 tabs + H2 headers | **Nested `ListWithContentTab`** (section rows + selectable tabs; content on the right) | **Do not port** — reuse vuio-commons list |
| `Cs2FormLine` / `Cs2FormBoundaries` | Form rows everywhere | `InfoRow` / editor item theme / new `FormLine` in vuio-commons | Prefer new thin FormLine matching game rem layout |
| `Cs2Select` + patched `react-select` | Nameset & plate dropdowns | Vanilla dropdown via `VanillaComponentResolver` **or** `DropdownDialog` | Drop react-select from ADR |
| `Cs2CheckboxWithLine` | Flags / toggles | Vanilla checkbox + label row | Resolve from game modules |
| `Cs2TriCheckbox` | Road prefix required/forbidden flags | Game `Checkbox` with **`multistate`** (already in UI modules; not fully typed in vuio-commons yet) | **Expose in VUIO** — see below; thin ADR/`TriCheckbox` wrapper only if mapping `null`↔`undefined` is needed |
| `Input` / `SimpleInput` / `NumberSimpleInput` | Text / numbers | `EllipsisTextInput` / vanilla inputs | Map props carefully (maxLength, blur commit) |
| `ColorRgbInput` | Region city map color | Vanilla color field or small port | Document game color picker module if found; else port |
| `GameScrollComponent` | Lists, word containers | `VanillaWidgets.instance.EditorScrollable` | Direct |
| `EuisTreeView` | Nameset folder trees (EUIS) | **Not needed** for city namesets — flat raw-name list via `ListWithContentTab` | Drop for ADR settings; keep `K45HierarchyMenu` only if another surface needs trees |
| `MultiUIValueBinding` / `InitializeBindings` / `ConstructorObjectToInstancesObject` / `BindingClassObj` | Plates / serials | `@klyte45/vuio-commons` `MultiUIValueBinding` + `MultiUIValueBindingTools.InitializeBindings` | Switch imports; verify API parity |
| `ErrorBoundary` | Root shell | Port minimal boundary to vuio-commons | Recommended |
| `replaceArgs` / `setupSignificance` | Was GitHub line viewer | Port only if still needed outside superseded GitHub picker UI | Optional |

## vuio-commons already available (prefer these)

From `_Frontends/UI/_shared/vuio-commons/index.tsx`:

| Export | Use for |
|--------|---------|
| `VanillaComponentResolver` | ToolButton, EllipsisTextInput, etc. |
| `VanillaWidgets` | EditorScrollable, themed bits |
| `MultiUIValueBinding` (+ Tools) | Plate/serial bindings |
| `ListWithContentTab` / `ListWithPreviewTab` | Root nav + override subnav |
| `K45HierarchyMenu` | **Not used** for ADR city namesets (flat list). Available for other mods/surfaces if needed |
| `DropdownDialog` | Select alternatives |
| `StringInputDialog` / `StringInputWithOverrideDialog` / `BaseStringInputDialog` | Import/edit name prompts |
| `FilePickerDialog` | **Required** for disk **and** GitHub browse/pick — do **not** rebuild EUIS folder/tree browsers |
| `ContextMenuButton` | Row overflow actions |
| `AmountValueSection` / `VectorSection` | Numeric/vector editors if needed |

## File / remote pickers (mandatory)

Whenever the ported UI needs the user to browse folders and pick a file — on **disk or GitHub** — use existing vuio-commons:

| Piece | Path |
|-------|------|
| Dialog UI | `@klyte45/vuio-commons` → `FilePickerDialog` |
| Disk listings | `BaseFileService.generateDataProvider` / ADR `*.file.listFiles` binders |
| GitHub listings | Custom `generateDataProvider` mapping `GitHubAddressesFilesService.listAtTreePoint` → `DataProviderItem[]` (`directory: true` for trees, `false` for blobs; `fullPath` = API url or path) |

Contract reminders from `FilePickerDialog`:

- Controlled via `isActive` / `setIsActive`
- Pass `allowedExtensions`, `initialFolder`, `generateDataProvider`, `translate`, `actionOnSuccess`
- Optional `bookmarks` (disk: mod/library roots; GitHub: repo root / known tree URLs if useful)
- Optional `resolveFileItemPresentation` for previews / invalid rows (not a substitute for hard GitHub fetch failures)

**GitHub navigation specifically:**

- Replace `NamesetGitHubSelectorCmp` / `NamesetGitHubCategoryCmp` / `NamesetGitHubLineViewer` **browse UI** with `FilePickerDialog` + GitHub-backed provider
- Keep blob fetch / parse / import confirm after `actionOnSuccess` (existing `getBlobData` + import screen)
- **Fetch errors (required UX):** when GitHub listing fails — including free API rate / listing limits (`success: false`, optional `resetTime` from `GitHubResponseContainer`) — the open **`FilePickerDialog` must switch into an error modal** (blocking) with **exactly two actions**:
  1. **Open GitHub in browser** → `GitHubAddressesFilesService.goToRepository()` (same intent as today’s `githubLibrary.visitRepository`)
  2. **Cancel** → close the picker and abort the import flow (`setIsActive(false)`; no partial import)
- Error body should reuse existing i18n where possible (`githubListing.errorLoadingRateLimit` with formatted reset time, or `githubListing.errorLoadingMsg`)
- Implement by extending `FilePickerDialog` (preferred: shared error-modal mode usable by any provider) **or** ADR wrapper that replaces picker content on provider failure — do **not** leave a dead empty list with only inline text
- No parallel EUIS tree UI; no toast-only handling for these hard failures

**Do not:**

- Reimplement `NamesetLibrarySelectorCmp` or `NamesetGitHubSelectorCmp` custom trees for navigation
- Invent a second picker chrome for GitHub vs disk
- Use Editor/EUIS-only file chrome
- Handle rate-limit only with inline list text and no Cancel / Open-repo buttons

String-only prompts (nameset name path after pick) still use `StringInputDialog` / `StringInputWithOverrideDialog` — separate from browse/pick.

## Navigation-specific notes

### Root tabs

EUIS used `react-tabs` inside `MainSideTabMenuComponent`. VUIO must **not** add `react-tabs` as a dependency (not an external). Use:

- `ListWithContentTab` with four list items, **or**
- Local state + button strip styled with vanilla theme classes

### Override subtabs

Replace `Cs2SideTabs` with **another `ListWithContentTab`** (nested under the root settings `ListWithContentTab` when Overrides is selected).

Rebuild `tabsOrder` from `OverrideSettingsCmp` as:

```ts
type NavEntry =
  | { type: "tab"; id: TabsNames; label: string }
  | { type: "section"; title: string };
```

Map to `listItems`: sections as `{ section: title }`, tabs as `{ displayName, value }`. Right pane renders the selected subtab component.

### Region horizontal tabs

Replace `Tab` / `TabList` / `TabPanel` from `react-tabs` with two buttons or a small local tab strip. Do not pull `react-tabs` into VUIO webpack.

## MultiUIValueBinding parity checklist

`main` imports from euis-components:

- `MultiUIValueBinding`
- `InitializeBindings`
- `ConstructorObjectToInstancesObject`
- `BindingClassObj`

vuio-commons exposes:

- `MultiUIValueBinding`
- `MultiUIValueBindingTools.InitializeBindings`
- Related constructor types in the same file

**Implementer task:** update plate/serial components to vuio API names; run in-game preview for all plate types and serial vehicle types.

## New / expose under vuio-commons (recommended before / during M1–M3)

| Widget | Why |
|--------|-----|
| `FormLine` | Dense settings forms; avoid reimplementing 50 times |
| **Expose game multistate `Checkbox`** | Road prefix flags — **not a greenfield TriCheckbox**. Game already ships three states |
| `ErrorBoundary` | Settings panel isolation |
| `ColorField` (optional) | Region cities |
| Optional: **`FilePickerDialog` error-modal mode** | GitHub rate-limit UX |

### Game three-state checkbox (verified via UI modules)

Source: `game-ui/common/input/toggle/checkbox/checkbox.tsx` + `toggle.tsx`

| Fact | Detail |
|------|--------|
| API | `Checkbox` accepts `multistate` (and passes through to `Toggle`) |
| Third state | `checked === undefined` with `multistate: true` → CSS class `'undefined'` |
| Change handler | `Toggle` uses `onMultistateChange` when `multistate` is true; cycles roughly `undefined → false → true → undefined` |
| In-game usage | e.g. statistics panel item (`multistate: e.isSubgroup`) |
| vuio-commons today | `VanillaWidgets.Checkbox` resolves the same module, but **`CheckboxProps` only types `checked: boolean` + `onChange: (boolean) => …`** — multistate not exposed |

**Plan:** widen `CheckboxProps` (and/or add a small `TriCheckbox` helper) to support `multistate` + `onMultistateChange` / `checked?: boolean`. Map ADR/EUIS `null` (forbid/required-forbidden middle) ↔ game `undefined` if road-prefix C# still uses null semantics.

**Do not** reinvent checkmark CSS from EUIS `forbid` class unless game `'undefined'` styling is insufficient after visual check.

Keep widgets generic (no ADR branding) so other K45 VUIO mods can reuse them.

## Explicit do-not-port list

| EUIS piece | Reason |
|------------|--------|
| `DefaultPanelScreen` | EUIS screen chrome; unnecessary under GamePanel + list/dialog layouts |
| `MainSideTabMenuComponent` + `mainSideTabs.scss` | Wrong host chrome; vanilla GamePanel decision |
| Editor `Portal` + absolute `Panel` for settings | Editor-only; breaks main `GamePanelRenderer` stack — use [03](03-shell-and-umm.md) |
| Custom EUIS disk **or GitHub** folder browsers for pick | Use **`FilePickerDialog`** + disk/`BaseFileService` or GitHub `generateDataProvider` |
| City nameset hierarchy (`NamesetCategoryCmp` / `categorizeFiles` / `EuisTreeView`) | Flat raw-name list on the left of `ListWithContentTab` instead |
| `Cs2SideTabs` | Nested **`ListWithContentTab`** for Override subtabs |
| Patched `react-select` tarball | Cohtml hacks for EUIS; use vanilla |
| `react-tabs` | Not a game external; replace |
| Emotion cache / systemjs-webpack-interop | EUIS/single-spa only |

## File-level import rewrite examples

**Before (main):**

```ts
import { DefaultPanelScreen, Cs2FormLine, Cs2Select } from "@klyte45/euis-components";
```

**After (vuio settings):**

```ts
import { VanillaComponentResolver, DropdownDialog, ListWithContentTab /* or FormLine */ } from "@klyte45/vuio-commons";
// No DefaultPanelScreen — content lives inside GamePanel / ListWithContentTab / dialogs
```

Exact widget choice is left to implementers as long as behavior matches [02](02-feature-inventory.md). Do **not** recreate `DefaultPanelScreen` in vuio-commons.
