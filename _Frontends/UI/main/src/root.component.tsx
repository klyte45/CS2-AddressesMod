
import { CityNamesetLibraryCmp } from "#components/fileManagement/CityNamesetLibraryCmp";
import { OverrideSettingsCmp } from "#components/overrides/OverrideSettingsCmp";
import { RegionEditor } from "#components/region/RegionEditor";
import { RoadPrefixCmp } from "#components/roadPrefix/RoadPrefixCmp";
import "#styles/main.scss";
import { translate } from "#utility/translate";
import { AdrMainService } from "@klyte45/adr-commons";
import { ErrorBoundary, MainSideTabMenuComponent, MenuItem } from "@klyte45/euis-components";
import { useEffect, useState } from "react";


export default () => {

  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    AdrMainService.isCityOrEditorLoaded().then(setIsReady);
  }, [])

  if (!isReady)
    return <>
      <h2 style={{ display: "flex", width: "100%", height: "100%", alignItems: "center", justifyContent: "center", fontSize: "300%" }}>{translate("loadACityWarningH2")}</h2>
    </>

  const menus: MenuItem[] = [
    {
      iconUrl: "coui://adr.k45/UI/images/outsideConnections.svg",
      name: translate("regionSettings.title"),
      panelContent: <RegionEditor />
    },
    {
      iconUrl: "coui://uil/Standard/NameSort.svg",
      name: translate("namesetManagement.title"),
      panelContent: <CityNamesetLibraryCmp />,
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
    },
  ]
  return <>
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

