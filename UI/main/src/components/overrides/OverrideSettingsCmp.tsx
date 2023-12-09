import { DistrictListItem, DistrictRelativeService } from "#service/DistrictRelativeService";
import { AdrCitywideSettings, NameFileManagementService, SimpleNameEntry } from "#service/NameFileManagementService";
import { Cs2Checkbox, Cs2FormLine, Cs2Select, Cs2SideTabs, DefaultPanelScreen, nameToString, translate } from "@klyte45/euis-components";
import { Component } from "react";

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
      NameFileManagementService.onCityDataReloaded(() => { this.getSettings(); });
      DistrictRelativeService.onDistrictChanged(() => this.listDistricts());
    });
    this.state = { currentTab: TabsNames.Roads }
  }


  override componentWillUnmount(): void {
    NameFileManagementService.offCityDataReloaded();
    DistrictRelativeService.offDistrictChanged();
  }
  async getSettings() {
    this.setState({ currentSettings: await NameFileManagementService.getCurrentCitywideSettings() });
  }
  async listFiles() {
    const simpleFiles = (await NameFileManagementService.listSimpleNames()).sort((a, b) => a.Name.localeCompare(b.Name))
    const generalSimpleFiles = [defaultSetting, ...simpleFiles]
    const indexedSimpleFiles = generalSimpleFiles.reduce((p, n) => {
      p[n.IdString] = n;
      return p;
    }, {} as Record<string, SimpleNameEntry>)
    const innerContextSimpleFiles = [defaultSettingRoadByDistrict, ...simpleFiles]
    this.setState({ simpleFiles: generalSimpleFiles, indexedSimpleFiles: indexedSimpleFiles, innerContextSimpleFiles: innerContextSimpleFiles });
  }
  async listDistricts() {
    const districtNames = (await DistrictRelativeService.listAllDistricts())?.sort((a, b) => nameToString(a.Name).localeCompare(nameToString(b.Name)))

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
          <Cs2Checkbox isChecked={() => this.state.currentSettings?.SurnameAtFirst} onValueToggle={(x) => NameFileManagementService.setSurnameAtFirst(x)} />
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
      </>,
      [TabsNames.Roads]: <>
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
        <Cs2FormLine title={translate("overrideSettings.useRoadNameAsStationName")}>
          <Cs2Checkbox isChecked={() => this.state.currentSettings?.RoadNameAsNameStation} onValueToggle={(x) => NameFileManagementService.setRoadNameAsNameStation(x)} />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.useRoadNameAsCargoStationName")}>
          <Cs2Checkbox isChecked={() => this.state.currentSettings?.RoadNameAsNameCargoStation} onValueToggle={(x) => NameFileManagementService.setRoadNameAsNameCargoStation(x)} />
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
            onChange={(x) => NameFileManagementService.setDefaultDistrictNameOverridesStr(x.IdString)}
            value={this.state.indexedSimpleFiles[this.state.currentSettings.DefaultDistrictNameOverridesStr]}
            defaultValue={defaultSetting}
          />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.useDistrictNameAsStationName")}>
          <Cs2Checkbox isChecked={() => this.state.currentSettings?.DistrictNameAsNameStation} onValueToggle={(x) => NameFileManagementService.setDistrictNameAsNameStation(x)} />
        </Cs2FormLine>
        <Cs2FormLine title={translate("overrideSettings.useDistrictNameAsCargoStationName")}>
          <Cs2Checkbox isChecked={() => this.state.currentSettings?.DistrictNameAsNameCargoStation} onValueToggle={(x) => NameFileManagementService.setDistrictNameAsNameCargoStation(x)} />
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
        i18nTitlePrefix={"overrideSettings.tab"} />


    </DefaultPanelScreen>;

  }
}


