import { LocElementType, VanillaComponentResolver, VanillaWidgets } from "@klyte45/vuio-commons";
import { selectedInfo } from "cs2/bindings";
import { ObjectTyped } from "object-typed";
import { useEffect, useState } from "react";
import { LocalizationStrings, PylonFormat, PylonMaterial, default as routesSystem } from "service/AdrHighwayRoutesSystem";
import { translate } from "utility/translate";

export const RoadMarkSettings = () => {
    const VW = VanillaWidgets.instance;
    const VR = VanillaComponentResolver.instance;
    const NumberDD = VW.DropdownField<number>();
    const [buildIdx, setBuildIdx] = useState(0);

    useEffect(() => {
        selectedInfo.selectedEntity$.subscribe(() => setBuildIdx(x => x + 1));
        if (selectedInfo.selectedEntity$.value.index) setBuildIdx(x => x + 1);
    }, []);

    return <>
        <h4>{translate(LocalizationStrings.poleSettings)}</h4>
        <VR.InfoRow left={<>{translate(LocalizationStrings.poleFormat)}</>} right={<div><NumberDD
            items={ObjectTyped.entries(PylonFormat).filter(x => typeof x[1] == 'number').map((x: [string, number]) => ({ value: x[1], displayName: { __Type: LocElementType.String, value: translate("PylonFormat." + x[0]) } }))}
            onChange={x => routesSystem.InfoPanel_PylonFormat.set(x).then(() => setBuildIdx(buildIdx + 1))}
            value={routesSystem.InfoPanel_PylonFormat.value}
        /></div>} />
        <VR.InfoRow left={<>{translate(LocalizationStrings.poleMaterial)}</>} right={<div><NumberDD
            items={ObjectTyped.entries(PylonMaterial).filter(x => typeof x[1] == 'number').map((x: [string, number]) => ({ value: x[1], displayName: { __Type: LocElementType.String, value: translate("PylonMaterial." + x[0]) } }))}
            onChange={x => routesSystem.InfoPanel_PylonMaterial.set(x).then(() => setBuildIdx(buildIdx + 1))}
            value={routesSystem.InfoPanel_PylonMaterial.value}
        /></div>} />
        <VR.InfoRow left={<>{translate(LocalizationStrings.poleHeight)}</>} right={<VW.FloatSlider
            onChange={x => routesSystem.InfoPanel_PylonHeight.set(x).then(() => setBuildIdx(buildIdx + 1))}
            value={routesSystem.InfoPanel_PylonHeight.value}
            min={0.25} max={10}
            fractionDigits={2}
        />} />
        <VR.InfoRow left={<>{translate(LocalizationStrings.useDoublePole)}</>} right={<VW.Checkbox
            onChange={x => routesSystem.InfoPanel_PylonCount.set(x ? 2 : 1).then(() => setBuildIdx(buildIdx + 1))}
            checked={routesSystem.InfoPanel_PylonCount.value == 2}
        />} />
        {
            routesSystem.InfoPanel_PylonCount.value == 2 &&
            <VR.InfoRow left={<>{translate(LocalizationStrings.poleSpacing)}</>} right={<VW.FloatSlider
                onChange={x => routesSystem.InfoPanel_PylonSpacing.set(x).then(() => setBuildIdx(buildIdx + 1))}
                value={routesSystem.InfoPanel_PylonSpacing.value}
                min={0.05} max={3}
                fractionDigits={2}
            />} />
        }
    </>;
};
