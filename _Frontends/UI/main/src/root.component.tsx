import CityNamesetLibraryCmp from "#components/fileManagement/CityNamesetLibraryCmp";
import { OverrideSettingsCmp } from "#components/overrides/OverrideSettingsCmp";
import { RoadPrefixCmp } from "#components/roadPrefix/RoadPrefixCmp";
import "#styles/main.scss";
import { translate } from "#utility/translate";
import { Component, useEffect, useState } from "react";
import { ErrorBoundary, MainSideTabMenuComponent, MenuItem } from "@klyte45/euis-components";
import { Tab, TabList, TabPanel, Tabs } from "react-tabs";
import { AdrCitywideSettings, DistrictListItem, DistrictRelativeService, NamesetService, nameToString, NamingRulesService, SimpleNameEntry } from "@klyte45/adr-commons";


export default () => {

  useEffect(() => {
    getSettings();
    NamingRulesService.onCityDataReloaded(() => { getSettings(); });
    listDistricts();
    DistrictRelativeService.onDistrictChanged(() => listDistricts());
    listCityNamesets();
    NamesetService.doOnCityNamesetsUpdated(() => listCityNamesets());

    return () => {
      DistrictRelativeService.offDistrictChanged();
      NamingRulesService.offCityDataReloaded();
      NamesetService.offCityNamesetsUpdated();
    }
  }, []);

  const [currentSettings, setCurrentSettings] = useState({} as AdrCitywideSettings);
  const [districts, setDistricts] = useState([] as DistrictListItem[])
  const [cityNamesets, setCityNamesets] = useState([] as SimpleNameEntry[])

  const getSettings = async () => {
    setCurrentSettings(await NamingRulesService.getCurrentCitywideSettings());
  }

  const listDistricts = async () => {
    const districtNames = (await DistrictRelativeService.listAllDistricts())?.sort((a, b) => nameToString(a.Name).localeCompare(nameToString(b.Name), undefined, { sensitivity: "base" }))
    setDistricts(districtNames);
  }

  const listCityNamesets = async () => {
    setCityNamesets(await NamesetService.listCityNamesets());
  }


  const menus: MenuItem[] = [
    {
      iconUrl: "coui://uil/Standard/NameSort.svg",
      name: translate("namesetManagement.title"),
      panelContent: <CityNamesetLibraryCmp />,
      tintedIcon: true
    },
    {
      iconUrl: "coui://uil/Standard/Tools.svg",
      name: translate("overrideSettings.title"),
      panelContent: <OverrideSettingsCmp currentSettings={currentSettings} districts={districts} cityNamesets={cityNamesets}/>
    },
    {
      iconUrl: "coui://uil/Standard/Highway.svg",
      name: translate("roadPrefixSettings.title"),
      panelContent: <RoadPrefixCmp currentSettings={currentSettings} />
    }
  ]
  return <>
    {/* <button style={{ position: "fixed", right: 0, top: 0, zIndex: 999 }} onClick={() => location.reload()}>RELOAD!!!</button> */}
    <ErrorBoundary>
      <MainSideTabMenuComponent
        items={menus}
        mainIconUrl="coui://adr.k45/UI/images/ADR.svg"
        modTitle="Addresses & Names"
        subtitle="Mod for CS2"
        tooltip="Addresses & Names Mod for CS2"
      />
    </ErrorBoundary>
  </>;
}

