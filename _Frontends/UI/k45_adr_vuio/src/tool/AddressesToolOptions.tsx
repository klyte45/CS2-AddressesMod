import { LocElementType, VanillaComponentResolver, VanillaWidgets } from "@klyte45/vuio-commons";
import { LocElement, toolbar } from "cs2/bindings";
import { FocusDisabled } from "cs2/input";
import { ModuleRegistryExtend } from "cs2/modding";
import { ObjectTyped } from "object-typed";
import { useState, useEffect } from "react";
import AdrHighwayRoutesSystem, { AdrFields, AdrFieldType, DisplayInformation, LocalizationStrings, PylonFormat, PylonMaterial } from "service/AdrHighwayRoutesSystem";
import routesSystem from "service/AdrHighwayRoutesSystem";
import { translate } from "utility/translate";
import { MetadataMount, MountMetadataComponentProps } from "components/MetadataMount";

export const AddressesToolOptions: ModuleRegistryExtend = (Component: any) => {
    return () => {
        const [isRoadMarker, setIsRoadMarker] = useState(false);

        const checkIfIsRoadMarker = () => AdrHighwayRoutesSystem.isCurrentPrefabRoadMarker().then(setIsRoadMarker);
        useEffect(() => {
            toolbar.selectedAsset$.subscribe(checkIfIsRoadMarker);
            checkIfIsRoadMarker();
        }, []);

        const result = Component();
        if (isRoadMarker) {
            result.props.children ??= [];
            result.props.children.unshift(<AdrRoadMarkerToolOptions />);
        }
        return result;
    };
};

const i_DoublePoleOn = "coui://uil/Standard/Checkmark.svg";
const i_DoublePoleOff = "coui://uil/Standard/XClose.svg";

const AdrRoadMarkerToolOptions = () => {
    const [buildIdx, setBuildIdx] = useState(0);

    const VCR = VanillaComponentResolver.instance;
    const VW = VanillaWidgets.instance;
    const NumberDD = VW.DropdownField<number>();
    const [metadata, setMetadata] = useState<AdrFields<string> | null>();
    const [displayNameTypes, setDisplayNameTypes] = useState<string[]>([]);
    const editorModule = VanillaWidgets.instance.editorItemModule;

    useEffect(() => {
        AdrHighwayRoutesSystem.getOptionsNamesFromMetadata().then(setDisplayNameTypes).then(() => routesSystem.refreshAllBindings());
    }, []);
    useEffect(() => {
        AdrHighwayRoutesSystem.getOptionsMetadataFromCurrentLayout().then((x) => {
            const data = x && JSON.parse(x);
            const validatedData = AdrHighwayRoutesSystem.validateMetadata(data);
            setMetadata(validatedData);
        });
    }, [routesSystem.Tool_DisplayInformation.value]);

    return <>
        <VCR.Section title={translate(LocalizationStrings.poleSettings)}><div /></VCR.Section>
        <VCR.Section title={translate(LocalizationStrings.poleFormat)}>
            <NumberDD
                items={ObjectTyped.entries(PylonFormat).filter(x => typeof x[1] == 'number').map((x: [string, number]) => ({ value: x[1], displayName: { __Type: LocElementType.String, value: translate("PylonFormat." + x[0]) } }))}
                onChange={x => routesSystem.Tool_PylonFormat.set(x).then(() => setBuildIdx(buildIdx + 1))}
                value={routesSystem.Tool_PylonFormat.value}
            />
        </VCR.Section>
        <VCR.Section title={translate(LocalizationStrings.poleMaterial)}>
            <NumberDD
                items={ObjectTyped.entries(PylonMaterial).filter(x => typeof x[1] == 'number').map((x: [string, number]) => ({ value: x[1], displayName: { __Type: LocElementType.String, value: translate("PylonMaterial." + x[0]) } }))}
                onChange={x => routesSystem.Tool_PylonMaterial.set(x).then(() => setBuildIdx(buildIdx + 1))}
                value={routesSystem.Tool_PylonMaterial.value}
            />
        </VCR.Section>
        <VCR.Section title={translate(LocalizationStrings.poleHeight)}>
            <FocusDisabled>
                <VW.FloatSlider
                    min={0.25} max={10}
                    fractionDigits={2}
                    value={routesSystem.Tool_PylonHeight.value} onChange={(x) => routesSystem.Tool_PylonHeight.set(x).then(() => setBuildIdx(buildIdx + 1))} />
            </FocusDisabled>
        </VCR.Section>
        <VCR.Section title={translate(LocalizationStrings.useDoublePole)}>
            <FocusDisabled>
                <VanillaComponentResolver.instance.ToolButton
                    selected={routesSystem.Tool_PylonCount.value > 1}
                    onSelect={() => routesSystem.Tool_PylonCount.set(routesSystem.Tool_PylonCount.value > 1 ? 1 : 2).then(() => setBuildIdx(buildIdx + 1))}
                    src={routesSystem.Tool_PylonCount.value > 1 ? i_DoublePoleOn : i_DoublePoleOff}
                    className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
            </FocusDisabled>
        </VCR.Section>
        {routesSystem.Tool_PylonCount.value > 1 &&
            <VCR.Section title={translate(LocalizationStrings.poleSpacing)}>
                <FocusDisabled>
                    <VW.FloatSlider
                        value={routesSystem.Tool_PylonSpacing.value} onChange={(x) => routesSystem.Tool_PylonSpacing.set(x).then(() => setBuildIdx(buildIdx + 1))}
                        max={3} min={0.05}
                        fractionDigits={2}
                    />
                </FocusDisabled>
            </VCR.Section>
        }
        <div style={{ height: "20rem" }} />
        <VCR.Section title={translate(LocalizationStrings.displayInformation)}>
            <NumberDD
                items={[{ value: 0, displayName: { __Type: LocElementType.String, value: translate("DisplayInformation." + DisplayInformation[DisplayInformation.ORIGINAL]) } as LocElement }].concat(
                    displayNameTypes.map((x, i) => ({
                        value: i + 1,
                        displayName: { __Type: LocElementType.String, value: x } as LocElement
                    })))}
                onChange={x => routesSystem.Tool_DisplayInformation.set(x).then(() => setBuildIdx(buildIdx + 1))}
                value={routesSystem.Tool_DisplayInformation.value}
            />
        </VCR.Section>
        {routesSystem.Tool_DisplayInformation.value >= DisplayInformation.CUSTOM_1 && routesSystem.Tool_DisplayInformation.value <= DisplayInformation.CUSTOM_7 &&
            (metadata ? <MetadataMount metadata={metadata} parameters={[routesSystem.Tool_NumericCustomParam1, routesSystem.Tool_NumericCustomParam2]} output={mountMetadataOptionsTool} /> :
                <VCR.Section title={translate(LocalizationStrings.customParams)}>
                    <FocusDisabled>
                        <VW.IntInputStandalone style={{ flexShrink: 4, textAlign: "right", maxWidth: "105rem", width: "auto", flexGrow: 2, marginRight: "7rem" }} className={editorModule.input}
                            value={routesSystem.Tool_NumericCustomParam1.value} onChange={(x) => routesSystem.Tool_NumericCustomParam1.set(x).then(() => setBuildIdx(buildIdx + 1))} />
                        <VW.IntInputStandalone style={{ flexShrink: 4, textAlign: "right", maxWidth: "105rem", flexGrow: 2, width: "auto", marginRight: "7rem" }} className={editorModule.input}
                            value={routesSystem.Tool_NumericCustomParam2.value} onChange={(x) => routesSystem.Tool_NumericCustomParam2.set(x).then(() => setBuildIdx(buildIdx + 1))} />
                    </FocusDisabled>
                </VCR.Section>)
        }
    </>;
};

function mountMetadataOptionsTool({ validOptions, setStoredValues, storedValues }: MountMetadataComponentProps) {
    const VCR = VanillaComponentResolver.instance;
    const VW = VanillaWidgets.instance;
    const NumberDD = VW.DropdownField<number>();
    const editorModule = VanillaWidgets.instance.editorItemModule;
    return <>
        {validOptions.map(x => x[1].type == AdrFieldType.SELECTION ? <>
            <VCR.Section title={engine.translate(x[1].localization)}>
                <NumberDD
                    items={ObjectTyped.entries(x[1].options).map((y: [number, string]) => ({ value: y[0] * 1, displayName: { __Type: LocElementType.String, value: engine.translate(y[1]) } }))}
                    onChange={y => setStoredValues(x[0], y * 1)}
                    value={storedValues[x[0]]} />
            </VCR.Section>
        </> : <>
            <VCR.Section title={engine.translate(x[1].localization)}>
                <FocusDisabled>
                    {typeof x[1].min === "number" && typeof x[1].max === "number"
                        ? <VW.IntSlider
                            value={storedValues[x[0]]}
                            onChange={(y) => setStoredValues(x[0], y)}
                            min={x[1].min}
                            max={x[1].max}
                        />
                        : <VW.IntInputStandalone style={{ flexShrink: 4, textAlign: "right", maxWidth: "105rem", width: "auto", flexGrow: 2, marginRight: "7rem" }} className={editorModule.input}
                            value={storedValues[x[0]]} onChange={y => setStoredValues(x[0], y * 1)} min={x[1].min} max={x[1].max} />
                    }
                </FocusDisabled>
            </VCR.Section>
        </>)}
    </>;
}
