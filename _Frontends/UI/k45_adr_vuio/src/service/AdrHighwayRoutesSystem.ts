import { MultiUIValueBinding, MultiUIValueBindingTools } from "@klyte45/vuio-commons";

const AdrHighwayRoutesSystem = {
    _prefix: "k45::adr.highwayRoutes",
    Tool_RouteId: MultiUIValueBinding<string>,
    Tool_RouteDirection: MultiUIValueBinding<number>,
    Tool_DisplayInformation: MultiUIValueBinding<number>,
    Tool_NumericCustomParam1: MultiUIValueBinding<number>,
    Tool_NumericCustomParam2: MultiUIValueBinding<number>,
    Tool_NewMileage: MultiUIValueBinding<number>,
    Tool_OverrideMileage: MultiUIValueBinding<boolean>,
    Tool_ReverseMileageCounting: MultiUIValueBinding<boolean>,
    InfoPanel_RouteId: MultiUIValueBinding<string>,
    InfoPanel_RouteDirection: MultiUIValueBinding<number>,
    InfoPanel_DisplayInformation: MultiUIValueBinding<number>,
    InfoPanel_NumericCustomParam1: MultiUIValueBinding<number>,
    InfoPanel_NumericCustomParam2: MultiUIValueBinding<number>,
    InfoPanel_NewMileage: MultiUIValueBinding<number>,
    InfoPanel_OverrideMileage: MultiUIValueBinding<boolean>,
    InfoPanel_ReverseMileageCounting: MultiUIValueBinding<boolean>,
}

export default MultiUIValueBindingTools.InitializeBindings(AdrHighwayRoutesSystem);