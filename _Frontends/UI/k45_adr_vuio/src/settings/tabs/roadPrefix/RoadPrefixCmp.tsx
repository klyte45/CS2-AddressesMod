import {
    AdrRoadPrefixRule,
    AdrRoadPrefixSetting,
    NamingRulesService,
    RoadFlags,
} from "adr";
import { FormLine, VanillaWidgets } from "@klyte45/vuio-commons";
import { useEffect, useMemo, useState } from "react";
import { translate } from "../../translate";
import styles from "../../styles/roadPrefixRuleEditor.module.scss";

const basicRule: AdrRoadPrefixRule = {
    MinSpeedKmh: 0,
    MaxSpeedKmh: 0,
    RequiredFlagsInt: 0,
    ForbiddenFlagsInt: 0,
    FormatPattern: "{name}",
    FullBridge: 0,
    AnyElevated: 0,
    MinCarLanes: 0,
    MaxCarLanes: 0,
    MinWidthM: 0,
    MaxWidthM: 0,
};

const basicObj: AdrRoadPrefixSetting = {
    AdditionalRules: [],
    FallbackRule: basicRule,
};

type ButtonStatus = {
    className: string;
    disabled: boolean;
    key: string;
};

const cloneRule = (rule: AdrRoadPrefixRule): AdrRoadPrefixRule => ({ ...rule });
const cloneSettings = (settings: AdrRoadPrefixSetting): AdrRoadPrefixSetting => ({
    FallbackRule: cloneRule(settings.FallbackRule ?? basicRule),
    AdditionalRules: (settings.AdditionalRules ?? []).map(cloneRule),
});
const newRule = (): AdrRoadPrefixRule => cloneRule(basicRule);

function arrayMove<T>(arr: T[], fromIndex: number, toIndex: number) {
    const element = arr[fromIndex];
    arr.splice(fromIndex, 1);
    arr.splice(toIndex, 0, element);
}

export const RoadPrefixCmp = () => {
    const VW = VanillaWidgets.instance;
    const editorModule = VW.editorItemModule;
    const IntInput = VW.IntInputStandalone;
    const FloatInput = VW.FloatInputStandalone;
    const Checkbox = VW.Checkbox;
    const Scrollable = VW.EditorScrollable;

    const [currentEditingRule, setCurrentEditingRule] = useState(-1);
    const [saveButtonState, setSaveButtonState] = useState(0);
    const [loadButtonState, setLoadButtonState] = useState(0);
    const [currentSettings, setCurrentSettings] = useState<AdrRoadPrefixSetting | null>(null);

    const loadSettings = async () => {
        const citySettings = await NamingRulesService.getCurrentCitywideSettings();
        setCurrentSettings(cloneSettings(citySettings.roadPrefixSetting ?? basicObj));
    };

    useEffect(() => {
        void loadSettings();
        const onReload = () => { void loadSettings(); };
        NamingRulesService.onCityDataReloaded(onReload);
        return () => {
            NamingRulesService.offCityDataReloaded();
        };
    }, []);

    const updateSettings = (mutate: (draft: AdrRoadPrefixSetting) => void) => {
        setCurrentSettings((prev) => {
            const source = prev ?? basicObj;
            const draft = cloneSettings(source);
            mutate(draft);
            void NamingRulesService.setAdrRoadPrefixSetting(draft);
            return draft;
        });
    };

    const doSave = async () => {
        setSaveButtonState(1);
        await NamingRulesService.saveRoadPrefixRulesFileDefault();
        setSaveButtonState(2);
        setTimeout(() => setSaveButtonState(0), 3000);
    };

    const doLoad = async () => {
        setLoadButtonState(999);
        const result = await NamingRulesService.loadRoadPrefixRulesFileDefault();
        setLoadButtonState(result);
        setCurrentEditingRule(-1);
        if (result === 1) {
            await loadSettings();
        }
        setTimeout(() => setLoadButtonState(0), 3000);
    };

    const loadButtonConfig = useMemo((): ButtonStatus => {
        if (saveButtonState !== 0) return { className: "darkestBtn", disabled: true, key: "loadingWaiting" };
        const loadStates: Record<number, ButtonStatus> = {
            0: { className: "neutralBtn", disabled: false, key: "loadFromDefaults" },
            999: { className: "darkestBtn", disabled: true, key: "loadingWaiting" },
            1: { className: "positiveBtn", disabled: true, key: "loadSuccess" },
            [-1]: { className: "negativeBtn", disabled: true, key: "loadErrorFileNotFound" },
            [-2]: { className: "negativeBtn", disabled: true, key: "loadErrorCheckLogs" },
        };
        return loadStates[loadButtonState] ?? { className: "negativeBtn", disabled: true, key: "loadErrorCheckLogs" };
    }, [loadButtonState, saveButtonState]);

    const saveButtonConfig = useMemo((): ButtonStatus => {
        if (loadButtonState !== 0) return { className: "darkestBtn", disabled: true, key: "savingWaiting" };
        const saveStates: Record<number, ButtonStatus> = {
            0: { className: "neutralBtn", disabled: false, key: "saveBtn" },
            1: { className: "darkestBtn", disabled: true, key: "savingWaiting" },
            2: { className: "positiveBtn", disabled: true, key: "saved" },
        };
        return saveStates[saveButtonState] ?? saveStates[0];
    }, [loadButtonState, saveButtonState]);

    if (!currentSettings) return null;

    const currentRule = currentEditingRule >= 0 && currentEditingRule < currentSettings.AdditionalRules.length
        ? currentSettings.AdditionalRules[currentEditingRule]
        : null;

    const setRuleFlag = (targetVal: RoadFlags, nextValue: boolean | undefined) => {
        if (currentEditingRule < 0) return;
        updateSettings((draft) => {
            const rule = draft.AdditionalRules[currentEditingRule];
            if (nextValue === undefined) {
                rule.RequiredFlagsInt &= ~targetVal;
                rule.ForbiddenFlagsInt |= targetVal;
            } else if (nextValue) {
                rule.ForbiddenFlagsInt &= ~targetVal;
                rule.RequiredFlagsInt |= targetVal;
            } else {
                rule.ForbiddenFlagsInt &= ~targetVal;
                rule.RequiredFlagsInt &= ~targetVal;
            }
        });
    };

    return (
        <div className={`${styles.k45_adr_roadPrefixSettings_root} k45_btnHost`}>
            <div className={styles.topBar}>
                <button
                    type="button"
                    className="neutralBtn"
                    onClick={() => {
                        updateSettings((draft) => {
                            draft.AdditionalRules.push(newRule());
                        });
                        setCurrentEditingRule(currentSettings.AdditionalRules.length);
                    }}
                >
                    {translate("roadPrefixSettings.addNewRule")}
                </button>
                <button
                    type="button"
                    className={loadButtonConfig.className}
                    onClick={() => { void doLoad(); }}
                    disabled={loadButtonConfig.disabled}
                >
                    {translate(`roadPrefixSettings.${loadButtonConfig.key}`)}
                </button>
                <button
                    type="button"
                    className={saveButtonConfig.className}
                    onClick={() => { void doSave(); }}
                    disabled={saveButtonConfig.disabled}
                >
                    {translate(`roadPrefixSettings.${saveButtonConfig.key}`)}
                </button>
            </div>

            <div className={styles.body}>
                <FormLine
                    title={translate("roadPrefixSettings.defaultFormat")}
                    subtitle={translate("roadPrefixSettings.subtitle")}
                >
                    <VW.StringInputField
                        className={editorModule.input}
                        value={currentSettings.FallbackRule?.FormatPattern ?? "{name}"}
                        onChange={(value) => {
                            if (!value.includes("{name}")) return;
                            updateSettings((draft) => {
                                draft.FallbackRule = { ...draft.FallbackRule, FormatPattern: value };
                            });
                        }}
                    />
                </FormLine>

                <div className={styles.ruleShell}>
                    <Scrollable className={styles.rulesList}>
                        {currentSettings.AdditionalRules.map((rule, i, arr) => (
                            <FormLine
                                compact={true}
                                key={`${i}-${rule.FormatPattern}`}
                                onClick={() => setCurrentEditingRule(i)}
                                title={`${i + 1}: ${rule.FormatPattern}`}
                                className={i === currentEditingRule ? styles.selectedItem : ""}
                            >
                                {i > 0 ? (
                                    <button
                                        type="button"
                                        className="neutralBtn"
                                        onClick={(evt) => {
                                            evt.stopPropagation();
                                            updateSettings((draft) => {
                                                arrayMove(draft.AdditionalRules, i, i - 1);
                                            });
                                            setCurrentEditingRule(i - 1);
                                        }}
                                    >
                                        <div className={styles.moveItemUp} />
                                    </button>
                                ) : <div className={styles.buttonPlaceholder} />}
                                {i < arr.length - 1 ? (
                                    <button
                                        type="button"
                                        className="neutralBtn"
                                        onClick={(evt) => {
                                            evt.stopPropagation();
                                            updateSettings((draft) => {
                                                arrayMove(draft.AdditionalRules, i, i + 1);
                                            });
                                            setCurrentEditingRule(i + 1);
                                        }}
                                    >
                                        <div className={styles.moveItemDown} />
                                    </button>
                                ) : <div className={styles.buttonPlaceholder} />}
                                <button
                                    type="button"
                                    className="negativeBtn"
                                    onClick={(evt) => {
                                        evt.stopPropagation();
                                        updateSettings((draft) => {
                                            draft.AdditionalRules.splice(i, 1);
                                        });
                                        setCurrentEditingRule((prev) => {
                                            if (prev > i) return prev - 1;
                                            if (prev === i) return -1;
                                            return prev;
                                        });
                                    }}
                                >
                                    <div className={styles.removeItem} />
                                </button>
                            </FormLine>
                        ))}
                    </Scrollable>

                    <div className={styles.ruleSettings}>
                        {currentRule && (
                            <>
                                <FormLine
                                    title={translate("roadPrefixSettings.patternFormat")}
                                    tooltip={translate("roadPrefixSettings.tooltip.patternFormat")}
                                >
                                    <VW.StringInputField
                                        className={editorModule.input}
                                        value={currentRule.FormatPattern}
                                        onChange={(value) => {
                                            if (!value.includes("{name}")) return;
                                            updateSettings((draft) => {
                                                draft.AdditionalRules[currentEditingRule].FormatPattern = value;
                                            });
                                        }}
                                    />
                                </FormLine>
                                <FormLine
                                    title={translate("roadPrefixSettings.minimumSpeed")}
                                    tooltip={translate("roadPrefixSettings.tooltip.minimumSpeed")}
                                >
                                    <IntInput
                                        className={`${editorModule.input} ${styles.numberInput}`}
                                        value={currentRule.MinSpeedKmh ?? 0}
                                        min={0}
                                        max={currentRule.MaxSpeedKmh ?? 0}
                                        onChange={(value) => updateSettings((draft) => {
                                            draft.AdditionalRules[currentEditingRule].MinSpeedKmh = value;
                                        })}
                                    />
                                </FormLine>
                                <FormLine
                                    title={translate("roadPrefixSettings.maximumSpeed")}
                                    tooltip={translate("roadPrefixSettings.tooltip.maximumSpeed")}
                                >
                                    <IntInput
                                        className={`${editorModule.input} ${styles.numberInput}`}
                                        value={currentRule.MaxSpeedKmh ?? 0}
                                        min={currentRule.MinSpeedKmh ?? 0}
                                        onChange={(value) => updateSettings((draft) => {
                                            draft.AdditionalRules[currentEditingRule].MaxSpeedKmh = value;
                                        })}
                                    />
                                </FormLine>

                                <h3 className={styles.sectionTitle}>{translate("roadPrefixSettings.requiredOrForbiddenTitle")}</h3>
                                {[
                                    RoadFlags.EnableZoning,
                                    RoadFlags.SeparatedCarriageways,
                                    RoadFlags.PreferTrafficLights,
                                    RoadFlags.UseHighwayRules,
                                    RoadFlags.Train,
                                    RoadFlags.Tram,
                                    RoadFlags.Subway,
                                ].map((flag) => {
                                    const currentValue =
                                        (currentRule.RequiredFlagsInt & flag) !== 0
                                            ? true
                                            : (currentRule.ForbiddenFlagsInt & flag) !== 0
                                                ? undefined
                                                : false;
                                    return (
                                        <FormLine title={translate(`roadPrefixSettings.flag${RoadFlags[flag]}`)} key={flag}>
                                            <Checkbox
                                                multistate={true}
                                                checked={currentValue}
                                                onMultistateChange={(value) => setRuleFlag(flag, value)}
                                            />
                                        </FormLine>
                                    );
                                })}

                                <FormLine
                                    title={translate("roadPrefixSettings.requireFullBridgeState")}
                                    tooltip={translate("roadPrefixSettings.tooltip.requireFullBridgeState")}
                                >
                                    <Checkbox
                                        multistate={true}
                                        checked={currentRule.FullBridge < 0 ? undefined : currentRule.FullBridge > 0}
                                        onMultistateChange={(value) => updateSettings((draft) => {
                                            draft.AdditionalRules[currentEditingRule].FullBridge = value === true ? 1 : value === undefined ? -1 : 0;
                                        })}
                                    />
                                </FormLine>
                                <FormLine
                                    title={translate("roadPrefixSettings.requireAnyElevatedState")}
                                    tooltip={translate("roadPrefixSettings.tooltip.requireAnyElevatedState")}
                                >
                                    <Checkbox
                                        multistate={true}
                                        checked={currentRule.AnyElevated < 0 ? undefined : currentRule.AnyElevated > 0}
                                        onMultistateChange={(value) => updateSettings((draft) => {
                                            draft.AdditionalRules[currentEditingRule].AnyElevated = value === true ? 1 : value === undefined ? -1 : 0;
                                        })}
                                    />
                                </FormLine>
                                <FormLine
                                    title={translate("roadPrefixSettings.minimumCarLanes")}
                                    tooltip={translate("roadPrefixSettings.tooltip.minimumCarLanes")}
                                >
                                    <IntInput
                                        className={`${editorModule.input} ${styles.numberInput}`}
                                        value={currentRule.MinCarLanes ?? 0}
                                        min={0}
                                        onChange={(value) => updateSettings((draft) => {
                                            draft.AdditionalRules[currentEditingRule].MinCarLanes = value;
                                        })}
                                    />
                                </FormLine>
                                <FormLine
                                    title={translate("roadPrefixSettings.maximumCarLanes")}
                                    tooltip={translate("roadPrefixSettings.tooltip.maximumCarLanes")}
                                >
                                    <IntInput
                                        className={`${editorModule.input} ${styles.numberInput}`}
                                        value={currentRule.MaxCarLanes ?? 0}
                                        min={0}
                                        onChange={(value) => updateSettings((draft) => {
                                            draft.AdditionalRules[currentEditingRule].MaxCarLanes = value;
                                        })}
                                    />
                                </FormLine>
                                <FormLine
                                    title={translate("roadPrefixSettings.minimumWidthM")}
                                    tooltip={translate("roadPrefixSettings.tooltip.minimumWidthM")}
                                >
                                    <FloatInput
                                        className={`${editorModule.input} ${styles.numberInput}`}
                                        value={currentRule.MinWidthM ?? 0}
                                        min={0}
                                        fractionDigits={1}
                                        onChange={(value) => updateSettings((draft) => {
                                            draft.AdditionalRules[currentEditingRule].MinWidthM = value;
                                        })}
                                    />
                                </FormLine>
                                <FormLine
                                    title={translate("roadPrefixSettings.maximumWidthM")}
                                    tooltip={translate("roadPrefixSettings.tooltip.maximumWidthM")}
                                >
                                    <FloatInput
                                        className={`${editorModule.input} ${styles.numberInput}`}
                                        value={currentRule.MaxWidthM ?? 0}
                                        min={0}
                                        fractionDigits={1}
                                        onChange={(value) => updateSettings((draft) => {
                                            draft.AdditionalRules[currentEditingRule].MaxWidthM = value;
                                        })}
                                    />
                                </FormLine>
                            </>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};
