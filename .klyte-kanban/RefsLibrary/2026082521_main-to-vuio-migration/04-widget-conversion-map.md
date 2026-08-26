# 04 — Widget conversion map

EUIS widgets used by `main` → VUIO / vanilla replacements. Confirmed strategy: **rebuild with vanilla**, do not port `Cs2SideTabs` or `MainSideTabMenuComponent` as-is.

## Primary table

| From `@klyte45/euis-components` | Used in `main` for | Target replacement | Action |
|--------------------------------|--------------------|--------------------|--------|
| `MainSideTabMenuComponent` / `MenuItem` | Root 4-tab shell | `Panel` + `ListWithContentTab` (or custom side list) | Rebuild |
| `DefaultPanelScreen` | Almost every sub-screen title/subtitle/buttons | `PanelSection` + title row + action `ToolButton`s | Rebuild thin helper in vuio-commons if repeated |
| `Cs2SideTabs` | Override 14 tabs + H2 headers | Vanilla selectable list + `section` rows (`ListWithContentTab` already supports section headers) | **Rebuild — do not port** |
| `Cs2FormLine` / `Cs2FormBoundaries` | Form rows everywhere | `InfoRow` / editor item theme / new `FormLine` in vuio-commons | Prefer new thin FormLine matching game rem layout |
| `Cs2Select` + patched `react-select` | Nameset & plate dropdowns | Vanilla dropdown via `VanillaComponentResolver` **or** `DropdownDialog` | Drop react-select from ADR |
| `Cs2CheckboxWithLine` | Flags / toggles | Vanilla checkbox + label row | Resolve from game modules |
| `Cs2TriCheckbox` | Road prefix required/forbidden flags | **New** `TriCheckbox` in vuio-commons | Must implement (null / true / false) |
| `Input` / `SimpleInput` / `NumberSimpleInput` | Text / numbers | `EllipsisTextInput` / vanilla inputs | Map props carefully (maxLength, blur commit) |
| `ColorRgbInput` | Region city map color | Vanilla color field or small port | Document game color picker module if found; else port |
| `GameScrollComponent` | Lists, word containers | `VanillaWidgets.instance.EditorScrollable` | Direct |
| `EuisTreeView` | Nameset folder trees | `K45HierarchyMenu` | Adapt tree → flat viewport model |
| `MultiUIValueBinding` / `InitializeBindings` / `ConstructorObjectToInstancesObject` / `BindingClassObj` | Plates / serials | `@klyte45/vuio-commons` `MultiUIValueBinding` + `MultiUIValueBindingTools.InitializeBindings` | Switch imports; verify API parity |
| `ErrorBoundary` | Root shell | Port minimal boundary to vuio-commons | Recommended |
| `replaceArgs` / `setupSignificance` | GitHub line viewer | Small util port or inline | Port helpers if still needed |

## vuio-commons already available (prefer these)

From `_Frontends/UI/_shared/vuio-commons/index.tsx`:

| Export | Use for |
|--------|---------|
| `VanillaComponentResolver` | ToolButton, EllipsisTextInput, etc. |
| `VanillaWidgets` | EditorScrollable, themed bits |
| `MultiUIValueBinding` (+ Tools) | Plate/serial bindings |
| `ListWithContentTab` / `ListWithPreviewTab` | Root nav + override subnav |
| `K45HierarchyMenu` | Nameset trees |
| `DropdownDialog` | Select alternatives |
| `StringInputDialog` / `StringInputWithOverrideDialog` / `BaseStringInputDialog` | Import/edit name prompts |
| `FilePickerDialog` | Disk paths if applicable |
| `ContextMenuButton` | Row overflow actions |
| `AmountValueSection` / `VectorSection` | Numeric/vector editors if needed |

## Navigation-specific notes

### Root tabs

EUIS used `react-tabs` inside `MainSideTabMenuComponent`. VUIO must **not** add `react-tabs` as a dependency (not an external). Use:

- `ListWithContentTab` with four list items, **or**
- Local state + button strip styled with vanilla theme classes

### Override subtabs

Rebuild `tabsOrder` from `OverrideSettingsCmp` as:

```ts
type NavEntry =
  | { type: "tab"; id: TabsNames; label: string }
  | { type: "section"; title: string };
```

Render sections as non-clickable `listItems` entries with `{ section: title }`, tabs as `{ displayName, value }`.

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

## New widgets to add under vuio-commons (recommended before / during M1–M3)

| Widget | Why |
|--------|-----|
| `FormLine` | Dense settings forms; avoid reimplementing 50 times |
| `TriCheckbox` | Road prefix flags |
| `ErrorBoundary` | Settings panel isolation |
| `ColorField` (optional) | Region cities |

Keep them generic (no ADR branding) so other K45 VUIO mods can reuse them.

## Explicit do-not-port list

| EUIS piece | Reason |
|------------|--------|
| `MainSideTabMenuComponent` + `mainSideTabs.scss` | Wrong host chrome; vanilla-first decision |
| `Cs2SideTabs` | Rebuild with list patterns |
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
import { VanillaComponentResolver, DropdownDialog /* or FormLine */ } from "@klyte45/vuio-commons";
import { PanelSection } from "cs2/ui";
```

Exact widget choice is left to implementers as long as behavior matches [02](02-feature-inventory.md).
