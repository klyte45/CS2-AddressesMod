import { ExtendedSimpleNameEntry, GitHubFileItem } from "@klyte45/adr-commons";
import { SimpleNameEntry } from "@klyte45/adr-commons";

export const entryToText = (entry: SimpleNameEntry) =>
    entry.Values.map((x, i) =>
        entry.ValuesAlternative[i] && entry.ValuesAlternative[i] != x
            ? `${x};${entry.ValuesAlternative[i]}`
            : x,
    ).join("\n");

export const parseNamesetText = (text: string, baseName: string): Omit<ExtendedSimpleNameEntry, "ChecksumString"> => {
    const newValues = text.split("\n").map((z) => z.split(";").map((y) => y.trim()).filter(Boolean));
    return {
        IdString: null as any,
        Name: baseName,
        Values: newValues.map((y) => y[0]),
        ValuesAlternative: newValues.map((y) => y[1] || y[0]),
    };
};

export const parseGitHubFile = (fileContents: string, path: string): ExtendedSimpleNameEntry => {
    const lines = fileContents.split("\n")
        .filter((x) => x.trim() !== "")
        .map((x) => { const parts = x.split(";"); return [parts[0], parts[1] ?? parts[0]]; });
    return {
        IdString: null as any,
        Name: `Downloads/${path.split("/").reverse()[0].replace(".txt", "")}`,
        Values: lines.map((x) => x[0].replace("{0}", "").trim()),
        ValuesAlternative: lines.map((x) => x[1].replace("{0}", "").trim()),
    };
};

export const namesetDisplayName = (entry: SimpleNameEntry) => entry.Name;
