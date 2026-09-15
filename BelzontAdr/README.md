# Addresses & Names Mod for Cities Skylines 2
## _Name it all as you wish_

This mod is the successor of the CS1 homonymous mod. It allows changing vanilla methods of name generation to custom name sets, and lets you build custom road (and other network) name groups with shared house numbering.

You can find several name files at [Addresses Files repository at GitHub](https://github.com/klyte45/AddressesFiles). These files will work for both CS1 and CS2 versions of this mod.

**NOTICE:** 
- Surname files are not compatible with the CS2 version. To make them compatible, open the file and remove all text like `{0}` from the file.
- Road qualifier files aren't compatible with CS2.

## Current features

- Settings available in the vanilla `Mods` menu through the Universal Mod Menu integration. Extra UI Screens is not required.
- **Road Grouping tool:** fork vanilla aggregations into custom A&N groups that share one name and one house-number sequence. Join or split membership with click, endpoint drag, and fixed pin; overlays show membership and number ranges. Options cover custom name, naming seed, and highway data. Enable the tool and its hotkey in mod options.
- **Multi-kind network groups:** roads, pathways, train/tram/subway, waterways, and taxiways. Street and highway may share a road group; mixing different network kinds is blocked with clear feedback. Non-road groups get kind-colored name labels.
- **Citywide convert / remove:** on Roads & Districts settings, convert selected network kinds to A&N aggregations, or remove A&N aggregations (optionally keeping highway groups).
- **A&N road name labels:** while the mod is loaded, road name labels are drawn by A&N (one-way arrows unchanged). Clicking a label selects the A&N group when applicable. Label sizing / automatic distance options are in General settings.
- **Highway settings editor:** register highways in the region settings UI. Assign route, direction, and mileage on an **A&N road group** from the Addresses selected-info panel or Road Grouping tool options. Highway name labels show a yellow route-code line (`prefix-suffix`, with optional direction). Useful with Write Everywhere and without it.
- **Road marker asset:** placeable props for Write Everywhere and custom numeric parameters. Markers are visual; they do not author highway route or mileage (that lives on the A&N group).
- **Neighbor cities editor:** register neighbor cities to be used when retrieving the outside connection name. They can be reached by land, water, air or any combination of them. A map shows them in the in-game settings UI.
- Edit names of any clickable object in the editor (and save changes to send with the maps created)
- Change sources references for stations names and seeds for roads and districts names in the selected info panel
- In-game filename viewer, showing the weight of each word.
  - Words with a higher weight tends to appear more frequently
  - Raise word weight by creating several lines with same content in the name file.
- Organize your file names using subfolders in the names folder
- Choose files to override name generation of:
  - Male cims
  - Female cims
  - Surnames (are last names as default, but may be inverted to follow oriental standard)
	- To use gendered surnames, in the surname files add the female and male version of the surname separated by a semicolon (;)
  - Dogs
  - Roads (Global and by district)
  - Districts
- Roads qualifier rules editor
  - Use ingame UI to create several rules to differentiate streets, avenues, highways, etc by their characteristics
  - Very limited options to customization at this moment, might be enhanced later
- Use district and road names to name your public transport stations!

Now it's possible to setup the mod to generate up to 5 given names and up to 5 surnames! Notice that very large names might be truncated by game limitations... This feature may receive a tunning in future releases to be more configurable.

## Road marker: Write Everywhere integration

Any modder interested on create WE Modules that serves content to road markers might want to describe the available settings to show in the game UI. Addresses offers two integer parameters that can be used as desired by module designers.

To generate the metadata, [this repository might be useful](https://github.com/klyte45/CS2-ADR-WE-road-marker-metadata). It's filled with the data describing how Addresses Marker as Signs Pack uses the two parameters.

Making story short, the parameters can be worked as bits groups, and can use dropdowns or integer inputs to set the values at UI. Simple conditions can be used to make a field be visible or not.

Highway route, direction, and mileage for Write Everywhere formulas are resolved from the **owning A&N road group**, not from the marker itself.

## Next steps

For 0.5+, it's planned to expand the Highway settings like the CS1 version did before. Expect for features like Highway shields editor, exit roads marking, procedural/automatic usages for the A&N Markers (mileage markes, exit information signs, etc.). For the A&N markers, it will can also be used for enhance automatic detailing for rails (auto place signaling) and other land type ways. All those markers feature expansion will rely on future WE performance updates planned for soon to make them as static as possible.

The road for 1.0 is longer, since there are a lot of other features from original mod that are missing yet.

## Experimental mod warning!
This mod may cause issues in the game due to its early stage of development. Treat it as experimental until it reaches version 1.0.

## Support
The most up to date information about installation and known issues and bugs is at the [forum topic](https://forum.paradoxplaza.com/forum/threads/addresses-names-mod-for-cs2.1651551/).
