import { LocElementType, VanillaComponentResolver, VanillaWidgets } from "@klyte45/vuio-commons";
import { toolbar } from "cs2/bindings";
import { FocusDisabled } from "cs2/input";
import { ModuleRegistryExtend } from "cs2/modding";
import { ObjectTyped } from "object-typed";
import { useState, useEffect } from "react";
import AdrHighwayRoutesSystem, { LocalizationStrings, PylonFormat, PylonMaterial } from "service/AdrHighwayRoutesSystem";
import routesSystem from "service/AdrHighwayRoutesSystem";
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

const i_DoublePoleOn = "coui://uil/Standard/Checkmark.svg";
const i_DoublePoleOff = "coui://uil/Standard/XClose.svg";

const AdrRoadMarkerToolOptions = () => {
    const [buildIdx, setBuildIdx] = useState(0);

    const VCR = VanillaComponentResolver.instance;
    const VW = VanillaWidgets.instance;
    const NumberDD = VW.DropdownField<number>();

    useEffect(() => {
        void routesSystem.refreshAllBindings();
    }, []);

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
    </>;
};
