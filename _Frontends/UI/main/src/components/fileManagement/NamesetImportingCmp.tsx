import { ExtendedSimpleNameEntry } from '#service/NamingRulesService';
import { translate } from "#utility/translate";
import { Input } from "@klyte45/euis-components";
import { Component } from "react";
import { NamesetWordsContainer } from './NamesetWordsContainer';

type State = {
    willRandomize: boolean,
    namesetData: ExtendedSimpleNameEntry,
    namesetNameImport: string
}


type Props = {
    namesetData: ExtendedSimpleNameEntry
    onBack: () => void,
    onOk: (x: State) => void
}



export default class NamesetImportingCmp extends Component<Props, State> {

    constructor(props: Props | Readonly<Props>) {
        super(props);
        this.state = {
            willRandomize: false,
            namesetData: props.namesetData,
            namesetNameImport: props.namesetData.Name
        }
    }

    render() {
        return <>
            <h1>{translate("namesetsImport.title")}</h1>
            <h3>{translate("namesetsImport.subtitle")}</h3>
            <section style={{ position: "absolute", bottom: this.props.onBack ? 52 : 0, left: 5, right: 5, top: 107 }}>
                <div>
                    <Input title={translate("namesetsImport.cityImportName")} getValue={() => this.state.namesetNameImport} onValueChanged={(x) => { this.setState({ namesetNameImport: x }); return x; }} />
                </div>
                <NamesetWordsContainer values={this.state.namesetData.Values} />
            </section>
            <div style={{ display: "flex", position: "absolute", left: 5, right: 5, bottom: 5, flexDirection: "row-reverse" }}>
                <button className="negativeBtn " onClick={this.props.onBack}>{translate("namesetsImport.cancel")}</button>
                <button className="positiveBtn " onClick={() => this.props.onOk(this.state)}>{translate("namesetsImport.import")}</button>
            </div>
        </>;
    }
}

