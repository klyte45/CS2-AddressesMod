import { Cs2FormLine } from "#components/common/Cs2FormLine";
import { DefaultPanelScreen } from "#components/common/DefaultPanelScreen";
import { Checkbox } from "#components/common/checkbox";
import Cs2Select from "#components/common/cs2-select";
import { AdrCitywideSettings, NameFileManagementService, SimpleNameEntry } from "#service/NameFileManagementService";
import "#styles/treeview.scss";
import "#styles/wordContainer.scss";
import translate from "#utility/translate";
import { Component } from "react";




const defaultSetting = { IdString: null, Values: [], Name: translate("overrideSettings.useVanillaOptionLbl") };
export class OverrideSettingsCmp extends Component<{}, {
  simpleFiles?: SimpleNameEntry[];
  currentSettings?: AdrCitywideSettings;
  indexedSimpleFiles?: Record<string, SimpleNameEntry>
}> {
  constructor(props) {
    super(props);
    engine.whenReady.then(() => {
      this.listFiles();
      this.getSettings();
      NameFileManagementService.onCityDataReloaded(() => this.getSettings());
    });
  }
  async getSettings() {
    this.setState({ currentSettings: await NameFileManagementService.getCurrentCitywideSettings() });
  }
  async listFiles() {
    const simpleFiles = (await NameFileManagementService.listSimpleNames()).sort((a, b) => a.Name.localeCompare(b.Name))
    simpleFiles.unshift(defaultSetting)
    const indexedSimpleFiles = simpleFiles.reduce((p, n) => {
      p[n.IdString] = n;
      return p;
    }, {} as Record<string, SimpleNameEntry>)
    this.setState({ simpleFiles: simpleFiles, indexedSimpleFiles: indexedSimpleFiles });
  }

  render() {
    if (!this.state?.currentSettings || !this.state?.currentSettings) return null;

    return <DefaultPanelScreen title={translate("overrideSettings.title")} subtitle={translate("overrideSettings.subtitle")}>
      <Cs2FormLine title={translate("overrideSettings.maleNamesFile")}>
        <Cs2Select
          options={Object.values(this.state.simpleFiles)}
          getOptionLabel={(x: SimpleNameEntry) => x?.Name}
          getOptionValue={(x: SimpleNameEntry) => x?.IdString}
          onChange={(x) => NameFileManagementService.setCitizenMaleNameOverridesStr(x.IdString)}
          value={this.state.indexedSimpleFiles[this.state.currentSettings.CitizenMaleNameOverridesStr]}
          defaultValue={defaultSetting}
        />
      </Cs2FormLine>
      <Cs2FormLine title={translate("overrideSettings.femaleNamesFile")}>
        <Cs2Select
          options={Object.values(this.state.simpleFiles)}
          getOptionLabel={(x: SimpleNameEntry) => x?.Name}
          getOptionValue={(x: SimpleNameEntry) => x?.IdString}
          onChange={(x) => NameFileManagementService.setCitizenFemaleNameOverridesStr(x.IdString)}
          value={this.state.indexedSimpleFiles[this.state.currentSettings.CitizenFemaleNameOverridesStr]}
          defaultValue={defaultSetting}
        />
      </Cs2FormLine>
      <Cs2FormLine title={translate("overrideSettings.surnamesFile")}>
        <Cs2Select
          options={Object.values(this.state.simpleFiles)}
          getOptionLabel={(x: SimpleNameEntry) => x?.Name}
          getOptionValue={(x: SimpleNameEntry) => x?.IdString}
          onChange={(x) => NameFileManagementService.setCitizenSurnameOverridesStr(x.IdString)}
          value={this.state.indexedSimpleFiles[this.state.currentSettings.CitizenSurnameOverridesStr]}
          defaultValue={defaultSetting}
        />
      </Cs2FormLine>
      <Cs2FormLine title={translate("overrideSettings.firstNameAtStart")}>
        <Checkbox isChecked={() => this.state.currentSettings?.SurnameAtFirst} onValueToggle={(x) => NameFileManagementService.setSurnameAtFirst(x)} />
      </Cs2FormLine>
      <Cs2FormLine title={translate("overrideSettings.dogsFile")}>
        <Cs2Select
          options={Object.values(this.state.simpleFiles)}
          getOptionLabel={(x: SimpleNameEntry) => x?.Name}
          getOptionValue={(x: SimpleNameEntry) => x?.IdString}
          onChange={(x) => NameFileManagementService.setCitizenDogOverridesStr(x.IdString)}
          value={this.state.indexedSimpleFiles[this.state.currentSettings.CitizenDogOverridesStr]}
          defaultValue={defaultSetting}
        />
      </Cs2FormLine>
      <Cs2FormLine title={translate("overrideSettings.roadsFile")}>
        <Cs2Select
          options={Object.values(this.state.simpleFiles)}
          getOptionLabel={(x: SimpleNameEntry) => x?.Name}
          getOptionValue={(x: SimpleNameEntry) => x?.IdString}
          onChange={(x) => NameFileManagementService.setDefaultRoadNameOverridesStr(x.IdString)}
          value={this.state.indexedSimpleFiles[this.state.currentSettings.DefaultRoadNameOverridesStr]}
          defaultValue={defaultSetting}
        />
      </Cs2FormLine>
    </DefaultPanelScreen>;

  }
}


