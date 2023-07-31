
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
    FormatPattern: string
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
    RoadPrefixSetting: AdrRoadPrefixSetting
}

export class NameFileManagementService {
    static async listSimpleNames(): Promise<SimpleNameEntry[]> { return await engine.call("k45::adr.main.listSimpleNames"); }
    static async reloadSimpleNames(): Promise<SimpleNameEntry[]> { return await engine.call("k45::adr.main.reloadSimpleNames"); }
    static async goToSimpleNamesFolder(): Promise<void> { return await engine.call("k45::adr.main.goToSimpleNamesFolder"); }
    static async getCurrentCitywideSettings(): Promise<AdrCitywideSettings> { return await engine.call("k45::adr.main.getCurrentCitywideSettings"); }
    static async setSurnameAtFirst(x: boolean): Promise<void> { await engine.call("k45::adr.main.setSurnameAtFirst", x); }
    static async setCitizenMaleNameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setCitizenMaleNameOverridesStr", x); }
    static async setCitizenFemaleNameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setCitizenFemaleNameOverridesStr", x); }
    static async setCitizenSurnameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setCitizenSurnameOverridesStr", x); }
    static async setCitizenDogOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setCitizenDogOverridesStr", x); }
    static async setDefaultRoadNameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setDefaultRoadNameOverridesStr", x); }
    static async setAdrRoadPrefixSetting(x: AdrRoadPrefixSetting): Promise<void> { await engine.call("k45::adr.main.setAdrRoadPrefixSetting", x); }
    static async onCityDataReloaded(x: () => void) { engine.on("k45::adr.main.onCurrentCitywideSettingsLoaded", x); }
}