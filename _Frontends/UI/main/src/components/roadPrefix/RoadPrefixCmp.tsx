import { Cs2FormLine } from "@klyte45/euis-components";
import { DefaultPanelScreen } from "@klyte45/euis-components";
import { Cs2TriCheckbox } from "@klyte45/euis-components";
import { SimpleInput } from "@klyte45/euis-components";
import { AdrRoadPrefixRule, AdrRoadPrefixSetting, NamingRulesService, RoadFlags } from "@klyte45/adr-commons";
import "#styles/roadPrefixRuleEditor.scss";
import { translate } from "#utility/translate";
import { Component } from "react";

const basicRule: AdrRoadPrefixRule = {
  MinSpeedKmh: 0,
  MaxSpeedKmh: 0,
  RequiredFlagsInt: 0,
  ForbiddenFlagsInt: 0,
  FormatPattern: "{name}",
  FullBridge: 0
}

export class RoadPrefixCmp extends Component<{}, {
  currentSettings?: AdrRoadPrefixSetting;
  currentEditingRule: number,
  saveButtonState: number,
  loadButtonState: number
}> {
  constructor(props) {
    super(props);
    this.state = {
      currentEditingRule: -1,
      saveButtonState: 0,
      loadButtonState: 0
    }
    engine.whenReady.then(() => {
      this.getSettings();
      NamingRulesService.onCityDataReloaded(() => this.getSettings());
    });
  }

  override componentWillUnmount() {
    NamingRulesService.offCityDataReloaded();
  }

  async getSettings() {
    const newVal = (await NamingRulesService.getCurrentCitywideSettings()).RoadPrefixSetting;
    this.setState({ currentSettings: newVal });
  }
  async doSave() {
    await new Promise((res) => this.setState({ saveButtonState: 1 }, () => res(0)))
    await NamingRulesService.saveRoadPrefixRulesFileDefault();
    await new Promise((res) => this.setState({ saveButtonState: 2 }, () => res(0)))
    setTimeout(() => this.setState({ saveButtonState: 0 }), 3000)
  }
  async doLoad() {
    await new Promise((res) => this.setState({ loadButtonState: 999 }, () => res(0)))
    var result = await NamingRulesService.loadRoadPrefixRulesFileDefault();
    await new Promise((res) => this.setState({ loadButtonState: result, currentEditingRule: -1 }, () => res(0)))
    setTimeout(() => this.setState({ loadButtonState: 0 }), 3000)
    this.getSettings();
  }
  render() {
    if (!this.state?.currentSettings || !this.state?.currentSettings) return null;
    const buttonRow = <>
      <button className="neutralBtn" onClick={() => {
        this.state.currentSettings.AdditionalRules ??= [];
        this.state.currentSettings.AdditionalRules.push(basicRule);
        NamingRulesService.setAdrRoadPrefixSetting(this.state.currentSettings);
      }}>{translate("roadPrefixSettings.addNewRule")}</button>
      {
        this.state.saveButtonState != 0 ? <button className="darkestBtn">{translate("roadPrefixSettings.loadFromDefaults")}</button>
          : this.state.loadButtonState == 0 ? <button className="neutralBtn" onClick={() => this.doLoad()}>{translate("roadPrefixSettings.loadFromDefaults")}</button>
            : this.state.loadButtonState == 999 ? <button className="darkestBtn">{translate("roadPrefixSettings.loadingWaiting")}</button>
              : this.state.loadButtonState == -1 ? <button className="negativeBtn">{translate("roadPrefixSettings.loadErrorFileNotFound")}</button>
                : this.state.loadButtonState == -2 ? <button className="negativeBtn">{translate("roadPrefixSettings.loadErrorCheckLogs")}</button>
                  : this.state.loadButtonState == 1 ? <button className="positiveBtn">{translate("roadPrefixSettings.loadSuccess")}</button>
                    : <button className="negativeBtn">{translate("roadPrefixSettings.loadUnknownState")}</button>
      }
      {
        this.state.loadButtonState != 0 ? <button className="darkestBtn">{translate("roadPrefixSettings.saveBtn")}</button>
          : this.state.saveButtonState == 0 ? <button className="neutralBtn" onClick={() => this.doSave()}>{translate("roadPrefixSettings.saveBtn")}</button>
            : this.state.saveButtonState == 1 ? <button className="darkestBtn">{translate("roadPrefixSettings.savingWaiting")}</button>
              : <button className="positiveBtn">{translate("roadPrefixSettings.saved")}</button>
      }
    </>
    return <DefaultPanelScreen title={translate("roadPrefixSettings.title")} subtitle={translate("roadPrefixSettings.subtitle")} buttonsRowContent={buttonRow}>
      <Cs2FormLine title={translate("roadPrefixSettings.defaultFormat")}>
        <SimpleInput
          getValue={() => this.state.currentSettings.FallbackRule?.FormatPattern}
          onValueChanged={(x) => {
            this.state.currentSettings.FallbackRule = { FormatPattern: x } as AdrRoadPrefixRule
            NamingRulesService.setAdrRoadPrefixSetting(this.state.currentSettings);
            return x;
          }}
          isValid={(newVal) => newVal?.includes("{name}")} />
      </Cs2FormLine>
      <div className="adrRulesContainer">
        <div className="rulesList">
          {
            this.state.currentSettings.AdditionalRules?.map((x, i, arr) => {
              return <Cs2FormLine
                compact={true}
                key={i}
                onClick={() => { this.setState({ currentEditingRule: i }) }} title={`${i + 1}: ${x.FormatPattern}`}
                className={i == this.state.currentEditingRule ? "selectedItem" : ""}>
                {i > 0 ? <button className="neutralBtn" onClick={() => { arraymove(arr, i, i - 1); NamingRulesService.setAdrRoadPrefixSetting(this.state.currentSettings) }}><div className="moveItemUp" /></button> : <div className="buttonPlaceholder"></div>}
                {i < arr.length - 1 ? <button className="neutralBtn" onClick={() => { arraymove(arr, i, i + 1); NamingRulesService.setAdrRoadPrefixSetting(this.state.currentSettings) }}><div className="moveItemDown" /></button> : <div className="buttonPlaceholder"></div>}
                <button className="negativeBtn" onClick={() => { arr.splice(i, 1); NamingRulesService.setAdrRoadPrefixSetting(this.state.currentSettings) }}><div className="removeItem" /></button>
              </Cs2FormLine>
            })
          }
        </div>
        <div className="ruleSetting">
          {this.state.currentEditingRule >= 0 && this.state.currentEditingRule < this.state.currentSettings.AdditionalRules?.length &&
            [null].map(x => {
              const currentRule = this.state.currentSettings.AdditionalRules[this.state.currentEditingRule];
              return <>
                <Cs2FormLine title={translate("roadPrefixSettings.patternFormat")}>
                  <SimpleInput
                    getValue={() => currentRule?.FormatPattern}
                    onValueChanged={(x) => {
                      if (!x?.includes("{name}")) return currentRule?.FormatPattern
                      currentRule.FormatPattern = x;
                      NamingRulesService.setAdrRoadPrefixSetting(this.state.currentSettings);
                      return x;
                    }}
                    isValid={(newVal) => newVal?.includes("{name}")} />
                </Cs2FormLine>
                <Cs2FormLine title={translate("roadPrefixSettings.minimumSpeed")}>
                  <SimpleInput
                    getValue={() => currentRule.MinSpeedKmh.toFixed()}
                    onValueChanged={(x) => {
                      if (!x.match(/^[0-9]+$/)) return currentRule.MinSpeedKmh.toFixed();
                      currentRule.MinSpeedKmh = parseInt(x)
                      NamingRulesService.setAdrRoadPrefixSetting(this.state.currentSettings);
                      return x;
                    }}
                    isValid={(newVal) => !!newVal.match(/^[0-9]+$/) && currentRule.MaxSpeedKmh >= parseInt(newVal)}
                    maxLength={3} />
                </Cs2FormLine>
                <Cs2FormLine title={translate("roadPrefixSettings.maximumSpeed")}>
                  <SimpleInput
                    getValue={() => currentRule.MaxSpeedKmh.toFixed()}
                    onValueChanged={(x) => {
                      const currentItem = this.state.currentSettings.AdditionalRules[this.state.currentEditingRule];
                      const newVal = parseInt(x);
                      if (!x.match(/^[0-9]+$/) || currentItem.MinSpeedKmh > newVal) return currentItem.MaxSpeedKmh.toFixed();
                      currentItem.MaxSpeedKmh = newVal
                      NamingRulesService.setAdrRoadPrefixSetting(this.state.currentSettings);
                      return newVal.toFixed();
                    }}
                    isValid={(newVal) => newVal.match(/^[0-9]+$/) && currentRule.MinSpeedKmh <= parseInt(newVal)}
                    maxLength={3} />
                </Cs2FormLine>
                <h3>{translate("roadPrefixSettings.requiredOrForbiddenTitle")}</h3>
                {
                  [RoadFlags.EnableZoning, RoadFlags.SeparatedCarriageways, RoadFlags.PreferTrafficLights, RoadFlags.UseHighwayRules].map((x, i) => {
                    const targetVal = x;
                    const editingItem = this.state.currentSettings.AdditionalRules[this.state.currentEditingRule]
                    const currentValue = (editingItem.RequiredFlagsInt & targetVal) != 0 ? true : (editingItem.ForbiddenFlagsInt & targetVal) != 0 ? null : false;
                    const onNewValue = (x) => {
                      if (x === null) {
                        currentRule.RequiredFlagsInt &= ~targetVal;
                        currentRule.ForbiddenFlagsInt |= targetVal;
                      } else if (x) {
                        currentRule.ForbiddenFlagsInt &= ~targetVal;
                        currentRule.RequiredFlagsInt |= targetVal;
                      } else {
                        currentRule.ForbiddenFlagsInt &= ~targetVal;
                        currentRule.RequiredFlagsInt &= ~targetVal;
                      }
                      NamingRulesService.setAdrRoadPrefixSetting(this.state.currentSettings);
                    }
                    return <Cs2FormLine title={translate("roadPrefixSettings.flag" + RoadFlags[x])} key={i}>
                      <Cs2TriCheckbox isChecked={currentValue} onValueToggle={onNewValue} />
                    </Cs2FormLine>
                  })
                }
                <Cs2FormLine title={translate("roadPrefixSettings.requireFullBridgeState")}>
                  <Cs2TriCheckbox isChecked={currentRule.FullBridge < 0 ? null : currentRule.FullBridge > 0} onValueToggle={(x) => {
                    currentRule.FullBridge = x === true ? 1 : x === null ? -1 : 0;
                    NamingRulesService.setAdrRoadPrefixSetting(this.state.currentSettings);
                    return x;
                  }} />
                </Cs2FormLine>
              </>
            })[0]}
        </div>
      </div>
    </DefaultPanelScreen>;
  }
}


function arraymove(arr: any[], fromIndex: number, toIndex: number) {
  var element = arr[fromIndex];
  arr.splice(fromIndex, 1);
  arr.splice(toIndex, 0, element);
}