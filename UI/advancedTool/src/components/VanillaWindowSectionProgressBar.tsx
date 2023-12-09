import { Component, ReactElement } from "react";
import { VanillaWindowSectionSubKeyValue } from "./VanillaWindowSectionSubKeyValue";

const badToGoodClasses = [
    "critical_cqP",
    "bad_BRS",
    "medium_P2l",
    "good_f9U",
    "excellent_cFG"
]

type Props = {
    title: (string | JSX.Element)
    value: number
    maximum: number
    minimum?: number
    statusFn?: (decimalPercentage: number) => 0 | 1 | 2 | 3 | 4
    unit?: (string | JSX.Element)
    subRows?: ((key: number) => ReactElement<VanillaWindowSectionSubKeyValue>)[]
}

function defaultStatusFn(decimalPercentage: number): 0 | 1 | 2 | 3 | 4 {
    if (decimalPercentage < .20) return 4;
    if (decimalPercentage < .50) return 3;
    if (decimalPercentage < .70) return 2;
    if (decimalPercentage < .95) return 1;
    return 0;
}

export class VanillaWindowSectionProgressBar extends Component<Props, {}> {
    render() {
        const min = this.props.minimum ?? 0;
        const percentage = (this.props.value - min) / (this.props.maximum - min)
        const statusIdx = (this.props.statusFn ?? defaultStatusFn)(percentage)
        const barText = `${this.props.value} / ${this.props.maximum} ${this.props.unit ?? ""}`.trim()
        const clampedPercentage = Math.max(0, Math.min(1, percentage));
        return <>
            <div className="info-section_I7V">
                <div className="content_Cdk item-focused_FuT">
                    <div className="info-row_QQ9 item-focused_FuT">
                        <div className="left_RyE uppercase_f0y">{this.props.title}</div>
                    </div>
                    <div className="capacity-bar_uEN">
                        <div className={["progress-bar_AtB", badToGoodClasses[statusIdx] ?? badToGoodClasses[0]].join(" ")}>
                            <div className="label_y0j">{barText}</div>
                            <div className="progress-bounds_D6g" style={{ width: 100 * clampedPercentage + "%" }}>
                                <div className="progress_EvF" style={{ width: 100 / clampedPercentage + "%" }}>
                                    <div className="progress-label_DqS">{barText}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                    {this.props.subRows && this.props.subRows.map((x, i) => x(i))}
                </div>
            </div>
        </>;
    }
}
