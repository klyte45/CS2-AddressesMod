import { DistrictListItem, DistrictRelativeService } from "@klyte45/adr-commons";
import { AdrCitywideSettings, NamingRulesService, SimpleNameEntry } from "@klyte45/adr-commons";
import { Cs2Checkbox, Cs2FormLine, Cs2Select, Cs2SideTabs, DefaultPanelScreen, Input, nameToString } from "@klyte45/euis-components";
import { Component } from "react";
import { translate } from "#utility/translate";
import { NamesetService } from "@klyte45/adr-commons";

enum TabsNames {
  Roads = "Roads",
  Citizen = "Citizen",
  District = "District",
}

const tabsOrder: (TabsNames | undefined)[] = [

]

type State = {
  simpleFiles?: SimpleNameEntry[];
  innerContextSimpleFiles?: SimpleNameEntry[];
  currentSettings?: AdrCitywideSettings;
  indexedSimpleFiles?: Record<string, SimpleNameEntry>
  currentTab: TabsNames,
  districts?: DistrictListItem[],
  selectedDistrict?: DistrictListItem
}


const defaultSetting = { IdString: null, Values: [], Name: translate("overrideSettings.useVanillaOptionLbl") };
const defaultSettingRoadByDistrict = { IdString: null, Values: [], Name: translate("overrideSettings.useSameAsCityOptionLbl") };
export class OverrideSettingsCmp extends Component<{}, State> {
  constructor(props) {
    super(props);
    engine.whenReady.then(() => {
      this.listFiles();
      this.getSettings();
      this.listDistricts();
      NamingRulesService.onCityDataReloaded(() => { this.getSettings(); });
      DistrictRelativeService.onDistrictChanged(() => this.listDistricts());
    });
    this.state = { currentTab: TabsNames.Roads }
  }


  override componentWillUnmount(): void {
    NamingRulesService.offCityDataReloaded();
    DistrictRelativeService.offDistrictChanged();
  }
  async getSettings() {
    this.setState({ currentSettings: await NamingRulesService.getCurrentCitywideSettings() });
  }
  async listFiles() {
    const simpleFiles = (await NamesetService.listCityNamesets()).sort((a, b) => a.Name.localeCompare(b.Name, undefined, { sensitivity: "base" }))
    const generalSimpleFiles = [defaultSetting, ...simpleFiles]
    const indexedSimpleFiles = generalSimpleFiles.reduce((p, n) => {
      p[n.IdString] = n;
      return p;
    }, {} as Record<string, SimpleNameEntry>)
    const innerContextSimpleFiles = [defaultSettingRoadByDistrict, ...simpleFiles]
    this.setState({ simpleFiles: generalSimpleFiles, indexedSimpleFiles: indexedSimpleFiles, innerContextSimpleFiles: innerContextSimpleFiles });
  }
  async listDistricts() {
    const districtNames = (await DistrictRelativeService.listAllDistricts())?.sort((a, b) => nameToString(a.Name).localeCompare(nameToString(b.Name), undefined, { sensitivity: "base" }))

    this.setState({ districts: districtNames, selectedDistrict: this.state?.selectedDistrict ? districtNames.find(x => x.Entity.Index == this.state.selectedDistrict.Entity.Index) : null });
  }

  private getComponents(): Record<TabsNames, JSX.Element> {
    return {
      [TabsNames.Citizen]: <>

        <Cs2FormLine title={translate("overrideSettings.maleNamesFile")}>
          <Cs2Select
            options={Object.values(this.state.simpleFiles)}
            getOptionLabel={(x: SimpleNameEntry) => x?.Name}
            getOptionValue={(x: SimpleNameEntry) => x?.IdString}
            onChange={(x) => NamingRulesService.setCitizenMaleNameOverridesStr(x.IdString)}
            value={this.state.indexedSimpleFiles[this.state.currentSettings.CitizenMaleNameOverridesStr]}
            defaultValue={defaultSetting}
          />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.femaleNamesFile")}>
          <Cs2Select
            options={Object.values(this.state.simpleFiles)}
            getOptionLabel={(x: SimpleNameEntry) => x?.Name}
            getOptionValue={(x: SimpleNameEntry) => x?.IdString}
            onChange={(x) => NamingRulesService.setCitizenFemaleNameOverridesStr(x.IdString)}
            value={this.state.indexedSimpleFiles[this.state.currentSettings.CitizenFemaleNameOverridesStr]}
            defaultValue={defaultSetting}
          />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.surnamesFile")}>
          <Cs2Select
            options={Object.values(this.state.simpleFiles)}
            getOptionLabel={(x: SimpleNameEntry) => x?.Name}
            getOptionValue={(x: SimpleNameEntry) => x?.IdString}
            onChange={(x) => NamingRulesService.setCitizenSurnameOverridesStr(x.IdString)}
            value={this.state.indexedSimpleFiles[this.state.currentSettings.CitizenSurnameOverridesStr]}
            defaultValue={defaultSetting}
          />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.firstNameAtStart")}>
          <Cs2Checkbox isChecked={() => this.state.currentSettings?.surnameAtFirst} onValueToggle={(x) => NamingRulesService.setSurnameAtFirst(x)} />
        </Cs2FormLine>
        <Input title={translate("overrideSettings.maximumGeneratedGivenNames")} getValue={() => this.state.currentSettings.MaximumGeneratedGivenNames.toFixed(0)} maxLength={1} isValid={(x) => parseInt(x) >= 1 && parseInt(x) <= 5} onValueChanged={async (x) => NamingRulesService.setMaxGivenNames(parseInt(x))} />
        <Input title={translate("overrideSettings.maximumGeneratedSurnames")} getValue={() => this.state.currentSettings.MaximumGeneratedSurnames.toFixed(0)} maxLength={1} isValid={(x) => parseInt(x) >= 1 && parseInt(x) <= 5} onValueChanged={async (x) => NamingRulesService.setMaxSurnames(parseInt(x))} />
        <Cs2FormLine title={translate("overrideSettings.dogsFile")}>
          <Cs2Select
            options={Object.values(this.state.simpleFiles)}
            getOptionLabel={(x: SimpleNameEntry) => x?.Name}
            getOptionValue={(x: SimpleNameEntry) => x?.IdString}
            onChange={(x) => NamingRulesService.setCitizenDogOverridesStr(x.IdString)}
            value={this.state.indexedSimpleFiles[this.state.currentSettings.CitizenDogOverridesStr]}
            defaultValue={defaultSetting}
          />
        </Cs2FormLine>
      </>,
      [TabsNames.Roads]: <>
        <Cs2FormLine title={translate("overrideSettings.roadsFile")}>
          <Cs2Select
            options={Object.values(this.state.simpleFiles)}
            getOptionLabel={(x: SimpleNameEntry) => x?.Name}
            getOptionValue={(x: SimpleNameEntry) => x?.IdString}
            onChange={(x) => NamingRulesService.setDefaultRoadNameOverridesStr(x.IdString)}
            value={this.state.indexedSimpleFiles[this.state.currentSettings.DefaultRoadNameOverridesStr]}
            defaultValue={defaultSetting}
          />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.useRoadNameAsStationName")}>
          <Cs2Checkbox isChecked={() => this.state.currentSettings?.roadNameAsNameStation} onValueToggle={(x) => NamingRulesService.setRoadNameAsNameStation(x)} />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.useRoadNameAsCargoStationName")}>
          <Cs2Checkbox isChecked={() => this.state.currentSettings?.roadNameAsNameCargoStation} onValueToggle={(x) => NamingRulesService.setRoadNameAsNameCargoStation(x)} />
        </Cs2FormLine>
        <h2>{translate("overrideSettings.perDistrictRoadsFile")}</h2>
        <Cs2Select
          options={this.state.districts}
          getOptionLabel={(x: DistrictListItem) => nameToString(x?.Name)}
          getOptionValue={(x: DistrictListItem) => x?.Entity.Index.toString()}
          onChange={(x) => this.setState({ selectedDistrict: x })}
          value={this.state.selectedDistrict}
        />
        {
          this.state.selectedDistrict && <>
            <Cs2Select
              options={this.state.innerContextSimpleFiles}
              getOptionLabel={(x: SimpleNameEntry) => x?.Name}
              getOptionValue={(x: SimpleNameEntry) => x?.IdString}
              onChange={async (x) => await DistrictRelativeService.setRoadNamesFile(this.state.selectedDistrict.Entity, x.IdString)}
              value={this.state.innerContextSimpleFiles.find(x => x.IdString == this.state.selectedDistrict?.CurrentValue)}
              defaultValue={defaultSettingRoadByDistrict}
            />
          </>
        }
      </>,
      [TabsNames.District]: <>
        <Cs2FormLine title={translate("overrideSettings.districtsFile")}>
          <Cs2Select
            options={Object.values(this.state.simpleFiles)}
            getOptionLabel={(x: SimpleNameEntry) => x?.Name}
            getOptionValue={(x: SimpleNameEntry) => x?.IdString}
            onChange={(x) => NamingRulesService.setDefaultDistrictNameOverridesStr(x.IdString)}
            value={this.state.indexedSimpleFiles[this.state.currentSettings.DefaultDistrictNameOverridesStr]}
            defaultValue={defaultSetting}
          />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.useDistrictNameAsStationName")}>
          <Cs2Checkbox isChecked={() => this.state.currentSettings?.districtNameAsNameStation} onValueToggle={(x) => NamingRulesService.setDistrictNameAsNameStation(x)} />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.useDistrictNameAsCargoStationName")}>
          <Cs2Checkbox isChecked={() => this.state.currentSettings?.districtNameAsNameCargoStation} onValueToggle={(x) => NamingRulesService.setDistrictNameAsNameCargoStation(x)} />
        </Cs2FormLine>
      </>,
    }
  }

  render() {
    if (!this.state?.currentSettings || !this.state?.currentSettings) return null;

    return <DefaultPanelScreen title={translate("overrideSettings.title")} subtitle={translate("overrideSettings.subtitle")} >

      <Cs2SideTabs<TabsNames>
        componentsMapViewer={this.getComponents()}
        tabsOrder={[TabsNames.Citizen, TabsNames.Roads, TabsNames.District]}
        currentTab={this.state.currentTab}
        onSetCurrentTab={(newTab) => this.setState({ currentTab: newTab })}
        i18nTitlePrefix={"overrideSettings.tab"}
        translateFn={translate} />


    </DefaultPanelScreen>;

  }
}


