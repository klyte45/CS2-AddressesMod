import { VanillaComponentResolver, LocElementType, VanillaWidgets } from "@klyte45/vuio-commons";
import { DropdownItem, toolbar } from "cs2/bindings";
import { FocusDisabled } from "cs2/input";
import { ModuleRegistryExtend } from "cs2/modding";
import { ObjectTyped } from "object-typed";
import { useState, useEffect } from "react";
import AdrHighwayRoutesSystem, { LocalizationStrings } from "service/AdrHighwayRoutesSystem";
import routesSystem, { DisplayInformation, RouteDirection, RouteItem } from "service/AdrHighwayRoutesSystem";
import { translate } from "utility/translate";

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
            <VCR.Section title={translate(LocalizationStrings.newMileage)}>
                <FocusDisabled>
                    <VCR.FloatInput style={{ flexShrink: 4, width: "auto", flexGrow: 2, textAlign: "right", marginRight: "7rem" }} className={editorModule.input} value={routesSystem.Tool_NewMileage.value} onChange={(x) => routesSystem.Tool_NewMileage.set(x).then(x => setBuildIdx(buildIdx + 1))} />
                    <VanillaComponentResolver.instance.ToolButton selected={routesSystem.Tool_ReverseMileageCounting.value} onSelect={() => routesSystem.Tool_ReverseMileageCounting.set(!routesSystem.Tool_ReverseMileageCounting.value).then(x => setBuildIdx(buildIdx + 1))} src={i_OverrideMileage} className={VanillaComponentResolver.instance.toolButtonTheme.button} tooltip={translate(LocalizationStrings.reverseMileageCounting)}></VanillaComponentResolver.instance.ToolButton>
                </FocusDisabled>
            </VCR.Section>
        }
        <VCR.Section title={translate(LocalizationStrings.displayInformation)}>
            <NumberDD
                items={ObjectTyped.entries(DisplayInformation).filter(x => typeof x[1] == 'number').map((x: [string, number]) => ({ value: x[1], displayName: { __Type: LocElementType.String, value: translate("DisplayInformation." + x[0]) } }))}
                onChange={x => routesSystem.Tool_DisplayInformation.set(x).then(x => setBuildIdx(buildIdx + 1))}
                value={routesSystem.Tool_DisplayInformation.value}
            />
        </VCR.Section>
        {routesSystem.Tool_DisplayInformation.value >= DisplayInformation.CUSTOM_1 && routesSystem.Tool_DisplayInformation.value <= DisplayInformation.CUSTOM_5 &&
            <VCR.Section title={translate(LocalizationStrings.customParams)}>
                <FocusDisabled>
                    <VW.IntInputStandalone style={{ flexShrink: 4, textAlign: "right", maxWidth: "105rem", width: "auto", flexGrow: 2, marginRight: "7rem" }} className={editorModule.input}
                        value={routesSystem.Tool_NumericCustomParam1.value} onChange={(x) => routesSystem.Tool_NumericCustomParam1.set(x).then(x => setBuildIdx(buildIdx + 1))} />
                    <VW.IntInputStandalone style={{ flexShrink: 4, textAlign: "right", maxWidth: "105rem", flexGrow: 2, width: "auto", marginRight: "7rem" }} className={editorModule.input}
                        value={routesSystem.Tool_NumericCustomParam2.value} onChange={(x) => routesSystem.Tool_NumericCustomParam2.set(x).then(x => setBuildIdx(buildIdx + 1))} />
                </FocusDisabled>
            </VCR.Section>
        }
    </>

}

/*
<VR.InfoRow left={<>{translate("RoadMarkSettings.RouteIdentifier")}</>} right={<div><StringDD
            onChange={x => routesSystem.Tool_RouteId.set(x).then(x => setBuildIdx(buildIdx + 1))}
            items={x}
            value={routesSystem.Tool_RouteId.value}
        /></div>} />
        <VR.InfoRow left={<>{translate("RoadMarkSettings.RouteDirection")}</>} right={<div><NumberDD
            items={ObjectTyped.entries(RouteDirection).filter(x => typeof x[1] == 'number').map((x: [string, number]) => ({ value: x[1], displayName: { __Type: LocElementType.String, value: translate("RouteDirection." + x[0]) } }))}
            onChange={x => routesSystem.Tool_RouteDirection.set(x).then(x => setBuildIdx(buildIdx + 1))}
            value={routesSystem.Tool_RouteDirection.value}
        /></div>} />
        <VR.InfoRow left={<>{replaceArgs(translate("RoadMarkSettings.OverrideMileage"), [useFormattedLargeNumber(1 * mileageMultiplier) + (localization.unitSettings.unitSystem ? " mi" : " km")])}</>} right={<div><VW.Checkbox
            onChange={x => routesSystem.Tool_OverrideMileage.set(x).then(x => setBuildIdx(buildIdx + 1))}
            checked={routesSystem.Tool_OverrideMileage.value}
        /></div>} />
        {routesSystem.Tool_OverrideMileage.value &&
            <VR.InfoSection>
                <VR.InfoRow left={<>{replaceArgs(translate("RoadMarkSettings.NewMileage"), [localization.unitSettings.unitSystem ? "mi" : "km"])}</>} right={<VW.FloatInputStandalone className={editorModule.input}
                    onChange={x => routesSystem.Tool_NewMileage.set(x * mileageMultiplier).then(x => setBuildIdx(buildIdx + 1))}
                    value={routesSystem.Tool_NewMileage.value / mileageMultiplier}
                    style={{ maxWidth: "150rem", textAlign: "right" }}
                />} />
                <VR.InfoRow left={<>{translate("RoadMarkSettings.ReverseMileageCounting")}</>} right={<div><VW.Checkbox
                    onChange={x => routesSystem.Tool_ReverseMileageCounting.set(x).then(x => setBuildIdx(buildIdx + 1))}
                    checked={routesSystem.Tool_ReverseMileageCounting.value}
                /></div>} />

            </VR.InfoSection>
        }
        <VR.InfoRow left={<>{translate("RoadMarkSettings.DisplayInformation")}</>} right={<div><NumberDD
            items={ObjectTyped.entries(DisplayInformation).filter(x => typeof x[1] == 'number').map((x: [string, number]) => ({ value: x[1], displayName: { __Type: LocElementType.String, value: translate("DisplayInformation." + x[0]) } }))}
            onChange={x => routesSystem.Tool_DisplayInformation.set(x).then(x => setBuildIdx(buildIdx + 1))}
            value={routesSystem.Tool_DisplayInformation.value}
        /></div>} />
        {routesSystem.Tool_DisplayInformation.value >= DisplayInformation.CUSTOM_1 && routesSystem.Tool_DisplayInformation.value <= DisplayInformation.CUSTOM_5 &&
            <VR.InfoSection>
                <VR.InfoRow left={<>{translate("RoadMarkSettings.CustomParam1")}</>} right={<VW.IntInputStandalone className={editorModule.input}
                    onChange={x => routesSystem.Tool_NumericCustomParam1.set(x).then(x => setBuildIdx(buildIdx + 1))}
                    value={routesSystem.Tool_NumericCustomParam1.value}
                    style={{ maxWidth: "150rem", textAlign: "right" }}
                />} />
                <VR.InfoRow left={<>{translate("RoadMarkSettings.CustomParam2")}</>} right={<VW.IntInputStandalone className={editorModule.input}
                    onChange={x => routesSystem.Tool_NumericCustomParam2.set(x).then(x => setBuildIdx(buildIdx + 1))}
                    value={routesSystem.Tool_NumericCustomParam2.value}
                    style={{ maxWidth: "150rem", textAlign: "right" }}
                />} />
            </VR.InfoSection>
        }*/