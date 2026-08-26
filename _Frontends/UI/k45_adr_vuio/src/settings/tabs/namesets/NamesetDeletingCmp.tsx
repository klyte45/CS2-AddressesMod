import { ExtendedSimpleNameEntry } from "@klyte45/adr-commons";
import { translate } from "../../translate";
import styles from "../../styles/namesetLibrary.module.scss";
import { NamesetWordsContainer } from "./NamesetWordsContainer";

type Props = {
    namesetData: ExtendedSimpleNameEntry;
    onBack: () => void;
    onOk: (namesetData: ExtendedSimpleNameEntry) => void;
};

export const NamesetDeletingCmp = ({ namesetData, onBack, onOk }: Props) => (
    <div className={`${styles.screenRoot} k45_btnHost`}>
        <div className={styles.screenBody}>
            <NamesetWordsContainer values={namesetData.Values} valuesAlternative={namesetData.ValuesAlternative} />
        </div>
        <div className={styles.bottomBar}>
            <button type="button" className="negativeBtn" onClick={() => onOk(namesetData)}>{translate("namesetDelete.yes")}</button>
            <button type="button" className="darkestBtn" onClick={onBack}>{translate("namesetDelete.no")}</button>
        </div>
    </div>
);
