import { VanillaComponentResolver, LocElementType, VanillaWidgets, replaceArgs } from "@klyte45/vuio-commons";
import { DropdownItem, LocElement, toolbar } from "cs2/bindings";
import { FocusDisabled } from "cs2/input";
import { useLocalization } from "cs2/l10n";
import { ModuleRegistryExtend } from "cs2/modding";
import { ObjectTyped } from "object-typed";
import { useState, useEffect } from "react";
import AdrHighwayRoutesSystem, { AdrFields, AdrFieldType, LocalizationStrings } from "service/AdrHighwayRoutesSystem";
import routesSystem, { DisplayInformation, RouteDirection, RouteItem } from "service/AdrHighwayRoutesSystem";
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



const i_OverrideMileage = "coui://uil/Standard/BoxTop.svg";
const i_ReverseMileage = "coui://uil/Standard/BoxTop.svg";

const AdrRoadMarkerToolOptions = () => {
    const [buildIdx, setBuildIdx] = useState(0)

    const VCR = VanillaComponentResolver.instance;
    const VW = VanillaWidgets.instance;
    const StringDD = VW.DropdownField<string>();
    const NumberDD = VW.DropdownField<number>();
    const [routesRegistered, setRoutesRegistered] = useState<RouteItem[]>([])
    const [metadata, setMetadata] = useState<AdrFields<string> | null>();
    const [displayNameTypes, setDisplayNameTypes] = useState<string[]>([])
    useEffect(() => {
        AdrHighwayRoutesSystem.getOptionsNamesFromMetadata().then(setDisplayNameTypes)
    }, [])
    useEffect(() => {
        AdrHighwayRoutesSystem.getOptionsMetadataFromCurrentLayout().then((x) => {
            const data = x && JSON.parse(x);
            const validatedData = AdrHighwayRoutesSystem.validateMetadata(data);
            setMetadata(validatedData)
        });
    }, [routesSystem.Tool_DisplayInformation.value])

    const localization = useLocalization();
    const mileageMultiplier = localization.unitSettings.unitSystem ? 1.609 : 1

    const editorModule = VanillaWidgets.instance.editorItemModule;
    const routeList = [
        { value: "0".repeat(32), displayName: { __Type: LocElementType.String, value: translate("HighwayRoutes.NoRoute") } }
    ].concat(
        routesRegistered.map((x) => ({ value: x.id, displayName: { __Type: LocElementType.String, value: x.name } })) as any[]
    ) as DropdownItem<string>[]

    return <>
        <VCR.Section title={translate(LocalizationStrings.routeIdentifier)}>
            <StringDD
                onChange={x => routesSystem.Tool_RouteId.set(x).then(x => setBuildIdx(buildIdx + 1))}
                items={routeList}
                value={routesSystem.Tool_RouteId.value}
            />
        </VCR.Section>
        <VCR.Section title={translate(LocalizationStrings.routeDirection)}>
            <NumberDD
                items={ObjectTyped.entries(RouteDirection).filter(x => typeof x[1] == 'number').map((x: [string, number]) => ({ value: x[1], displayName: { __Type: LocElementType.String, value: translate("RouteDirection." + x[0]) } }))}
                onChange={x => routesSystem.Tool_RouteDirection.set(x).then(x => setBuildIdx(buildIdx + 1))}
                value={routesSystem.Tool_RouteDirection.value}
            />
        </VCR.Section>
        <VCR.Section title={translate(LocalizationStrings.overrideMileageShort)}>
            <FocusDisabled>
                <VanillaComponentResolver.instance.ToolButton selected={routesSystem.Tool_OverrideMileage.value} onSelect={() => routesSystem.Tool_OverrideMileage.set(!routesSystem.Tool_OverrideMileage.value).then(x => setBuildIdx(buildIdx + 1))} src={i_OverrideMileage} className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
            </FocusDisabled>
        </VCR.Section>
        {routesSystem.Tool_OverrideMileage.value &&
            <VCR.Section title={replaceArgs(translate(LocalizationStrings.newMileage), [localization.unitSettings.unitSystem ? "mi" : "km"])}>
                <FocusDisabled>
                    <VCR.FloatInput
                        style={{ flexShrink: 4, width: "auto", flexGrow: 2, textAlign: "right", marginRight: "7rem" }} className={editorModule.input}
                        onChange={x => routesSystem.Tool_NewMileage.set(x * mileageMultiplier).then(x => setBuildIdx(buildIdx + 1))}
                        value={routesSystem.Tool_NewMileage.value / mileageMultiplier} />
                    <VanillaComponentResolver.instance.ToolButton selected={routesSystem.Tool_ReverseMileageCounting.value} onSelect={() => routesSystem.Tool_ReverseMileageCounting.set(!routesSystem.Tool_ReverseMileageCounting.value).then(x => setBuildIdx(buildIdx + 1))} src={i_OverrideMileage} className={VanillaComponentResolver.instance.toolButtonTheme.button} tooltip={translate(LocalizationStrings.reverseMileageCounting)}></VanillaComponentResolver.instance.ToolButton>
                </FocusDisabled>
            </VCR.Section>
        }
        <VCR.Section title={translate(LocalizationStrings.displayInformation)}>
            <NumberDD
                items={[{ value: 0, displayName: { __Type: LocElementType.String, value: translate("DisplayInformation." + DisplayInformation[DisplayInformation.ORIGINAL]) } as LocElement }].concat(
                    displayNameTypes.map((x, i) => ({
                        value: i + 1,
                        displayName: { __Type: LocElementType.String, value: x } as LocElement 
                    })))}
                onChange={x => routesSystem.Tool_DisplayInformation.set(x).then(x => setBuildIdx(buildIdx + 1))}
                value={routesSystem.Tool_DisplayInformation.value}
            />
        </VCR.Section>
        {routesSystem.Tool_DisplayInformation.value >= DisplayInformation.CUSTOM_1 && routesSystem.Tool_DisplayInformation.value <= DisplayInformation.CUSTOM_7 &&
            (metadata ? <MetadataMount metadata={metadata} parameters={[routesSystem.Tool_NumericCustomParam1, routesSystem.Tool_NumericCustomParam2]} output={mountMetadataOptionsTool} /> :
                <VCR.Section title={translate(LocalizationStrings.customParams)}>
                    <FocusDisabled>
                        <VW.IntInputStandalone style={{ flexShrink: 4, textAlign: "right", maxWidth: "105rem", width: "auto", flexGrow: 2, marginRight: "7rem" }} className={editorModule.input}
                            value={routesSystem.Tool_NumericCustomParam1.value} onChange={(x) => routesSystem.Tool_NumericCustomParam1.set(x).then(x => setBuildIdx(buildIdx + 1))} />
                        <VW.IntInputStandalone style={{ flexShrink: 4, textAlign: "right", maxWidth: "105rem", flexGrow: 2, width: "auto", marginRight: "7rem" }} className={editorModule.input}
                            value={routesSystem.Tool_NumericCustomParam2.value} onChange={(x) => routesSystem.Tool_NumericCustomParam2.set(x).then(x => setBuildIdx(buildIdx + 1))} />
                    </FocusDisabled>
                </VCR.Section>)
        }
    </>

}

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
                    <VW.IntInputStandalone style={{ flexShrink: 4, textAlign: "right", maxWidth: "105rem", width: "auto", flexGrow: 2, marginRight: "7rem" }} className={editorModule.input}
                        value={storedValues[x[0]]} onChange={y => setStoredValues(x[0], y * 1)} min={x[1].min} max={x[1].max} />
                </FocusDisabled>
            </VCR.Section>
        </>)}
    </>;
}
