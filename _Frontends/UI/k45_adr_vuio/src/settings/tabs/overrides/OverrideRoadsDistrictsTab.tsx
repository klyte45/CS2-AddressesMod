import { AdrCitywideSettings, DistrictListItem, DistrictRelativeService, NamingRulesService, SimpleNameEntry, nameToString } from "@klyte45/adr-commons";
import { FormLine, LocElementType, VanillaWidgets } from "@klyte45/vuio-commons";
import { DropdownItem } from "cs2/bindings";
import { useEffect, useState } from "react";
import { translate } from "../../translate";
import { defaultSetting, defaultSettingRoadByDistrict } from "./overrideDefaults";
import { NamesetSelectLine } from "./NamesetSelectLine";

type Props = {
    simpleFiles: SimpleNameEntry[];
    indexedSimpleFiles: Record<string, SimpleNameEntry>;
    currentSettings: AdrCitywideSettings;
    innerContextSimpleFiles: SimpleNameEntry[];
    districts: DistrictListItem[];
};

export const OverrideRoadsDistrictsTab = ({
    simpleFiles,
    indexedSimpleFiles,
    currentSettings,
    innerContextSimpleFiles,
    districts,
}: Props) => {
    const VW = VanillaWidgets.instance;
    const StringDD = VW.DropdownField<string>();
    const [selectedDistrict, setSelectedDistrict] = useState<DistrictListItem | undefined>(undefined);

    useEffect(() => {
        setSelectedDistrict((prev) => (prev ? districts.find((x) => x.Entity.Index == prev.Entity.Index) : undefined));
    }, [districts]);

    const districtItems: DropdownItem<string>[] = districts.map((d) => ({
        value: d.Entity.Index.toString(),
        displayName: { __Type: LocElementType.String, value: nameToString(d.Name) ?? "" },
    }));

    return <>
        <NamesetSelectLine
            title={translate("overrideSettings.districtsFile")}
            options={simpleFiles}
            value={indexedSimpleFiles[currentSettings.DefaultDistrictNameOverridesStr] ?? defaultSetting()}
            onChange={(x) => NamingRulesService.setDefaultDistrictNameOverridesStr(x.IdString)}
        />
        <NamesetSelectLine
            title={translate("overrideSettings.roadsFile")}
            options={simpleFiles}
            value={indexedSimpleFiles[currentSettings.DefaultRoadNameOverridesStr] ?? defaultSetting()}
            onChange={(x) => NamingRulesService.setDefaultRoadNameOverridesStr(x.IdString)}
        />
        <FormLine title={translate("overrideSettings.useRoadNameAsStationName")}>
            <VW.Checkbox
                checked={!!currentSettings?.roadNameAsNameStation}
                onChange={(x) => NamingRulesService.setRoadNameAsNameStation(x)}
            />
        </FormLine>
        <FormLine title={translate("overrideSettings.useRoadNameAsCargoStationName")}>
            <VW.Checkbox
                checked={!!currentSettings?.roadNameAsNameCargoStation}
                onChange={(x) => NamingRulesService.setRoadNameAsNameCargoStation(x)}
            />
        </FormLine>
        <h2>{translate("overrideSettings.perDistrictRoadsFile")}</h2>
        <FormLine title={translate("overrideSettings.perDistrictRoadsFile")}>
            <StringDD
                items={districtItems}
                value={selectedDistrict?.Entity.Index.toString()}
                onChange={(id) => setSelectedDistrict(districts.find((d) => d.Entity.Index.toString() === id))}
            />
        </FormLine>
        {selectedDistrict && (
            <NamesetSelectLine
                title={translate("overrideSettings.roadsFile")}
                options={innerContextSimpleFiles}
                value={
                    innerContextSimpleFiles.find((x) => x.IdString == selectedDistrict.CurrentValue)
                    ?? defaultSettingRoadByDistrict()
                }
                onChange={async (x) => {
                    await DistrictRelativeService.setRoadNamesFile(selectedDistrict.Entity, x.IdString);
                }}
            />
        )}
    </>;
};
