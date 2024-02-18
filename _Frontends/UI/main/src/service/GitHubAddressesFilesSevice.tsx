
export type GitHubTreeItem = {
    path: string,
    mode: string,
    type: "tree",
    sha: string,
    url: string
}
export type GitHubFileItem = {
    path: string,
    mode: string,
    type: "blob",
    sha: string,
    url: string,
    size: number
}

export type GitHubItem = GitHubFileItem | GitHubTreeItem

type GitHubResponseTree = {
    sha: string,
    url: string,
    tree: GitHubItem[],
    truncated: boolean
}
type GitHubResponseBlob = {
    sha: string,
    nodeId: string,
    size: number
    url: string,
    content: string,
    encoding: "base64"
}

export type GitHubResponseContainer<T> = {
    success: boolean,
    data: T,
    resetTime?: number
}

export class GitHubAddressesFilesSevice {
    private static readonly ADR_FILES_REPO_URL = "https://api.github.com/repos/klyte45/AddressesFiles/git/trees/master";
    static async listAtTreePoint(url?: string): Promise<GitHubResponseContainer<GitHubItem[]>> {
        try {
            const result = await new Promise<GitHubResponseTree>((resp, err) => {
                try {
                    var xhr = new XMLHttpRequest();
                    xhr.onreadystatechange = () => {
                        if (xhr.readyState === 4) {
                            if (xhr.status / 100 == 2) {
                                resp(JSON.parse(xhr.response));
                            } else {
                                err(xhr)
                            }
                        }
                    };
                    xhr.onerror = (x) => {
                        console.log(x);
                        err(xhr)
                    }
                    xhr.open("GET", url ?? this.ADR_FILES_REPO_URL)
                    xhr.send("");
                } catch {
                    err(xhr)
                }
            })
            return {
                success: true,
                data: result.tree.filter(x => x.type == "tree" || x.path.endsWith(".txt"))
            }
        } catch (err) {
            const xhr = err as XMLHttpRequest
            const rateResetTime = xhr.getResponseHeader?.("X-Ratelimit-Reset");
            console.log(xhr.getAllResponseHeaders())
            console.log(rateResetTime)
            return {
                success: false,
                data: null,
                resetTime: rateResetTime != null ? parseInt(rateResetTime) : null
            }
        }
    }

    static async getBlobData(url: string) {
        const result = await new Promise<GitHubResponseBlob>(resp => {
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = () => {
                if (xhr.readyState === 4) {
                    resp(JSON.parse(xhr.response));
                }
            };
            xhr.open("GET", url)
            xhr.send("");
        })

        return (await engine.call("k45::adr.main.atob", result.content)) as string
    }

    static async goToRepository(): Promise<string> { return await engine.call("k45::adr.namesets.goToGitHubRepo"); }

}
