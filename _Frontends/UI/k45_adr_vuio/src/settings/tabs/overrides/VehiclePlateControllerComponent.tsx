import {
    ConstructorObjectToInstancesObject,
    FormLine,
    LocElementType,
    MultiUIValueBinding,
    MultiUIValueBindingTools,
    VanillaWidgets,
} from "@klyte45/vuio-commons";
import { DropdownItem } from "cs2/bindings";
import { useEffect, useRef, useState } from "react";
import { translate } from "../../translate";
import styles from "../../styles/vehiclePlateController.module.scss";

const BasePlatesController = {
    LettersAllowed: MultiUIValueBinding<string[]>,
    FlagsLocal: MultiUIValueBinding<number>,
    FlagsCarNumber: MultiUIValueBinding<number>,
    FlagsRandomized: MultiUIValueBinding<number>,
    MonthsFromEpochOffset: MultiUIValueBinding<number>,
    SerialIncrementEachMonth: MultiUIValueBinding<number>,
};

type Props = {
    type: string;
};

type PlateBindings = ConstructorObjectToInstancesObject<typeof BasePlatesController>;

const digitSourceOptions = [0, 1, 2];

const monthDropdownItems: DropdownItem<number>[] = [...Array(12)].map((_, i) => ({
    value: i,
    displayName: { __Type: LocElementType.String, value: engine.translate("Common.MONTH:" + i) },
}));

const MonthsFromEpochField = ({ value, onChange }: { value: number; onChange: (v: number) => void }) => {
    const VW = VanillaWidgets.instance;
    const NumberDD = VW.DropdownField<number>();
    const editorModule = VW.editorItemModule;
    const safeValue = Math.max(0, value ?? 0);
    const month = safeValue % 12;
    const year = Math.floor(safeValue / 12);

    useEffect(() => {
        if ((value ?? 0) < 0) {
            onChange(0);
        }
    }, [value, onChange]);

    return (
        <div className={styles.monthYearRow}>
            <div className={styles.monthDropdown}>
                <NumberDD
                    items={monthDropdownItems}
                    value={month}
                    onChange={(m) => onChange(year * 12 + m)}
                />
            </div>
            <span className={styles.monthYearSep}>/</span>
            <VW.IntInputStandalone
                className={`${editorModule.input} ${styles.yearInput}`}
                min={0}
                value={year}
                onChange={(y) => onChange(Math.max(0, y) * 12 + month)}
            />
        </div>
    );
};

export const VehiclePlateControllerComponent = ({ type }: Props) => {
    const [controllerData, setControllerData] = useState<PlateBindings>();
    const [, setBuildIdx] = useState(0);
    const bindingsRef = useRef<PlateBindings>();
    const [previewPlates, setPreviewPlates] = useState<string[]>([]);
    const VW = VanillaWidgets.instance;
    const NumberDD = VW.DropdownField<number>();
    const editorModule = VW.editorItemModule;

    const refreshPreview = async () => {
        try {
            const plates: string[] = await engine.call("k45::adr.vehiclePlate." + type + ".generatePreviewPlates");
            setPreviewPlates(plates);
        } catch { /* ignore */ }
    };

    useEffect(() => {
        const tmp = MultiUIValueBindingTools.InitializeBindings({
            _prefix: "k45::adr.vehiclePlate." + type,
            ...BasePlatesController,
        });
        bindingsRef.current = tmp;
        setControllerData(tmp);
        Object.entries(tmp).forEach(([x, y]) => y.subscribe(async (z) => {
            setBuildIdx(x + JSON.stringify(z) as any);
            refreshPreview();
        }));
        refreshPreview();
        return () => {
            bindingsRef.current && Object.values(bindingsRef.current).forEach((x) => x.dispose());
        };
    }, [type]);

    if (!controllerData) return <></>;

    const setDigitSource = (i: number, source: number) => {
        controllerData.FlagsCarNumber.set(
            controllerData.FlagsCarNumber.value & ~(source == 2 ? 0 : 1 << i) | (source == 2 ? 1 << i : 0),
        );
        controllerData.FlagsLocal.set(
            controllerData.FlagsLocal.value & ~(source == 1 ? 0 : 1 << i) | (source == 1 ? 1 << i : 0),
        );
    };

    return <>
        <FormLine title={translate("vehiclePlate.monthsOffsetForGenerator")}>
            <MonthsFromEpochField
                value={controllerData.MonthsFromEpochOffset.value ?? 0}
                onChange={(x) => controllerData.MonthsFromEpochOffset.set(x)}
            />
        </FormLine>
        <FormLine title={translate("vehiclePlate.serialIncrementEachMonth")}>
            <VW.IntInputStandalone
                className={editorModule.input}
                style={{ maxWidth: "120rem", textAlign: "right" }}
                min={0}
                value={controllerData.SerialIncrementEachMonth.value ?? 0}
                onChange={(x) => controllerData.SerialIncrementEachMonth.set(x)}
            />
        </FormLine>
        <FormLine title={translate("vehiclePlate.digitsQuantity")}>
            <VW.IntSlider
                min={1}
                max={9}
                value={controllerData.LettersAllowed.value?.length ?? 1}
                onChange={(x) => {
                    const newSize = [...new Array(x)].map((_, i) => controllerData.LettersAllowed.value[i] || "0123456789");
                    controllerData.LettersAllowed.set(newSize);
                }}
            />
        </FormLine>
        <h2>{translate("vehiclePlate.identifierCharSettingsTitle." + type)}</h2>
        <div className={styles.vehiclePlateLettersEditorContainer}>
            {controllerData.LettersAllowed.value?.map((_, i) => {
                const digitFlagId = controllerData.LettersAllowed.value.length - i - 1;
                const slotType = controllerData.FlagsLocal.value & (1 << digitFlagId)
                    ? 1
                    : controllerData.FlagsCarNumber.value & (1 << digitFlagId) ? 2 : 0;
                const slotClass = [styles.slotRegional, styles.slotLocal, styles.slotCarNumber][slotType];
                const items: DropdownItem<number>[] = digitSourceOptions.map((v) => ({
                    value: v,
                    displayName: { __Type: LocElementType.String, value: translate("vehiclePlate.identifierDigitSource." + v) },
                }));
                return (
                    <div key={i} className={`${styles.letterItem} ${slotClass}`}>
                        <NumberDD
                            items={items}
                            value={slotType}
                            onChange={(v) => setDigitSource(digitFlagId, v)}
                        />
                        <CharsAllowedInput controllerData={controllerData} i={i} />
                        <button
                            type="button"
                            className={(controllerData.FlagsRandomized.value & (1 << i)) != 0 ? "positiveBtn" : "blackBtn"}
                            onClick={() => {
                                const on = (controllerData.FlagsRandomized.value & (1 << i)) == 0;
                                controllerData.FlagsRandomized.set(
                                    controllerData.FlagsRandomized.value & ~(on ? 0 : 1 << i) | (on ? 1 << i : 0),
                                );
                            }}
                        >
                            {translate("vehiclePlate.allowRandomizeDigitLabel")}
                        </button>
                    </div>
                );
            })}
        </div>
        {previewPlates.length > 0 && (
            <div>
                <h3>{translate("vehiclePlate.preview")}</h3>
                <div className={styles.previewWrap}>
                    {previewPlates.map((plate, i) => (
                        <div key={i} className={`neutralBtn ${styles.previewPlate}`}>{plate}</div>
                    ))}
                </div>
            </div>
        )}
    </>;
};

const CharsAllowedInput = ({ controllerData, i }: { controllerData: PlateBindings; i: number }) => {
    const VW = VanillaWidgets.instance;
    const StringInput = VW.StringInputField;
    const editorModule = VW.editorItemModule;
    const [typingText, setTypingText] = useState(controllerData.LettersAllowed.value[i] ?? "");

    useEffect(() => {
        setTypingText(controllerData.LettersAllowed.value[i] ?? "");
    }, [controllerData.LettersAllowed.value[i]]);

    const commit = () => {
        if (!typingText) {
            setTypingText(controllerData.LettersAllowed.value[i] ?? "");
            return;
        }
        const newArr = [...controllerData.LettersAllowed.value];
        newArr[i] = Object.keys([...typingText].reduce((p, n) => { p[n] = true; return p; }, {} as Record<string, boolean>)).join("");
        controllerData.LettersAllowed.set(newArr);
    };

    return (
        <StringInput
            className={`${editorModule.input} ${editorModule.multilineText} ${styles.lettersSet}`}
            value={typingText}
            multiline={6}
            onChange={setTypingText}
            onChangeEnd={commit}
        />
    );
};
