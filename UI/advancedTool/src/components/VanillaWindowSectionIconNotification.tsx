import { Component } from "react";


type Props = {
    description: string | JSX.Element
    icon: string
}
export class VanillaWindowSectionIconNotification extends Component<Props, {}> {
    render() {
        return <>
            <div className="notifications-section_cKq">
                <div className="notification_CLy item-focused_FuT">
                    <img className="icon_UMr" src={this.props.icon} /><div className="label_RLF">{this.props.description}</div>
                </div>
            </div>
        </>;
    }
}
