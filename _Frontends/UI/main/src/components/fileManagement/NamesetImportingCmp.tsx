import { translate } from "#utility/translate";
import { ExtendedSimpleNameEntry } from "@klyte45/adr-commons";
import { DefaultPanelScreen, Input } from "@klyte45/euis-components";
import { useState } from "react";
import { NamesetWordsContainer } from './NamesetWordsContainer';

type State = {

}


type Props = {
    namesetData: ExtendedSimpleNameEntry
    onBack: () => void,
    onOk: (namesetData: ExtendedSimpleNameEntry, namesetNameImport: string) => void
}



export const NamesetImportingCmp = ({ namesetData, onBack, onOk }: Props) => {

    const [namesetNameImport, setNamesetNameImport] = useState(namesetData.Name)

    const buttonsRowContent = <>
        <button className="negativeBtn " onClick={onBack}>{translate("namesetsImport.cancel")}</button>
        <button className="positiveBtn " onClick={() => onOk(namesetData, namesetNameImport)}>{translate("namesetsImport.import")}</button>
    </>
    return <>
        <DefaultPanelScreen title={translate("namesetsImport.title")} subtitle={translate("namesetsImport.subtitle")} buttonsRowContent={buttonsRowContent}>
            <div>
                <Input title={translate("namesetsImport.cityImportName")} getValue={() => namesetNameImport} onValueChanged={(x) => { setNamesetNameImport(x); return x; }} />
            </div>
            <NamesetWordsContainer values={namesetData.Values} />
        </DefaultPanelScreen>
    </>;

}

