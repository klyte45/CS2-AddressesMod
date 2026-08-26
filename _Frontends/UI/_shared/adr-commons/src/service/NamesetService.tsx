import { SimpleNameEntry } from "./NamingRulesService";


export class NamesetService {

    static doOnCityNamesetsUpdated(event: () => void) { engine.on("k45::adr.namesets.onCityNamesetsChanged", event); }
    static offCityNamesetsUpdated() { engine.off("k45::adr.namesets.onCityNamesetsChanged"); }

    static async goToDiskSimpleNamesFolder(): Promise<void> { return await engine.call("k45::adr.namesets.goToSimpleNamesFolder"); }
    static async updateNameset(IdString: string, Name: string, Values: string[], ValuesAlternative: string[]) { await engine.call("k45::adr.namesets.updateForCity", IdString, Name, Values, ValuesAlternative); }
    static async deleteNamesetFromCity(GuidString: string) { await engine.call("k45::adr.namesets.deleteFromCity", GuidString); }
    static async sendNamesetForCity(name: string, values: string[], ValuesAlternative: string[]) { await engine.call("k45::adr.namesets.addNamesetToCity", name, values,ValuesAlternative); }

    static async listCityNamesets(): Promise<SimpleNameEntry[]> { return await engine.call("k45::adr.namesets.listCityNamesets"); }
    static async listLibraryNamesets(): Promise<SimpleNameEntry[]> { return await engine.call("k45::adr.namesets.listLibraryNamesets"); }
    static async listNamesetFolderFiles(folder: string, allowedExtension: string): Promise<{ displayName: string; directory: boolean; fullPath: string }[]> {
        return await engine.call("k45::adr.namesets.file.listFiles", folder, allowedExtension);
    }
    static async readNamesetFromDisk(path: string): Promise<SimpleNameEntry> {
        return await engine.call("k45::adr.namesets.readNamesetFromDisk", path);
    }
    static async reloadLibraryNamesets(): Promise<boolean> { return await engine.call("k45::adr.namesets.reloadLibraryNamesets"); }
    static async isLibraryLoading(): Promise<boolean> { return await engine.call("k45::adr.namesets.isLibraryLoading"); }
    static onLibraryReloaded(event: (namesets: SimpleNameEntry[]) => void) { engine.on("k45::adr.namesets.onLibraryReloaded", event); }
    static offLibraryReloaded() { engine.off("k45::adr.namesets.onLibraryReloaded"); }
    static async exportFromCityToLibrary(IdString: string): Promise<string> { return await engine.call("k45::adr.namesets.exportToLibrary", IdString); }

    static async sortValues(values: string[]): Promise<string[]> { return await engine.call("k45::adr.namesets.sortValues", values); }
}


