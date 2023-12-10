import { translate } from "#utility/translate"
import { Component } from "react";
import { NamesetService } from "#service/NamesetService";
import { categorizeFiles } from "#utility/categorizeFiles";
import { NamesetCategoryCmp } from "./NamesetCategoryCmp";
import { NamesetLineViewer } from "./NamesetLineViewer";
import { ExtendedSimpleNameEntry } from "#service/NamingRulesService";
import { GameScrollComponent } from "@klyte45/euis-components";

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
        return <>
            <h1>{translate("namesetsLibrary.title")}</h1>
            <h3>{translate("namesetsLibrary.subtitle")}</h3>
            <section style={{ overflow: "scroll", position: "absolute", bottom: this.props.onBack ? 52 : 0, left: 5, right: 5, top: 107 }}>
                <GameScrollComponent >
                    <NamesetCategoryCmp entry={this.state?.availableNamesets} doWithNamesetData={(x, i) => <NamesetLineViewer entry={x} key={i} actionButtons={this.props.actionButtons} />} />
                </GameScrollComponent>
            </section>
            {this.props.onBack && <div style={{ display: "flex", position: "absolute", left: 5, right: 5, bottom: 5, flexDirection: "row-reverse" }}>
                <button className="negativeBtn" onClick={this.props.onBack}>{translate("namesetsLibrary.back")}</button>
                <button className="neutralBtn" onClick={() => NamesetService.reloadLibraryNamesets().then(() => this.updateNamesets())}>{translate("namesetsLibrary.reloadFiles")}</button>
                <button className="neutralBtn" onClick={() => NamesetService.goToDiskSimpleNamesFolder()}>{translate("namesetsLibrary.goToLibraryFolder")}</button>
            </div>}
        </>;
    }
}


