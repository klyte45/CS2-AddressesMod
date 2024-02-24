import { Entity } from "../../";
export class SelectInfoPanelService {

    static async getEntityOptions(x: Entity): Promise<any> { return await engine.call("k45::adr.selectionPanel.getEntityOptions", x); }
}
