import { ListWithContentTab } from "@klyte45/vuio-commons";
import { useMemo, useState } from "react";
import { translate } from "../../translate";
import styles from "../../styles/regionEditor.module.scss";
import { HighwayRegisterManagement } from "./subpages/HighwayRegisterManagement";
import { RegionCitiesManagement } from "./subpages/RegionCitiesManagement";

type RegionTabKey = "highways" | "cities";

export const RegionEditor = () => {
    const [currentTab, setCurrentTab] = useState<RegionTabKey>("highways");

    const listItems = useMemo(() => [
        { displayName: translate("highwayRegisterEditor.tabTitle"), value: "highways" },
        { displayName: translate("regionCityEditor.tabTitle"), value: "cities" },
    ], []);

    const content = currentTab === "highways"
        ? <HighwayRegisterManagement />
        : <RegionCitiesManagement />;

    return (
        <ListWithContentTab
            className={styles.regionNav}
            listItems={listItems}
            selectedKey={currentTab}
            onChangeSelection={(key) => setCurrentTab(key as RegionTabKey)}
        >
            <div className={styles.regionBody}>
                {content}
            </div>
        </ListWithContentTab>
    );
};
