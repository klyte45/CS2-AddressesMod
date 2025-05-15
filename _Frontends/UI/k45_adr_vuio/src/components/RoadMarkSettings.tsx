
import { replaceArgs } from "@klyte45/adr-commons";
import { VanillaComponentResolver, VanillaWidgets, LocElementType } from "@klyte45/vuio-commons";
import { DropdownItem, LocElement } from "cs2/bindings";
import { useCachedLocalization, useLocalization } from "cs2/l10n";
import { formatLargeNumber, useFormattedLargeNumber } from "cs2/utils";
import { ObjectTyped } from "object-typed";
import { useState } from "react";
import routesSystem, { DisplayInformation, LocalizationStrings, RouteDirection, RouteItem } from "service/AdrHighwayRoutesSystem";
import { translate } from "utility/translate";


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
            items={ObjectTyped.entries(DisplayInformation).filter(x => typeof x[1] == 'number').map((x: [string, number]) => ({ value: x[1], displayName: { __Type: LocElementType.String, value: translate("DisplayInformation." + x[0]) } }))}
            onChange={x => routesSystem.InfoPanel_DisplayInformation.set(x).then(x => setBuildIdx(buildIdx + 1))}
            value={routesSystem.InfoPanel_DisplayInformation.value}
        /></div>} />
        {routesSystem.InfoPanel_DisplayInformation.value >= DisplayInformation.CUSTOM_1 && routesSystem.InfoPanel_DisplayInformation.value <= DisplayInformation.CUSTOM_7 &&
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
            </VR.InfoSection>
        }
    </>
}
