import { AdrEntityType, Entity, SelectInfoPanelService, SelectedInfoOptions, toEntityTyped } from "adr";
import { VanillaComponentResolver } from '@klyte45/vuio-commons';
import { useCallback, useEffect, useState } from "react";
import { translate } from "utility/translate";
import { SeedManagementOptionsComponent } from "./SeedManagementOptions";
import { HighwayAggregateSettings } from "./HighwayAggregateSettings";
import { StationBuildingOptionsComponent } from "./StationBuildingOptions";
import { VehicleDataDetailSection } from "./VehicleDataDetailSection";
import { RoadMarkSettings } from "./RoadMarkSettings";
import { VehicleSourceSettings } from "./VehicleSourceSettings";
import { selectedInfo } from "cs2/bindings";

type Props = { isEditor?: boolean, onChange?: () => any };

/** Session cache of ADR SIP foldout expanded state, keyed by AdrEntityType name. Defaults collapsed. */
const adrSipFoldoutExpanded: Record<string, boolean> = {};

const getAdrSipFoldoutExpanded = (blockKey: string) => adrSipFoldoutExpanded[blockKey] ?? false;
const setAdrSipFoldoutExpanded = (blockKey: string, expanded: boolean) => {
    adrSipFoldoutExpanded[blockKey] = expanded;
};

export const AddressesInfoOptionsComponent = ({ onChange }: Props) => {

    const [optionsResult, setOptionsResult] = useState([] as SelectedInfoOptions[]);

    const entity = toEntityTyped(selectedInfo.selectedEntity$.value)

    useEffect(() => {
        loadOptions(entity)
    }, [entity.Index, selectedInfo.titleSection$.value])

    const loadOptions = async (entity: Entity) => {
        if (!entity) {
            setOptionsResult([]);
            return;
        }
        const call = SelectInfoPanelService.getEntityOptions(entity);
        const result = await call
        setOptionsResult(result)
    }

    const onValueChanged = async () => {
        await onChange?.();
        await loadOptions(entity)
    }

    const onToggleFoldout = useCallback((blockKey: string, expanded: boolean) => {
        setAdrSipFoldoutExpanded(blockKey, expanded);
    }, []);


    const getSubRows = (optionResult: SelectedInfoOptions) => {
        switch (optionResult?.type?.value__) {
            case AdrEntityType.PublicTransportStation:
            case AdrEntityType.CargoTransportStation:
                return <StationBuildingOptionsComponent onChanged={() => onValueChanged()} entityOrigin={entity} response={optionResult} />
            case AdrEntityType.RoadAggregation:
            case AdrEntityType.District:
                return <SeedManagementOptionsComponent onChanged={() => onValueChanged()} response={optionResult} />
            case AdrEntityType.HighwayAggregate:
                return <HighwayAggregateSettings onChanged={() => onValueChanged()} response={optionResult} />
            case AdrEntityType.Vehicle:
                return <VehicleDataDetailSection />
            case AdrEntityType.RoadMark:
                return <RoadMarkSettings />
            case AdrEntityType.VehicleSource:
                return <VehicleSourceSettings />
            default:
                return null;
        }
    }

    const getLeftColumnText = (optionResult: SelectedInfoOptions) => {
        switch (optionResult.type.value__) {
            case AdrEntityType.Vehicle:
                return translate("AddressesInfoOptions.ExtraDataInformationFor");
            case AdrEntityType.RoadMark:
            case AdrEntityType.VehicleSource:
            case AdrEntityType.HighwayAggregate:
                return translate("AddressesInfoOptions.SettingsFor");
            default:
                return translate("AddressesInfoOptions.NamingReference");
        }
    }


    if (!optionsResult) return <></>;
    var result = [];
    for (const optionResult of optionsResult) {
        const valueType = AdrEntityType[optionResult?.type?.value__];
        const VR = VanillaComponentResolver.instance;
        const blockKey = String(valueType);
        const subRows = getSubRows(optionResult);
        const titleRow = <VR.InfoRow uppercase={true} disableFocus={true} icon="coui://adr.k45/UI/images/ADR.svg"
            left={<>{getLeftColumnText(optionResult)}</>}
            right={<div style={{ whiteSpace: "pre-wrap" }}>{translate("AdrEntityType." + valueType)}</div>}
        />;
        if (subRows) {
            result.push(<VR.InfoSectionFoldout
                key={"adr_sip_" + blockKey}
                disableFocus={true}
                className="k45_modding"
                initialExpanded={getAdrSipFoldoutExpanded(blockKey)}
                onToggleExpanded={(expanded: boolean) => onToggleFoldout(blockKey, expanded)}
                tooltip={<>{translate("AddressesInfoOptions.Tooltip." + valueType)}</>}
                header={titleRow}
            >
                {subRows}
            </VR.InfoSectionFoldout>)
        } else {
            result.push(<VR.InfoSection key={"adr_sip_" + blockKey} disableFocus={true} className="k45_modding"
                tooltip={<>{translate("AddressesInfoOptions.Tooltip." + valueType)}</>}>
                {titleRow}
            </VR.InfoSection>)
        }
    }
    return <>{result}</>;

}
