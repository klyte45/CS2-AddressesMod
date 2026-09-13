import { Entity, SelectInfoPanelService, SelectedInfoOptions, replaceArgs } from "adr";
import { VanillaComponentResolver } from '@klyte45/vuio-commons';
import { useState } from "react";
import { translate } from "utility/translate";

type Props = {
    response: SelectedInfoOptions,
    onChanged: () => Promise<any>,
    /** Seed host (ADR/vanilla aggregate). Defaults to response.entityValue. */
    seedEntity?: Entity,
    /** When false, omit Naming Source row — only seed action buttons (tool options). Default true for SIP. */
    showNamingSource?: boolean,
};

export const SeedManagementOptionsComponent = ({ response, onChanged, seedEntity, showNamingSource = true }: Props) => {

    const [loading, setLoading] = useState(false);
    const seedTarget = seedEntity ?? response.entityValue;

    const randomizeSeed = async () => {
        setLoading(true)
        await SelectInfoPanelService.redrawSeed(seedTarget);
        await onChanged();
        setLoading(false)
    }

    const changeSeedRefDelta = async (delta: number) => {
        setLoading(true)
        await SelectInfoPanelService.changeSeedByDelta(seedTarget, delta);
        await onChanged();
        setLoading(false)
    }

    if (loading) return <>Loading...</>
    const VR = VanillaComponentResolver.instance;
    if (!VR || !response) return <></>
    const focusKey = VR.FOCUS_DISABLED;

    if (response.hasCustomNameList) {
        const theme = VR.themeGamepadToolOptions;
        const btnClass = theme.iconButton;
        const right = <>
            <VR.Tooltip tooltip={translate("SeedManagementOptions.UsePreviousNameInList")}>
                <VR.IconButton focusKey={focusKey} className={btnClass} theme={theme}
                    onSelect={() => changeSeedRefDelta(-1)} src="Media/Glyphs/ThickStrokeArrowLeft.svg" tinted={true} />
            </VR.Tooltip>
            <VR.Tooltip tooltip={translate("SeedManagementOptions.RegenerateNameTooltip")}>
                <VR.IconButton focusKey={focusKey} className={btnClass} theme={theme}
                    onSelect={() => randomizeSeed()} src="Media/Glyphs/Dice.svg" tinted={true} />
            </VR.Tooltip>
            <VR.Tooltip tooltip={translate("SeedManagementOptions.UseNextNameInList")}>
                <VR.IconButton focusKey={focusKey} className={btnClass} theme={theme}
                    onSelect={() => changeSeedRefDelta(1)} src="Media/Glyphs/ThickStrokeArrowRight.svg" tinted={true} />
            </VR.Tooltip>
        </>
        return <>
            {showNamingSource && (
                <VR.InfoRow
                    left={<>{translate("SeedManagementOptions.NamingSource")}</>}
                    right={<>{replaceArgs(translate("SeedManagementOptions.GeneratorFilenameTemplate"), { filename: response.customNameListName!! })}</>}
                />
            )}
            <VR.InfoRow
                subRow={showNamingSource}
                left={<>{translate("SeedManagementOptions.GenerationSeedActions")}</>}
                right={right}
            />
        </>
    }

    if (!showNamingSource) return <></>;

    return <>
        <VR.InfoRow left={<>{translate("SeedManagementOptions.NamingSource")}</>} right={<>{translate("SeedManagementOptions.UsingVanillaNameGenerator")}</>} />
    </>
}
