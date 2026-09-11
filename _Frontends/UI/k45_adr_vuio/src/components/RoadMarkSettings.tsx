import { LocElementType, VanillaComponentResolver, VanillaWidgets } from "@klyte45/vuio-commons";
import { LocElement, selectedInfo } from "cs2/bindings";
import { ObjectTyped } from "object-typed";
import { useEffect, useState } from "react";
import { AdrFields, AdrFieldType, default as AdrHighwayRoutesSystem, DisplayInformation, LocalizationStrings, PylonFormat, PylonMaterial, default as routesSystem } from "service/AdrHighwayRoutesSystem";
import { translate } from "utility/translate";
import { MetadataMount, MountMetadataComponentProps } from "./MetadataMount";

export const RoadMarkSettings = () => {
    const VW = VanillaWidgets.instance;
    const VR = VanillaComponentResolver.instance;
    const NumberDD = VW.DropdownField<number>();
    const editorModule = VanillaWidgets.instance.editorItemModule;
    const [buildIdx, setBuildIdx] = useState(0);
    const [metadata, setMetadata] = useState<AdrFields<string> | null>();
    const [displayNameTypes, setDisplayNameTypes] = useState<string[]>([]);

    useEffect(() => {
        AdrHighwayRoutesSystem.getOptionsNamesFromMetadata().then(setDisplayNameTypes);
    }, [buildIdx]);
    useEffect(() => {
        AdrHighwayRoutesSystem.getOptionsMetadataFromLayout(routesSystem.InfoPanel_DisplayInformation.value).then((x) => {
            const data = x && JSON.parse(x);
            const validatedData = AdrHighwayRoutesSystem.validateMetadata(data);
            setMetadata(validatedData);
        });
    }, [routesSystem.InfoPanel_DisplayInformation.value]);

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
        <div style={{ height: "20rem" }} />
        <VR.InfoRow left={<>{translate(LocalizationStrings.displayInformation)}</>} right={<div><NumberDD
            items={[{ value: 0, displayName: { __Type: LocElementType.String, value: translate("DisplayInformation." + DisplayInformation[DisplayInformation.ORIGINAL]) } as LocElement }].concat(
                displayNameTypes.map((x, i) => ({
                    value: i + 1,
                    displayName: { __Type: LocElementType.String, value: x } as LocElement
                })))}
            onChange={x => routesSystem.InfoPanel_DisplayInformation.set(x).then(() => setBuildIdx(buildIdx + 1))}
            value={routesSystem.InfoPanel_DisplayInformation.value}
        /></div>} />
        {routesSystem.InfoPanel_DisplayInformation.value >= DisplayInformation.CUSTOM_1 && routesSystem.InfoPanel_DisplayInformation.value <= DisplayInformation.CUSTOM_7 &&
            (metadata ? <MetadataMount metadata={metadata} parameters={[routesSystem.InfoPanel_NumericCustomParam1, routesSystem.InfoPanel_NumericCustomParam2]} output={mountMetadataOptionsTool} /> :
                <VR.InfoSection>
                    <VR.InfoRow left={<>{translate(LocalizationStrings.customParam1)}</>} right={<VW.IntInputStandalone className={editorModule.input}
                        onChange={x => routesSystem.InfoPanel_NumericCustomParam1.set(x).then(() => setBuildIdx(buildIdx + 1))}
                        value={routesSystem.InfoPanel_NumericCustomParam1.value}
                        style={{ maxWidth: "150rem", textAlign: "right" }}
                    />} />
                    <VR.InfoRow left={<>{translate(LocalizationStrings.customParam2)}</>} right={<VW.IntInputStandalone className={editorModule.input}
                        onChange={x => routesSystem.InfoPanel_NumericCustomParam2.set(x).then(() => setBuildIdx(buildIdx + 1))}
                        value={routesSystem.InfoPanel_NumericCustomParam2.value}
                        style={{ maxWidth: "150rem", textAlign: "right" }}
                    />} />
                </VR.InfoSection>)
        }
    </>;
};

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
            <VR.InfoRow left={<div style={style}>{engine.translate(x[1].localization)}</div>} right={
                typeof x[1].min === "number" && typeof x[1].max === "number"
                    ? <VW.IntSlider
                        value={storedValues[x[0]]}
                        onChange={(y) => setStoredValues(x[0], y)}
                        min={x[1].min}
                        max={x[1].max}
                    />
                    : <VW.IntInputStandalone className={editorModule.input}
                        value={storedValues[x[0]]} onChange={y => setStoredValues(x[0], y * 1)}
                        min={x[1].min} max={x[1].max}
                        style={{ maxWidth: "150rem", textAlign: "right" }}
                    />
            } />
        )}
    </VR.InfoSection>;
}
