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

export default {
    ...MultiUIValueBindingTools.InitializeBindings(AdrHighwayRoutesSystem),
    async isCurrentPrefabRoadMarker(): Promise<boolean> { return await engine.call(AdrHighwayRoutesSystem._prefix + ".isCurrentPrefabRoadMarker"); }
}

export enum RouteDirection {
    UNDEFINED,
    NORTH,
    NORTHEAST,
    EAST,
    SOUTHEAST,
    SOUTH,
    SOUTHWEST,
    WEST,
    NORTHWEST,
    INTERNAL,
    EXTERNAL
}

export enum DisplayInformation {
    ORIGINAL,
    CUSTOM_1,
    CUSTOM_2,
    CUSTOM_3,
    CUSTOM_4,
    CUSTOM_5,
    CUSTOM_6,
    CUSTOM_7
}

export type RouteItem = {
    name: string,
    id: string
}

export const LocalizationStrings = {
    routeIdentifier: "RoadMarkSettings.RouteIdentifier",
    routeDirection: "RoadMarkSettings.RouteDirection",
    overrideMileage: "RoadMarkSettings.OverrideMileage",
    overrideMileageShort: "RoadMarkSettings.OverrideMileageShort",
    overrideMileageToggleButton: "RoadMarkSettings.OverrideMileageToggleButton",
    newMileage: "RoadMarkSettings.NewMileage",
    reverseMileageCounting: "RoadMarkSettings.ReverseMileageCounting",
    displayInformation: "RoadMarkSettings.DisplayInformation",
    customParam1: "RoadMarkSettings.CustomParam1",
    customParams: "RoadMarkSettings.CustomParams",
    customParam2: "RoadMarkSettings.CustomParam2"
}