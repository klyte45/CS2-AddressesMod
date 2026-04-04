# Patching Strategy for Custom Address Format

*Researched: 2026-04-04 by Claude Sonnet 4.6*

## Goal

Allow players to configure a custom format string for how building addresses are displayed. The default game format (e.g. `{NUMBER} {ROAD}` in en-US) would be replaceable with any user-defined pattern.

## Approach: Custom format string with token substitution

Instead of calling `NameSystem.Name.FormattedName("Assets.ADDRESS_NAME_FORMAT", ...)`, when a custom format is set we directly substitute tokens and return `Name.CustomName(formattedResult)`.

This is analogous to how citizen name formatting is done:
```csharp
internal string DoNameFormat(string name, string surname) => 
    CurrentCitySettings.surnameAtFirst ? $"{surname} {name}" : $"{name} {surname}";
```

### Token substitution approach

The custom format string is stored as a plain string in `AdrCitywideSettings`, e.g.:
- Default (empty/null): falls through to original `FormattedName("Assets.ADDRESS_NAME_FORMAT", ...)`
- Custom: `"{number} {street}"`, `"{street}, {number}"`, `"{number} {street}, {district}"`, etc.

Substitution replaces tokens before returning the name as `Name.CustomName(result)`.

## Patch points

Both of these methods in `AdrNameSystemOverrides.cs` already have the needed data:

### `GetSpawnableBuildingName`

```csharp
// Current code (after our road-qualifier patches):
var roadName = pattern.Replace("{name}", genName);
__result = NameSystem.Name.FormattedName("Assets.ADDRESS_NAME_FORMAT", new string[]
{
    "ROAD", roadName, "NUMBER", num.ToString()
});
return false;
```

Proposed change:
```csharp
var roadName = pattern.Replace("{name}", genName);
var customFmt = adrMainSystem.CurrentCitySettings.AddressFormatPattern;
if (string.IsNullOrEmpty(customFmt))
{
    __result = NameSystem.Name.FormattedName("Assets.ADDRESS_NAME_FORMAT", 
        new[]{ "ROAD", roadName, "NUMBER", num.ToString() });
}
else
{
    __result = Name.CustomName(
        FormatAddress(customFmt, roadName, num.ToString(), building, entity));
}
return false;
```

### `GetStaticTransportStopName`

Same pattern as above.

### Helper `FormatAddress`

```csharp
private static string FormatAddress(string pattern, string roadName, string number, 
    Entity buildingEntity, Entity roadAggregateEntity)
{
    var result = pattern
        .Replace("{street}", roadName)
        .Replace("{number}", number);
    
    if (result.Contains("{district}"))
    {
        var districtName = GetBuildingDistrictName(buildingEntity);
        result = result.Replace("{district}", districtName ?? "");
    }
    
    return result.Trim();
}
```

### `GetBuildingDistrictName`

```csharp
private static string GetBuildingDistrictName(Entity buildingEntity)
{
    if (!entityManager.TryGetComponent<Building>(buildingEntity, out var building)) return null;
    if (!entityManager.TryGetComponent<BorderDistrict>(building.m_RoadEdge, out var border)) return null;
    
    // Determine which side the building is on
    Entity districtEntity = GetBuildingSideDistrict(buildingEntity, building, border);
    if (districtEntity == Entity.Null) return null;
    
    return m_nameSystem.GetRenderedLabelName(districtEntity);
}

private static Entity GetBuildingSideDistrict(Entity buildingEntity, Building building, BorderDistrict border)
{
    if (border.m_Left == Entity.Null && border.m_Right == Entity.Null) return Entity.Null;
    if (border.m_Left == Entity.Null) return border.m_Right;
    if (border.m_Right == Entity.Null) return border.m_Left;
    
    // Both sides exist — determine which side the building is on
    if (!entityManager.TryGetComponent<Curve>(building.m_RoadEdge, out var curve)) return border.m_Left;
    if (!entityManager.TryGetComponent<Game.Objects.Transform>(buildingEntity, out var transform)) return border.m_Left;
    
    float3 curvePt = MathUtils.Position(curve.m_Bezier, building.m_CurvePosition);
    float2 tangent = math.normalizesafe(MathUtils.Tangent(curve.m_Bezier, building.m_CurvePosition).xz);
    float2 rightNormal = new float2(tangent.y, -tangent.x);
    float2 toBldg = transform.m_Position.xz - curvePt.xz;
    
    return math.dot(rightNormal, toBldg) >= 0f ? border.m_Right : border.m_Left;
}
```

## Storage: AdrCitywideSettings

Add a new field:
```csharp
private string addressFormatPattern = null;  // null = use game default
public string AddressFormatPattern { get => addressFormatPattern; set => addressFormatPattern = value; }
```

Since `AdrCitywideSettings` implements `ISerializable`, adding this field **MUST** increment `CURRENT_VERSION` from 1 to 2 with proper migration:
```csharp
private const uint CURRENT_VERSION = 2;

// In Serialize():
writer.Write(addressFormatPattern ?? "");

// In Deserialize():
if (version >= 2)
{
    reader.Read(out string fmt);
    addressFormatPattern = string.IsNullOrEmpty(fmt) ? null : fmt;
}
```

## Backend binding in AdrMainSystem

Add to `SetupCallBinder`:
```csharp
doBindLink("main.setAddressFormatPattern", (string x) => { 
    CurrentCitySettings.AddressFormatPattern = string.IsNullOrEmpty(x) ? null : x; 
    MarkRoadsDirty(); 
    NotifyChanges(); 
});
```

## Fallback behavior

- If `AddressFormatPattern` is null/empty → use vanilla `Assets.ADDRESS_NAME_FORMAT` (full localization support, localized order per language)
- If set → use direct string substitution with `Name.CustomName()`
- `Assets.NAMED_ADDRESS_NAME_FORMAT` (brand+address): when a custom format is set, either also honor it or ignore it. Recommendation: also apply custom format but prepend brand if available. Add `{brand}` token support for this case.

## Notes on `GetName` / non-buildings

The `GetName` redirect is used for aggregates (roads). Address format is NOT used for road aggregate names — those use the road prefix pattern (`{name}` format). So only `GetSpawnableBuildingName` and `GetStaticTransportStopName` need to be modified.
