///<reference path="euis.d.ts" />
import { NameFileViewerCmp } from "#components/fileManagement/NameFileViewerCmp";
import { OverrideSettingsCmp } from "#components/overrides/OverrideSettingsCmp";
import { RoadPrefixCmp } from "#components/roadPrefix/RoadPrefixCmp";
import "#styles/main.scss";
import "#styles/react-tabs.scss";
import translate from "#utility/translate";
import { Component } from "react";
import { Tab, TabList, TabPanel, Tabs } from "react-tabs";


export default class Root extends Component<{}, {}> {

  constructor(props) {
    super(props);
  }
  async reloadFiles() {
  }

  render() {
    return <>
      <button style={{ position: "fixed", right: 0, top: 0, zIndex: 999 }} onClick={() => location.reload()}>RELOAD!!!</button>
      <Tabs defaultIndex={2}>
        <TabList>
          <Tab>{translate("fileViewer.title")}</Tab>
          <Tab>{translate("overrideSettings.title")}</Tab>
          <Tab>{translate("roadPrefixSettings.title")}</Tab>
        </TabList>
        <TabPanel>
          <NameFileViewerCmp />
        </TabPanel>
        <TabPanel>
          <OverrideSettingsCmp />
        </TabPanel>
        <TabPanel>
          <RoadPrefixCmp />
        </TabPanel>
      </Tabs>
    </>;
  }
}
