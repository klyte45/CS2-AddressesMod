import { Entity, SelectInfoPanelService, SelectedInfoOptions, replaceArgs } from "@klyte45/adr-commons";
import { VanillaComponentResolver } from '@klyte45/vuio-commons';
import { Component } from "react";
import { translate } from "utility/translate";

type Props = { entity: Entity, response: SelectedInfoOptions, onChanged: () => Promise<any> };
type State = { loading: boolean }



export class SeedManagementOptionsComponent extends Component<Props, State> {
    constructor(props: Props) {
        super(props)
    }

    private async randomizeSeed(target: Entity) {
        await new Promise((res) => this.setState({ loading: true }, () => { res(0) }))
        await SelectInfoPanelService.redrawSeed(target);
        await this.props.onChanged();
        await new Promise((res) => this.setState({ loading: false }, () => { res(0) }))
    }

    private async changeSeedRefDelta(target: Entity, delta: number) {
        await new Promise((res) => this.setState({ loading: true }, () => { res(0) }))
        await SelectInfoPanelService.changeSeedByDelta(target, delta);
        await this.props.onChanged();
        await new Promise((res) => this.setState({ loading: false }, () => { res(0) }))
    }

    render() {
        if (this.state?.loading) return <>Loading...</>
        const VR = VanillaComponentResolver.instance;
        const props = this.props
        if (!VR || !props.response) return <></>
        const rowTheme = [VR.themeToggleLine?.focusableToggle, VR.themeToggleLine?.row, VR.themeToggleLine?.spaceBetween].join(" ");
        const currentVal = props.response.entityValue;
        const focusKey = VR.FOCUS_DISABLED;

        if (props.response.hasCustomNameList) {

            const right = <>
                <VR.Tooltip tooltip={translate("SeedManagementOptions.UsePreviousNameInList")} >
                    <VR.IconButton focusKey={focusKey} onSelect={() => this.changeSeedRefDelta(currentVal, -1)} style={{ background: "var(--accentColorNormal)" }} theme={VR.themeGamepadToolOptions} src="Media/Glyphs/ThickStrokeArrowLeft.svg" tinted={true} />
                </VR.Tooltip>
                <VR.Tooltip tooltip={translate("SeedManagementOptions.RegenerateNameTooltip")}>
                    <VR.IconButton focusKey={focusKey} onSelect={() => this.randomizeSeed(currentVal)} className="" theme={VR.themeGamepadToolOptions} src="Media/Glyphs/Dice.svg" tinted={true} />
                </VR.Tooltip>
                <VR.Tooltip tooltip={translate("SeedManagementOptions.UseNextNameInList")}>
                    <VR.IconButton focusKey={focusKey} onSelect={() => this.changeSeedRefDelta(currentVal, 1)} className={VR.themeGamepadToolOptions.arrowButton} theme={VR.themeGamepadToolOptions} src="Media/Glyphs/ThickStrokeArrowRight.svg" tinted={true} />
                </VR.Tooltip>
            </>
            return <>
                <VR.InfoRow left={<>{translate("SeedManagementOptions.NamingSource")}</>} right={<>{replaceArgs(translate("SeedManagementOptions.GeneratorFilenameTemplate"), { filename: props.response.customNameListName!! })}</>} />
                <VR.InfoRow subRow={true} left={<>{translate("SeedManagementOptions.GenerationSeedActions")}</>} right={right} />
            </>
        } else {
            return <>
                <VR.InfoRow left={<>{translate("SeedManagementOptions.NamingSource")}</>} right={<>{translate("SeedManagementOptions.UsingVanillaNameGenerator")}</>} />
            </>
        }


    }
}
