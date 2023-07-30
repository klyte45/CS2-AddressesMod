///<reference path="euis.d.ts" />
import { Cs2FormLine } from "#components/common/Cs2FormLine";
import "#styles/main.scss"
import "#styles/react-tabs.scss"
import translate from "#utility/translate";
import { Component } from "react";
import { Tabs, TabList, Tab, TabPanel } from "react-tabs";

type SimpleNameEntry = {
  IdString: string,
  Name: string,
  Values: string[]
}


export default class Root extends Component<{}, { items?: SimpleNameEntry[] }> {

  constructor(props) {
    super(props);
    engine.whenReady.then(() => {
      this.reloadFiles();
    })
  }
  async reloadFiles() {
    this.setState({ items: await engine.call("k45::adr.main.listSimpleNames") })
  }

  render() {
    return <>
      <button style={{ position: "fixed", right: 0, top: 0, zIndex: 999 }} onClick={() => location.reload()}>RELOAD!!!</button>
      <Tabs defaultIndex={2}>
        <TabList>
          <Tab>{translate("cityPalettesLibrary.title")}</Tab>
          <Tab>{translate("palettesSettings.title")}</Tab>
          <Tab>{translate("lineList.title")}</Tab>
        </TabList>
        <TabPanel>
          {this.state?.items.map((x, i) => <Cs2FormLine key={i} title={`${x.IdString}: ${x.Name}`} >{`${x.Values.length} items`} </Cs2FormLine>)}
        </TabPanel>
        <TabPanel></TabPanel>
        <TabPanel></TabPanel>
      </Tabs>
    </>;
  }
}