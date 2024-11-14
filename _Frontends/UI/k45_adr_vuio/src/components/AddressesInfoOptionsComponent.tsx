import { AdrEntityType, SelectInfoPanelService, SelectedInfoOptions, toEntityTyped } from "@klyte45/adr-commons";
import { VanillaComponentResolver } from '@klyte45/vuio-commons';
import { ValueBinding } from "cs2/api";
import { Entity } from "cs2/utils";
import { useEffect, useState } from "react";
import { translate } from "utility/translate";
import { SeedManagementOptionsComponent } from "./SeedManagementOptions";
import { StationBuildingOptionsComponent } from "./StationBuildingOptions";
import { VehicleDataDetailSection } from "./VehicleDataDetailSection";

type Props = { entity: ValueBinding<Entity>, entityRef?: any, isEditor?: boolean, onChange?: () => any };

export const AddressesInfoOptionsComponent = ({ entity, entityRef, onChange }: Props) => {

    const [optionsResult, setOptionsResult] = useState(undefined as SelectedInfoOptions | undefined);

    useEffect(() => {
        loadOptions(entity.value)
    }, [entityRef, entity.value])

    const loadOptions = async (entity: Entity) => {
        if (!entity) {
            setOptionsResult(undefined);
            return;
        }
        const call = SelectInfoPanelService.getEntityOptions(toEntityTyped(entity));
        const result = await call
        setOptionsResult(result)
    }

    const onValueChanged = async () => {
        await onChange?.();
        await loadOptions(entity.value)
    }


    const getSubRows = () => {
        switch (optionsResult?.type?.value__) {
            case AdrEntityType.PublicTransportStation:
            case AdrEntityType.CargoTransportStation:
                return <StationBuildingOptionsComponent onChanged={() => onValueChanged()} entity={toEntityTyped(entity.value)} response={optionsResult} />
            case AdrEntityType.RoadAggregation:
            case AdrEntityType.District:
                return <SeedManagementOptionsComponent onChanged={() => onValueChanged()} entity={toEntityTyped(entity.value)} response={optionsResult} />
            case AdrEntityType.Vehicle:
                return <VehicleDataDetailSection entity={toEntityTyped(entity.value)} />
            default:
                return <></>
        }
    }


    if (!optionsResult) return <></>;
    const valueType = AdrEntityType[optionsResult?.type?.value__];
    const VR = VanillaComponentResolver.instance;
    return <VR.InfoSection disableFocus={true}  >
        <VR.InfoRow uppercase={true} icon="coui://adr.k45/UI/images/ADR.svg" left={<>{translate("AddressesInfoOptions.NamingReference")}</>} right={<div style={{ whiteSpace: "pre-wrap" }}>{translate("AdrEntityType." + valueType, valueType)}</div>} tooltip={<>{translate("AddressesInfoOptions.Tooltip." + valueType)}</>} />
        {getSubRows()}
    </VR.InfoSection>

}
