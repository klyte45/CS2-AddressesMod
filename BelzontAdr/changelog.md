# v0.2.4r0 (06-APR-26)

- **Custom address format:** new "Address Format" tab in Override Settings lets you define a custom address pattern using tokens {number}, {street}, {district}, {brand}; clickable token chips insert at cursor and a live preview updates in real time; applies to both buildings and transport stops
- Added 3 new road qualifier rule conditions: elevation state (bridge/tunnel/at road level), car lane count range, and road width range
- Road qualifier condition fields now show tooltips describing what each setting controls
- Vehicle plate settings include a live preview showing sample generated plates, updating in real time as settings change
- Vehicle plate slot types (Regional, Local, Car Number) are now color-coded in the plate pattern editor
- Name files in the library now auto-reload within ~2 seconds when modified by an external editor, without requiring a manual reload
- Name file loading is now asynchronous; no more game startup freeze with large libraries; a loading indicator is shown and unchanged files are skipped on reload
- Mod UI is now available in all Cities: Skylines 2 supported languages via machine translation (German, Spanish, French, Italian, Japanese, Korean, Dutch, Polish, Russian, Simplified Chinese)
- Fixed city name seeds not being saved with the city; previously road and district names could change every time the city was reloaded
- Fixed region neighbor city list sometimes showing stale data after saving a city entry
- Fixed GitHub name file import treating each character as a separate name entry instead of each line
- Fixed transport station address reference panel appearing for buildings that already have a custom name set
- Fixed address customization logic and custom name display options in building info panels
- Fixed multiple UI state issues in naming and plate settings panels that could cause inputs not to update or settings not to be applied
- Fixed the "Forbid" icon not appearing on tristate toggle buttons
