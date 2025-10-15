import { EnumValueType } from "@klyte45/adr-commons";
import { Entity } from "@klyte45/vuio-commons";

export default class {
    static _prefix = "k45::adr.vehicleSpawner"

    public static async getSpawnerData(entity: Entity): Promise<SpawnerDataSafe> { return await engine.call(this._prefix + ".getSpawnerData", entity); }
    public static async setCustomId(entity: Entity, customId: string): Promise<boolean> { return await engine.call(this._prefix + ".setCustomId", entity, customId); }

}

export type SpawnerDataSafe = {
    sourceKind: EnumValueType<VehicleSourceKind>;
    categorySerialNumber: number;
    categorySerialNumberSet: boolean;
    customId: string;
    totalVehiclesSpawned: number
};

export enum VehicleSourceKind {
    Police,
    Hospital,
    Deathcare,
    FireResponse,
    Garbage,
    PublicTransport,
    CargoTransport,
    Maintenance,
    Post,
    CommercialCompany,
    IndustrialCompany,
    PublicTransport_Taxi,
    PublicTransport_Bus,
    Unknown = 0xffff_fffd,
    TransportCompany = 0xffff_FFFe,
    Other = 0xffff_ffff,
}