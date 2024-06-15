import { translate } from "#utility/translate"
import { Component } from "react";
import { NamesetService } from "@klyte45/adr-commons";
import { categorizeFiles } from "#utility/categorizeFiles";
import { NamesetCategoryCmp } from "./NamesetCategoryCmp";
import { NamesetLineViewer } from "./NamesetLineViewer";
import { ExtendedSimpleNameEntry } from "@klyte45/adr-commons";
import { DefaultPanelScreen, GameScrollComponent } from "@klyte45/euis-components";

type State = {
    availableNamesets: NamesetStructureTreeNode,
}

type NamesetStructureTreeNode = {
    rootContent: ExtendedSimpleNameEntry[],
    subtrees: Record<string, NamesetStructureTreeNode>
}

type Props = {
    actionButtons?: (nameset: ExtendedSimpleNameEntry) => JSX.Element,
    onBack?: () => void
}



export default class NamesetLibrarySelectorCmp extends Component<Props, State> {

    constructor(props) {
        super(props);
        this.state = {
            availableNamesets: { subtrees: {}, rootContent: [] }
        }
    }
    componentDidMount() {
        const _this = this;
        engine.whenReady.then(async () => {
            this.updateNamesets();
        })
    }
    private async updateNamesets() {
        const namesetsSaved = await NamesetService.listLibraryNamesets();
        const namesetTree = categorizeFiles(namesetsSaved)
        const root = namesetTree[""]?.rootContent ?? []
        delete namesetTree[""];
        this.setState({
            availableNamesets: {
                rootContent: root,
                subtrees: namesetTree
            }
        });
    }

    render() {
        const buttonsRowContent = <>
            <button className="negativeBtn" onClick={this.props.onBack}>{translate("namesetsLibrary.back")}</button>
            <button className="neutralBtn" onClick={() => NamesetService.reloadLibraryNamesets().then(() => this.updateNamesets())}>{translate("namesetsLibrary.reloadFiles")}</button>
            <button className="neutralBtn" onClick={() => NamesetService.goToDiskSimpleNamesFolder()}>{translate("namesetsLibrary.goToLibraryFolder")}</button>
        </>
        return <>
            <DefaultPanelScreen title={translate("namesetsLibrary.title")} subtitle={translate("namesetsLibrary.subtitle")} buttonsRowContent={buttonsRowContent} scrollable>
                <NamesetCategoryCmp entry={this.state?.availableNamesets} doWithNamesetData={(x, i) => <NamesetLineViewer entry={x} key={i} actionButtons={this.props.actionButtons} />} />
            </DefaultPanelScreen>
        </>;
    }
}


