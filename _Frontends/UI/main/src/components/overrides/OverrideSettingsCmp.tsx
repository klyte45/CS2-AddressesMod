import { translate } from "#utility/translate";
import { AdrCitywideSettings, DistrictListItem, DistrictRelativeService, NamesetService, nameToString, NamingRulesService, SimpleNameEntry } from "@klyte45/adr-commons";
import { BindingClassObj, Cs2SideTabs, DefaultPanelScreen } from "@klyte45/euis-components";
import { useEffect, useState } from "react";
import { OverrideCitizenTab } from "./OverrideCitizenTab";
import { OverrideRoadsDistrictsTab } from "./OverrideRoadsDistrictsTab";
import { VehiclePlateControllerComponent } from "./VehiclePlateControllerComponent";
import { VehicleSerialControllerComponent } from "./VehicleSerialControllerComponent";

enum TabsNames {
  RoadsDistricts = "RoadsDistricts",
  Citizen = "Citizen",
  RoadPlates = "RoadPlates",
  RailPlates = "RailPlates",
  WaterPlates = "WaterPlates",
  AirPlates = "AirPlates",
  BusSerial = "BusSerial",
  TaxiSerial = "TaxiSerial",
  PoliceSerial = "PoliceSerial",
  FiretruckSerial = "FiretruckSerial",
  AmbulanceSerial = "AmbulanceSerial",
  GarbageSerial = "GarbageSerial",
  PostalSerial = "PostalSerial",


}



export const defaultSetting = () => { return { IdString: null, Values: [], ValuesAlternative: [], Name: translate("overrideSettings.useVanillaOptionLbl") }; }
export const defaultSettingRoadByDistrict = () => { return { IdString: null, Values: [], ValuesAlternative: [], Name: translate("overrideSettings.useSameAsCityOptionLbl") }; }


export const OverrideSettingsCmp = ({ }) => {
  NamesetService.doOnCityNamesetsUpdated(() => NamesetService.listCityNamesets().then(x => listFiles(x)));

  useEffect(() => {
    NamesetService.listCityNamesets().then(x => listFiles(x))
    return () => NamesetService.offCityNamesetsUpdated();
  }, [])


  useEffect(() => {
    getSettings();
    NamingRulesService.onCityDataReloaded(() => { getSettings(); });
    listDistricts();
    DistrictRelativeService.onDistrictChanged(() => listDistricts());

    return () => {
      DistrictRelativeService.offDistrictChanged();
      NamingRulesService.offCityDataReloaded();
    }
  }, []);

  const [currentSettings, setCurrentSettings] = useState({} as AdrCitywideSettings);

  const getSettings = async () => {
    setCurrentSettings(await NamingRulesService.getCurrentCitywideSettings());
  }

  const [districts, setDistricts] = useState([] as DistrictListItem[])
  const listDistricts = async () => {
    const districtNames = (await DistrictRelativeService.listAllDistricts())?.sort((a, b) => nameToString(a.Name).localeCompare(nameToString(b.Name), undefined, { sensitivity: "base" }))
    setDistricts(districtNames);
  }


  const tabsOrder: Parameters<typeof Cs2SideTabs<TabsNames>>[0]['tabsOrder'] = [
    TabsNames.Citizen,
    TabsNames.RoadsDistricts,
    { type: "H2", title: translate("overrideSettings.subCategory.vehicleIdentifiers") },
    TabsNames.RoadPlates,
    TabsNames.RailPlates,
    TabsNames.WaterPlates,
    TabsNames.AirPlates,
    { type: "H2", title: translate("overrideSettings.subCategory.roadVehicleSerial") },
    TabsNames.BusSerial,
    TabsNames.TaxiSerial,
    TabsNames.PoliceSerial,
    TabsNames.FiretruckSerial,
    TabsNames.AmbulanceSerial,
    TabsNames.GarbageSerial,
    TabsNames.PostalSerial
  ]

  const [currentTab, setCurrentTab] = useState(tabsOrder[0] as TabsNames);
  const [simpleFiles, setSimpleFiles] = useState([] as SimpleNameEntry[]);
  const [innerContextSimpleFiles, setInnerContextSimpleFiles] = useState([] as SimpleNameEntry[]);
  const [indexedSimpleFiles, setIndexedSimpleFiles] = useState({} as Record<string, SimpleNameEntry>);

  const listFiles = async (cityNamesets: SimpleNameEntry[]) => {
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
      [TabsNames.RoadPlates]: <VehiclePlateControllerComponent type="road" />,
      [TabsNames.WaterPlates]: <VehiclePlateControllerComponent type="water" />,
      [TabsNames.AirPlates]: <VehiclePlateControllerComponent type="air" />,
      [TabsNames.RailPlates]: <VehiclePlateControllerComponent type="rail" />,
      [TabsNames.BusSerial]: <VehicleSerialControllerComponent type="bus" />,
      [TabsNames.TaxiSerial]: <VehicleSerialControllerComponent type="taxi" />,
      [TabsNames.PoliceSerial]: <VehicleSerialControllerComponent type="police" />,
      [TabsNames.FiretruckSerial]: <VehicleSerialControllerComponent type="firetruck" />,
      [TabsNames.AmbulanceSerial]: <VehicleSerialControllerComponent type="ambulance" />,
      [TabsNames.GarbageSerial]: <VehicleSerialControllerComponent type="garbage" />,
      [TabsNames.PostalSerial]: <VehicleSerialControllerComponent type="postal" />
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



