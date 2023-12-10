import { GitHubFileItem } from "#service/GitHubAddressesFilesSevice";
import { translate } from "#utility/translate";
import { Cs2FormLine, replaceArgs, setupSignificance } from "@klyte45/euis-components";
import { Component } from "react";


type Props = {
    entry: GitHubFileItem;
    actionButtons?: (nameset: GitHubFileItem) => JSX.Element;
};

export class NamesetGitHubLineViewer extends Component<Props> {
    render() {
        return <div>
            <Cs2FormLine compact={true} title={<>
                <div>{this.props.entry.path.split("/").reverse()[0].replace(".txt", "")}</div>
                <div style={{ fontSize: "75%" }}>{`${translate("namesetLine.fileSize")} ${this.props.entry.size} (${replaceArgs(translate("namesetLine.approximateLinesText"), { min: setupSignificance(this.props.entry.size / 17, 2), max: setupSignificance(this.props.entry.size / 7, 2) })})`}</div>
            </>}>
                {this.props.actionButtons &&
                    <div className="w20" style={{ flexDirection: "row-reverse", alignSelf: "center", display: "flex" }}>
                        {this.props.actionButtons(this.props.entry)}
                    </div>}
            </Cs2FormLine>
        </div>;
    }
}
