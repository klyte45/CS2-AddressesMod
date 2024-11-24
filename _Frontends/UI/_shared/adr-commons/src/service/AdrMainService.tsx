
export class AdrMainService {
    static async isCityOrEditorLoaded(): Promise<boolean> { return await engine.call("k45::adr.main.isCityOrEditorLoaded"); }
}
