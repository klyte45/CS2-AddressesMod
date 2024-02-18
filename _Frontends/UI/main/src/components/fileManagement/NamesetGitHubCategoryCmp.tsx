import { GitHubAddressesFilesSevice, GitHubFileItem, GitHubTreeItem } from "#service/GitHubAddressesFilesSevice";
import { translate } from "#utility/translate";
import { replaceArgs } from "@klyte45/euis-components";
import { Component } from "react";
import TreeView from "react-treeview";

type State = {
    showing: Record<string, boolean>;
    currentTree?: GitHubTreeItem[];
    currentFiles?: GitHubFileItem[];
    loaded?: boolean;
    error?: boolean;
    resetDate?: Date
};

type Props = {
    treeUrl: string | null;
    doWithGitHubData: (x: GitHubFileItem, i: number) => JSX.Element;
};

export class NamesetGitHubCategoryCmp extends Component<Props, State> {

    constructor(props) {
        super(props);
        this.state = {
            showing: {},
            currentTree: null,
            currentFiles: null,
        };
        this.loadUrl()
    }
    async loadUrl() {
        try {
            const treeData = await GitHubAddressesFilesSevice.listAtTreePoint(this.props.treeUrl)
            if (treeData.success) {
                this.setState({
                    currentTree: treeData.data.filter(x => x.type == "tree") as GitHubTreeItem[],
                    currentFiles: treeData.data.filter(x => x.type == "blob") as GitHubFileItem[],
                    loaded: true
                })
            } else {
                this.setState({
                    error: true,
                    resetDate: treeData.resetTime ? new Date(treeData.resetTime) : null
                })
            }
        } catch {
            this.setState({
                error: true,
                resetDate: null
            })
        }
    }



    render() {
        if (this.state.error) return <div>{this.state.resetDate ? replaceArgs(translate("githubListing.errorLoadingRateLimit"), { formattedTime: this.state.resetDate.toString() }) : translate("githubListing.errorLoadingMsg")}</div>
        if (!this.state.loaded) return <div>{translate("githubListing.loading")}</div>
        if (!this.state.currentTree && !this.state.currentFiles) return <div>{translate("githubListing.noEntries")}</div>
        return <>
            {this.state.currentTree.sort((a, b) => a.path.localeCompare(b.path)).map((x, i) => {
                return <TreeView
                    nodeLabel={x.path.split("/").reverse()[0]}
                    key={i}
                    collapsed={!this.state.showing[x.path]}
                    onClick={() => this.toggle(x.path)}
                ><NamesetGitHubCategoryCmp treeUrl={x.url} doWithGitHubData={this.props.doWithGitHubData} /></TreeView>;
            })}
            {this.state.currentFiles.sort((a, b) => a.path.localeCompare(b.path)).map(this.props.doWithGitHubData)}
        </>;
    }
    toggle(item: string): void {
        this.state.showing[item] = !this.state.showing[item];
        this.setState(this.state);
    }
}