import { Component, ReactElement } from "react";
import { VanillaWindowSectionSubKeyValue } from "./VanillaWindowSectionSubKeyValue";


export type VanillaWindowSectionMainKeyValueProps = {
    keyName: (string | JSX.Element),
    value: (string | JSX.Element),
    subRows?: ((key: number) => ReactElement<VanillaWindowSectionSubKeyValue>)[]
    onClick?: () => void
}
export class VanillaWindowSectionMainKeyValue extends Component<VanillaWindowSectionMainKeyValueProps, {}> {
    render() {
        return <>
            <div className="info-section_I7V">
                <div className="content_Cdk item-focused_FuT">
                    <div className={"info-row_QQ9 item-focused_FuT " + (this.props.onClick ? "link_ICj" : "")}>
                        <div className={"left_RyE uppercase_f0y " + (this.props.onClick ? "link_ICj" : "")}>{this.props.keyName}</div>
                        <div onClick={this.props.onClick} className={"right_ZUb " + (this.props.onClick ? "link_ICj" : "")}>{this.props.value}</div>
                    </div>
                    {this.props.subRows && this.props.subRows.map((x, i) => x(i))}
                </div>
            </div>
        </>;
    }
}
