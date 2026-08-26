import { ExtendedSimpleNameEntry, SimpleNameEntry } from "@klyte45/adr-commons";
import { FormLine, VanillaWidgets } from "@klyte45/vuio-commons";
import { useEffect, useState } from "react";
import { translate } from "../../translate";
import styles from "../../styles/namesetLibrary.module.scss";
import { entryToText, parseNamesetText } from "./namesetUtils";

type Props = {
    entryData: SimpleNameEntry;
    onBack: () => void;
    onOk: (x: Omit<ExtendedSimpleNameEntry, "ChecksumString">) => void;
};

export const NamesetEditorCmp = ({ entryData, onBack, onOk }: Props) => {
    const VW = VanillaWidgets.instance;
    const StringInput = VW.StringInputField;
    const Scrollable = VW.EditorScrollable;
    const editorModule = VW.editorItemModule;

    const [namesetData, setNamesetData] = useState(entryData);
    const [textValue, setTextValue] = useState(() => entryToText(entryData));

    useEffect(() => {
        setNamesetData(entryData);
        setTextValue(entryToText(entryData));
    }, [entryData]);

    const commitText = () => {
        setNamesetData(parseNamesetText(textValue, namesetData.Name));
    };

    return (
        <div className={`${styles.screenRoot} k45_btnHost`}>
            <div className={styles.screenBody}>
                <div className={styles.namesetTitle}>{namesetData.Name.split("/").pop()}</div>
                <div className={styles.divider} />
                <FormLine title={translate("namesetsImport.pathToNameset")}>
                    <StringInput
                        className={editorModule.input}
                        value={namesetData.Name}
                        onChange={(x) => setNamesetData({ ...namesetData, Name: x })}
                    />
                </FormLine>
                <Scrollable>
                    <StringInput
                        className={`${editorModule.input} ${editorModule.multilineText} ${styles.textEditor}`}
                        value={textValue}
                        multiline={Math.max(6, namesetData.Values.length + 2)}
                        onChange={setTextValue}
                        onChangeEnd={commitText}
                    />
                </Scrollable>
            </div>
            <div className={styles.bottomBar}>
                <button type="button" className="negativeBtn" onClick={onBack}>{translate("namesetEditor.cancel")}</button>
                <button type="button" className="positiveBtn" onClick={() => onOk(parseNamesetText(textValue, namesetData.Name))}>{translate("namesetEditor.save")}</button>
            </div>
        </div>
    );
};
