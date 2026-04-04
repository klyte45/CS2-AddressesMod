# Address Format — UI Design Plan

*Researched: 2026-04-04 by Claude Sonnet 4.6*

## Feature Location

The address format editor should be a **new tab** in the Override Settings panel (`OverrideSettingsCmp.tsx`), which currently has:
- `Citizen` tab — given/family name files, surname-at-first toggle
- `RoadsDistricts` tab — road/district name files, station naming options
- Vehicle identifier tabs (Road, Rail, Water, Air plates; serial serial numbers)

**Proposed location**: Add an `AddressFormat` tab immediately after `RoadsDistricts`, as it is thematically closest to road-related naming.

## Tab order change

```typescript
const tabsOrder = [
  TabsNames.Citizen,
  TabsNames.RoadsDistricts,
  TabsNames.AddressFormat,   // NEW
  { type: "H2", title: translate("overrideSettings.subCategory.vehicleIdentifiers") },
  // ... rest unchanged
]
```

## New tab component: `AddressFormatTab.tsx`

### Form elements

1. **Format pattern input** — text field, single line
   - Label: `overrideSettings.addressFormat.pattern`
   - Placeholder hint: leave empty to use game default
   - On change: call `NamingRulesService.setAddressFormatPattern(value)`
   
2. **Live preview** — read-only display showing `"e.g. 42 Oak Avenue"` using the current pattern with mock values

3. **Token insertion chips** — row of buttons for each supported placeholder:
   - `[{number}]` `[{street}]` `[{district}]` `[{brand}]`
   - Clicking inserts the token at the cursor position in the text field (or appends)

4. **Reset button** — clears the pattern (restores game default)
   - Label: `overrideSettings.addressFormat.useDefault`

5. **Format for named buildings** (optional extension):
   - Separate field for `NAMED_ADDRESS_NAME_FORMAT` override pattern
   - Extra token `{brand}` available

### Mockup structure (JSX pseudocode)

```tsx
export const AddressFormatTab = ({ currentSettings }) => {
  const [pattern, setPattern] = useState(currentSettings?.AddressFormatPattern ?? "");
  
  const preview = computePreview(pattern);  // mock substitution
  
  const insertToken = (token: string) => {
    setPattern(p => p + token);
  };
  
  return <>
    <Cs2FormLine title={translate("overrideSettings.addressFormat.patternLabel")}>
      <Input 
        getValue={() => pattern}
        onValueChanged={(v) => {
          setPattern(v);
          NamingRulesService.setAddressFormatPattern(v);
        }}
      />
    </Cs2FormLine>
    <div className="tokenChips">
      {["{number}", "{street}", "{district}", "{brand}"].map(tok =>
        <button key={tok} onClick={() => insertToken(tok)}>{tok}</button>
      )}
    </div>
    <div className="addressPreview">
      {translate("overrideSettings.addressFormat.previewLabel")}: {preview}
    </div>
    <button onClick={() => { setPattern(""); NamingRulesService.setAddressFormatPattern(""); }}>
      {translate("overrideSettings.addressFormat.resetToDefault")}
    </button>
  </>;
};
```

## Required i18n keys (examples — all need localizing)

```
overrideSettings.tab.AddressFormat = "Address Format"
overrideSettings.addressFormat.patternLabel = "Address pattern"
overrideSettings.addressFormat.patternPlaceholder = "Leave empty for game default"
overrideSettings.addressFormat.previewLabel = "Preview"
overrideSettings.addressFormat.resetToDefault = "Use game default"
overrideSettings.addressFormat.tokenHint = "Click to insert placeholder:"
overrideSettings.addressFormat.info = "Use {number}, {street}, {district}, {brand} as placeholders. Leave empty to use the game's localized format."
```

## NamingRulesService extensions (adr-commons)

```typescript
// In AdrCitywideSettings type:
AddressFormatPattern: string;  // null/empty = game default

// In NamingRulesService class:
static async setAddressFormatPattern(x: string): Promise<void> {
    await engine.call("k45::adr.main.setAddressFormatPattern", x);
}
```

## Alternative UI location considered

**Info panel (k45_adr_vuio)**: Rejected. Address format is a city-wide setting, not an entity-specific setting. It belongs in the Override Settings panel.

**Separate panel screen**: Possible but unnecessary. The tab approach keeps all naming conventions together in one place, following the existing design pattern.
