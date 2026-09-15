# v0.4.0r0 (15-SEP-26)
- **Segments Grouping tool** (Ctrl+Shift+A, rebindable at options): fork vanilla aggregations into custom A&N groups; join/split membership (click, endpoint drag, fixed pin); shared name and house-number sequence; overlay ribbons and number cards; options for custom name, seed, and highway; error and InputHint tooltips.
- **Citywide convert / remove** on Roads & Districts settings: convert selected network kinds to A&N aggregations, or remove them (optional skip to keep highway groups). Useful to add labels to all your non-roads! Segments that belongs to a building (like train stations) are not automatically converted, but can receive an A&N Aggregation by using the Grouping Tool.
- **Highway data now lives on A&N road groups**, not on road markers. Assign route, direction, and mileage from the Addresses selected-info panel or Road Grouping tool options. Markers stay visual. **Existing saves need highway routes reassigned** on A&N groups (no automatic migration).
- **Segment name labels are drawn by A&N** while the mod is loaded (vanilla road-name labels are replaced; one-way arrows unchanged). Clicking a label selects the A&N group when applicable. This enable labels being rendered for non-road segments like **Railways, Tramways, Waterways, etc.**. Each kind of label have a different text outline color.
- Namesets and automatic road naming apply to **roads**; other network kinds use kind-based default names. Highway settings remain road-group only.
- New Write Everywhere formulas resolve route and mileage from the owning A&N group.
- Mod options for Road Grouping activation and road-name label sizing / automatic distance (General).
- Fixed: Stations named from a highway source show the route code instead of an empty name.
- Fixed highway mileage/direction and related label display edge cases (including negative numbering).
- Fixed non-road A&N name labels that could fail to draw in common cases.

## FROM v0.3.0r1 (09-SEP-26)
- Removed the Extra UI Screens requirement for ADR settings
- Moved mod settings to the Universal Mod Menu
