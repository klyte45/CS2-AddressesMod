import { Component, ReactElement } from "react";
import { GameScrollComponent } from "./GameScrollComponent";
import './VanillaSideWindow.scss';
import { VanillaWindowActionButton } from "./VanillaWindowActionButton";
import { VanillaWindowSectionDescription } from "./VanillaWindowSectionDescription";
import { VanillaWindowSectionMainKeyValue } from "./VanillaWindowSectionMainKeyValue";
import { VanillaWindowTitle, VanillaWindowTitlteProps } from "./VanillaWindowTitle";
import { VanillaWindowSectionProgressBar } from "./VanillaWindowSectionProgressBar";
import { VanillaWindowToggleButton } from "./VanillaWindowToggleButton";

type VanillaSideWindowProps = {
    title: VanillaWindowTitlteProps
    sections?: ((key: number) => ReactElement<VanillaWindowSectionProgressBar | VanillaWindowSectionMainKeyValue | VanillaWindowSectionDescription>)[]
    leftFooterButtons?: ((key: number) => ReactElement<VanillaWindowActionButton | VanillaWindowToggleButton>)[]
    rightFooterButtons?: ((key: number) => ReactElement<VanillaWindowActionButton | VanillaWindowToggleButton>)[]
}



export class VanillaSideWindow extends Component<VanillaSideWindowProps, {
}> {
    constructor(props: VanillaSideWindowProps) {
        super(props);
        this.state = {}
    }


    render() {
        return <>
            <div className="info-layout_BVk">
                <div className="row_OqM selected-info-panel_iIe">
                    <div className="panel_YqS selected-info-panel_gG8">
                        <div className="header_H_U header_Bpo ">
                            <VanillaWindowTitle {...this.props.title} />
                        </div>
                        <div className="content_XD5 content_AD7 child-opacity-transition_nkS">
                            <GameScrollComponent>
                                {this.props.sections && this.props.sections.map((x, i) => x(i))}
                                <div className="bottom-padding_JS3">
                                </div>
                            </GameScrollComponent>
                        </div>
                        <div className="footer_Pa9 footer_pD5 ">
                            <div className="actions-section_X1x">
                                {this.props.leftFooterButtons && this.props.leftFooterButtons.map((x, i) => x(i))}
                                {this.props.rightFooterButtons && <div className="end-of-line-btn_Kfh">
                                    {this.props.rightFooterButtons.map((x, i) => x(i))}
                                </div>}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </>;
    }
}

