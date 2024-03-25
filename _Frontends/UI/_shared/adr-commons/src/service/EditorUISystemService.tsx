import { Entity, ValuableName } from "../../";

export type AdrEntityEditorData = {
    name: ValuableName
}

export class EditorUISystemService {
    static async getEntityData(x: Entity): Promise<AdrEntityEditorData> { return await engine.call("k45::adr.editorUI.getEntityData", x); }
    static async setEntityCustomName(x: Entity, name?: string): Promise<void> { return await engine.call("k45::adr.editorUI.setEntityCustomName", x, name); }
}
