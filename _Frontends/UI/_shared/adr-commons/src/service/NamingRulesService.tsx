import { Entity } from "../../";

export type SimpleNameEntry = {
    IdString: string,
    Name: string,
    Values: string[]
    _CurrName?: string
}
export type ExtendedSimpleNameEntry = SimpleNameEntry & {
    _CurrName?: string;
}

export enum RoadFlags {
    EnableZoning = 0x1,
    SeparatedCarriageways = 0x2,
    PreferTrafficLights = 0x4,
    DefaultIsForward = 0x8,
    UseHighwayRules = 0x10,
    DefaultIsBackward = 0x20,
    HasStreetLights = 0x40
}

export type AdrRoadPrefixRule = {
    MinSpeedKmh: number,
    MaxSpeedKmh: number,
    RequiredFlagsInt: number | RoadFlags,
    ForbiddenFlagsInt: number | RoadFlags,
    FormatPattern: string,
    FullBridge: 0 | -1 | 1
}

export type AdrRoadPrefixSetting = {
    FallbackRule: AdrRoadPrefixRule
    AdditionalRules: AdrRoadPrefixRule[]
}

export type AdrCitywideSettings = {
    SurnameAtFirst: boolean,
    CitizenMaleNameOverridesStr: string,
    CitizenFemaleNameOverridesStr: string,
    CitizenSurnameOverridesStr: string,
    CitizenDogOverridesStr: string
    DefaultRoadNameOverridesStr: string,
    DefaultDistrictNameOverridesStr: string,
    RoadPrefixSetting: AdrRoadPrefixSetting
    RoadNameAsNameStation: boolean
    RoadNameAsNameCargoStation: boolean
    DistrictNameAsNameStation: boolean
    DistrictNameAsNameCargoStation: boolean
    MaximumGeneratedSurnames: number
    MaximumGeneratedGivenNames: number
}

export class NamingRulesService {
    static onCityDataReloaded(x: () => void) { return engine.on("k45::adr.main.onCurrentCitywideSettingsLoaded", x); }
    static offCityDataReloaded() { engine.off("k45::adr.main.onCurrentCitywideSettingsLoaded"); }


    static async getCurrentCitywideSettings(): Promise<AdrCitywideSettings> { return await engine.call("k45::adr.main.getCurrentCitywideSettings"); }
    static async setSurnameAtFirst(x: boolean): Promise<void> { await engine.call("k45::adr.main.setSurnameAtFirst", x); }
    static async setCitizenMaleNameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setCitizenMaleNameOverridesStr", x); }
    static async setCitizenFemaleNameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setCitizenFemaleNameOverridesStr", x); }
    static async setCitizenSurnameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setCitizenSurnameOverridesStr", x); }
    static async setCitizenDogOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setCitizenDogOverridesStr", x); }
    static async setDefaultRoadNameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setDefaultRoadNameOverridesStr", x); }
    static async setAdrRoadPrefixSetting(x: AdrRoadPrefixSetting): Promise<void> { await engine.call("k45::adr.main.setAdrRoadPrefixSetting", x); }
    static async setDefaultDistrictNameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setDefaultDistrictNameOverridesStr", x); }
    static async setRoadNameAsNameStation(x: boolean): Promise<void> { await engine.call("k45::adr.main.setRoadNameAsNameStation", x); }
    static async setRoadNameAsNameCargoStation(x: boolean): Promise<void> { await engine.call("k45::adr.main.setRoadNameAsNameCargoStation", x); }
    static async setDistrictNameAsNameStation(x: boolean): Promise<void> { await engine.call("k45::adr.main.setDistrictNameAsNameStation", x); }
    static async setDistrictNameAsNameCargoStation(x: boolean): Promise<void> { await engine.call("k45::adr.main.setDistrictNameAsNameCargoStation", x); }

    static async setMaxSurnames(x: number): Promise<string> { return await engine.call("k45::adr.main.setMaxSurnames", x); }

    static async setMaxGivenNames(x: number): Promise<string> { return await engine.call("k45::adr.main.setMaxGivenNames", x); }


    static async exploreToRoadPrefixRulesFileDefault(): Promise<void> { await engine.call("k45::adr.main.exploreToRoadPrefixRulesFileDefault"); }
    static async saveRoadPrefixRulesFileDefault(): Promise<void> { await engine.call("k45::adr.main.saveRoadPrefixRulesFileDefault"); }
    static async loadRoadPrefixRulesFileDefault(): Promise<number> { return await engine.call("k45::adr.main.loadRoadPrefixRulesFileDefault"); }

}

