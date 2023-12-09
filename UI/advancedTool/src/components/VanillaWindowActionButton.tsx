import { Component } from "react";

type SimpleActionButton = {
    iconURL: string,
    onClick: () => void,
    tooltip?: string
}

export class VanillaWindowActionButton extends Component<SimpleActionButton, {}> {
    render() {
        const props = this.props as SimpleActionButton;
        return <>
            <button className="button_Z9O button_ECf item_It6 item-mouse-states_Fmi item-selected_tAM item-focused_FuT button_Z9O button_ECf item_It6 item-mouse-states_Fmi item-selected_tAM item-focused_FuT button_xGY"
                onClick={() => props.onClick()}>
                <img data-tooltip={this.props.tooltip} className="icon_Tdt icon_soN icon_Iwk" src={props.iconURL} />
            </button>
        </>;
    }
}


