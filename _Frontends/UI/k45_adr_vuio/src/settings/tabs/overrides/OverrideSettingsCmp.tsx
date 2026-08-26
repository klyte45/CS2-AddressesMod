import {
    AdrCitywideSettings,
    DistrictListItem,
    DistrictRelativeService,
    NamesetService,
    nameToString,
    NamingRulesService,
    SimpleNameEntry,
} from "@klyte45/adr-commons";
import { ListWithContentTab } from "@klyte45/vuio-commons";
import { useEffect, useMemo, useState } from "react";
import { translate } from "../../translate";
import { AddressFormatTab } from "./AddressFormatTab";
import { OverrideCitizenTab } from "./OverrideCitizenTab";
import { OverrideRoadsDistrictsTab } from "./OverrideRoadsDistrictsTab";
import { defaultSetting, defaultSettingRoadByDistrict, OverrideTabs } from "./overrideDefaults";
import { VehiclePlateControllerComponent } from "./VehiclePlateControllerComponent";
import { VehicleSerialControllerComponent } from "./VehicleSerialControllerComponent";
import styles from "../../styles/overrideSettings.module.scss";

type ListItem =
    | { section: string; displayName?: undefined }
    | { displayName: string; value: string };

export const OverrideSettingsCmp = () => {
    const [currentSettings, setCurrentSettings] = useState({} as AdrCitywideSettings);
    const [districts, setDistricts] = useState([] as DistrictListItem[]);
    const [simpleFiles, setSimpleFiles] = useState([] as SimpleNameEntry[]);
    const [innerContextSimpleFiles, setInnerContextSimpleFiles] = useState([] as SimpleNameEntry[]);
    const [indexedSimpleFiles, setIndexedSimpleFiles] = useState({} as Record<string, SimpleNameEntry>);
    const [currentTab, setCurrentTab] = useState<OverrideTabs>(OverrideTabs.Citizen);

    useEffect(() => {
        NamesetService.listCityNamesets().then((x) => listFiles(x));
        NamesetService.doOnCityNamesetsUpdated(() => NamesetService.listCityNamesets().then((x) => listFiles(x)));
        return () => NamesetService.offCityNamesetsUpdated();
    }, []);

    useEffect(() => {
        getSettings();
        NamingRulesService.onCityDataReloaded(() => { getSettings(); });
        listDistricts();
        DistrictRelativeService.onDistrictChanged(() => listDistricts());
        return () => {
            DistrictRelativeService.offDistrictChanged();
            NamingRulesService.offCityDataReloaded();
        };
    }, []);

    const getSettings = async () => {
        setCurrentSettings(await NamingRulesService.getCurrentCitywideSettings());
    };

    const listDistricts = async () => {
        const districtNames = (await DistrictRelativeService.listAllDistricts())
            ?.sort((a, b) => (nameToString(a.Name) ?? "").localeCompare(nameToString(b.Name) ?? "", undefined, { sensitivity: "base" }));
        setDistricts(districtNames ?? []);
    };

    const listFiles = async (cityNamesets: SimpleNameEntry[]) => {
        const files = cityNamesets.map((x) => { x.Values = []; return x; })
            .sort((a, b) => a.Name.localeCompare(b.Name, undefined, { sensitivity: "base" }));
        const generalSimpleFiles = [defaultSetting()].concat(files);
        const indexed = generalSimpleFiles.reduce((p, n) => {
            p[n.IdString as string] = n;
            return p;
        }, {} as Record<string, SimpleNameEntry>);
        setSimpleFiles(generalSimpleFiles);
        setIndexedSimpleFiles(indexed);
        setInnerContextSimpleFiles([defaultSettingRoadByDistrict(), ...files]);
    };

    const listItems = useMemo((): ListItem[] => [
        { displayName: translate("overrideSettings.tab.Citizen"), value: OverrideTabs.Citizen },
        { displayName: translate("overrideSettings.tab.RoadsDistricts"), value: OverrideTabs.RoadsDistricts },
        { displayName: translate("overrideSettings.tab.AddressFormat"), value: OverrideTabs.AddressFormat },
        { section: translate("overrideSettings.subCategory.vehicleIdentifiers") },
        { displayName: translate("overrideSettings.tab.RoadPlates"), value: OverrideTabs.RoadPlates },
        { displayName: translate("overrideSettings.tab.RailPlates"), value: OverrideTabs.RailPlates },
        { displayName: translate("overrideSettings.tab.WaterPlates"), value: OverrideTabs.WaterPlates },
        { displayName: translate("overrideSettings.tab.AirPlates"), value: OverrideTabs.AirPlates },
        { section: translate("overrideSettings.subCategory.roadVehicleSerial") },
        { displayName: translate("overrideSettings.tab.BusSerial"), value: OverrideTabs.BusSerial },
        { displayName: translate("overrideSettings.tab.TaxiSerial"), value: OverrideTabs.TaxiSerial },
        { displayName: translate("overrideSettings.tab.PoliceSerial"), value: OverrideTabs.PoliceSerial },
        { displayName: translate("overrideSettings.tab.FiretruckSerial"), value: OverrideTabs.FiretruckSerial },
        { displayName: translate("overrideSettings.tab.AmbulanceSerial"), value: OverrideTabs.AmbulanceSerial },
        { displayName: translate("overrideSettings.tab.GarbageSerial"), value: OverrideTabs.GarbageSerial },
        { displayName: translate("overrideSettings.tab.PostalSerial"), value: OverrideTabs.PostalSerial },
        { displayName: translate("overrideSettings.tab.DeathcareSerial"), value: OverrideTabs.DeathcareSerial },
    ], []);

    const content = (() => {
        switch (currentTab) {
            case OverrideTabs.Citizen:
                return <OverrideCitizenTab simpleFiles={simpleFiles} indexedSimpleFiles={indexedSimpleFiles} currentSettings={currentSettings} />;
            case OverrideTabs.RoadsDistricts:
                return (
                    <OverrideRoadsDistrictsTab
                        simpleFiles={simpleFiles}
                        indexedSimpleFiles={indexedSimpleFiles}
                        currentSettings={currentSettings}
                        innerContextSimpleFiles={innerContextSimpleFiles}
                        districts={districts}
                    />
                );
            case OverrideTabs.AddressFormat:
                return <AddressFormatTab currentSettings={currentSettings} />;
            case OverrideTabs.RoadPlates: return <VehiclePlateControllerComponent type="road" />;
            case OverrideTabs.WaterPlates: return <VehiclePlateControllerComponent type="water" />;
            case OverrideTabs.AirPlates: return <VehiclePlateControllerComponent type="air" />;
            case OverrideTabs.RailPlates: return <VehiclePlateControllerComponent type="rail" />;
            case OverrideTabs.BusSerial: return <VehicleSerialControllerComponent type="bus" />;
            case OverrideTabs.TaxiSerial: return <VehicleSerialControllerComponent type="taxi" />;
            case OverrideTabs.PoliceSerial: return <VehicleSerialControllerComponent type="police" />;
            case OverrideTabs.FiretruckSerial: return <VehicleSerialControllerComponent type="firetruck" />;
            case OverrideTabs.AmbulanceSerial: return <VehicleSerialControllerComponent type="ambulance" />;
            case OverrideTabs.GarbageSerial: return <VehicleSerialControllerComponent type="garbage" />;
            case OverrideTabs.PostalSerial: return <VehicleSerialControllerComponent type="postal" />;
            case OverrideTabs.DeathcareSerial: return <VehicleSerialControllerComponent type="deathcare" />;
            default: return null;
        }
    })();

    if (!currentSettings) return null;

    return (
        <div className={styles.overrideRoot}>
            <div className={styles.header}>
                <h2>{translate("overrideSettings.title")}</h2>
                <div className={styles.subtitle}>{translate("overrideSettings.subtitle")}</div>
            </div>
            <ListWithContentTab
                className={styles.overrideNav}
                listItems={listItems}
                selectedKey={currentTab}
                onChangeSelection={(key) => setCurrentTab(key as OverrideTabs)}
            >
                <div className={styles.overrideBody}>{content}</div>
            </ListWithContentTab>
        </div>
    );
};
