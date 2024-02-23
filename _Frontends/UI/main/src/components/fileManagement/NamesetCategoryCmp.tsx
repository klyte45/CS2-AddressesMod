import { ObjectTyped } from "object-typed";
import { Component } from "react";
import TreeView from "react-treeview";
import { SimpleNameEntry } from "@klyte45/adr-commons";
import { StructureTreeNode } from "#utility/categorizeFiles";

export class NamesetCategoryCmp extends Component<{ entry: StructureTreeNode; doWithNamesetData: (x: SimpleNameEntry, i: number) => JSX.Element }, { showing: Record<string, boolean>; }> {

    constructor(props) {
        super(props);
        this.state = {
            showing: {}
        };
    }

    render() {
        return <>
            {ObjectTyped.entries(this.props.entry.subtrees).sort((a, b) => a[0].localeCompare(b[0], undefined, { sensitivity: "base" })).map((x, i) => {
                return <TreeView
                    nodeLabel={x[0]}
                    key={i}
                    collapsed={!this.state.showing[x[0]]}
                    onClick={() => this.toggle(x[0])}
                ><NamesetCategoryCmp entry={x[1]} doWithNamesetData={this.props.doWithNamesetData} /></TreeView>;
            })}
            {this.props.entry.rootContent.sort((a, b) => a.Name.localeCompare(b.Name, undefined, { sensitivity: "base" })).map(this.props.doWithNamesetData)}
        </>;
    }
    toggle(item: string): void {
        this.state.showing[item] = !this.state.showing[item];
        this.setState(this.state);
    }
}