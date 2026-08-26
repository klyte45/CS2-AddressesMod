import { ExtendedSimpleNameEntry } from "@klyte45/adr-commons";
import { FormLine, VanillaWidgets } from "@klyte45/vuio-commons";
import { useState } from "react";
import { translate } from "../../translate";
import styles from "../../styles/namesetLibrary.module.scss";
import { NamesetWordsContainer } from "./NamesetWordsContainer";

type Props = {
    namesetData: ExtendedSimpleNameEntry;
    onBack: () => void;
    onOk: (namesetData: ExtendedSimpleNameEntry, namesetNameImport: string) => void;
};

export const NamesetImportingCmp = ({ namesetData, onBack, onOk }: Props) => {
    const StringInput = VanillaWidgets.instance.StringInputField;
    const editorModule = VanillaWidgets.instance.editorItemModule;
    const [namesetNameImport, setNamesetNameImport] = useState(namesetData.Name);

    return (
        <div className={`${styles.screenRoot} k45_btnHost`}>
            <div className={styles.toolbar}>
                <button type="button" className="negativeBtn" onClick={onBack}>{translate("namesetsImport.cancel")}</button>
                <button type="button" className="positiveBtn" onClick={() => onOk(namesetData, namesetNameImport)}>{translate("namesetsImport.import")}</button>
            </div>
            <div className={styles.screenBody}>
                <FormLine title={translate("namesetsImport.cityImportName")}>
                    <StringInput
                        className={editorModule.input}
                        value={namesetNameImport}
                        onChange={setNamesetNameImport}
                    />
                </FormLine>
                <NamesetWordsContainer values={namesetData.Values} valuesAlternative={namesetData.ValuesAlternative} />
            </div>
        </div>
    );
};
