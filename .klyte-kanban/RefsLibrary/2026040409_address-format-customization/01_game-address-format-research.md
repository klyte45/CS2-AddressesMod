# Game Address Format — Research

*Researched: 2026-04-04 by Claude Sonnet 4.6*

## How the game formats addresses

The game formats building addresses in `Game.UI.NameSystem` (decompiled: `CS2_decompile_1.5.6/Game/UI/NameSystem.cs`).

### Key localization keys

| Key | Parameters | Default format (inferred) | Usage |
|-----|-----------|--------------------------|-------|
| `Assets.ADDRESS_NAME_FORMAT` | `ROAD`, `NUMBER` | `{NUMBER} {ROAD}` | Residential, transport stops |
| `Assets.NAMED_ADDRESS_NAME_FORMAT` | `NAME`, `ROAD`, `NUMBER` | `{NAME}, {NUMBER} {ROAD}` | Commercial/industrial with brand |

These keys are resolved at **runtime via the game's localization system**, not hardcoded in C#. The actual format string (e.g. `{NUMBER} {ROAD}`) lives in the game's own locale files — NOT in any decompiled source. This is why the user's tip says "it's from game localization, so you won't find it directly on decompiled sources."

The `NameSystem.Name.FormattedName(key, args[])` call passes named parameters:
- `"ROAD"` → the road name string
- `"NUMBER"` → the building number as string

### Current ADR patches

`AdrNameSystemOverrides.cs` already patches both address-producing methods:

- `GetSpawnableBuildingName` — produces addresses for houses/commercial/industrial
- `GetStaticTransportStopName` — produces addresses for building-based transport stops

Both patches **already intercept address formatting** and call `NameSystem.Name.FormattedName("Assets.ADDRESS_NAME_FORMAT", ...)` with their own road name (after applying road prefix rules). This is the **patch point** for custom format.

### Code location where format output happens

```csharp
// AdrNameSystemOverrides.cs ~line 120
var roadName = pattern.Replace("{name}", genName);
__result = NameSystem.Name.FormattedName("Assets.ADDRESS_NAME_FORMAT", new string[]
{
    "ROAD", roadName,
    "NUMBER", num.ToString()
});
```

## How to determine the district for a building

Each road edge can have a `BorderDistrict` component (from `Game.Areas`) with `m_Left` and `m_Right` entity references.

```csharp
struct BorderDistrict : IComponentData {
    Entity m_Left;   // district on left side of road direction
    Entity m_Right;  // district on right side of road direction
}
```

The **side** a building sits on is determined by comparing the building's world position to the road's tangent vector at the building's curve position:

1. Get `Building.m_RoadEdge` → the road edge entity
2. Get `Curve` component from the road edge → `m_Bezier`
3. Sample tangent at `Building.m_CurvePosition` using `MathUtils.Tangent(curve.m_Bezier, curvePos).xz`
4. Compute the road-right normal: `math.normalizesafe(new float2(tangent.y, -tangent.x))`
5. Get the building's world position from `Game.Objects.Transform.m_Position`
6. Evaluate position relative to road curve point at that t value
7. If the building is to the right of the tangent direction → `m_Right`, else → `m_Left`

Note: `Building.m_RoadEdge` is the raw (non-aggregated) road edge, same as used in `BuildingUtils.GetAddress`. The `BorderDistrict` component is on the raw edge, not on the aggregate.

## Summary of address data available at format time

At the point where `GetSpawnableBuildingName` / `GetStaticTransportStopName` runs:

| Datum | Source |
|-------|--------|
| Road name (formatted with prefix) | `roadName` variable (already computed) |
| Building number | `num` (from `BuildingUtils.GetAddress`) |
| Road edge entity | `entity` (from `BuildingUtils.GetAddress` — the aggregate entity!) |
| Raw building entity | `building` parameter |
| District (left/right) | `BorderDistrict` on the raw edge, side from building position |

Important: `BuildingUtils.GetAddress` returns the **aggregate** entity as `road`, not the raw edge. To get `BorderDistrict`:
- From the aggregate → `AggregateElement` buffer → pick the element matching the building's `m_RoadEdge` → that raw edge has `BorderDistrict`.
- **Or**: use `Building.m_RoadEdge` directly when available. Need to pass `building` entity to the formatting helper separately from the aggregate `entity`.
