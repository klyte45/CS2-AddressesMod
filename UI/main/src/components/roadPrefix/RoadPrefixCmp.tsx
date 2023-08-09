import { Cs2FormLine } from "#components/common/Cs2FormLine";
import { DefaultPanelScreen } from "#components/common/DefaultPanelScreen";
import { TriCheckbox } from "#components/common/checkbox";
import { SimpleInput } from "#components/common/input";
import { AdrRoadPrefixRule, AdrRoadPrefixSetting, NameFileManagementService, RoadFlags } from "#service/NameFileManagementService";
import "#styles/roadPrefixRuleEditor.scss";
import translate from "#utility/translate";
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
  currentEditingRule: number
}> {
  constructor(props) {
    super(props);
    this.state = {
      currentEditingRule: -1
    }
    engine.whenReady.then(() => {
      this.getSettings();
      NameFileManagementService.onCityDataReloaded(() => this.getSettings());
    });
  }

  override componentWillUnmount() {
    NameFileManagementService.offCityDataReloaded();
  }

  async getSettings() {
    const newVal = (await NameFileManagementService.getCurrentCitywideSettings()).RoadPrefixSetting;
    this.setState({ currentSettings: newVal });
  }

  render() {
    if (!this.state?.currentSettings || !this.state?.currentSettings) return null;
    const buttonRow = <>
      <button className="neutralBtn" onClick={() => {
        this.state.currentSettings.AdditionalRules ??= [];
        this.state.currentSettings.AdditionalRules.push(basicRule);
        NameFileManagementService.setAdrRoadPrefixSetting(this.state.currentSettings);
      }}>{translate("roadPrefixSettings.addNewRule")}</button>
    </>
    return <DefaultPanelScreen title={translate("roadPrefixSettings.title")} subtitle={translate("roadPrefixSettings.subtitle")} buttonsRowContent={buttonRow}>
      <Cs2FormLine title={translate("roadPrefixSettings.defaultFormat")}>
        <SimpleInput
          getValue={() => this.state.currentSettings.FallbackRule?.FormatPattern}
          onValueChanged={(x) => {
            this.state.currentSettings.FallbackRule = { FormatPattern: x } as AdrRoadPrefixRule
            NameFileManagementService.setAdrRoadPrefixSetting(this.state.currentSettings);
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
                {i > 0 ? <button className="neutralBtn" onClick={() => { arraymove(arr, i, i - 1); NameFileManagementService.setAdrRoadPrefixSetting(this.state.currentSettings) }}><div className="moveItemUp" /></button> : <div className="buttonPlaceholder"></div>}
                {i < arr.length - 1 ? <button className="neutralBtn" onClick={() => { arraymove(arr, i, i + 1); NameFileManagementService.setAdrRoadPrefixSetting(this.state.currentSettings) }}><div className="moveItemDown" /></button> : <div className="buttonPlaceholder"></div>}
                <button className="negativeBtn" onClick={() => { arr.splice(i, 1); NameFileManagementService.setAdrRoadPrefixSetting(this.state.currentSettings) }}><div className="removeItem" /></button>
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
                      NameFileManagementService.setAdrRoadPrefixSetting(this.state.currentSettings);
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
                      NameFileManagementService.setAdrRoadPrefixSetting(this.state.currentSettings);
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
                      NameFileManagementService.setAdrRoadPrefixSetting(this.state.currentSettings);
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
                      NameFileManagementService.setAdrRoadPrefixSetting(this.state.currentSettings);
                    }
                    return <Cs2FormLine title={translate("roadPrefixSettings.flag" + RoadFlags[x])} key={i}>
                      <TriCheckbox isChecked={currentValue} onValueToggle={onNewValue} />
                    </Cs2FormLine>
                  })
                }
                <Cs2FormLine title={translate("roadPrefixSettings.requireFullBridgeState")}>
                  <TriCheckbox isChecked={currentRule.FullBridge < 0 ? null : currentRule.FullBridge > 0} onValueToggle={(x) => {
                    currentRule.FullBridge = x === true ? 1 : x === null ? -1 : 0;
                    NameFileManagementService.setAdrRoadPrefixSetting(this.state.currentSettings);
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