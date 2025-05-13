import { Entity, ValuableName } from "../../"

export type EnumValueType<E> = {
    value__: E
}

export type SelectedInfoOptions = {
    targetEntityToName: Entity
    type: EnumValueType<AdrEntityType>
    entityValue: Entity
    buildingsOptions: EntityOption[]
    roadAggegateOptions: EntityOption[]
    allowDistrict: boolean
    districtRef: Entity
    hasCustomNameList?: boolean
    customNameListName?: string

}
export enum AdrEntityType {
    CustomName = -1,
    None = 0,
    PublicTransportStation,
    CargoTransportStation,
    RoadAggregation,
    District,
    Vehicle,
    RoadMark
}
export type EntityOption = {
    entity: Entity
    name: ValuableName

}

export type ADRVehicleData = {
    plateCategory: EnumValueType<VehiclePlateCategory>
    cityOrigin: Entity;
    serialNumber: number;
    calculatedPlate: string;
    manufactureMonthsFromEpoch: number;
}

export enum VehiclePlateCategory
{
    Road,
    Air,
    Water,
    Rail
}