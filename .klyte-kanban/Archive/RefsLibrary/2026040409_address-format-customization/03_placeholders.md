# Address Format — Available Placeholders

*Researched: 2026-04-04 by Claude Sonnet 4.6*

## Core Placeholders (always available)

| Token | Maps to | Source | Notes |
|-------|---------|--------|-------|
| `{number}` | Building address number | `BuildingUtils.GetAddress` → `num` | Integer (e.g. `42`) |
| `{street}` | Street/road name with prefix | `roadName` (already computed w/ prefix rules) | e.g. `"Oak Avenue"`, `"R-17"` |

## District Placeholder

| Token | Maps to | Source | Notes |
|-------|---------|--------|-------|
| `{district}` | District name for the building's road side | `BorderDistrict` on `Building.m_RoadEdge` | May be empty string if road has no district |

**Side determination**: `BorderDistrict.m_Left` / `m_Right` corresponds to game's concept of left/right relative to the road's own direction of travel. For a building, we project its world position against the road tangent at the building's curve position to determine left (m_Left) vs right (m_Right).

**Empty handling**: When no district exists on either side, `{district}` returns empty string. The format system should collapse/trim extra whitespace/punctuation around empty tokens (a cleanup pass after substitution is recommended).

## Additional Placeholders — Feasible

| Token | Feasibility | Source | Notes |
|-------|-------------|--------|-------|
| `{brand}` | ✅ Easy | `Building` → `Renter` → `CompanyData.m_Brand` | Company brand name; empty for residential |
| `{postalCode}` | ⚠️ Moderate | No built-in CS2 system. Would need to be generated/configured by the mod | Could be district-based or grid-based |
| `{cityName}` | ✅ Easy | Game settings / `CityName` from `ServiceBuildingSystem` or similar | The player's city name |
| `{neighborhoodName}` | ❓ Unclear | CS2 has "neighborhoods" as informal groupings? Needs research | May not exist as a distinct entity type |

## Placeholders NOT Recommended

| Token | Reason not suitable |
|-------|-------------------|
| `{buildingType}` | Too verbose; localized names vary per language; not generally used in real-world addresses |
| `{prefabName}` | Internal identifier; meaningless to end users |
| `{coordinates}` | Not a real-world address component |

## Proposed Initial Set (MVP)

For the initial implementation, support:
1. `{number}` — building number
2. `{street}` — street name (with road prefix rules applied)
3. `{district}` — district name (empty if none)
4. `{brand}` — company brand (empty for residential/no renter)

These four cover the most common international address formats:
- **US style**: `{number} {street}` → `42 Oak Avenue`
- **European style**: `{street} {number}` → `Oak Avenue 42`
- **With district**: `{district}, {street} {number}` → `Downtown, Oak Avenue 42`
- **Commercial**: `{brand}, {number} {street}` → `SuperMart, 42 Oak Avenue`

## Localization Note

The game's `Assets.ADDRESS_NAME_FORMAT` varies by locale (en-US uses `{NUMBER} {ROAD}`, European locales may swap the order). When a custom format is set by the player, it overrides the locale-based default. The UI should clearly communicate this trade-off.

## UI Representation for Placeholders

The UI should show available tokens as clickable chips/buttons that insert the token into the text field. A live preview showing a mock address (using fictional values) would help players understand the effect.

Example mock values for preview:
- `{number}` → `42`
- `{street}` → `"Oak Avenue"` (or current city's first road name)
- `{district}` → `"Downtown"` (or current city's first district name)
- `{brand}` → `"SuperMart"`
