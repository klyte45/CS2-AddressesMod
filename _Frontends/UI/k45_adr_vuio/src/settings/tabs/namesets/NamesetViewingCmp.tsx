import { ExtendedSimpleNameEntry } from "@klyte45/adr-commons";
import { translate } from "../../translate";
import styles from "../../styles/namesetLibrary.module.scss";
import { NamesetWordsContainer } from "./NamesetWordsContainer";

type Props = {
    namesetData: ExtendedSimpleNameEntry;
    onBack: () => void;
};

export const NamesetViewingCmp = ({ namesetData, onBack }: Props) => (
    <div className={`${styles.screenRoot} k45_btnHost`}>
        <div className={styles.toolbar}>
            <button type="button" className="negativeBtn" onClick={onBack}>{translate("namesetsLibrary.back")}</button>
        </div>
        <div className={styles.screenBody}>
            <div className={styles.namesetTitle}>{namesetData.Name}</div>
            <NamesetWordsContainer values={namesetData.Values} valuesAlternative={namesetData.ValuesAlternative} />
        </div>
    </div>
);
