import { AdrMainService } from "@klyte45/adr-commons";
import { ErrorBoundary, VanillaComponentResolver } from "@klyte45/vuio-commons";
import { FocusDisabled } from "cs2/input";
import { getModule } from "cs2/modding";
import { useEffect, useMemo, useState, type ComponentType, type ReactNode } from "react";
import { translate } from "./translate";
import { CityNamesetLibraryCmp } from "./tabs/namesets/CityNamesetLibraryCmp";
import { OverrideSettingsCmp } from "./tabs/overrides/OverrideSettingsCmp";
import { RegionEditor } from "./tabs/region/RegionEditor";
import { RoadPrefixCmp } from "./tabs/roadPrefix/RoadPrefixCmp";
import styles from "./AdrSettingsPanel.module.scss";

type RootTabKey = "namesets" | "overrides" | "roadPrefix" | "region";

type AdrSettingsPanelProps = {
    onClose: () => void;
};

type GamePanelProps = {
    header?: ReactNode;
    theme?: Record<string, string>;
    className?: string;
    onClose?: () => void;
    children?: ReactNode;
};

const rootTabs: RootTabKey[] = ["namesets", "overrides", "roadPrefix", "region"];

const tabPlaceholders: Partial<Record<RootTabKey, string>> = {
    roadPrefix: "Road prefix — Sprint 4",
    region: "Region — Sprint 5–6",
};

/** Same Panel instance as PanelTitleBar so PanelContext (theme + onClose) matches. */
const GamePanel = getModule("game-ui/common/panel/panel.tsx", "Panel") as ComponentType<GamePanelProps>;
/** Overview panels (Economy / City Info / Transportation) use this chrome. */
const gamePanelTheme = getModule("game-ui/common/panel/themes/iceflake-panel.module.scss", "classes") as Record<string, string>;

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

    const tabLabels = useMemo(() => ({
        namesets: translate("namesetManagement.title"),
        overrides: translate("overrideSettings.title"),
        roadPrefix: translate("roadPrefixSettings.title"),
        region: translate("regionSettings.title"),
    }), []);

    const VR = VanillaComponentResolver.instance;
    const layoutClass = VR.gameMainScreenModule.centerPanelLayout;
    const PanelTitleBar = VR.PanelTitleBar;
    const TabBar = VR.TabBar;
    const Tab = VR.Tab;
    const TabNav = VR.TabNav;

    const selectTab = (tab: string) => {
        setSelectedTab(tab as RootTabKey);
    };

    const header = (
        <FocusDisabled>
            <PanelTitleBar icon="coui://adr.k45/UI/images/ADR.svg">
                Addresses & Names
            </PanelTitleBar>
            <TabBar>
                {rootTabs.map((tab) => (
                    <Tab
                        key={tab}
                        id={tab}
                        selectedId={selectedTab}
                        onSelect={selectTab}
                    >
                        {tabLabels[tab]}
                    </Tab>
                ))}
            </TabBar>
        </FocusDisabled>
    );

    const tabBody = (() => {
        if (selectedTab === "namesets") {
            return <CityNamesetLibraryCmp />;
        }
        if (selectedTab === "overrides") {
            return <OverrideSettingsCmp />;
        }
        if (selectedTab === "roadPrefix") {
            return <RoadPrefixCmp />;
        }
        if (selectedTab === "region") {
            return <RegionEditor />;
        }
        return <div className={styles.tabPlaceholder}>{tabPlaceholders[selectedTab]}</div>;
    })();

    const body = !isReady
        ? <div className={styles.cityGate}>{translate("loadACityWarningH2")}</div>
        : (
            <TabNav tabs={rootTabs} selectedTab={selectedTab} onSelect={selectTab as any}>
                {tabBody}
            </TabNav>
        );

    return (
        <div className={layoutClass}>
            <GamePanel
                className={styles.adrSettingsPanel}
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
    );
};

export { AdrSettingsPanelId } from "./AdrSettingsPanelId";
