import { ObjectTyped } from "object-typed";
import { Component } from "react";
import TreeView from "react-treeview";
import "#styles/treeview.scss"
import { StructureTreeNode } from "./NameFileViewerCmp";
import { SimpleNameEntry } from "#service/NameFileManagementService";

export class NameFileCategoryCmp extends Component<{ entry: StructureTreeNode; doWithPaletteData: (x: SimpleNameEntry, i: number) => JSX.Element }, { showing: Record<string, boolean>; }> {

    constructor(props) {
        super(props);
        this.state = {
            showing: {}
        };
    }

    render() {
        return <>
            {ObjectTyped.entries(this.props.entry.subtrees).sort((a, b) => a[0].localeCompare(b[0])).map((x, i) => {
                return <TreeView
                    nodeLabel={x[0]}
                    key={i}
                    collapsed={!this.state.showing[x[0]]}
                    onClick={() => this.toggle(x[0])}
                ><NameFileCategoryCmp entry={x[1]} doWithPaletteData={this.props.doWithPaletteData} /></TreeView>;
            })}
            {this.props.entry.rootContent.sort((a, b) => a.Name.localeCompare(b.Name)).map(this.props.doWithPaletteData)}
        </>;
    }
    toggle(item: string): void {
        this.state.showing[item] = !this.state.showing[item];
        this.setState(this.state);
    }
}
