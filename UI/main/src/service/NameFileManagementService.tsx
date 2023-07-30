
export type SimpleNameEntry = {
    IdString: string,
    Name: string,
    Values: string[]
    _CurrName?: string
}
export type ExtendedSimpleNameEntry = SimpleNameEntry & {
    _CurrName?: string;
}

export type AdrCitywideSettings = {
    SurnameAtFirst: boolean,
    CitizenMaleNameOverridesStr: string,
    CitizenFemaleNameOverridesStr: string,
    CitizenSurnameOverridesStr: string,
    CitizenDogOverridesStr: string
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
    static async onCityDataReloaded(x: () => void) {  engine.on("k45::adr.main.onCurrentCitywideSettingsLoaded", x); }
}