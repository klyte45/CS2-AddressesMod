import { AdrEntityType, Entity, SelectInfoPanelService, SelectedInfoOptions, toEntityTyped } from "@klyte45/adr-commons";
import { VanillaComponentResolver } from '@klyte45/vuio-commons';
import { ValueBinding } from "cs2/api";
import { useEffect, useState } from "react";
import { translate } from "utility/translate";
import { SeedManagementOptionsComponent } from "./SeedManagementOptions";
import { StationBuildingOptionsComponent } from "./StationBuildingOptions";
import { VehicleDataDetailSection } from "./VehicleDataDetailSection";
import { RoadMarkSettings } from "./RoadMarkSettings";
import { VehicleSourceSettings } from "./VehicleSourceSettings";
import { selectedInfo } from "cs2/bindings";

type Props = { isEditor?: boolean, onChange?: () => any };

export const AddressesInfoOptionsComponent = ({ onChange }: Props) => {

    const [optionsResult, setOptionsResult] = useState([] as SelectedInfoOptions[]);

    const entity = toEntityTyped(selectedInfo.selectedEntity$.value)

    useEffect(() => {
        loadOptions(entity)
    }, [entity.Index])

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


    const getSubRows = (optionResult: SelectedInfoOptions) => {
        switch (optionResult?.type?.value__) {
            case AdrEntityType.PublicTransportStation:
            case AdrEntityType.CargoTransportStation:
                return <StationBuildingOptionsComponent onChanged={() => onValueChanged()} entityOrigin={entity} response={optionResult} />
            case AdrEntityType.RoadAggregation:
            case AdrEntityType.District:
                return <SeedManagementOptionsComponent onChanged={() => onValueChanged()} response={optionResult} />
            case AdrEntityType.Vehicle:
                return <VehicleDataDetailSection />
            case AdrEntityType.RoadMark:
                return <RoadMarkSettings />
            case AdrEntityType.VehicleSource:
                return <VehicleSourceSettings />
            default:
                return <></>
        }
    }

    const getLeftColumnText = (optionResult: SelectedInfoOptions) => {
        switch (optionResult.type.value__) {
            case AdrEntityType.Vehicle:
                return translate("AddressesInfoOptions.ExtraDataInformationFor");
            case AdrEntityType.RoadMark:
            case AdrEntityType.VehicleSource:
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
        result.push(<VR.InfoSection disableFocus={true} className="k45_modding" >
            <VR.InfoRow uppercase={true} icon="coui://adr.k45/UI/images/ADR.svg"
                left={<>{getLeftColumnText(optionResult)}</>}
                right={<div style={{ whiteSpace: "pre-wrap" }}>{translate("AdrEntityType." + valueType)}</div>}
                tooltip={<>{translate("AddressesInfoOptions.Tooltip." + valueType)}</>} />
            {getSubRows(optionResult)}
        </VR.InfoSection>)
    }
    return <>{result}</>;

}
