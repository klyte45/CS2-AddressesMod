import { Entity, SelectInfoPanelService, SelectedInfoOptions, replaceArgs } from "@klyte45/adr-commons";
import { VanillaComponentResolver } from '@klyte45/vuio-commons';
import { useState } from "react";
import { translate } from "utility/translate";

type Props = { entity: Entity, response: SelectedInfoOptions, onChanged: () => Promise<any> };


export const SeedManagementOptionsComponent = ({ entity, response, onChanged }: Props) => {

    const [loading, setLoading] = useState(false); 

    const randomizeSeed = async (target: Entity) => {
        setLoading(true)
        await SelectInfoPanelService.redrawSeed(target);
        await onChanged();
        setLoading(false)
    }

    const changeSeedRefDelta = async (target: Entity, delta: number) => {
        setLoading(true)
        await SelectInfoPanelService.changeSeedByDelta(target, delta);
        await onChanged();
        setLoading(false)
    }

    if (loading) return <>Loading...</>
    const VR = VanillaComponentResolver.instance;
    if (!VR || !response) return <></>
    const currentVal = response.entityValue;
    const focusKey = VR.FOCUS_DISABLED;

    if (response.hasCustomNameList) {

        const right = <>
            <VR.Tooltip tooltip={translate("SeedManagementOptions.UsePreviousNameInList")} >
                <VR.IconButton focusKey={focusKey} onSelect={() => changeSeedRefDelta(currentVal, -1)} style={{ background: "var(--accentColorNormal)" }} theme={VR.themeGamepadToolOptions} src="Media/Glyphs/ThickStrokeArrowLeft.svg" tinted={true} />
            </VR.Tooltip>
            <VR.Tooltip tooltip={translate("SeedManagementOptions.RegenerateNameTooltip")}>
                <VR.IconButton focusKey={focusKey} onSelect={() => randomizeSeed(currentVal)} className="" theme={VR.themeGamepadToolOptions} src="Media/Glyphs/Dice.svg" tinted={true} />
            </VR.Tooltip>
            <VR.Tooltip tooltip={translate("SeedManagementOptions.UseNextNameInList")}>
                <VR.IconButton focusKey={focusKey} onSelect={() => changeSeedRefDelta(currentVal, 1)} className={VR.themeGamepadToolOptions.arrowButton} theme={VR.themeGamepadToolOptions} src="Media/Glyphs/ThickStrokeArrowRight.svg" tinted={true} />
            </VR.Tooltip>
        </>
        return <>
            <VR.InfoRow left={<>{translate("SeedManagementOptions.NamingSource")}</>} right={<>{replaceArgs(translate("SeedManagementOptions.GeneratorFilenameTemplate"), { filename: response.customNameListName!! })}</>} />
            <VR.InfoRow subRow={true} left={<>{translate("SeedManagementOptions.GenerationSeedActions")}</>} right={right} />
        </>
    } else {
        return <>
            <VR.InfoRow left={<>{translate("SeedManagementOptions.NamingSource")}</>} right={<>{translate("SeedManagementOptions.UsingVanillaNameGenerator")}</>} />
        </>
    }



}
