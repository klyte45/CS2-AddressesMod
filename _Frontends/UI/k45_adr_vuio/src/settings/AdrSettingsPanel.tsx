import { AdrMainService } from "@klyte45/adr-commons";
import { ErrorBoundary, ListWithContentTab, VanillaComponentResolver } from "@klyte45/vuio-commons";
import { Panel } from "cs2/ui";
import { useEffect, useMemo, useState } from "react";
import { translate } from "./translate";
import styles from "./AdrSettingsPanel.module.scss";

type RootTabKey = "namesets" | "overrides" | "roadPrefix" | "region";

type AdrSettingsPanelProps = {
    onClose: () => void;
};

const tabPlaceholders: Record<RootTabKey, string> = {
    namesets: "Namesets — Sprint 3",
    overrides: "Overrides — Sprint 2",
    roadPrefix: "Road prefix — Sprint 4",
    region: "Region — Sprint 5–6",
};

export const AdrSettingsPanel = ({ onClose }: AdrSettingsPanelProps) => {
    const [isReady, setIsReady] = useState(false);
    const [selectedTab, setSelectedTab] = useState<RootTabKey>("namesets");

    useEffect(() => {
        let cancelled = false;
        AdrMainService.isCityOrEditorLoaded().then((ready) => {
            if (!cancelled) {
                setIsReady(ready);
            }
        });
        return () => {
            cancelled = true;
        };
    }, []);

    const listItems = useMemo(() => ([
        { displayName: translate("namesetManagement.title"), value: "namesets" as const },
        { displayName: translate("overrideSettings.title"), value: "overrides" as const },
        { displayName: translate("roadPrefixSettings.title"), value: "roadPrefix" as const },
        { displayName: translate("regionSettings.title"), value: "region" as const },
    ]), []);

    const VR = VanillaComponentResolver.instance;
    const layoutClass = VR.gameMainScreenModule.centerPanelLayout;
    const PanelTitleBar = VR.PanelTitleBar;

    const header = (
        <PanelTitleBar icon="coui://adr.k45/UI/images/ADR.svg" onCloseOverride={onClose}>
            Addresses & Names
        </PanelTitleBar>
    );

    const body = !isReady
        ? <div className={styles.cityGate}>{translate("loadACityWarningH2")}</div>
        : (
            <ListWithContentTab
                className={styles.settingsNav}
                listItems={listItems}
                selectedKey={selectedTab}
                onChangeSelection={(key) => setSelectedTab(key as RootTabKey)}
            >
                <div className={styles.tabPlaceholder}>{tabPlaceholders[selectedTab]}</div>
            </ListWithContentTab>
        );

    return (
        <div className={layoutClass}>
            <Panel className={styles.adrSettingsPanel} header={header} onClose={onClose}>
                <ErrorBoundary>
                    <div className={styles.panelBody}>{body}</div>
                </ErrorBoundary>
            </Panel>
        </div>
    );
};

export { AdrSettingsPanelId } from "./AdrSettingsPanelId";
