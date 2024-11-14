import { Entity, ValuableName } from "../../"

export type EnumValueType<E> = {
    value__: E
}

export type SelectedInfoOptions = {
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
    Vehicle
}
export type EntityOption = {
    entity: Entity
    name: ValuableName

}

export type ADRVehicleData = {
    cityOrigin: Entity;
    serialNumber: number;
    calculatedPlate: string;
    manufactureMonthsFromEpoch: number;
}