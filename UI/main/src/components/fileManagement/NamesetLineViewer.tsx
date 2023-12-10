import { Component } from "react";
import { Cs2FormLine } from "@klyte45/euis-components";
import { ExtendedSimpleNameEntry } from "#service/NamingRulesService";
import { translate } from "#utility/translate";


export class NamesetLineViewer extends Component<{
    entry: ExtendedSimpleNameEntry;
    actionButtons?: (nameset: ExtendedSimpleNameEntry) => JSX.Element;
}> {
    render() {
        return <div>
            <Cs2FormLine compact={true} title={<>
                <div>{this.props.entry._CurrName ?? this.props.entry.Name}</div>
                <div style={{ fontSize: "75%" }}>{`${translate("namesetLine.entriesCountLbl")} ${this.props.entry.Values.length}`}</div>
            </>}>
                {this.props.actionButtons &&
                    <div className="w20" style={{ flexDirection: "row-reverse", alignSelf: "center", display: "flex" }}>
                        {this.props.actionButtons(this.props.entry)}
                    </div>}
            </Cs2FormLine>
        </div>;
    }
}
