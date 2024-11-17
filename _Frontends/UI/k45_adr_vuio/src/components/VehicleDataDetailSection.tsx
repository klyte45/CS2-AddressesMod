import { ADRVehicleData, Entity, VehicleService, SelectInfoPanelService, SelectedInfoOptions, replaceArgs, VehiclePlateCategory } from "@klyte45/adr-commons";
import { VanillaComponentResolver } from '@klyte45/vuio-commons';
import { useEffect, useState } from "react";
import { translate } from "utility/translate";

type Props = { entity: Entity };


export const VehicleDataDetailSection = ({ entity }: Props) => {

    const [loading, setLoading] = useState(true);
    const [currentData, setCurrentData] = useState(undefined as any as ADRVehicleData);

    useEffect(() => {
        setLoading(true);
        VehicleService.getAdrData(entity).then(x => {
            setLoading(false);
            setCurrentData(x);
        });
    }, [entity])

    if (loading) return <>Loading...</>
    const VR = VanillaComponentResolver.instance;
    if (!VR || !currentData) return <></>

    var refMonth = currentData.manufactureMonthsFromEpoch;

    return <>
        <VR.InfoRow left={<>{translate(`vehicleDataVuio.vehiclePlateTitle.${VehiclePlateCategory[currentData.plateCategory.value__]}`)}</>} right={currentData.calculatedPlate} />
        <VR.InfoRow subRow={true} left={<>{translate("vehicleDataVuio.serialNumber")}</>} right={currentData.serialNumber} />
        <VR.InfoRow subRow={true} left={<>{translate("vehicleDataVuio.manufactureMonthsFromEpoch")}</>} right={engine.translate("Common.MONTH_SHORT:" + (refMonth % 12)) + " " + Math.floor(refMonth / 12)} />
    </>



}
