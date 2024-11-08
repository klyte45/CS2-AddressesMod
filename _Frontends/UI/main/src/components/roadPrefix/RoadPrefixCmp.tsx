import "#styles/roadPrefixRuleEditor.scss";
import { translate } from "#utility/translate";
import { AdrRoadPrefixRule, AdrRoadPrefixSetting, NamingRulesService, RoadFlags } from "@klyte45/adr-commons";
import { DefaultPanelScreen, Cs2FormLine, SimpleInput, Cs2TriCheckbox } from "@klyte45/euis-components";
import { useEffect, useState } from "react";

const basicRule: AdrRoadPrefixRule = {
  MinSpeedKmh: 0,
  MaxSpeedKmh: 0,
  RequiredFlagsInt: 0,
  ForbiddenFlagsInt: 0,
  FormatPattern: "{name}",
  FullBridge: 0
}

const basicObj: AdrRoadPrefixSetting = {
  AdditionalRules: [],
  FallbackRule: basicRule
}

export const RoadPrefixCmp = () => {
  const [currentEditingRule, setCurrentEditingRule] = useState(-1);
  const [saveButtonState, setSaveButtonState] = useState(0);
  const [loadButtonState, setLoadButtonState] = useState(0);
  const [currentSettings, setCurrentSettings] = useState(basicObj);

  useEffect(() => {
    getSettings();
    NamingRulesService.onCityDataReloaded(() => {
      return getSettings();
    });
    return () => { NamingRulesService.offCityDataReloaded(); }
  }, [])

  const getSettings = async () => {
    var result = (await NamingRulesService.getCurrentCitywideSettings()).roadPrefixSetting;
    console.log("getter", result)
    setCurrentSettings(result ?? basicObj);
  }
  const doSave = async () => {
    setSaveButtonState(1);
    await NamingRulesService.saveRoadPrefixRulesFileDefault();
    setSaveButtonState(2);
    setTimeout(() => setSaveButtonState(0), 3000)
  }
  const doLoad = async () => {
    setLoadButtonState(999)
    var result = await NamingRulesService.loadRoadPrefixRulesFileDefault();
    setLoadButtonState(result)
    setCurrentEditingRule(-1)
    setTimeout(() => setLoadButtonState(0), 3000)
    getSettings();
  }
  if (!currentSettings) return null;
  const buttonRow = <>
    <button className="neutralBtn" onClick={() => {
      currentSettings.AdditionalRules ??= [];
      currentSettings.AdditionalRules.push(basicRule);
      NamingRulesService.setAdrRoadPrefixSetting(currentSettings);
    }}>{translate("roadPrefixSettings.addNewRule")}</button>
    {
      saveButtonState != 0 ? <button className="darkestBtn">{translate("roadPrefixSettings.loadFromDefaults")}</button>
        : loadButtonState == 0 ? <button className="neutralBtn" onClick={() => doLoad()}>{translate("roadPrefixSettings.loadFromDefaults")}</button>
          : loadButtonState == 999 ? <button className="darkestBtn">{translate("roadPrefixSettings.loadingWaiting")}</button>
            : loadButtonState == -1 ? <button className="negativeBtn">{translate("roadPrefixSettings.loadErrorFileNotFound")}</button>
              : loadButtonState == -2 ? <button className="negativeBtn">{translate("roadPrefixSettings.loadErrorCheckLogs")}</button>
                : loadButtonState == 1 ? <button className="positiveBtn">{translate("roadPrefixSettings.loadSuccess")}</button>
                  : <button className="negativeBtn">{translate("roadPrefixSettings.loadUnknownState")}</button>
    }
    {
      loadButtonState != 0 ? <button className="darkestBtn">{translate("roadPrefixSettings.saveBtn")}</button>
        : saveButtonState == 0 ? <button className="neutralBtn" onClick={() => doSave()}>{translate("roadPrefixSettings.saveBtn")}</button>
          : saveButtonState == 1 ? <button className="darkestBtn">{translate("roadPrefixSettings.savingWaiting")}</button>
            : <button className="positiveBtn">{translate("roadPrefixSettings.saved")}</button>
    }
  </>
  return !currentSettings ? <></> :
    <DefaultPanelScreen title={translate("roadPrefixSettings.title")} subtitle={translate("roadPrefixSettings.subtitle")} buttonsRowContent={buttonRow}>
      <Cs2FormLine title={translate("roadPrefixSettings.defaultFormat")}>
        <SimpleInput
          getValue={() => currentSettings.FallbackRule?.FormatPattern}
          onValueChanged={(x) => {
            currentSettings.FallbackRule = { FormatPattern: x } as AdrRoadPrefixRule
            NamingRulesService.setAdrRoadPrefixSetting(currentSettings);
            return x;
          }}
          isValid={(newVal) => newVal?.includes("{name}")} />
      </Cs2FormLine>
      <div className="adrRulesContainer">
        <div className="rulesList">
          {
            currentSettings.AdditionalRules?.map((x, i, arr) => {
              return <Cs2FormLine
                compact={true}
                key={i}
                onClick={() => { setCurrentEditingRule(i) }} title={`${i + 1}: ${x.FormatPattern}`}
                className={i == currentEditingRule ? "selectedItem" : ""}>
                {i > 0 ? <button className="neutralBtn" onClick={() => { arraymove(arr, i, i - 1); NamingRulesService.setAdrRoadPrefixSetting(currentSettings) }}><div className="moveItemUp" /></button> : <div className="buttonPlaceholder"></div>}
                {i < arr.length - 1 ? <button className="neutralBtn" onClick={() => { arraymove(arr, i, i + 1); NamingRulesService.setAdrRoadPrefixSetting(currentSettings) }}><div className="moveItemDown" /></button> : <div className="buttonPlaceholder"></div>}
                <button className="negativeBtn" onClick={() => { arr.splice(i, 1); NamingRulesService.setAdrRoadPrefixSetting(currentSettings) }}><div className="removeItem" /></button>
              </Cs2FormLine>
            })
          }
        </div>
        <div className="ruleSetting">
          {currentEditingRule >= 0 && currentEditingRule < currentSettings.AdditionalRules?.length &&
            [null].map(x => {
              const currentRule = currentSettings.AdditionalRules[currentEditingRule];
              return <>
                <Cs2FormLine title={translate("roadPrefixSettings.patternFormat")}>
                  <SimpleInput
                    getValue={() => currentRule?.FormatPattern}
                    onValueChanged={(x) => {
                      if (!x?.includes("{name}")) return currentRule?.FormatPattern
                      currentRule.FormatPattern = x;
                      NamingRulesService.setAdrRoadPrefixSetting(currentSettings);
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
                      NamingRulesService.setAdrRoadPrefixSetting(currentSettings);
                      return x;
                    }}
                    isValid={(newVal) => !!newVal.match(/^[0-9]+$/) && currentRule.MaxSpeedKmh >= parseInt(newVal)}
                    maxLength={3} />
                </Cs2FormLine>
                <Cs2FormLine title={translate("roadPrefixSettings.maximumSpeed")}>
                  <SimpleInput
                    getValue={() => currentRule.MaxSpeedKmh.toFixed()}
                    onValueChanged={(x) => {
                      const currentItem = currentSettings.AdditionalRules[currentEditingRule];
                      const newVal = parseInt(x);
                      if (!x.match(/^[0-9]+$/) || currentItem.MinSpeedKmh > newVal) return currentItem.MaxSpeedKmh.toFixed();
                      currentItem.MaxSpeedKmh = newVal
                      NamingRulesService.setAdrRoadPrefixSetting(currentSettings);
                      return newVal.toFixed();
                    }}
                    isValid={(newVal) => newVal.match(/^[0-9]+$/) && currentRule.MinSpeedKmh <= parseInt(newVal)}
                    maxLength={3} />
                </Cs2FormLine>
                <h3>{translate("roadPrefixSettings.requiredOrForbiddenTitle")}</h3>
                {
                  [RoadFlags.EnableZoning, RoadFlags.SeparatedCarriageways, RoadFlags.PreferTrafficLights, RoadFlags.UseHighwayRules].map((x, i) => {
                    const targetVal = x;
                    const editingItem = currentSettings.AdditionalRules[currentEditingRule]
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
                      NamingRulesService.setAdrRoadPrefixSetting(currentSettings);
                    }
                    return <Cs2FormLine title={translate("roadPrefixSettings.flag" + RoadFlags[x])} key={i}>
                      <Cs2TriCheckbox isChecked={currentValue} onValueToggle={onNewValue} />
                    </Cs2FormLine>
                  })
                }
                <Cs2FormLine title={translate("roadPrefixSettings.requireFullBridgeState")}>
                  <Cs2TriCheckbox isChecked={currentRule.FullBridge < 0 ? null : currentRule.FullBridge > 0} onValueToggle={(x) => {
                    currentRule.FullBridge = x === true ? 1 : x === null ? -1 : 0;
                    NamingRulesService.setAdrRoadPrefixSetting(currentSettings);
                    return x;
                  }} />
                </Cs2FormLine>
              </>
            })[0]}
        </div>
      </div>
    </DefaultPanelScreen>;

}


function arraymove(arr: any[], fromIndex: number, toIndex: number) {
  var element = arr[fromIndex];
  arr.splice(fromIndex, 1);
  arr.splice(toIndex, 0, element);
}