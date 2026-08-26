import { VanillaComponentResolver } from "@klyte45/vuio-commons";
import { FocusDisabled } from "cs2/input";
import { AdrSettingsPanelId } from "./AdrSettingsPanelId";
import styles from "./AdrUniversalModButton.module.scss";

export const AdrUniversalModButton = () => {
    const openSettings = () => {
        VanillaComponentResolver.instance.toggleGamePanel(AdrSettingsPanelId);
    };

    return (
        <FocusDisabled>
            <button type="button" className={styles.modButton} onClick={openSettings}>
                <img className={styles.icon} src="coui://adr.k45/UI/images/ADR.svg" alt="" />
            </button>
        </FocusDisabled>
    );
};
