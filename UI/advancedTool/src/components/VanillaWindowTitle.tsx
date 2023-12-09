import { Component } from "react";
import { KeyboardEvent } from "react";

export type VanillaWindowTitlteProps = {
    value: string,
    isInput?: boolean,
    iconSrc?: string,
    tooltip?: string,
    maxLength?: number,
    onSend?: (newValue: string) => string
    onClose?: () => void,
}
export class VanillaWindowTitle extends Component<VanillaWindowTitlteProps, {
    value: string;
}> {

    constructor(props: VanillaWindowTitlteProps) {
        super(props);
        this.state = {
            value: props.value
        };
    }
    onKeyDown(x: KeyboardEvent<HTMLInputElement>): void {
        if (x.key == "Escape") {
            const currentTarget = x.currentTarget;
            this.setState({ value: this.props.value }, () => {
                currentTarget.value = this.state.value;
                currentTarget.blur();
            });
        } else if (x.key == "Enter") {
            x.currentTarget.blur();
        }
    }
    render() {
        return <>
            <div className="title-bar_PF4">
                {this.props.iconSrc && <img className="icon_OVK" src={this.props.iconSrc} />}
                <div className="title_SVH title_zQN">
                    <div className="ellipsis-text-input_h9o container_Q9t" data-tooltip={this.props.tooltip}>
                        <input className={"input_MMV input_lbJ " + (this.props.isInput ? "" : "disabled")} type="text" maxLength={this.props.maxLength ?? 64} value={this.state.value}
                            onKeyDown={(x) => this.onKeyDown(x)}
                            onChange={x => this.setState({ value: this.props.onSend ? this.props.onSend(x.target.value) : x.target.value })} />

                        <div className="label_qsp label_GFm">{this.state.value}</div>
                    </div>
                </div>
                {this.props.onClose && <button className="button_bvQ button_bvQ close-button_wKK" onClick={() => this.props.onClose()}>
                    <div className="tinted-icon_iKo icon_PhD" style={{ maskImage: 'url(Media/Glyphs/Close.svg)' }}></div>
                </button>}
            </div>
        </>;
    }
}
