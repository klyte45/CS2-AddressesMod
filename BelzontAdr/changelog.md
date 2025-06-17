# v0.2.0r3 (17-JUN-25)

- Fixed lack of randomizing for names when creating new cities.

## FROM v0.2.0r0 (11-JUN-25)

## Highway & Road System
- Added a new prop that allows customizing some road attributes. It prevents the game from losing the name data if a road is deleted. Available at road services tab.
- Information it can hold:
  - Road settings (previosly registered on Addresses EUIS UI)
  - Override the mileage counting
  - Show a custom information when used along **Write Everywhere** supported modules. By default, it's the 120 speed limit prop (even on NA theme).
- The game lots numbering now follow the road mileage, instead of the zoning cell estimated number. This change ensures that the numbering is more accurate and consistent with the actual road distances, making override the mileage more meaningful.
- After the asset making support to custom meshes and images get released, other models can easily added by adding the custom component `BelzontAdr.ADRRoadMarkerObject` on prefab settings.
- The Write Everywhere module may have a metadata information describing how the settings will be shown to player. A sample project to generate the metadata is linked at mod page.

## Region Map Features & Visualization
- New screen that show the city map and all roads and rails built in the city. Also show region cities!
- Includes feature to show world position when hovering the mouse in the region editor.
- It can show no neighbors or neighbors by connection type (land, air or water)
- You can select points on the map with a double-click, you can set it as a reference point field for highways in the editor.
- The region cities can be set in a cardinal point (8 basic points) or be set custom angles. Angles conflicting are resolved automatically by the mod.

## Data Handling
- Updated systems to use a new serialization method and stopped using XML for savegames.

## General Fixes
- Removed unused content.
- Fixes for 1.3 patch.
