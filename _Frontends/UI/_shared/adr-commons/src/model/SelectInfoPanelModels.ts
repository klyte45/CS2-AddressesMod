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
}
export enum AdrEntityType {
    None = 0,
    PublicTransportStation,
    CargoTransportStation,
    RoadAggregation
}
export type EntityOption = {
    entity: Entity
    name: ValuableName

}