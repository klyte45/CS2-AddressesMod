import { Component } from "react";


type TogglableButton = {
    iconURL: string,
    onToggle: (newValue: boolean) => void,
    stateValue: boolean,
    tooltip?: string
}
export class VanillaWindowToggleButton extends Component<TogglableButton, {}> {
    render() {
        const props = this.props as TogglableButton;
        return <>
            <button className={"button_Z9O button_ECf item_It6 item-mouse-states_Fmi item-selected_tAM item-focused_FuT button_Z9O button_ECf item_It6 item-mouse-states_Fmi item-selected_tAM item-focused_FuT button_xGY toggle_LKL " + (props.stateValue ? "" : "off_X4Y")}
                onClick={() => props.onToggle(!props.stateValue)}>
                <div data-tooltip={this.props.tooltip} className="tinted-icon_iKo icon_Tdt icon_soN icon_Iwk" style={{ maskImage: `url(${props.iconURL})` }}></div>
            </button>
        </>;
    }
}
