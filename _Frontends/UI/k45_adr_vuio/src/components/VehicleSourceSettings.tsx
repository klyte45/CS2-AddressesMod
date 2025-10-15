
import { toEntityTyped, VanillaComponentResolver, VanillaWidgets } from "@klyte45/vuio-commons";
import useAsyncMemo from "@klyte45/vuio-commons/src/utils/useAsyncMemo";
import { selectedInfo } from "cs2/bindings";
import { useEffect, useState } from "react";
import { LocalizationStrings, default as routesSystem } from "service/AdrHighwayRoutesSystem";
import AdrVehicleSpawnerSystem, { VehicleSourceKind } from "service/AdrVehicleSpawnerSystem";
import { translate } from "utility/translate";


export const VehicleSourceSettings = () => {
    const VW = VanillaWidgets.instance;
    const VR = VanillaComponentResolver.instance;
    const editorModule = VanillaWidgets.instance.editorItemModule;
    const [buildIdx, setBuildIdx] = useState(0);
    const [customId, setCustomId] = useState("");


    const sourceData = useAsyncMemo(async () => {
        return AdrVehicleSpawnerSystem.getSpawnerData(toEntityTyped(selectedInfo.selectedEntity$.value)).then(x => {
            setCustomId(x?.customId);
            return x;
        });
    }, [selectedInfo.selectedEntity$.value.index, buildIdx])



    return !sourceData ? <>Loading...</> : <>
        <VR.InfoRow left={<>{translate("vehicleSpawner.sourceKind")}</>} right={translate("vehicleSpawner.sourceKind." + VehicleSourceKind[sourceData.sourceKind.value__])} />
        <VR.InfoRow left={<>{translate("vehicleSpawner.categorySerialNumber")}</>} right={<>{sourceData.categorySerialNumberSet ? sourceData.categorySerialNumber : translate("vehicleSpawner.categorySerialNumber.notSet")}</>} />
        <VR.InfoRow left={<>{translate("vehicleSpawner.lastInternalId")}</>} right={<>{sourceData.totalVehiclesSpawned}</>} />
        <VR.InfoRow left={<>{translate("vehicleSpawner.customId")}</>} right={<VW.StringInputField className={editorModule.input}
            onChange={setCustomId}
            onChangeEnd={x => AdrVehicleSpawnerSystem.setCustomId(toEntityTyped(selectedInfo.selectedEntity$.value), customId).then(() => setBuildIdx(buildIdx + 1))}
            value={customId}
        />} />


    </>
}