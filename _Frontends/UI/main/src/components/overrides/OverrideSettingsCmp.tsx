import { translate } from "#utility/translate";
import { AdrCitywideSettings, DistrictListItem, NamesetService, SimpleNameEntry } from "@klyte45/adr-commons";
import { BindingClassObj, Cs2SideTabs, DefaultPanelScreen } from "@klyte45/euis-components";
import { useEffect, useState } from "react";
import { OverrideCitizenTab } from "./OverrideCitizenTab";
import { OverrideRoadsDistrictsTab } from "./OverrideRoadsDistrictsTab";
import { VehiclePlateControllerComponent } from "./VehiclePlateControllerComponent";

enum TabsNames {
  RoadsDistricts = "RoadsDistricts",
  Citizen = "Citizen",
  RoadPlates = "RoadPlates",
  RailPlates = "RailPlates",
  WaterPlates = "WaterPlates",
  AirPlates = "AirPlates"
}



export const defaultSetting = () => { return { IdString: null, Values: [], Name: translate("overrideSettings.useVanillaOptionLbl") }; }
export const defaultSettingRoadByDistrict = () => { return { IdString: null, Values: [], Name: translate("overrideSettings.useSameAsCityOptionLbl") }; }

type Props = {
  currentSettings: AdrCitywideSettings;
  districts: DistrictListItem[];
  cityNamesets: SimpleNameEntry[];
};

export const OverrideSettingsCmp = ({
  currentSettings,
  districts,
  cityNamesets
}: Props) => {
  useEffect(() => {
    listFiles()
  }, [cityNamesets])

  const tabsOrder: Parameters<typeof Cs2SideTabs<TabsNames>>[0]['tabsOrder'] = [
    TabsNames.Citizen,
    TabsNames.RoadsDistricts,
    { type: "H2", title: translate("overrideSettings.subCategory.vehicleIdentifiers") },
    TabsNames.RoadPlates,
    TabsNames.RailPlates,
    TabsNames.WaterPlates,
    TabsNames.AirPlates,
  ]

  const [currentTab, setCurrentTab] = useState(tabsOrder[0] as TabsNames);
  const [simpleFiles, setSimpleFiles] = useState([] as SimpleNameEntry[]);
  const [innerContextSimpleFiles, setInnerContextSimpleFiles] = useState([] as SimpleNameEntry[]);
  const [indexedSimpleFiles, setIndexedSimpleFiles] = useState({} as Record<string, SimpleNameEntry>);

  const listFiles = async () => {
    const simpleFiles = cityNamesets.map(x => { x.Values = []; return x; }).sort((a, b) => a.Name.localeCompare(b.Name, undefined, { sensitivity: "base" }))
    const generalSimpleFiles = [defaultSetting()].concat(simpleFiles)
    const indexedSimpleFiles = generalSimpleFiles.reduce((p, n) => {
      p[n.IdString] = n;
      return p;
    }, {} as Record<string, SimpleNameEntry>)
    const innerContextSimpleFiles = [defaultSettingRoadByDistrict(), ...simpleFiles]
    setSimpleFiles(generalSimpleFiles)
    setIndexedSimpleFiles(indexedSimpleFiles)
    setInnerContextSimpleFiles(innerContextSimpleFiles)
  }

  const getComponents = (): Record<TabsNames, JSX.Element> => {
    return {
      [TabsNames.Citizen]: <OverrideCitizenTab simpleFiles={simpleFiles} indexedSimpleFiles={indexedSimpleFiles} currentSettings={currentSettings} />,
      [TabsNames.RoadsDistricts]: <OverrideRoadsDistrictsTab
        simpleFiles={simpleFiles}
        indexedSimpleFiles={indexedSimpleFiles}
        currentSettings={currentSettings}
        innerContextSimpleFiles={innerContextSimpleFiles}
        districts={districts} />,
      [TabsNames.RoadPlates]: <VehiclePlateControllerComponent prefix="k45::adr.vehiclePlate.road" />,
      [TabsNames.WaterPlates]: <VehiclePlateControllerComponent prefix="k45::adr.vehiclePlate.water" />,
      [TabsNames.AirPlates]: <VehiclePlateControllerComponent prefix="k45::adr.vehiclePlate.air" />,
      [TabsNames.RailPlates]: <VehiclePlateControllerComponent prefix="k45::adr.vehiclePlate.rail" />
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



