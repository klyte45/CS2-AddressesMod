import { ExtendedSimpleNameEntry } from "@klyte45/adr-commons";
import { translate } from "#utility/translate";
import { DefaultPanelScreen, Input } from "@klyte45/euis-components";
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
        const buttonsRowContent = <>
            <button className="negativeBtn " onClick={this.props.onBack}>{translate("namesetsImport.cancel")}</button>
            <button className="positiveBtn " onClick={() => this.props.onOk(this.state)}>{translate("namesetsImport.import")}</button>
        </>
        return <>
            <DefaultPanelScreen title={translate("namesetsImport.title")} subtitle={translate("namesetsImport.subtitle")} buttonsRowContent={buttonsRowContent}>
                <div>
                    <Input title={translate("namesetsImport.cityImportName")} getValue={() => this.state.namesetNameImport} onValueChanged={(x) => { this.setState({ namesetNameImport: x }); return x; }} />
                </div>
                <NamesetWordsContainer values={this.state.namesetData.Values} />
            </DefaultPanelScreen>

        </>
            ;
    }
}

