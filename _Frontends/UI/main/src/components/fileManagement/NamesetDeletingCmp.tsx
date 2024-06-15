import { ExtendedSimpleNameEntry } from "@klyte45/adr-commons";
import { translate } from "#utility/translate"
import { Component } from "react";
import { NamesetWordsContainer } from './NamesetWordsContainer';
import { DefaultPanelScreen } from "@klyte45/euis-components";

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
        return <DefaultPanelScreen title={translate("namesetDelete.title")} subtitle={translate("namesetDelete.subtitle")} buttonsRowContent={<>
            <button className="negativeBtn" onClick={() => this.props.onOk(this.state.namesetData)}>{translate("namesetDelete.yes")}</button>
            <button className="darkestBtn" onClick={this.props.onBack}>{translate("namesetDelete.no")}</button>
        </>}>
            <NamesetWordsContainer values={this.props.namesetData.Values} />
        </DefaultPanelScreen>;
    }
}

