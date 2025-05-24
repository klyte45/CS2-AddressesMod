
import { replaceArgs } from "@klyte45/adr-commons";
import { LocElementType, VanillaComponentResolver, VanillaWidgets } from "@klyte45/vuio-commons";
import { DropdownItem, LocElement, selectedInfo } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";
import { useFormattedLargeNumber } from "cs2/utils";
import { ObjectTyped } from "object-typed";
import { useEffect, useState } from "react";
import { AdrFields, AdrFieldType, default as AdrHighwayRoutesSystem, DisplayInformation, LocalizationStrings, RouteDirection, RouteItem, default as routesSystem } from "service/AdrHighwayRoutesSystem";
import { translate } from "utility/translate";
import { MetadataMount, MountMetadataComponentProps } from "./MetadataMount";


export const RoadMarkSettings = () => {
    const VW = VanillaWidgets.instance;
    const VR = VanillaComponentResolver.instance;
    const NumberDD = VW.DropdownField<number>();
    const StringDD = VW.DropdownField<string>();
    const editorModule = VanillaWidgets.instance.editorItemModule;
    const [buildIdx, setBuildIdx] = useState(0)
    const [routesRegistered, setRoutesRegistered] = useState<RouteItem[]>([])

    const localization = useLocalization();
    const mileageMultiplier = localization.unitSettings.unitSystem ? 1.609 : 1


    const [metadata, setMetadata] = useState<AdrFields<string> | null>();
    const [displayNameTypes, setDisplayNameTypes] = useState<string[]>([])
    useEffect(() => {
        AdrHighwayRoutesSystem.getOptionsNamesFromMetadata().then(setDisplayNameTypes)
    }, [buildIdx])
    useEffect(() => {
        AdrHighwayRoutesSystem.getOptionsMetadataFromLayout(routesSystem.InfoPanel_DisplayInformation.value).then((x) => {
            const data = x && JSON.parse(x);
            const validatedData = AdrHighwayRoutesSystem.validateMetadata(data);
            setMetadata(validatedData)
        });
    }, [routesSystem.InfoPanel_DisplayInformation.value])

    useEffect(() => {
        selectedInfo.selectedEntity$.subscribe(() => setBuildIdx(buildIdx + 1));        
    }, [])

    //
    /*
    InfoPanel_RouteId
    InfoPanel_RouteDirection
    InfoPanel_DisplayInformation
    InfoPanel_NumericCustomParam1
    InfoPanel_NumericCustomParam2
    InfoPanel_NewMileage
    InfoPanel_OverrideMileage
    InfoPanel_ReverseMileageCounting
    */
    const routeList = [
        { value: "0".repeat(32), displayName: { __Type: LocElementType.String, value: translate("HighwayRoutes.NoRoute") } }
    ].concat(
        routesRegistered.map((x) => ({ value: x.id, displayName: { __Type: LocElementType.String, value: x.name } })) as any[]
    ) as DropdownItem<string>[]
    return <>
        <VR.InfoRow left={<>{translate(LocalizationStrings.routeIdentifier)}</>} right={<div><StringDD
            onChange={x => routesSystem.InfoPanel_RouteId.set(x).then(x => setBuildIdx(buildIdx + 1))}
            items={routeList}
            value={routesSystem.InfoPanel_RouteId.value}
        /></div>} />
        <VR.InfoRow left={<>{translate(LocalizationStrings.routeDirection)}</>} right={<div><NumberDD
            items={ObjectTyped.entries(RouteDirection).filter(x => typeof x[1] == 'number').map((x: [string, number]) => ({ value: x[1], displayName: { __Type: LocElementType.String, value: translate("RouteDirection." + x[0]) } }))}
            onChange={x => routesSystem.InfoPanel_RouteDirection.set(x).then(x => setBuildIdx(buildIdx + 1))}
            value={routesSystem.InfoPanel_RouteDirection.value}
        /></div>} />
        <VR.InfoRow left={<>{replaceArgs(translate(LocalizationStrings.overrideMileage), [useFormattedLargeNumber(1 * mileageMultiplier) + (localization.unitSettings.unitSystem ? " mi" : " km")])}</>} right={<div><VW.Checkbox
            onChange={x => routesSystem.InfoPanel_OverrideMileage.set(x).then(x => setBuildIdx(buildIdx + 1))}
            checked={routesSystem.InfoPanel_OverrideMileage.value}
        /></div>} />
        {routesSystem.InfoPanel_OverrideMileage.value &&
            <VR.InfoSection>
                <VR.InfoRow left={<>{replaceArgs(translate(LocalizationStrings.newMileage), [localization.unitSettings.unitSystem ? "mi" : "km"])}</>} right={<VW.FloatInputStandalone className={editorModule.input}
                    onChange={x => routesSystem.InfoPanel_NewMileage.set(x * mileageMultiplier).then(x => setBuildIdx(buildIdx + 1))}
                    value={routesSystem.InfoPanel_NewMileage.value / mileageMultiplier}
                    style={{ maxWidth: "150rem", textAlign: "right" }}
                />} />
                <VR.InfoRow left={<>{translate(LocalizationStrings.reverseMileageCounting)}</>} right={<div><VW.Checkbox
                    onChange={x => routesSystem.InfoPanel_ReverseMileageCounting.set(x).then(x => setBuildIdx(buildIdx + 1))}
                    checked={routesSystem.InfoPanel_ReverseMileageCounting.value}
                /></div>} />

            </VR.InfoSection>
        }
        <VR.InfoRow left={<>{translate(LocalizationStrings.displayInformation)}</>} right={<div><NumberDD
            items={[{ value: 0, displayName: { __Type: LocElementType.String, value: translate("DisplayInformation." + DisplayInformation[DisplayInformation.ORIGINAL]) } as LocElement }].concat(
                displayNameTypes.map((x, i) => ({
                    value: i + 1,
                    displayName: { __Type: LocElementType.String, value: x } as LocElement
                })))}
            onChange={x => routesSystem.InfoPanel_DisplayInformation.set(x).then(x => setBuildIdx(buildIdx + 1))}
            value={routesSystem.InfoPanel_DisplayInformation.value}
        /></div>} />
        {routesSystem.InfoPanel_DisplayInformation.value >= DisplayInformation.CUSTOM_1 && routesSystem.InfoPanel_DisplayInformation.value <= DisplayInformation.CUSTOM_7 &&
            (metadata ? <MetadataMount metadata={metadata} parameters={[routesSystem.InfoPanel_NumericCustomParam1, routesSystem.InfoPanel_NumericCustomParam2]} output={mountMetadataOptionsTool} /> :
                <VR.InfoSection>
                    <VR.InfoRow left={<>{translate(LocalizationStrings.customParam1)}</>} right={<VW.IntInputStandalone className={editorModule.input}
                        onChange={x => routesSystem.InfoPanel_NumericCustomParam1.set(x).then(x => setBuildIdx(buildIdx + 1))}
                        value={routesSystem.InfoPanel_NumericCustomParam1.value}
                        style={{ maxWidth: "150rem", textAlign: "right" }}
                    />} />
                    <VR.InfoRow left={<>{translate(LocalizationStrings.customParam2)}</>} right={<VW.IntInputStandalone className={editorModule.input}
                        onChange={x => routesSystem.InfoPanel_NumericCustomParam2.set(x).then(x => setBuildIdx(buildIdx + 1))}
                        value={routesSystem.InfoPanel_NumericCustomParam2.value}
                        style={{ maxWidth: "150rem", textAlign: "right" }}
                    />} />
                </VR.InfoSection>)

        }
    </>
}

function mountMetadataOptionsTool({ validOptions, setStoredValues, storedValues }: MountMetadataComponentProps) {
    const VR = VanillaComponentResolver.instance;
    const VW = VanillaWidgets.instance;
    const NumberDD = VW.DropdownField<number>();
    const editorModule = VanillaWidgets.instance.editorItemModule;
    const style = { maxWidth: "200rem" };
    return <VR.InfoSection>
        {validOptions.map(x => x[1].type == AdrFieldType.SELECTION ?
            <VR.InfoRow left={<div style={style}>{engine.translate(x[1].localization)}</div>} right={<div style={style}><NumberDD
                items={ObjectTyped.entries(x[1].options).map((y: [number, string]) => ({ value: y[0] * 1, displayName: { __Type: LocElementType.String, value: engine.translate(y[1]) } }))}
                onChange={y => setStoredValues(x[0], y * 1)}
                value={storedValues[x[0]]}
            /></div>} />
            :
            <VR.InfoRow left={<div style={style}>{engine.translate(x[1].localization)}</div>} right={<VW.IntInputStandalone className={editorModule.input}
                value={storedValues[x[0]]} onChange={y => setStoredValues(x[0], y * 1)}
                min={x[1].min} max={x[1].max}
                style={{ maxWidth: "150rem", textAlign: "right" }}
            />} />
        )}
    </VR.InfoSection>;
}