import { translate } from '#utility/translate';
import { RegionCityEditingDTO, RegionService } from '@klyte45/adr-commons';
import { ColorRgbInput, Cs2CheckboxWithLine, Cs2FormBoundaries, Cs2FormLine, Cs2Select, Input, NumberSimpleInput } from '@klyte45/euis-components';
import { useEffect, useState } from 'react';
import { ListItemData, _GenericListing } from './_GenericListing';
import './RegionCitiesManagement.scss';


const REGION_CITY_EDITOR = "regionCityEditor";

type MainProps = {
    onCitiesChanged: () => void;
}

export function RegionCitiesManagement({ onCitiesChanged }: MainProps) {
    const [selectedCity, setSelectedCity] = useState<Partial<RegionCityEditingDTO>>();
    if (!selectedCity) {
        return <RegionCityListing onSelectItem={setSelectedCity} />;
    }

    return <RegionCityRegisterForm
        selectedCity={selectedCity}
        setSelectedCity={setSelectedCity}
        onCancel={() => setSelectedCity(undefined)}
        onSave={() => RegionService.saveRegionCity(selectedCity as RegionCityEditingDTO).then(() => {
            setSelectedCity(undefined);
            setTimeout(onCitiesChanged, 250); // Delay to ensure the UI updates properly
        })}
    />;
}

export default RegionCitiesManagement;

type RegionCityListProps = {
    onSelectItem: (city: Partial<RegionCityEditingDTO>) => void;
};

function RegionCityListing({ onSelectItem }: RegionCityListProps) {
    const [cities, setCities] = useState([] as RegionCityEditingDTO[]);

    useEffect(() => {
        RegionService.listAllRegionCities().then((x) => setCities(x.sort((a, b) => a.name.localeCompare(b.name))));
    }, []);

    const items: ListItemData<RegionCityEditingDTO>[] = cities.map((x) => ({
        key: x.entity.Index,
        title: x.name,
        subTitle: `${(x.centerAzimuth).toFixed(1)}°`,
        color: x.mapColor,
        actions: [
            {
                label: translate(`${REGION_CITY_EDITOR}.editBtn`),
                onClick: () => onSelectItem(x),
            },
        ],
        raw: x,
    }));

    return (
        <_GenericListing
            title={translate(`${REGION_CITY_EDITOR}.titleList`)}
            items={items}
            noItemsMessage={translate(`${REGION_CITY_EDITOR}.noCitiesRegisteredMessage`)}
            onAdd={() => onSelectItem({})}
            addBtnLabel={translate(`${REGION_CITY_EDITOR}.addBtn`)}
        />
    );
}

type RegionCityFormProps = {
    selectedCity: Partial<RegionCityEditingDTO>;
    setSelectedCity: (hw: Partial<RegionCityEditingDTO> | undefined) => void;
    onCancel: () => void;
    onSave: () => void;
};

function RegionCityRegisterForm({ onCancel, onSave, selectedCity, setSelectedCity }: RegionCityFormProps) {
    const directionPresets = [
        { value: 0, position: 0, label: translate(`${REGION_CITY_EDITOR}.directionPresetNorth`) },
        { value: 1, position: 45, label: translate(`${REGION_CITY_EDITOR}.directionPresetNortheast`) },
        { value: 2, position: 90, label: translate(`${REGION_CITY_EDITOR}.directionPresetEast`) },
        { value: 3, position: 135, label: translate(`${REGION_CITY_EDITOR}.directionPresetSoutheast`) },
        { value: 4, position: 180, label: translate(`${REGION_CITY_EDITOR}.directionPresetSouth`) },
        { value: 5, position: 225, label: translate(`${REGION_CITY_EDITOR}.directionPresetSouthwest`) },
        { value: 6, position: 270, label: translate(`${REGION_CITY_EDITOR}.directionPresetWest`) },
        { value: 7, position: 315, label: translate(`${REGION_CITY_EDITOR}.directionPresetNorthwest`) },
        { value: -1, position: -1, label: translate(`${REGION_CITY_EDITOR}.directionPresetCustom`) }
    ];

    // Stub for RegionCityRegisterForm

    const [directionPreset, setDirectionPreset] = useState<number>()

    useEffect(() => {
        if (selectedCity.centerAzimuth !== undefined && selectedCity.degreesLeft === 22.5 && selectedCity.degreesRight === 22.5) {
            const preset = directionPresets.find((x) => x.position === selectedCity.centerAzimuth);
            setDirectionPreset(preset ? preset.value : -1);
        } else {
            setDirectionPreset(-1);
        }
    }, []);

    useEffect(() => {
        if (selectedCity.centerAzimuth === undefined && selectedCity.degreesLeft === undefined && selectedCity.degreesRight === undefined) {
            setDirectionPreset(0);
            setSelectedCity({
                ...selectedCity,
                centerAzimuth: 0,
                degreesLeft: 22.5,
                degreesRight: 22.5,
            });
        } else if (directionPreset !== undefined && directionPreset >= 0 && directionPreset < directionPresets.length) {
            const preset = directionPresets[directionPreset];
            setSelectedCity({
                ...selectedCity,
                centerAzimuth: preset.position,
                degreesLeft: 22.5,
                degreesRight: 22.5,
            });
        }
    }, [directionPreset]);

    const isValid = selectedCity.name && selectedCity.name.length > 0
        && selectedCity.centerAzimuth !== undefined && selectedCity.centerAzimuth >= 0 && selectedCity.centerAzimuth <= 360
        && selectedCity.degreesLeft !== undefined && selectedCity.degreesLeft >= 0 && selectedCity.degreesLeft <= 180
        && selectedCity.degreesRight !== undefined && selectedCity.degreesRight >= 0 && selectedCity.degreesRight <= 180
        && selectedCity.mapColor && /^#[0-9A-Fa-f]{6}$/.test(selectedCity.mapColor)
        && (selectedCity.reachableByLand || selectedCity.reachableByWater || selectedCity.reachableByAir);

    return <Cs2FormBoundaries className="hwEditingForm">
        <Input title={translate(`${REGION_CITY_EDITOR}.name`)} getValue={() => selectedCity.name} onValueChanged={(x) => { setSelectedCity({ ...selectedCity, name: x }); return x; }} />
        <Cs2FormLine title={translate(`${REGION_CITY_EDITOR}.directionPreset`)}>
            <Cs2Select
                value={(directionPresets[directionPreset] || directionPresets[directionPresets.length - 1])}
                getOptionLabel={(x) => x.label}
                getOptionValue={(x) => x.value.toString()}
                onChange={(x) => { setDirectionPreset(x.value); }}
                options={directionPresets}
            />
        </Cs2FormLine>
        <Cs2FormLine title={translate(`${REGION_CITY_EDITOR}.centerPosition`)}>
            <NumberSimpleInput disabled={directionPreset != -1} max={360} min={0} precision={1} getValue={() => selectedCity.centerAzimuth} onValueChanged={(x) => { setSelectedCity({ ...selectedCity, centerAzimuth: x }); return x; }} />
        </Cs2FormLine>
        <Cs2FormLine title={translate(`${REGION_CITY_EDITOR}.offsetAzimuthLR`)} subtitle={translate(`${REGION_CITY_EDITOR}.offsetAzimuthLRSubtitle`)}>
            <NumberSimpleInput disabled={directionPreset != -1} max={180} min={0} precision={1} getValue={() => selectedCity.degreesLeft} onValueChanged={(x) => { setSelectedCity({ ...selectedCity, degreesLeft: x }); return x; }} />
            <NumberSimpleInput disabled={directionPreset != -1} max={180} min={0} precision={1} getValue={() => selectedCity.degreesRight} onValueChanged={(x) => { setSelectedCity({ ...selectedCity, degreesRight: x }); return x; }} />
        </Cs2FormLine>
        <ColorRgbInput title={translate(`${REGION_CITY_EDITOR}.cityColor`)} getValue={() => selectedCity.mapColor} onValueChanged={(x) => { setSelectedCity({ ...selectedCity, mapColor: x }); return x; }} />
        <Cs2CheckboxWithLine title={translate(`${REGION_CITY_EDITOR}.reachableByLand`)} isChecked={() => selectedCity.reachableByLand ?? false} onValueToggle={(x) => { setSelectedCity({ ...selectedCity, reachableByLand: x }); return x; }} />
        <Cs2CheckboxWithLine title={translate(`${REGION_CITY_EDITOR}.reachableByWater`)} isChecked={() => selectedCity.reachableByWater ?? false} onValueToggle={(x) => { setSelectedCity({ ...selectedCity, reachableByWater: x }); return x; }} />
        <Cs2CheckboxWithLine title={translate(`${REGION_CITY_EDITOR}.reachableByAir`)} isChecked={() => selectedCity.reachableByAir ?? false} onValueToggle={(x) => { setSelectedCity({ ...selectedCity, reachableByAir: x }); return x; }} />

        <div className="formGap" />
        <div className="bottomActions">
            <button className="positiveBtn" disabled={!isValid} onClick={onSave}>{translate(`${REGION_CITY_EDITOR}.save`)}</button>
            <button className="negativeBtn" onClick={onCancel}>{translate(`${REGION_CITY_EDITOR}.cancel`)}</button>
        </div>
    </Cs2FormBoundaries>
        ;
}
