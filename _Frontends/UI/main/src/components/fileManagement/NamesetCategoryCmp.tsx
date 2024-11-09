import { StructureTreeNode } from "#utility/categorizeFiles";
import { SimpleNameEntry } from "@klyte45/adr-commons";
import EuisTreeView from "@klyte45/euis-components/src/components/EuisTreeView";
import { ObjectTyped } from "object-typed";

type Props = {
    entry: StructureTreeNode;
    doWithNamesetData: (x: SimpleNameEntry, i: number) => JSX.Element;
};

export const NamesetCategoryCmp = ({ entry, doWithNamesetData }: Props) => {
    return <>
        {ObjectTyped.entries(entry.subtrees).sort((a, b) => a[0].localeCompare(b[0], undefined, { sensitivity: "base" })).map((x, i) => {
            return <EuisTreeView nodeLabel={x[0]} key={i}><NamesetCategoryCmp entry={x[1]} doWithNamesetData={doWithNamesetData} /></EuisTreeView>;
        })}
        {entry.rootContent.sort((a, b) => a.Name.localeCompare(b.Name, undefined, { sensitivity: "base" })).map(doWithNamesetData)}
    </>;
}