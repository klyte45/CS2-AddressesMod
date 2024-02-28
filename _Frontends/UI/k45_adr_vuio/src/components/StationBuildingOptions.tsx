import { Entity, SelectedInfoOptions, nameToString } from "@klyte45/adr-commons";
import { useUniqueFocusKey } from "common/focus/focus-key";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver";

type Props = { entity: Entity, response: SelectedInfoOptions };
type State = {}


// interface RadioButtonF extends Component<{
//     checked: boolean,
//     disabled: boolean,
//     theme: Theme,
//     style: CSSStyleRule,
//     className: string
// }, any> { }

export function StationBuildingOptionsComponent(props: Props) {
    const VR = VanillaComponentResolver.instance;
    if (!VR || !props.response) return <></>
    const isDistrictSelected = props.response.entityValue.Index == props.response.districtRef.Index
    const rowTheme = [VR.themeToggleLine?.focusableToggle, VR.themeToggleLine?.row, VR.themeToggleLine?.spaceBetween].join(" ");
    const currentVal = props.response.entityValue;
    const focusKey = VR.useUniqueFocusKey("TESTE", "K1");
    return <>
        <VR.InfoRow subRow={rowTheme} className={rowTheme} left={<><VR.RadioToggle focusKey={focusKey} className={VR.themeToggleLine.toggle} checked={isDistrictSelected} disabled={!props.response.allowDistrict} />Use district</>} right={<></>} />
        {
            props.response.roadAggegateOptions.map(x =>
                <VR.InfoRow className={rowTheme} left={<><VR.RadioToggle focusKey={focusKey} className={VR.themeToggleLine.toggle} checked={currentVal.Index == x.entity.Index} />Road: {nameToString(x.name)}</>} />)
        }
        {
            props.response.buildingsOptions.map(x =>
                <VR.InfoRow className={rowTheme} left={<><VR.RadioToggle focusKey={focusKey} className={VR.themeToggleLine.toggle} checked={currentVal.Index == x.entity.Index} />Building: {nameToString(x.name)}</>} />)
        }
    </>
}
