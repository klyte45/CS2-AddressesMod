import { GitHubAddressesFilesSevice, GitHubFileItem } from "@klyte45/adr-commons";
import { NamesetService } from "@klyte45/adr-commons";
import { categorizeFiles } from "#utility/categorizeFiles";
import { translate } from "#utility/translate";
import { Component } from "react";
import { NamesetGitHubCategoryCmp } from "./NamesetGitHubCategoryCmp";
import { NamesetGitHubLineViewer } from "./NamesetGitHubLineViewer";
import { GameScrollComponent } from "@klyte45/euis-components";

type State = {}

type Props = {
    actionButtons?: (nameset: GitHubFileItem) => JSX.Element,
    onBack?: () => void
}



export default class NamesetGitHubSelectorCmp extends Component<Props, State> {

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
            <h1>{translate("githubLibrary.title")}</h1>
            <h3>{translate("githubLibrary.subtitle")}</h3>
            <section style={{ overflow: "scroll", position: "absolute", bottom: this.props.onBack ? 52 : 0, left: 5, right: 5, top: 107 }}>
                <GameScrollComponent>
                    <NamesetGitHubCategoryCmp treeUrl={null} doWithGitHubData={(x, i) => <NamesetGitHubLineViewer entry={x} key={i} actionButtons={this.props.actionButtons} />} />
                </GameScrollComponent>
            </section>
            {this.props.onBack && <div style={{ display: "flex", position: "absolute", left: 5, right: 5, bottom: 5, flexDirection: "row-reverse" }}>
                <button className="negativeBtn" onClick={this.props.onBack}>{translate("githubLibrary.back")}</button>
                <button className="neutralBtn" onClick={() => GitHubAddressesFilesSevice.goToRepository()}>{translate("githubLibrary.visitRepository")}</button>
            </div>}
        </>;
    }
}


