import { Component } from "react";


type Props = {
    keyName: (string | JSX.Element),
    value: (string | JSX.Element),
    onClick?: () => void
}
export class VanillaWindowSectionSubKeyValue extends Component<Props, {}> {
    render() {
        return <>
            <div className={"info-row_QQ9 item-focused_FuT subRow_NJI " + (this.props.onClick ? "link_ICj" : "")}>
                <div className={"left_RyE " + (this.props.onClick ? "link_ICj" : "")}>{this.props.keyName}</div>
                <div onClick={this.props.onClick} className={"right_ZUb " + (this.props.onClick ? "link_ICj" : "")}>{this.props.value}</div>
            </div>
        </>;
    }
}
