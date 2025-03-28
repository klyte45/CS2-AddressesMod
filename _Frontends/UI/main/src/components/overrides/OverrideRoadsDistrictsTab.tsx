import { translate } from "#utility/translate";
import { AdrCitywideSettings, DistrictListItem, DistrictRelativeService, NamingRulesService, SimpleNameEntry, nameToString } from "@klyte45/adr-commons";
import { Cs2CheckboxWithLine, Cs2FormLine, Cs2Select } from "@klyte45/euis-components";
import { useEffect, useState } from "react";
import { defaultSetting, defaultSettingRoadByDistrict } from "./OverrideSettingsCmp";

type Props = {
  simpleFiles: SimpleNameEntry[];
  indexedSimpleFiles: Record<string, SimpleNameEntry>;
  currentSettings: AdrCitywideSettings;
  innerContextSimpleFiles: SimpleNameEntry[];
  districts: DistrictListItem[];
}
export const OverrideRoadsDistrictsTab = ({ simpleFiles, indexedSimpleFiles, currentSettings, innerContextSimpleFiles, districts }: Props): JSX.Element => {

  const [selectedDistrict, setSelectedDistrict] = useState(undefined as DistrictListItem | undefined)
  useEffect(() => {
    setSelectedDistrict(selectedDistrict ? districts.find(x => x.Entity.Index == selectedDistrict.Entity.Index) : null)
  }, [districts])

  return <>
    <Cs2FormLine title={translate("overrideSettings.districtsFile")}>
      <Cs2Select
        options={Object.values(simpleFiles)}
        getOptionLabel={(x: SimpleNameEntry) => x?.Name}
        getOptionValue={(x: SimpleNameEntry) => x?.IdString}
        onChange={(x) => NamingRulesService.setDefaultDistrictNameOverridesStr(x.IdString)}
        value={indexedSimpleFiles[currentSettings.DefaultDistrictNameOverridesStr]}
        defaultValue={defaultSetting()} />
    </Cs2FormLine>
    <Cs2FormLine title={translate("overrideSettings.roadsFile")}>
      <Cs2Select
        options={Object.values(simpleFiles)}
        getOptionLabel={(x: SimpleNameEntry) => x?.Name}
        getOptionValue={(x: SimpleNameEntry) => x?.IdString}
        onChange={(x) => NamingRulesService.setDefaultRoadNameOverridesStr(x.IdString)}
        value={indexedSimpleFiles[currentSettings.DefaultRoadNameOverridesStr]}
        defaultValue={defaultSetting()} />
    </Cs2FormLine>
    <Cs2CheckboxWithLine title={translate("overrideSettings.useRoadNameAsStationName")} isChecked={() => currentSettings?.roadNameAsNameStation} onValueToggle={(x) => NamingRulesService.setRoadNameAsNameStation(x)} />
    <Cs2CheckboxWithLine title={translate("overrideSettings.useRoadNameAsCargoStationName")} isChecked={() => currentSettings?.roadNameAsNameCargoStation} onValueToggle={(x) => NamingRulesService.setRoadNameAsNameCargoStation(x)} />
    <h2>{translate("overrideSettings.perDistrictRoadsFile")}</h2>
    <Cs2Select
      options={districts}
      getOptionLabel={(x: DistrictListItem) => nameToString(x?.Name)}
      getOptionValue={(x: DistrictListItem) => x?.Entity.Index.toString()}
      onChange={(x) => setSelectedDistrict(x)}
      value={selectedDistrict} />
    {selectedDistrict && <>
      <Cs2Select
        options={innerContextSimpleFiles}
        getOptionLabel={(x: SimpleNameEntry) => x?.Name}
        getOptionValue={(x: SimpleNameEntry) => x?.IdString}
        onChange={async (x) => await DistrictRelativeService.setRoadNamesFile(selectedDistrict.Entity, x.IdString)}
        value={innerContextSimpleFiles.find(x => x.IdString == selectedDistrict?.CurrentValue)}
        defaultValue={defaultSettingRoadByDistrict()} />
    </>}
  </>;
}
