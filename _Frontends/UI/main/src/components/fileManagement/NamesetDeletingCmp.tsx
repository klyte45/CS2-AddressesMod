import { ExtendedSimpleNameEntry } from '#service/NamingRulesService';
import { translate } from "#utility/translate"
import { Component } from "react";
import { NamesetWordsContainer } from './NamesetWordsContainer';

type State = {
    namesetData: ExtendedSimpleNameEntry,
}


type Props = {
    namesetData: ExtendedSimpleNameEntry
    onBack: () => void,
    onOk: (namesetData: ExtendedSimpleNameEntry) => void
}



export default class NamesetDeletingCmp extends Component<Props, State> {

    constructor(props: Props | Readonly<Props>) {
        super(props);
        this.state = {
            namesetData: props.namesetData,
        }
    }

    render() {
        return <>
            <h1>{translate("namesetDelete.title")}</h1>
            <h3>{translate("namesetDelete.subtitle")}</h3>
            <section style={{ position: "absolute", bottom: this.props.onBack ? 52 : 0, left: 5, right: 5, top: 107 }}>
                <NamesetWordsContainer values={this.props.namesetData.Values} />
            </section>
            <div style={{ display: "flex", position: "absolute", left: 5, right: 5, bottom: 5, flexDirection: "row-reverse" }}>
                <button className="negativeBtn" onClick={() => this.props.onOk(this.state.namesetData)}>{translate("namesetDelete.yes")}</button>
                <button className="darkestBtn" onClick={this.props.onBack}>{translate("namesetDelete.no")}</button>
            </div>
        </>;
    }
}

