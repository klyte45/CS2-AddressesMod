import { MultiUIValueBinding, MultiUIValueBindingTools } from "@klyte45/vuio-commons";
import { LocElement } from "cs2/l10n";
import { ObjectTyped } from "object-typed";

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
    Tool_PylonCount: MultiUIValueBinding<number>,
    Tool_PylonSpacing: MultiUIValueBinding<number>,
    Tool_PylonMaterial: MultiUIValueBinding<number>,
    Tool_PylonHeight: MultiUIValueBinding<number>,
    Tool_PylonFormat: MultiUIValueBinding<number>,
    InfoPanel_RouteId: MultiUIValueBinding<string>,
    InfoPanel_RouteDirection: MultiUIValueBinding<number>,
    InfoPanel_DisplayInformation: MultiUIValueBinding<number>,
    InfoPanel_NumericCustomParam1: MultiUIValueBinding<number>,
    InfoPanel_NumericCustomParam2: MultiUIValueBinding<number>,
    InfoPanel_NewMileage: MultiUIValueBinding<number>,
    InfoPanel_OverrideMileage: MultiUIValueBinding<boolean>,
    InfoPanel_ReverseMileageCounting: MultiUIValueBinding<boolean>,
    InfoPanel_PylonCount: MultiUIValueBinding<number>,
    InfoPanel_PylonSpacing: MultiUIValueBinding<number>,
    InfoPanel_PylonMaterial: MultiUIValueBinding<number>,
    InfoPanel_PylonHeight: MultiUIValueBinding<number>,
    InfoPanel_PylonFormat: MultiUIValueBinding<number>,

}

export default {
    ...MultiUIValueBindingTools.InitializeBindings(AdrHighwayRoutesSystem),
    async refreshAllBindings() {
        (ObjectTyped.keys(AdrHighwayRoutesSystem) as Exclude<keyof typeof AdrHighwayRoutesSystem, '_prefix'>[]).filter(x => !x.startsWith("_")).map((binding) => this[binding].reactivate())
    },
    async isCurrentPrefabRoadMarker(): Promise<boolean> { return await engine.call(AdrHighwayRoutesSystem._prefix + ".isCurrentPrefabRoadMarker"); },
    async getOptionsMetadataFromCurrentLayout(): Promise<string | null> { return await engine.call(AdrHighwayRoutesSystem._prefix + ".getOptionsMetadataFromCurrentLayout"); },
    async getOptionsMetadataFromLayout(enumId: DisplayInformation & number): Promise<string | null> { return await engine.call(AdrHighwayRoutesSystem._prefix + ".getOptionsMetadataFromLayout", enumId); },
    async getOptionsNamesFromMetadata(): Promise<string[]> {
        return await engine.call(AdrHighwayRoutesSystem._prefix + ".getOptionsNamesFromMetadata");
    },
    validateMetadata(metadata: any): AdrFields<string> | null {
        if (metadata && typeof metadata == 'object') {
            const validKeys = ObjectTyped.keys(metadata) as string[];
            const validEntries = ObjectTyped.fromEntries(ObjectTyped.entries(metadata).filter(x => {
                const value = x[1] as AdrFieldData<string>;
                if (typeof value.localization != 'string'
                    || typeof value.parameter != "number"
                    || ![0, 1].includes(value.parameter)
                    || typeof value.position != "number"
                    || value.position < 0 || value.position > 31
                    || typeof value.size != 'number'
                    || value.size <= 0 || value.size > 31
                    || (value.size + value.position) > 31
                    || ![AdrFieldType.NUMBER, AdrFieldType.SELECTION].includes(value.type)
                ) {
                    // console.log(x[0], "invalid basic", x[1])
                    return false;
                }
                if (value.type == AdrFieldType.NUMBER) {
                    if (
                        (value.max && typeof value.max != "number")
                        || (value.min && typeof value.min != "number")
                    ) {
                        // console.log(x[0], "invalid num")
                        return false;
                    }
                }
                if (value.type == AdrFieldType.SELECTION) {
                    if (typeof value.options != 'object'
                        && ObjectTyped.entries(value.options).some(x => isNaN(parseInt(x[0] as string)) || typeof x[1] != 'string')
                    ) {
                        // console.log(x[0], "invalid sel")
                        return false;
                    }
                }
                if (value.condition != undefined && !validateCondition(value.condition, validKeys)) {
                    // console.log(x[0], "invalid cond")
                    return false;
                }
                return true;
            }) as [string, AdrFieldData<string>][]) as AdrFields<string>;
            return validEntries;
        }
        return null;
    }
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

export enum PylonFormat {
    Cylinder,
    Cubic
}
export enum PylonMaterial {
    Metal,
    Wood
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
    customParam2: "RoadMarkSettings.CustomParam2",
    poleFormat: "RoadMarkSettings.PoleFormat",
    poleMaterial: "RoadMarkSettings.PoleMaterial",
    poleSettings: "RoadMarkSettings.PoleSettings",
    poleHeight: "RoadMarkSettings.PoleHeight",
    useDoublePole: "RoadMarkSettings.UseDoublePole",
    poleSpacing: "RoadMarkSettings.PoleSpacing",
}


export enum AdrFieldType {
    SELECTION = "sel",
    NUMBER = 'num'
}

type AndOperator<T extends string> = { and: Condition<T>[] }
type OrOperator<T extends string> = { or: Condition<T>[] }
type EqOperator<T extends string> = { eq: [T, number] }
type NeOperator<T extends string> = { ne: [T, number] }
type LtOperator<T extends string> = { lt: [T, number] }
type GtOperator<T extends string> = { gt: [T, number] }

function validateCondition(item: any, validProperties: string[]): boolean {
    if (typeof item == 'object') {
        const keys = Object.keys(item);
        if (keys.length != 1) return false;
        const value = item[keys[0]];
        if (!Array.isArray(value)) return false;
        switch (keys[0]) {
            case "and":
            case "or":
                return value.every(x => validateCondition(x, validProperties));
            case "eq":
            case "ne":
            case "gt":
            case "lt":
                if (value.length != 2 || !validProperties.includes(value[0]) || typeof value[1] != 'number') {
                    // console.log(item, "invalid cond")
                    return false;
                }
                return true;
        }
    }
    return false;
}

export type Condition<T extends string> = AndOperator<T> | OrOperator<T> | EqOperator<T> | NeOperator<T> | LtOperator<T> | GtOperator<T>

export type AdrFieldData<T extends string> = {
    localization: string
    parameter: 0 | 1
    position: number
    size: number
    condition?: Condition<T>
} & (
        {
            type: AdrFieldType.SELECTION
            options: Record<number, string>
        } | {
            type: AdrFieldType.NUMBER,
            min?: number,
            max?: number
        }
    )
export type AdrFields<T extends string> = Record<T, AdrFieldData<T>>;
