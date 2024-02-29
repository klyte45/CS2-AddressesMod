import { Entity } from "../../";
import { SelectedInfoOptions } from "../model/SelectInfoPanelModels"
export class SelectInfoPanelService {

    static async getEntityOptions(x: Entity): Promise<SelectedInfoOptions> { return await engine.call("k45::adr.selectionPanel.getEntityOptions", x); }
    static async setEntityNamingRef(x: Entity, ref: Entity): Promise<SelectedInfoOptions> { return await engine.call("k45::adr.selectionPanel.setEntityRoadReference", x, ref); }
}
