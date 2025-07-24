import { VanillaComponentResolver, LocElementType, VanillaWidgets, replaceArgs, VectorSectionEditable } from "@klyte45/vuio-commons";
import { DropdownItem, LocElement, toolbar } from "cs2/bindings";
import { FocusDisabled } from "cs2/input";
import { useLocalization } from "cs2/l10n";
import { ModuleRegistryExtend } from "cs2/modding";
import { ObjectTyped } from "object-typed";
import { useState, useEffect } from "react";
import AdrHighwayRoutesSystem, { AdrFields, AdrFieldType, LocalizationStrings, PylonFormat, PylonMaterial } from "service/AdrHighwayRoutesSystem";
import routesSystem, { DisplayInformation, RouteDirection, RouteItem } from "service/AdrHighwayRoutesSystem";
import { translate } from "utility/translate";
import { MetadataMount, MountMetadataComponentProps } from "components/MetadataMount";
import { HighwayRoutesService } from "@klyte45/adr-commons";

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



const i_OverrideMileageOn = "coui://uil/Standard/Checkmark.svg";
const i_OverrideMileageOff = "coui://uil/Standard/XClose.svg";
const i_ReverseMileageOn = "coui://uil/Standard/ArrowSortHighDown.svg";
const i_ReverseMileageOff = "coui://uil/Standard/ArrowSortLowDown.svg";

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
        AdrHighwayRoutesSystem.getOptionsNamesFromMetadata().then(setDisplayNameTypes);
        HighwayRoutesService.listHighwaysRegistered().then(x => setRoutesRegistered(x.map(y => ({ id: y.Id, name: `${y.prefix}-${y.suffix} ${y.name}` })).sort((a, b) => a.name.localeCompare(b.name))))
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
        <div style={{ height: "20rem" }} />
        <VCR.Section title={translate(LocalizationStrings.poleSettings)}><div /></VCR.Section>
        <VCR.Section title={translate(LocalizationStrings.poleFormat)}>
            <NumberDD
                items={ObjectTyped.entries(PylonFormat).filter(x => typeof x[1] == 'number').map((x: [string, number]) => ({ value: x[1], displayName: { __Type: LocElementType.String, value: translate("PylonFormat." + x[0]) } }))}
                onChange={x => routesSystem.Tool_PylonFormat.set(x).then(x => setBuildIdx(buildIdx + 1))}
                value={routesSystem.Tool_PylonFormat.value}
            />
        </VCR.Section>
        <VCR.Section title={translate(LocalizationStrings.poleMaterial)}>
            <NumberDD
                items={ObjectTyped.entries(PylonMaterial).filter(x => typeof x[1] == 'number').map((x: [string, number]) => ({ value: x[1], displayName: { __Type: LocElementType.String, value: translate("PylonMaterial." + x[0]) } }))}
                onChange={x => routesSystem.Tool_PylonMaterial.set(x).then(x => setBuildIdx(buildIdx + 1))}
                value={routesSystem.Tool_PylonMaterial.value}
            />
        </VCR.Section>
        <VCR.Section title={translate(LocalizationStrings.poleHeight)}>
            <FocusDisabled>
                <VW.FloatInputStandalone style={{ flexShrink: 1, textAlign: "right", width: "auto", flexGrow: 2, marginRight: "7rem" }} className={editorModule.input}
                    min={0.25} max={10}
                    value={routesSystem.Tool_PylonHeight.value} onChange={(x) => routesSystem.Tool_PylonHeight.set(x).then(x => setBuildIdx(buildIdx + 1))} />
            </FocusDisabled>
        </VCR.Section>
        <VCR.Section title={translate(LocalizationStrings.useDoublePole)}>
            <FocusDisabled>
                <VanillaComponentResolver.instance.ToolButton
                    selected={routesSystem.Tool_PylonCount.value > 1}
                    onSelect={() => routesSystem.Tool_PylonCount.set(routesSystem.Tool_PylonCount.value > 1 ? 1 : 2).then(x => setBuildIdx(buildIdx + 1))}
                    src={routesSystem.Tool_PylonCount.value > 1 ? i_OverrideMileageOn : i_OverrideMileageOff}
                    className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
            </FocusDisabled>
        </VCR.Section>
        {routesSystem.Tool_PylonCount.value > 1 &&
            <VCR.Section title={translate(LocalizationStrings.poleSpacing)}>
                <FocusDisabled>
                    <VW.FloatInputStandalone style={{ flexShrink: 1, textAlign: "right", width: "auto", flexGrow: 2, marginRight: "7rem" }} className={editorModule.input}
                        value={routesSystem.Tool_PylonSpacing.value} onChange={(x) => routesSystem.Tool_PylonSpacing.set(x).then(x => setBuildIdx(buildIdx + 1))}
                        max={3} min={0.05}
                    />
                </FocusDisabled>
            </VCR.Section>
        }
        <div style={{ height: "20rem" }} />
        <VCR.Section title={translate(LocalizationStrings.overrideMileageShort)}>
            <FocusDisabled>
                <VanillaComponentResolver.instance.ToolButton selected={routesSystem.Tool_OverrideMileage.value} onSelect={() => routesSystem.Tool_OverrideMileage.set(!routesSystem.Tool_OverrideMileage.value).then(x => setBuildIdx(buildIdx + 1))} src={routesSystem.Tool_OverrideMileage.value ? i_OverrideMileageOn : i_OverrideMileageOff} className={VanillaComponentResolver.instance.toolButtonTheme.button}></VanillaComponentResolver.instance.ToolButton>
            </FocusDisabled>
        </VCR.Section>
        {routesSystem.Tool_OverrideMileage.value &&
            <>
                <VectorSectionEditable title={translate(LocalizationStrings.newMileage)}
                    valueGetter={() => [routesSystem.Tool_NewMileage.value.toFixed(3)]}
                    valueGetterFormatted={() => [routesSystem.Tool_NewMileage.value.toFixed(3) + (localization.unitSettings.unitSystem ? "mi" : "km")]}
                    onValueChanged={(i, x) => {
                        const newVal = parseFloat(x.replaceAll(",", "."));
                        if (isNaN(newVal)) return;
                        routesSystem.Tool_NewMileage.set(newVal * mileageMultiplier).then(x => setBuildIdx(buildIdx + 1))
                    }} extraContent={<VanillaComponentResolver.instance.ToolButton selected={routesSystem.Tool_ReverseMileageCounting.value} onSelect={() => routesSystem.Tool_ReverseMileageCounting.set(!routesSystem.Tool_ReverseMileageCounting.value).then(x => setBuildIdx(buildIdx + 1))} src={routesSystem.Tool_ReverseMileageCounting.value ? i_ReverseMileageOn : i_ReverseMileageOff} className={VanillaComponentResolver.instance.toolButtonTheme.button} tooltip={translate(LocalizationStrings.reverseMileageCounting)}></VanillaComponentResolver.instance.ToolButton>} />
            </>
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
