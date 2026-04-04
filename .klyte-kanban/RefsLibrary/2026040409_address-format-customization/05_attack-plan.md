# Address Format Feature — Attack Plan

*Authored: 2026-04-04 by Claude Sonnet 4.6*

## Feature summary

Allow players to customize the format of building addresses displayed in-game (e.g. change `42 Oak Avenue` to `Oak Avenue, 42` or `Oak Avenue 42, Downtown`). This involves:

1. Storing the custom format string in `AdrCitywideSettings` (with proper version bump)
2. Patching `AdrNameSystemOverrides` to apply the custom format
3. Supporting `{number}`, `{street}`, `{district}`, and `{brand}` placeholders
4. Adding a UI tab to the Override Settings panel

## Epics

| Epic | ID | Description |
|------|----|-------------|
| `addrfmt` | E-01 | Custom Address Format — end-to-end feature |
| `distlookup` | E-02 | District Lookup Utility — reusable helper for side-aware district resolution |

E-02 is a sub-epic that can also serve future features beyond address formatting (e.g. showing district name in info panels, per-district road format overrides).

---

## Sprint 003 (10 tasks)

### Task 0025 — `add-address-format-field-to-citywideSettings` (P1, epic: addrfmt)

**Background**: `AdrCitywideSettings` is an `ISerializable` class. Adding a field requires incrementing `CURRENT_VERSION` (currently 1 → must become 2).

**User story**: As a developer, I want `AdrCitywideSettings` to have an `AddressFormatPattern` string field so that custom address format can be persisted per city.

**DoD**:
- [ ] `CURRENT_VERSION` bumped 1 → 2
- [ ] New `addressFormatPattern` field (private, nullable string)
- [ ] Public getter/setter `AddressFormatPattern`
- [ ] `Serialize()` writes the new field (empty string for null)
- [ ] `Deserialize()` reads the field only if `version >= 2`
- [ ] No regression: old savegames (version 1) deserialize cleanly with `null` default
- [ ] Build passes

---

### Task 0026 — `bind-address-format-to-main-system` (P1, epic: addrfmt)

**Background**: `AdrMainSystem` exposes city settings to the frontend via `SetupCallBinder`. A new binding is needed for the address format field.

**DoD**:
- [ ] `"main.setAddressFormatPattern"` binding added to `SetupCallBinder`
- [ ] Setter clears null on empty string input
- [ ] Setter calls `MarkRoadsDirty()` and `NotifyChanges()`
- [ ] `getCurrentCitywideSettings` already returns the full `AdrCitywideSettings` object including new field *(no extra work needed, auto-serialized)*
- [ ] Build passes

---

### Task 0027 — `add-district-side-lookup-helper` (P2, epic: distlookup)

**Background**: To support `{district}` placeholder, we need to determine which district a building is in based on road-side geometry. The logic belongs in `AdrMainSystem` or a new static utility class.

**User story**: As a developer, I want a reusable method to get the district name for a building based on which side of the road it's on.

**DoD**:
- [ ] `GetBuildingSideDistrictName(Entity buildingEntity, NameSystem nameSystem)` method exists (on `AdrMainSystem` or `AdrNameSystemExtensions`)
- [ ] Uses `Building.m_RoadEdge` → `BorderDistrict` → side check via tangent/dot product
- [ ] Returns `null` if no district found on either side
- [ ] Returns district name string when found (via `NameSystem.GetRenderedLabelName`)
- [ ] Handles case where only one side has a district
- [ ] Build passes

---

### Task 0028 — `patch-spawnable-building-address-format` (P1, epic: addrfmt)

**Background**: `AdrNameSystemOverrides.GetSpawnableBuildingName` already formats building addresses. It needs to apply the custom format when set.

**DoD**:
- [ ] When `AddressFormatPattern` is null/empty, behavior is unchanged (uses `FormattedName("Assets.ADDRESS_NAME_FORMAT", ...)`)
- [ ] When `AddressFormatPattern` is set, token substitution is applied: `{number}`, `{street}`, `{district}`, `{brand}`
- [ ] `{district}` resolved via Task 0027 helper
- [ ] `{brand}` resolved only if brand exists, otherwise empty string
- [ ] Empty `{district}` / `{brand}` collapses surrounding double-spaces (trim pass)
- [ ] `NAMED_ADDRESS_NAME_FORMAT` path also applies custom format if set
- [ ] Build passes

---

### Task 0029 — `patch-transport-stop-address-format` (P1, epic: addrfmt)

**Background**: `AdrNameSystemOverrides.GetStaticTransportStopName` patches transport stop addresses. Same custom format logic needs to apply here.

**DoD**:
- [ ] When `AddressFormatPattern` is null/empty, behavior unchanged
- [ ] When set, applies token substitution (`{number}`, `{street}`, `{district}`)
- [ ] `{brand}` not applicable for transport stops (always empty)
- [ ] Build passes

---

### Task 0030 — `add-address-format-type-to-adr-commons` (P2, epic: addrfmt)

**Background**: The `AdrCitywideSettings` TypeScript type in `adr-commons` must include the new `AddressFormatPattern` field and a new `setAddressFormatPattern` method in `NamingRulesService`.

**DoD**:
- [ ] `AddressFormatPattern: string` added to `AdrCitywideSettings` type
- [ ] `static async setAddressFormatPattern(x: string): Promise<void>` added to `NamingRulesService`
- [ ] adr-commons package builds without errors

---

### Task 0031 — `create-address-format-tab-component` (P2, epic: addrfmt)

**Background**: A new `AddressFormatTab.tsx` component is needed for the Override Settings panel.

**DoD**:
- [ ] `AddressFormatTab` component created at `src/components/overrides/AddressFormatTab.tsx`
- [ ] Contains format pattern text input (single line)
- [ ] Contains clickable token chips: `{number}`, `{street}`, `{district}`, `{brand}`
- [ ] Contains live preview section with mock substitution (mock values: number=`42`, street=`"Oak Avenue"`, district=`"Downtown"`, brand=`"SuperMart"`)
- [ ] Contains "Use game default" reset button
- [ ] Calls `NamingRulesService.setAddressFormatPattern` on change
- [ ] Component compiles without TS errors

---

### Task 0032 — `integrate-address-format-tab-in-overrideSettings` (P2, epic: addrfmt)

**Background**: The new `AddressFormatTab` needs to be added to the `OverrideSettingsCmp.tsx` tab list.

**DoD**:
- [ ] `TabsNames.AddressFormat` enum value added
- [ ] Tab inserted after `RoadsDistricts` in `tabsOrder`
- [ ] `getComponents()` map includes `TabsNames.AddressFormat` → `<AddressFormatTab .../>`
- [ ] No existing tabs broken
- [ ] Frontend builds without errors

---

### Task 0033 — `add-address-format-i18n-keys` (P3, epic: addrfmt)

**Background**: New UI labels need i18n entries in `i18n.csv` and all 11 locale files.

**DoD**:
- [ ] `i18n.csv` has all required keys (see `04_ui-plan.md` for list)
- [ ] All 11 locale CSV files have the English text copied for each new key
- [ ] No duplicate or missing keys
- [ ] Build passes

---

### Task 0034 — `e2e-address-format-validation` (P2, epic: addrfmt)

**Background**: Integration validation that the full pipeline works end-to-end in an actual game session.

**DoD**:
- [ ] Custom format `{number} {street}` produces same output as vanilla game default
- [ ] Custom format `{street}, {number}` shows reversed order
- [ ] Format `{number} {street}, {district}` shows district name when building is in a district
- [ ] Format `{number} {street}, {district}` shows `{number} {street}` cleanly when building has no district (no trailing comma/space)
- [ ] Setting format persists across save/load (serialization test)
- [ ] Clearing format (empty string) restores locale-default ordering
- [ ] Transport stops also reflect the custom format

---

## Sprint 004 (extensions, if time allows)

### Task 0035 — `add-postalcode-generation-system` (P3)

Research and implement a grid-based or district-based postal code assignment system. Each district or map cell gets a numerical code. Token `{postalCode}` added to address formatter.

### Task 0036 — `per-district-address-format-override` (P4)

Allow per-district overrides of the address format (districts may have different conventions). Stored in `ADRDistrictData`.

### Task 0037 — `address-format-in-road-labels` (P3)

Show building address in the road label tooltip or in the info panel for buildings.

---

## Dependencies

```
0025 → 0026
0025 → 0028 (needs field)
0027 → 0028 (needs district helper)
0027 → 0029
0025 → 0028
0026 → 0028 (binding feeds formatting)
0030 → 0031
0031 → 0032
0028, 0029, 0032, 0033 → 0034
```

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Forgetting to bump CURRENT_VERSION | Low (documented) | High (save corruption) | Memory note + code review checklist |
| `GetRenderedLabelName` failing for district names | Low | Medium | Fallback to `GetName` if needed |
| Token substitution producing double-spaces | Medium | Low | Post-substitution trim/normalize pass |
| Frontend live preview showing wrong locale output | Low | Low | Use hardcoded English mock values |
