import { translate } from "#utility/translate";
import { ExtendedSimpleNameEntry } from "@klyte45/adr-commons";
import { Cs2FormLine } from "@klyte45/euis-components";


type Props = {
    entry: ExtendedSimpleNameEntry;
    actionButtons?: (nameset: ExtendedSimpleNameEntry) => JSX.Element;
};

export const NamesetLineViewer = ({ entry, actionButtons }: Props) => {
    return <div>
        <Cs2FormLine compact={true} title={<>
            <div>{entry._CurrName ?? entry.Name}</div>
            <div style={{ fontSize: "75%" }}>{`${translate("namesetLine.entriesCountLbl")} ${entry.Values.length}`}</div>
        </>}>
            {actionButtons &&
                <div className="w20" style={{ flexDirection: "row-reverse", alignSelf: "center", display: "flex" }}>
                    {actionButtons(entry)}
                </div>}
        </Cs2FormLine>
    </div>;
}
