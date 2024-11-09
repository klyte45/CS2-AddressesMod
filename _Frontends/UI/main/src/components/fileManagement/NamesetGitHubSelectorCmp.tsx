import { translate } from "#utility/translate";
import { GitHubAddressesFilesSevice, GitHubFileItem } from "@klyte45/adr-commons";
import { DefaultPanelScreen } from "@klyte45/euis-components";
import { NamesetGitHubCategoryCmp } from "./NamesetGitHubCategoryCmp";
import { NamesetGitHubLineViewer } from "./NamesetGitHubLineViewer";

type Props = {
    actionButtons?: (nameset: GitHubFileItem) => JSX.Element,
    onBack?: () => void
}

export const NamesetGitHubSelectorCmp = ({ actionButtons, onBack }: Props) => {
    const buttonsRowContent = <>
        <button className="negativeBtn" onClick={onBack}>{translate("githubLibrary.back")}</button>
        <button className="neutralBtn" onClick={() => GitHubAddressesFilesSevice.goToRepository()}>{translate("githubLibrary.visitRepository")}</button>
    </>
    return <DefaultPanelScreen title={translate("githubLibrary.title")} subtitle={translate("githubLibrary.subtitle")} buttonsRowContent={buttonsRowContent} scrollable>
        <NamesetGitHubCategoryCmp treeUrl={null} doWithGitHubData={(x, i) => <NamesetGitHubLineViewer entry={x} key={i} actionButtons={actionButtons} />} />
    </DefaultPanelScreen>;

}


