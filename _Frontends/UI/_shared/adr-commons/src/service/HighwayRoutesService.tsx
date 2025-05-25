
const PREFIX = "k45::adr.highwayRoutes.";
export class HighwayRoutesService {
    static async listHighwaysRegistered(): Promise<HighwayData[]> { return await engine.call(PREFIX + "listHighwaysRegistered"); }
    static async saveHighwayData(data: HighwayData): Promise<boolean> { return await engine.call(PREFIX + "saveHighwayData", data); }
}

export type HighwayData = {
    Id: string;
    prefix: string;
    suffix: string;
    name: string;
    refStartPoint: [number, number];
}