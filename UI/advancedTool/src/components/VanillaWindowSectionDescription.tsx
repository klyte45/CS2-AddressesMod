import { Component } from "react";

type EffectsProps = {
    icon: string,
    description: (string | JSX.Element)
}

type Props = {
    paragraphs: (string | JSX.Element)[]
    effects?: EffectsProps[]
}
export class VanillaWindowSectionDescription extends Component<Props, {}> {
    render() {
        return <>
            <div className="info-section_I7V description-section_wKs">
                <div className="content_Cdk item-focused_FuT">
                    <div className="description_jgm">
                        <div className="paragraphs_nbD">
                            {this.props.paragraphs.map((x, i) => <p className="" key={i}>{x}</p>)}
                        </div>
                    </div>
                    <div className="effects-row_FPw">
                        {this.props.effects && this.props.effects.map((x, i) =>
                            <div className="field_AjF" key={i}>
                                <img className="icon_WOc" src={x.icon} />
                                <div className="content_dov">{x.description}</div>
                            </div>)
                        }
                    </div>
                </div>
            </div>
        </>;
    }
}
