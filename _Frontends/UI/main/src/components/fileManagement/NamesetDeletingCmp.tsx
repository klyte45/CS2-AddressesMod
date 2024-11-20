import { translate } from "#utility/translate";
import { ExtendedSimpleNameEntry } from "@klyte45/adr-commons";
import { DefaultPanelScreen } from "@klyte45/euis-components";
import { NamesetWordsContainer } from './NamesetWordsContainer';

type Props = {
    namesetData: ExtendedSimpleNameEntry
    onBack: () => void,
    onOk: (namesetData: ExtendedSimpleNameEntry) => void
}

export const NamesetDeletingCmp = ({ namesetData, onBack, onOk }: Props) =>
    <>
        <DefaultPanelScreen title={translate("namesetDelete.title")} subtitle={translate("namesetDelete.subtitle")} buttonsRowContent={<>
            <button className="negativeBtn" onClick={() => onOk(namesetData)}>{translate("namesetDelete.yes")}</button>
            <button className="darkestBtn" onClick={onBack}>{translate("namesetDelete.no")}</button>
        </>}>
            <NamesetWordsContainer values={namesetData.Values}  valuesAlternative={namesetData.ValuesAlternative}/>
        </DefaultPanelScreen>
    </>