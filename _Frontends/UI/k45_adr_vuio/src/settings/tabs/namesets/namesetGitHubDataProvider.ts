import { GitHubAddressesFilesService } from "adr";
import { DataProvider } from "@klyte45/vuio-commons";

export const GITHUB_NAMESETS_ROOT = "github://root";

export type GitHubPickerError = {
    resetTime: Date | null;
};

/** Maps navigated logical paths (e.g. `Europe/France`) to GitHub tree API URLs. */
const treeUrlByLogicalPath = new Map<string, string>();

const normalizeLogicalFolder = (folder: string): string => {
    const normalized = (folder || GITHUB_NAMESETS_ROOT).replace(/\\/g, "/").replace(/\/+$/, "");
    if (!normalized || normalized === GITHUB_NAMESETS_ROOT) return "";
    const prefix = `${GITHUB_NAMESETS_ROOT}/`;
    if (normalized.startsWith(prefix)) return normalized.slice(prefix.length);
    // Legacy / typed API URLs — treat as opaque roots without a logical path.
    return "";
};

const resolveTreeUrl = (folder: string, logicalPath: string): string | undefined => {
    if (!logicalPath) return undefined;
    const mapped = treeUrlByLogicalPath.get(logicalPath);
    if (mapped) return mapped;
    const normalized = folder.replace(/\\/g, "/").replace(/\/+$/, "");
    if (normalized.startsWith("http://") || normalized.startsWith("https://")) return normalized;
    return undefined;
};

const entryName = (path: string) => path.split("/").filter(Boolean).pop() ?? path;

export async function generateGitHubNamesetDataProvider(
    folder: string,
    allowedExtension: string,
): Promise<{ items: DataProvider; error: GitHubPickerError | null }> {
    const logicalPath = normalizeLogicalFolder(folder);
    if (!logicalPath) treeUrlByLogicalPath.clear();

    const url = resolveTreeUrl(folder, logicalPath);
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
        .map((x) => {
            const name = entryName(x.path);
            const childLogical = logicalPath ? `${logicalPath}/${name}` : name;
            if (x.type === "tree") {
                treeUrlByLogicalPath.set(childLogical, x.url);
                return {
                    displayName: name,
                    directory: true,
                    fullPath: `${GITHUB_NAMESETS_ROOT}/${childLogical}/`,
                };
            }
            return {
                displayName: name,
                directory: false,
                fullPath: `${x.url}#${childLogical}`,
            };
        });
    return { items, error: null };
}

export async function loadGitHubNamesetFromPickerPath(encodedPath: string) {
    const hashIdx = encodedPath.indexOf("#");
    const blobUrl = hashIdx >= 0 ? encodedPath.slice(0, hashIdx) : encodedPath;
    const ghPath = hashIdx >= 0 ? encodedPath.slice(hashIdx + 1) : "import.txt";
    const fileContents = await GitHubAddressesFilesService.getBlobData(blobUrl);
    const name = ghPath.replace(/\.txt$/i, "");
    const lines = fileContents.split("\n")
        .filter((x) => x.trim() !== "")
        .map((x) => { const parts = x.split(";"); return [parts[0], parts[1] ?? parts[0]]; });
    return {
        IdString: null as any,
        Name: name || "<?>",
        Values: lines.map((x) => x[0].replace("{0}", "").trim()),
        ValuesAlternative: lines.map((x) => x[1].replace("{0}", "").trim()),
    };
}
