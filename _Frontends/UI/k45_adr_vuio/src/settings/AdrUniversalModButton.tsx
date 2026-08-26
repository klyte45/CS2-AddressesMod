import { VanillaComponentResolver } from "@klyte45/vuio-commons";
import { AdrSettingsPanelId } from "./AdrSettingsPanelId";
import styles from "./AdrUniversalModButton.module.scss";

export const AdrUniversalModButton = () => {
    const openSettings = () => {
        VanillaComponentResolver.instance.toggleGamePanel(AdrSettingsPanelId);
    };

    return (
        <button type="button" className={styles.modButton} onClick={openSettings}>
            <img className={styles.icon} src="coui://adr.k45/UI/images/ADR.svg" alt="" />
            <div className={styles.text}>
                <div className={styles.title}>Addresses & Names</div>
                <div className={styles.description}>Mod for CS2</div>
            </div>
        </button>
    );
};
