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

    // Locale keys match EUIS Cs2SideTabs: i18nTitlePrefix + tabId → overrideSettings.tabCitizen (no dot).
    const tabLabel = (tab: OverrideTabs) => translate(`overrideSettings.tab${tab}`);

    const listItems = useMemo((): ListItem[] => [
        { displayName: tabLabel(OverrideTabs.Citizen), value: OverrideTabs.Citizen },
        { displayName: tabLabel(OverrideTabs.RoadsDistricts), value: OverrideTabs.RoadsDistricts },
        { displayName: tabLabel(OverrideTabs.AddressFormat), value: OverrideTabs.AddressFormat },
        { section: translate("overrideSettings.subCategory.vehicleIdentifiers") },
        { displayName: tabLabel(OverrideTabs.RoadPlates), value: OverrideTabs.RoadPlates },
        { displayName: tabLabel(OverrideTabs.RailPlates), value: OverrideTabs.RailPlates },
        { displayName: tabLabel(OverrideTabs.WaterPlates), value: OverrideTabs.WaterPlates },
        { displayName: tabLabel(OverrideTabs.AirPlates), value: OverrideTabs.AirPlates },
        { section: translate("overrideSettings.subCategory.roadVehicleSerial") },
        { displayName: tabLabel(OverrideTabs.BusSerial), value: OverrideTabs.BusSerial },
        { displayName: tabLabel(OverrideTabs.TaxiSerial), value: OverrideTabs.TaxiSerial },
        { displayName: tabLabel(OverrideTabs.PoliceSerial), value: OverrideTabs.PoliceSerial },
        { displayName: tabLabel(OverrideTabs.FiretruckSerial), value: OverrideTabs.FiretruckSerial },
        { displayName: tabLabel(OverrideTabs.AmbulanceSerial), value: OverrideTabs.AmbulanceSerial },
        { displayName: tabLabel(OverrideTabs.GarbageSerial), value: OverrideTabs.GarbageSerial },
        { displayName: tabLabel(OverrideTabs.PostalSerial), value: OverrideTabs.PostalSerial },
        { displayName: tabLabel(OverrideTabs.DeathcareSerial), value: OverrideTabs.DeathcareSerial },
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
        <ListWithContentTab
            className={styles.overrideNav}
            listItems={listItems}
            selectedKey={currentTab}
            onChangeSelection={(key) => setCurrentTab(key as OverrideTabs)}
        >
            <div className={styles.overrideBody}>{content}</div>
        </ListWithContentTab>
    );
};
