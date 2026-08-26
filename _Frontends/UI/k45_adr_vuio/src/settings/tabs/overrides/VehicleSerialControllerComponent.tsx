import {
    ConstructorObjectToInstancesObject,
    FormLine,
    LocElementType,
    MultiUIValueBinding,
    MultiUIValueBindingTools,
    VanillaWidgets,
} from "@klyte45/vuio-commons";
import { DropdownItem } from "cs2/bindings";
import { MutableRefObject, useEffect, useRef, useState } from "react";
import { translate } from "../../translate";
import styles from "../../styles/vehiclePlateController.module.scss";

const BaseSerialController = {
    LettersAllowed: MultiUIValueBinding<string[]>,
    FlagsOwnSerial: MultiUIValueBinding<number>,
    FlagsCarNumber: MultiUIValueBinding<number>,
    BuildingIdOnStart: MultiUIValueBinding<number>,
};

type Props = {
    type: string;
};

type SerialBindings = ConstructorObjectToInstancesObject<typeof BaseSerialController>;

const digitSourceOptions = [0, 1];

export const VehicleSerialControllerComponent = ({ type }: Props) => {
    const [controllerData, setControllerData] = useState<SerialBindings>();
    const [, setBuildIdx] = useState(0);
    const bindingsRef = useRef<SerialBindings>();
    const VW = VanillaWidgets.instance;
    const NumberDD = VW.DropdownField<number>();
    const editorModule = VW.editorItemModule;

    useEffect(() => {
        const tmp = MultiUIValueBindingTools.InitializeBindings({
            _prefix: "k45::adr.vehicleSerial." + type,
            ...BaseSerialController,
        });
        bindingsRef.current = tmp;
        setControllerData(tmp);
        Object.entries(tmp).forEach(([x, y]) => y.subscribe(async (z) => {
            setBuildIdx(x + JSON.stringify(z) as any);
        }));
        return () => {
            bindingsRef.current && Object.values(bindingsRef.current).forEach((x) => x.dispose());
        };
    }, [type]);

    if (!controllerData) return <></>;

    const setDigitSource = (i: number, source: number) => {
        controllerData.FlagsCarNumber.set(
            controllerData.FlagsCarNumber.value & ~(source == 1 ? 0 : 1 << i) | (source == 1 ? 1 << i : 0),
        );
        controllerData.FlagsOwnSerial.set(
            controllerData.FlagsOwnSerial.value & ~(source == 0 ? 0 : 1 << i) | (source == 0 ? 1 << i : 0),
        );
    };

    const optionsBuildingId = [
        { label: translate("vehicleSerial.buildingCityIdPosition.start"), value: 1 },
        { label: translate("vehicleSerial.buildingCityIdPosition.end"), value: 0 },
        { label: translate("vehicleSerial.buildingCityIdPosition.none"), value: -1 },
    ];

    const buildingItems: DropdownItem<number>[] = optionsBuildingId.map((o) => ({
        value: o.value,
        displayName: { __Type: LocElementType.String, value: o.label },
    }));

    return <>
        <FormLine title={translate("vehicleSerial.buildingCityIdPosition")}>
            <NumberDD
                items={buildingItems}
                value={controllerData.BuildingIdOnStart.value}
                onChange={(v) => controllerData.BuildingIdOnStart.set(v)}
            />
        </FormLine>
        <FormLine title={translate("vehiclePlate.digitsQuantity")}>
            <VW.IntInputStandalone
                className={editorModule.input}
                style={{ maxWidth: "80rem", textAlign: "right" }}
                min={1}
                max={9}
                value={controllerData.LettersAllowed.value?.length ?? 1}
                onChange={(x) => {
                    const newSize = [...new Array(x)].map((_, i) => controllerData.LettersAllowed.value[i] || "0123456789");
                    controllerData.LettersAllowed.set(newSize);
                }}
            />
        </FormLine>
        <h2>{translate("vehicleSerial.identifierCharSettingsTitle." + type)}</h2>
        <div className={styles.vehiclePlateLettersEditorContainer}>
            {controllerData.LettersAllowed.value?.map((_, i) => {
                const digitFlagId = controllerData.LettersAllowed.value.length - i - 1;
                const slotType = controllerData.FlagsOwnSerial.value & (1 << digitFlagId)
                    ? 0
                    : controllerData.FlagsCarNumber.value & (1 << digitFlagId) ? 1 : -1;
                const items: DropdownItem<number>[] = digitSourceOptions.map((v) => ({
                    value: v,
                    displayName: { __Type: LocElementType.String, value: translate("vehicleSerial.identifierDigitSource." + v) },
                }));
                return (
                    <div key={i} className={styles.letterItem}>
                        <NumberDD
                            items={items}
                            value={slotType < 0 ? 0 : slotType}
                            onChange={(v) => setDigitSource(digitFlagId, v)}
                        />
                        <CharsAllowedInput controllerData={controllerData} i={i} />
                    </div>
                );
            })}
        </div>
    </>;
};

const CharsAllowedInput = ({ controllerData, i }: { controllerData: SerialBindings; i: number }) => {
    const refTextArea = useRef() as MutableRefObject<HTMLTextAreaElement>;

    useEffect(() => {
        if (refTextArea.current) {
            refTextArea.current.value = controllerData.LettersAllowed.value[i];
        }
    }, [controllerData.LettersAllowed.value[i]]);

    const onSuccessChange = () => {
        const typingText = refTextArea.current.value;
        if (!typingText) {
            onRevert();
            return;
        }
        const newArr = [...controllerData.LettersAllowed.value];
        newArr[i] = Object.keys([...typingText].reduce((p, n) => { p[n] = true; return p; }, {} as Record<string, boolean>)).join("");
        controllerData.LettersAllowed.set(newArr);
        refTextArea.current.blur();
    };

    const onRevert = () => {
        refTextArea.current.value = controllerData.LettersAllowed.value[i];
    };

    return (
        <textarea
            ref={refTextArea}
            className={styles.lettersSet}
            onBlur={onRevert}
            onKeyDown={(x) => {
                if (x.key == "Escape") {
                    (x.target as HTMLTextAreaElement).blur();
                    x.preventDefault();
                    onRevert();
                } else if (x.key == "Enter") {
                    x.preventDefault();
                    onSuccessChange();
                }
            }}
        />
    );
};
