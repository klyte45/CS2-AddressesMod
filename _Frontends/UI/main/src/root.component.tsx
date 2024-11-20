
import { CityNamesetLibraryCmp } from "#components/fileManagement/CityNamesetLibraryCmp";
import { OverrideSettingsCmp } from "#components/overrides/OverrideSettingsCmp";
import { RoadPrefixCmp } from "#components/roadPrefix/RoadPrefixCmp";
import "#styles/main.scss";
import { translate } from "#utility/translate";
import { AdrCitywideSettings, DistrictListItem, DistrictRelativeService, NamesetService, nameToString, NamingRulesService, SimpleNameEntry } from "@klyte45/adr-commons";
import { ErrorBoundary, MainSideTabMenuComponent, MenuItem } from "@klyte45/euis-components";
import { useEffect, useState } from "react";


export default () => {



  const menus: MenuItem[] = [
    {
      iconUrl: "coui://uil/Standard/NameSort.svg",
      name: translate("namesetManagement.title"),
      panelContent: <CityNamesetLibraryCmp/>,
      tintedIcon: true
    },
    {
      iconUrl: "coui://uil/Standard/Tools.svg",
      name: translate("overrideSettings.title"),
      panelContent: <OverrideSettingsCmp />
    },
    {
      iconUrl: "coui://uil/Standard/Highway.svg",
      name: translate("roadPrefixSettings.title"),
      panelContent: <RoadPrefixCmp />
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

