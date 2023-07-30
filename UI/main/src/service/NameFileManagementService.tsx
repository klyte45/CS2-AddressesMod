
export type SimpleNameEntry = {
    IdString: string,
    Name: string,
    Values: string[]
    _CurrName?: string
}
export type ExtendedSimpleNameEntry = SimpleNameEntry & {
    _CurrName?: string;
}

export class NameFileManagementService {
    static async listSimpleNames(): Promise<SimpleNameEntry[]> {
        return await engine.call("k45::adr.main.listSimpleNames");
    }
    static async reloadSimpleNames(): Promise<SimpleNameEntry[]> {
        return await engine.call("k45::adr.main.reloadSimpleNames");
    }
    static async goToSimpleNamesFolder(): Promise<void> {
        return await engine.call("k45::adr.main.goToSimpleNamesFolder");
    }
}