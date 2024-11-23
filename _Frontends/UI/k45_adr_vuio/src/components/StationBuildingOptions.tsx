import { Entity, SelectInfoPanelService, SelectedInfoOptions, nameToString, replaceArgs } from "@klyte45/adr-commons";
import { VanillaComponentResolver } from '@klyte45/vuio-commons';
import { useState } from "react";
import { translate } from "utility/translate";

type Props = { entityOrigin: Entity, response: SelectedInfoOptions, onChanged: () => Promise<any> };

export const StationBuildingOptionsComponent = ({ entityOrigin, onChanged, response }: Props) => {

    const [loading, setLoading] = useState(false);

    const setReference = async (target: Entity, reference: Entity) => {
        setLoading(true);
        await SelectInfoPanelService.setEntityNamingRef(target, reference, entityOrigin);
        await onChanged();
        setLoading(false);
    }

    if (loading) return <>Loading...</>
    const VR = VanillaComponentResolver.instance;
    if (!VR || !response) return <></>
    const isDistrictSelected = response.entityValue.Index == response.districtRef.Index
    const rowTheme = [VR.themeToggleLine?.focusableToggle, VR.themeToggleLine?.row, VR.themeToggleLine?.spaceBetween].join(" ");
    const currentVal = response.entityValue;
    const focusKey = VR.FOCUS_DISABLED;
    return <> 
        <VR.InfoRow subRow={true} className={rowTheme} left={<><VR.RadioToggle onChange={() => setReference(response.targetEntityToName, { Index: 0, Version: 0 })} focusKey={focusKey} className={VR.themeToggleLine.toggle} checked={currentVal.Index == 0} />{translate("StationBuildingOptions.UseDefault")}</>} />
        {response.districtRef.Index > 0 && <VR.InfoRow subRow={true} className={rowTheme} left={<><VR.RadioToggle onChange={() => setReference(response.targetEntityToName, response.districtRef)} focusKey={focusKey} className={VR.themeToggleLine.toggle} checked={isDistrictSelected} />{translate("StationBuildingOptions.UseDistrictOpt")}</>} />}
        {
            response.roadAggegateOptions.map(x =>
                <VR.InfoRow subRow={true} className={rowTheme} left={<><VR.RadioToggle onChange={() => setReference(response.targetEntityToName, x.entity)} focusKey={focusKey} className={VR.themeToggleLine.toggle} checked={currentVal.Index == x.entity.Index} />{replaceArgs(translate("StationBuildingOptions.RoadOptPattern", "{name}"), { name: nameToString(x.name)!! })}</>} />)
        }
        {
            response.buildingsOptions.map(x =>
                <VR.InfoRow subRow={true} className={rowTheme} left={<><VR.RadioToggle onChange={() => setReference(response.targetEntityToName, x.entity)} focusKey={focusKey} className={VR.themeToggleLine.toggle} checked={currentVal.Index == x.entity.Index} />{replaceArgs(translate("StationBuildingOptions.BuildingOptPattern", "{name}"), { name: nameToString(x.name)!! })}</>} />)
        }
    </>

}
