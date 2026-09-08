import { RegionCityEditingDTO, RegionService } from "@klyte45/adr-commons";
import { ColorUtils, FormLine, LocElementType, VanillaComponentResolver, VanillaWidgets } from "@klyte45/vuio-commons";
import { DropdownItem } from "cs2/bindings";
import { useEffect, useMemo, useState } from "react";
import { translate } from "../../../translate";
import shared from "../../../styles/regionEditor.module.scss";
import { GenericListing, ListItemData } from "./_GenericListing";

const REGION_CITY_EDITOR = "regionCityEditor";

export function RegionCitiesManagement() {
    const [selectedCity, setSelectedCity] = useState<Partial<RegionCityEditingDTO>>();
    const [reloadToken, setReloadToken] = useState(0);

    if (selectedCity) {
        return (
            <RegionCityRegisterForm
                selectedCity={selectedCity}
                setSelectedCity={setSelectedCity}
                onCancel={() => setSelectedCity(undefined)}
                onSave={async () => {
                    await RegionService.saveRegionCity(selectedCity as RegionCityEditingDTO);
                    setSelectedCity(undefined);
                    setReloadToken((x) => x + 1);
                }}
            />
        );
    }

    return <RegionCityListing onSelectItem={setSelectedCity} reloadToken={reloadToken} />;
}

type RegionCityListProps = {
    onSelectItem: (city: Partial<RegionCityEditingDTO>) => void;
    reloadToken: number;
};

function RegionCityListing({ onSelectItem, reloadToken }: RegionCityListProps) {
    const [cities, setCities] = useState([] as RegionCityEditingDTO[]);
    const [airFilter, setAirFilter] = useState(true);
    const [landFilter, setLandFilter] = useState(true);
    const [waterFilter, setWaterFilter] = useState(true);

    useEffect(() => {
        RegionService.listAllRegionCities().then((items) => setCities(items.sort((a, b) => a.name.localeCompare(b.name))));
    }, [reloadToken]);

    const items: ListItemData[] = cities.filter((x) =>
        (airFilter && x.reachableByAir)
        || (landFilter && x.reachableByLand)
        || (waterFilter && x.reachableByWater)
        || (!x.reachableByAir && !x.reachableByLand && !x.reachableByWater),
    ).map((x) => ({
        key: x.entity.Index,
        title: x.name,
        subTitle: (
            <div className={shared.reachabilityRow}>
                <span>{x.centerAzimuth.toFixed(1)}°</span>
                {x.reachableByLand && <div className={`${shared.iconReachable} ${shared.land}`} />}
                {x.reachableByWater && <div className={`${shared.iconReachable} ${shared.water}`} />}
                {x.reachableByAir && <div className={`${shared.iconReachable} ${shared.air}`} />}
            </div>
        ),
        color: x.mapColor,
        actions: [
            {
                label: translate(`${REGION_CITY_EDITOR}.editBtn`),
                onClick: () => onSelectItem(x),
            },
            {
                label: translate(`${REGION_CITY_EDITOR}.removeBtn`),
                className: "negativeBtn",
                onClick: async () => {
                    await RegionService.removeRegionCity(x.entity);
                    setCities((prev) => prev.filter((c) => c.entity.Index !== x.entity.Index));
                },
            },
        ],
    }));

    const titleEnd = (
        <div className={`${shared.filters} k45_btnHost`}>
            <button
                type="button"
                data-tooltip={translate(`${REGION_CITY_EDITOR}.reachableByLand`)}
                className={`${shared.filterButton} ${shared.land} ${landFilter ? shared.selected : ""}`}
                onClick={() => setLandFilter(!landFilter)}
            />
            <button
                type="button"
                data-tooltip={translate(`${REGION_CITY_EDITOR}.reachableByWater`)}
                className={`${shared.filterButton} ${shared.water} ${waterFilter ? shared.selected : ""}`}
                onClick={() => setWaterFilter(!waterFilter)}
            />
            <button
                type="button"
                data-tooltip={translate(`${REGION_CITY_EDITOR}.reachableByAir`)}
                className={`${shared.filterButton} ${shared.air} ${airFilter ? shared.selected : ""}`}
                onClick={() => setAirFilter(!airFilter)}
            />
        </div>
    );

    return (
        <GenericListing
            title={translate(`${REGION_CITY_EDITOR}.titleList`)}
            titleEnd={titleEnd}
            items={items}
            noItemsMessage={translate(`${REGION_CITY_EDITOR}.noCitiesRegisteredMessage`)}
            onAdd={() => onSelectItem({})}
            addBtnLabel={translate(`${REGION_CITY_EDITOR}.addBtn`)}
        />
    );
}

type RegionCityFormProps = {
    selectedCity: Partial<RegionCityEditingDTO>;
    setSelectedCity: (city: Partial<RegionCityEditingDTO> | undefined) => void;
    onCancel: () => void;
    onSave: () => void;
};

function RegionCityRegisterForm({ onCancel, onSave, selectedCity, setSelectedCity }: RegionCityFormProps) {
    const VW = VanillaWidgets.instance;
    const editorModule = VW.editorItemModule;
    const NumberDD = VW.DropdownField<number>();
    const FloatInput = VW.FloatInputStandalone;
    const Checkbox = VW.Checkbox;
    const ColorField = VanillaComponentResolver.instance.ColorField;
    const noFocus = VanillaComponentResolver.instance.FOCUS_DISABLED;

    const directionPresets = useMemo(() => [
        { value: 0, position: 0, label: translate(`${REGION_CITY_EDITOR}.directionPresetNorth`) },
        { value: 1, position: 45, label: translate(`${REGION_CITY_EDITOR}.directionPresetNortheast`) },
        { value: 2, position: 90, label: translate(`${REGION_CITY_EDITOR}.directionPresetEast`) },
        { value: 3, position: 135, label: translate(`${REGION_CITY_EDITOR}.directionPresetSoutheast`) },
        { value: 4, position: 180, label: translate(`${REGION_CITY_EDITOR}.directionPresetSouth`) },
        { value: 5, position: 225, label: translate(`${REGION_CITY_EDITOR}.directionPresetSouthwest`) },
        { value: 6, position: 270, label: translate(`${REGION_CITY_EDITOR}.directionPresetWest`) },
        { value: 7, position: 315, label: translate(`${REGION_CITY_EDITOR}.directionPresetNorthwest`) },
        { value: -1, position: -1, label: translate(`${REGION_CITY_EDITOR}.directionPresetCustom`) },
    ], []);

    const directionItems: DropdownItem<number>[] = directionPresets.map((x) => ({
        value: x.value,
        displayName: { __Type: LocElementType.String, value: x.label },
    }));

    const [directionPreset, setDirectionPreset] = useState<number>(-1);

    useEffect(() => {
        if (selectedCity.centerAzimuth !== undefined && selectedCity.degreesLeft === 22.5 && selectedCity.degreesRight === 22.5) {
            const preset = directionPresets.find((x) => x.position === selectedCity.centerAzimuth);
            setDirectionPreset(preset ? preset.value : -1);
        } else if (selectedCity.centerAzimuth === undefined && selectedCity.degreesLeft === undefined && selectedCity.degreesRight === undefined) {
            setDirectionPreset(0);
            setSelectedCity({
                ...selectedCity,
                centerAzimuth: 0,
                degreesLeft: 22.5,
                degreesRight: 22.5,
            });
        } else {
            setDirectionPreset(-1);
        }
    }, []);

    useEffect(() => {
        if (directionPreset >= 0) {
            const preset = directionPresets.find((x) => x.value === directionPreset);
            if (preset) {
                setSelectedCity({
                    ...selectedCity,
                    centerAzimuth: preset.position,
                    degreesLeft: 22.5,
                    degreesRight: 22.5,
                });
            }
        }
    }, [directionPreset]);

    const isValid = !!selectedCity.name
        && selectedCity.centerAzimuth !== undefined && selectedCity.centerAzimuth >= 0 && selectedCity.centerAzimuth <= 360
        && selectedCity.degreesLeft !== undefined && selectedCity.degreesLeft >= 0 && selectedCity.degreesLeft <= 180
        && selectedCity.degreesRight !== undefined && selectedCity.degreesRight >= 0 && selectedCity.degreesRight <= 180
        && !!selectedCity.mapColor && /^#[0-9A-Fa-f]{6}$/.test(selectedCity.mapColor)
        && !!(selectedCity.reachableByLand || selectedCity.reachableByWater || selectedCity.reachableByAir);

    return (
        <div className={`${shared.editorForm} k45_btnHost`}>
            <FormLine title={translate(`${REGION_CITY_EDITOR}.name`)}>
                <VW.StringInputField
                    className={editorModule.input}
                    value={selectedCity.name ?? ""}
                    onChange={(value) => setSelectedCity({ ...selectedCity, name: value })}
                />
            </FormLine>
            <FormLine title={translate(`${REGION_CITY_EDITOR}.directionPreset`)}>
                <NumberDD
                    items={directionItems}
                    value={directionPreset}
                    onChange={(value) => setDirectionPreset(value)}
                />
            </FormLine>
            <FormLine title={translate(`${REGION_CITY_EDITOR}.centerPosition`)}>
                <FloatInput
                    className={`${editorModule.input} ${shared.numberInput}`}
                    value={selectedCity.centerAzimuth ?? 0}
                    min={0}
                    max={360}
                    fractionDigits={1}
                    disabled={directionPreset !== -1}
                    onChange={(value) => setSelectedCity({ ...selectedCity, centerAzimuth: value })}
                />
            </FormLine>
            <FormLine
                title={translate(`${REGION_CITY_EDITOR}.offsetAzimuthLR`)}
                subtitle={translate(`${REGION_CITY_EDITOR}.offsetAzimuthLRSubtitle`)}
            >
                <div className={shared.positionRow}>
                    <FloatInput
                        className={`${editorModule.input} ${shared.numberInput}`}
                        value={selectedCity.degreesLeft ?? 0}
                        min={0}
                        max={180}
                        fractionDigits={1}
                        disabled={directionPreset !== -1}
                        onChange={(value) => setSelectedCity({ ...selectedCity, degreesLeft: value })}
                    />
                    <FloatInput
                        className={`${editorModule.input} ${shared.numberInput}`}
                        value={selectedCity.degreesRight ?? 0}
                        min={0}
                        max={180}
                        fractionDigits={1}
                        disabled={directionPreset !== -1}
                        onChange={(value) => setSelectedCity({ ...selectedCity, degreesRight: value })}
                    />
                </div>
            </FormLine>
            <FormLine title={translate(`${REGION_CITY_EDITOR}.cityColor`)}>
                <ColorField
                    //className={editorModule.input}
                    focusKey={noFocus}
                    value={ColorUtils.toColor01(selectedCity.mapColor ?? "#ffffff")}
                    alpha={false}
                    popupDirection="down"
                    hideHint
                    colorWheel
                    hexInput
                    onChange={(value) => setSelectedCity({ ...selectedCity, mapColor: ColorUtils.toRGBHex(value) as `#${string}` })}
                />
            </FormLine>
            <FormLine title={translate(`${REGION_CITY_EDITOR}.reachableByLand`)}>
                <Checkbox
                    checked={!!selectedCity.reachableByLand}
                    onChange={(value) => setSelectedCity({ ...selectedCity, reachableByLand: value })}
                />
            </FormLine>
            <FormLine title={translate(`${REGION_CITY_EDITOR}.reachableByWater`)}>
                <Checkbox
                    checked={!!selectedCity.reachableByWater}
                    onChange={(value) => setSelectedCity({ ...selectedCity, reachableByWater: value })}
                />
            </FormLine>
            <FormLine title={translate(`${REGION_CITY_EDITOR}.reachableByAir`)}>
                <Checkbox
                    checked={!!selectedCity.reachableByAir}
                    onChange={(value) => setSelectedCity({ ...selectedCity, reachableByAir: value })}
                />
            </FormLine>
            <div className={shared.formGap} />
            <div className={shared.bottomActions}>
                <button type="button" className="positiveBtn" disabled={!isValid} onClick={onSave}>{translate(`${REGION_CITY_EDITOR}.save`)}</button>
                <button type="button" className="negativeBtn" onClick={onCancel}>{translate(`${REGION_CITY_EDITOR}.cancel`)}</button>
            </div>
        </div>
    );
}
