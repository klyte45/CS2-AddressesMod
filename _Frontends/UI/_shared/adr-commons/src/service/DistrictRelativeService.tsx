import { Entity, NameCustom, NameFormatted } from "../../";

export type DistrictListItem = {
    Entity: Entity;
    Name: NameCustom | NameFormatted;
    CurrentValue: string;
}

export class DistrictRelativeService {
    static async listAllDistricts(): Promise<DistrictListItem[]> { return await engine.call("k45::adr.district.listAllDistricts"); }
    static async setRoadNamesFile(district: Entity, guid: string): Promise<void> { await engine.call("k45::adr.district.setRoadNamesFile", district, guid); }
    static onDistrictChanged(x: () => void) { return engine.on("k45::adr.district.onDistrictsChanged", x); }
    static offDistrictChanged() { engine.off("k45::adr.district.onDistrictsChanged"); }
}
