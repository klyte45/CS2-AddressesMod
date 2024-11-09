import { translate } from "#utility/translate";
import { GitHubFileItem } from "@klyte45/adr-commons";
import { Cs2FormLine, replaceArgs, setupSignificance } from "@klyte45/euis-components";


type Props = {
    entry: GitHubFileItem;
    actionButtons?: (nameset: GitHubFileItem) => JSX.Element;
};

export const NamesetGitHubLineViewer = ({ entry, actionButtons }: Props) => <div>
    <Cs2FormLine compact={true} title={<>
        <div>{entry.path.split("/").reverse()[0].replace(".txt", "")}</div>
        <div style={{ fontSize: "75%" }}>{`${translate("namesetLine.fileSize")} ${entry.size} (${replaceArgs(translate("namesetLine.approximateLinesText"), { min: setupSignificance(entry.size / 17, 2), max: setupSignificance(entry.size / 7, 2) })})`}</div>
    </>}>
        {actionButtons &&
            <div className="w20" style={{ flexDirection: "row-reverse", alignSelf: "center", display: "flex" }}>
                {actionButtons(entry)}
            </div>}
    </Cs2FormLine>
</div>
