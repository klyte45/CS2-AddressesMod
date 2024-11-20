import { translate } from "#utility/translate";
import { DefaultPanelScreen, Input } from "@klyte45/euis-components";
import { useState } from "react";
import { NamesetWordsContainer } from "./NamesetWordsContainer";
import { ExtendedSimpleNameEntry } from "@klyte45/adr-commons";

type Props = {
    namesetData: ExtendedSimpleNameEntry
    onBack: () => void,
}


export const NamesetViewingCmp = ({ namesetData, onBack }: Props) => {

    const [namesetNameImport, setNamesetNameImport] = useState(namesetData.Name);

    const buttonsRowContent = <>
        <button className="negativeBtn " onClick={onBack}>{translate("namesetsLibrary.back")}</button>
    </>;
    return <>
        <DefaultPanelScreen title={namesetData.Name} buttonsRowContent={buttonsRowContent}>
            <NamesetWordsContainer values={namesetData.Values} valuesAlternative={namesetData.ValuesAlternative} />
        </DefaultPanelScreen>
    </>;

};
