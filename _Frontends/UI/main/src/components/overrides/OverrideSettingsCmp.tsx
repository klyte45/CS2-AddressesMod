import { translate } from "#utility/translate";
import { AdrCitywideSettings, DistrictListItem, DistrictRelativeService, NamesetService, NamingRulesService, SimpleNameEntry } from "@klyte45/adr-commons";
import { Cs2Checkbox, Cs2FormLine, Cs2Select, Cs2SideTabs, DefaultPanelScreen, Input, nameToString } from "@klyte45/euis-components";
import { useEffect, useState } from "react";

enum TabsNames {
  RoadsDistricts = "RoadsDistricts",
  Citizen = "Citizen",
}

const tabsOrder: (TabsNames | undefined)[] = [TabsNames.Citizen, TabsNames.RoadsDistricts]

const defaultSetting = { IdString: null, Values: [], Name: translate("overrideSettings.useVanillaOptionLbl") };
const defaultSettingRoadByDistrict = { IdString: null, Values: [], Name: translate("overrideSettings.useSameAsCityOptionLbl") };

export const OverrideSettingsCmp = ({
  currentSettings,
  districts,
  cityNamesets
}: {
  currentSettings: AdrCitywideSettings,
  districts: DistrictListItem[]
  cityNamesets: SimpleNameEntry[]
}) => {
  useEffect(() => {
    listFiles();
  }, []);

  useEffect(() => {
    setSelectedDistrict(selectedDistrict ? districts.find(x => x.Entity.Index == selectedDistrict.Entity.Index) : null)
  }, [districts])

  useEffect(() => {
    listFiles()
  }, [cityNamesets])

  const [currentTab, setCurrentTab] = useState(tabsOrder[0]);
  const [simpleFiles, setSimpleFiles] = useState([] as SimpleNameEntry[]);
  const [innerContextSimpleFiles, setInnerContextSimpleFiles] = useState([] as SimpleNameEntry[]);
  const [indexedSimpleFiles, setIndexedSimpleFiles] = useState({} as Record<string, SimpleNameEntry>);
  const [selectedDistrict, setSelectedDistrict] = useState(undefined as DistrictListItem | undefined)

  const listFiles = async () => {
    const simpleFiles = cityNamesets.map(x => { x.Values = []; return x; }).sort((a, b) => a.Name.localeCompare(b.Name, undefined, { sensitivity: "base" }))
    const generalSimpleFiles = [defaultSetting].concat(simpleFiles)
    const indexedSimpleFiles = generalSimpleFiles.reduce((p, n) => {
      p[n.IdString] = n;
      return p;
    }, {} as Record<string, SimpleNameEntry>)
    const innerContextSimpleFiles = [defaultSettingRoadByDistrict, ...simpleFiles]
    setSimpleFiles(generalSimpleFiles)
    setIndexedSimpleFiles(indexedSimpleFiles)
    setInnerContextSimpleFiles(innerContextSimpleFiles)
  }

  const getComponents = (): Record<TabsNames, JSX.Element> => {
    return {
      [TabsNames.Citizen]: <>

        <Cs2FormLine title={translate("overrideSettings.maleNamesFile")}>
          <Cs2Select
            options={Object.values(simpleFiles)}
            getOptionLabel={(x: SimpleNameEntry) => x?.Name}
            getOptionValue={(x: SimpleNameEntry) => x?.IdString}
            onChange={(x) => NamingRulesService.setCitizenMaleNameOverridesStr(x.IdString)}
            value={indexedSimpleFiles[currentSettings.CitizenMaleNameOverridesStr]}
            defaultValue={defaultSetting}
          />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.femaleNamesFile")}>
          <Cs2Select
            options={Object.values(simpleFiles)}
            getOptionLabel={(x: SimpleNameEntry) => x?.Name}
            getOptionValue={(x: SimpleNameEntry) => x?.IdString}
            onChange={(x) => NamingRulesService.setCitizenFemaleNameOverridesStr(x.IdString)}
            value={indexedSimpleFiles[currentSettings.CitizenFemaleNameOverridesStr]}
            defaultValue={defaultSetting}
          />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.surnamesFile")}>
          <Cs2Select
            options={Object.values(simpleFiles)}
            getOptionLabel={(x: SimpleNameEntry) => x?.Name}
            getOptionValue={(x: SimpleNameEntry) => x?.IdString}
            onChange={(x) => NamingRulesService.setCitizenSurnameOverridesStr(x.IdString)}
            value={indexedSimpleFiles[currentSettings.CitizenSurnameOverridesStr]}
            defaultValue={defaultSetting}
          />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.firstNameAtStart")}>
          <Cs2Checkbox isChecked={() => currentSettings?.surnameAtFirst} onValueToggle={(x) => NamingRulesService.setSurnameAtFirst(x)} />
        </Cs2FormLine>
        <Input title={translate("overrideSettings.maximumGeneratedGivenNames")} getValue={() => currentSettings.MaximumGeneratedGivenNames?.toFixed(0)} maxLength={1} isValid={(x) => parseInt(x) >= 1 && parseInt(x) <= 5} onValueChanged={async (x) => NamingRulesService.setMaxGivenNames(parseInt(x))} />
        <Input title={translate("overrideSettings.maximumGeneratedSurnames")} getValue={() => currentSettings.MaximumGeneratedSurnames?.toFixed(0)} maxLength={1} isValid={(x) => parseInt(x) >= 1 && parseInt(x) <= 5} onValueChanged={async (x) => NamingRulesService.setMaxSurnames(parseInt(x))} />
        <Cs2FormLine title={translate("overrideSettings.dogsFile")}>
          <Cs2Select
            options={Object.values(simpleFiles)}
            getOptionLabel={(x: SimpleNameEntry) => x?.Name}
            getOptionValue={(x: SimpleNameEntry) => x?.IdString}
            onChange={(x) => NamingRulesService.setCitizenDogOverridesStr(x.IdString)}
            value={indexedSimpleFiles[currentSettings.CitizenDogOverridesStr]}
            defaultValue={defaultSetting}
          />
        </Cs2FormLine>
      </>,
      [TabsNames.RoadsDistricts]: <>
       <Cs2FormLine title={translate("overrideSettings.districtsFile")}>
          <Cs2Select
            options={Object.values(simpleFiles)}
            getOptionLabel={(x: SimpleNameEntry) => x?.Name}
            getOptionValue={(x: SimpleNameEntry) => x?.IdString}
            onChange={(x) => NamingRulesService.setDefaultDistrictNameOverridesStr(x.IdString)}
            value={indexedSimpleFiles[currentSettings.DefaultDistrictNameOverridesStr]}
            defaultValue={defaultSetting}
          />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.roadsFile")}>
          <Cs2Select
            options={Object.values(simpleFiles)}
            getOptionLabel={(x: SimpleNameEntry) => x?.Name}
            getOptionValue={(x: SimpleNameEntry) => x?.IdString}
            onChange={(x) => NamingRulesService.setDefaultRoadNameOverridesStr(x.IdString)}
            value={indexedSimpleFiles[currentSettings.DefaultRoadNameOverridesStr]}
            defaultValue={defaultSetting}
          />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.useRoadNameAsStationName")}>
          <Cs2Checkbox isChecked={() => currentSettings?.roadNameAsNameStation} onValueToggle={(x) => NamingRulesService.setRoadNameAsNameStation(x)} />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.useRoadNameAsCargoStationName")}>
          <Cs2Checkbox isChecked={() => currentSettings?.roadNameAsNameCargoStation} onValueToggle={(x) => NamingRulesService.setRoadNameAsNameCargoStation(x)} />
        </Cs2FormLine>
        <h2>{translate("overrideSettings.perDistrictRoadsFile")}</h2>
        <Cs2Select
          options={districts}
          getOptionLabel={(x: DistrictListItem) => nameToString(x?.Name)}
          getOptionValue={(x: DistrictListItem) => x?.Entity.Index.toString()}
          onChange={(x) => setSelectedDistrict(x)}
          value={selectedDistrict}
        />
        {
          selectedDistrict && <>
            <Cs2Select
              options={innerContextSimpleFiles}
              getOptionLabel={(x: SimpleNameEntry) => x?.Name}
              getOptionValue={(x: SimpleNameEntry) => x?.IdString}
              onChange={async (x) => await DistrictRelativeService.setRoadNamesFile(selectedDistrict.Entity, x.IdString)}
              value={innerContextSimpleFiles.find(x => x.IdString == selectedDistrict?.CurrentValue)}
              defaultValue={defaultSettingRoadByDistrict}
            />
          </>
        }
      </>   
    }
  }


  return !!currentSettings && <DefaultPanelScreen title={translate("overrideSettings.title")} subtitle={translate("overrideSettings.subtitle")} >

    <Cs2SideTabs<TabsNames>
      componentsMapViewer={getComponents()}
      tabsOrder={tabsOrder}
      currentTab={currentTab}
      onSetCurrentTab={setCurrentTab}
      i18nTitlePrefix={"overrideSettings.tab"}
      translateFn={translate} />

  </DefaultPanelScreen>;


}


