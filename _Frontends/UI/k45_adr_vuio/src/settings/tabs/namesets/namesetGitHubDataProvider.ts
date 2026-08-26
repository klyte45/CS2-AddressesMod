import { GitHubAddressesFilesService } from "@klyte45/adr-commons";
import { DataProvider } from "@klyte45/vuio-commons";

export const GITHUB_NAMESETS_ROOT = "github://root";

export type GitHubPickerError = {
    resetTime: Date | null;
};

export async function generateGitHubNamesetDataProvider(
    folder: string,
    allowedExtension: string,
): Promise<{ items: DataProvider; error: GitHubPickerError | null }> {
    const url = !folder || folder === GITHUB_NAMESETS_ROOT ? undefined : folder.replace(/\/$/, "");
    const result = await GitHubAddressesFilesService.listAtTreePoint(url);
    if (!result.success || !result.data) {
        return {
            items: [],
            error: {
                resetTime: result.resetTime != null ? new Date(result.resetTime * 1000) : null,
            },
        };
    }
    const ext = allowedExtension.replace("*", "");
    const items: DataProvider = result.data
        .filter((x) => x.type === "tree" || x.path.endsWith(ext))
        .map((x) => ({
            displayName: x.path.split("/").pop() ?? x.path,
            directory: x.type === "tree",
            fullPath: x.type === "tree" ? `${x.url}/` : `${x.url}#${x.path}`,
        }));
    return { items, error: null };
}

export async function loadGitHubNamesetFromPickerPath(encodedPath: string) {
    const [blobUrl, ghPath = "import.txt"] = encodedPath.split("#");
    const fileContents = await GitHubAddressesFilesService.getBlobData(blobUrl);
    const fileName = ghPath.split("/").pop() ?? "import.txt";
    const lines = fileContents.split("\n")
        .filter((x) => x.trim() !== "")
        .map((x) => { const parts = x.split(";"); return [parts[0], parts[1] ?? parts[0]]; });
    return {
        IdString: null as any,
        Name: `Downloads/${fileName.replace(".txt", "")}`,
        Values: lines.map((x) => x[0].replace("{0}", "").trim()),
        ValuesAlternative: lines.map((x) => x[1].replace("{0}", "").trim()),
    };
}