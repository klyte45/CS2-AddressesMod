import { ErrorBoundary, VanillaComponentResolver } from "@klyte45/vuio-commons";
import { FocusDisabled } from "cs2/input";
import { Portal } from "cs2/ui";
import { AdrSettingsShell, GamePanel, gamePanelTheme } from "./AdrSettingsPanel";
import styles from "./AdrSettingsPanel.module.scss";

type AdrEditorSettingsPopupProps = {
    isOpen: boolean;
    onClose: () => void;
};

export const AdrEditorSettingsPopup = ({ isOpen, onClose }: AdrEditorSettingsPopupProps) => {
    const PanelTitleBar = VanillaComponentResolver.instance.PanelTitleBar;

    if (!isOpen) {
        return null;
    }

    return (
        <AdrSettingsShell
            titleBar={(
                <PanelTitleBar icon="coui://adr.k45/UI/images/ADR.svg">
                    Addresses & Names
                </PanelTitleBar>
            )}
        >
            {({ header, body }) => (
                <Portal>
                    <div className={styles.editorPopupContainer}>
                        <GamePanel
                            className={[styles.adrSettingsPanel, styles.editorSettingsPanel].join(" ")}
                            theme={gamePanelTheme}
                            header={header}
                            onClose={onClose}
                        >
                            <FocusDisabled>
                                <ErrorBoundary>
                                    <div className={styles.panelBody}>{body}</div>
                                </ErrorBoundary>
                            </FocusDisabled>
                        </GamePanel>
                    </div>
                </Portal>
            )}
        </AdrSettingsShell>
    );
};
