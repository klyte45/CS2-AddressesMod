import { Entity, SelectInfoPanelService, SelectedInfoOptions, nameToString, replaceArgs } from "@klyte45/adr-commons";
import { useUniqueFocusKey } from "common/focus/focus-key";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver";
import { Component } from "react";
import { translate } from "utility/translate";

type Props = { entity: Entity, response: SelectedInfoOptions, onChanged: () => Promise<any> };
type State = { loading: boolean }



export class SeedManagementOptionsComponent extends Component<Props, State>{
    constructor(props: Props) {
        super(props)
    }

    private async randomizeSeed(target: Entity) {
        await new Promise((res) => this.setState({ loading: true }, () => { res(0) }))
        await SelectInfoPanelService.redrawSeed(target);
        await this.props.onChanged();
        await new Promise((res) => this.setState({ loading: false }, () => { res(0) }))
    }

    render() {
        if (this.state?.loading) return <>Loading...</>
        const VR = VanillaComponentResolver.instance;
        const props = this.props
        if (!VR || !props.response) return <></>
        const isDistrictSelected = props.response.entityValue.Index == props.response.districtRef.Index
        const rowTheme = [VR.themeToggleLine?.focusableToggle, VR.themeToggleLine?.row, VR.themeToggleLine?.spaceBetween].join(" ");
        const currentVal = props.response.entityValue;
        const focusKey = VR.FOCUS_DISABLED;
        return <>
            <VR.InfoRow subRow={rowTheme} className={rowTheme} left={<>{translate("SeedManagementOptions.GenerationSeedActions")}</>} right={<>
                <VR.Tooltip tooltip={translate("SeedManagementOptions.RegenerateNameTooltip")}>
                    <VR.IconButton onSelect={() => this.randomizeSeed(currentVal)} className={VR.themeGamepadToolOptions.arrowButton} theme={VR.themeGamepadToolOptions} src="Media/Glyphs/Dice.svg" tinted={true} />
                </VR.Tooltip>
            </>} />
        </>
    }
}
