import { ADRVehicleData, Entity } from "../../";

const baseCall = "k45::adr.vehicles."
export class VehicleService {
    static async getAdrData(e: Entity): Promise<ADRVehicleData> { return await engine.call(baseCall + "getAdrData", e); }
}
