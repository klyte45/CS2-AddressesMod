import { Component } from "react";
import { Cs2FormLine } from "#components/common/Cs2FormLine";
import { ExtendedSimpleNameEntry } from "#service/NameFileManagementService";


export class NameFileEntryCmp extends Component<{
    entry: ExtendedSimpleNameEntry;
    onView: (entry: ExtendedSimpleNameEntry) => void
}> {
    render() {
        return <div>
            <Cs2FormLine compact={true} title={this.props.entry._CurrName ?? this.props.entry.Name}><button className="neutralBtn" onClick={() => this.props.onView(this.props.entry)}>VIEW</button></Cs2FormLine>
        </div>;
    }
}
