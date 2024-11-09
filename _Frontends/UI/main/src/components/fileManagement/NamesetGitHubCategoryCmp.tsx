import { translate } from "#utility/translate";
import { GitHubAddressesFilesSevice, GitHubFileItem, GitHubTreeItem } from "@klyte45/adr-commons";
import { replaceArgs } from "@klyte45/euis-components";
import EuisTreeView from "@klyte45/euis-components/src/components/EuisTreeView";
import { useEffect, useState } from "react";

type Props = {
    treeUrl: string | null;
    doWithGitHubData: (x: GitHubFileItem, i: number) => JSX.Element;
};

export const NamesetGitHubCategoryCmp = ({ treeUrl, doWithGitHubData }: Props) => {

    const [currentTree, setCurrentTree] = useState(null as GitHubTreeItem[]);
    const [currentFiles, setCurrentFiles] = useState(null as GitHubFileItem[]);
    const [loaded, setLoaded] = useState(undefined as boolean);
    const [error, setError] = useState(undefined as boolean);
    const [resetDate, setResetDate] = useState(undefined as Date);


    const loadUrl = async () => {
        try {
            const treeData = await GitHubAddressesFilesSevice.listAtTreePoint(treeUrl)
            if (treeData.success) {
                setCurrentTree(treeData.data.filter(x => x.type == "tree") as GitHubTreeItem[])
                setCurrentFiles(treeData.data.filter(x => x.type == "blob") as GitHubFileItem[])
                setLoaded(true)
            } else {
                setError(true)
                setResetDate(treeData.resetTime ? new Date(treeData.resetTime) : null)
            }
        } catch {
            setError(true)
            setResetDate(null)
        }
    }
    useEffect(() => { loadUrl() }, [])


    if (error) return <div>{resetDate ? replaceArgs(translate("githubListing.errorLoadingRateLimit"), { formattedTime: resetDate.toString() }) : translate("githubListing.errorLoadingMsg")}</div>
    if (!loaded) return <div>{translate("githubListing.loading")}</div>
    if (!currentTree && !currentFiles) return <div>{translate("githubListing.noEntries")}</div>
    return <>
        {currentTree.sort((a, b) => a.path.localeCompare(b.path)).map((x, i) => {
            return <EuisTreeView
                nodeLabel={x.path.split("/").reverse()[0]}
                key={i}
            ><NamesetGitHubCategoryCmp treeUrl={x.url} doWithGitHubData={doWithGitHubData} /></EuisTreeView>;
        })}
        {currentFiles.sort((a, b) => a.path.localeCompare(b.path)).map(doWithGitHubData)}
    </>;

}