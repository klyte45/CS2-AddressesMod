import { Entity } from "../../../../_replacements/euis-components/src/utility/Entity";

export class RegionService {
    static listOutsideConnections() {
        return engine.call("k45::adr.regions.listOutsideConnections");
    }
    static getCityBounds() {
        return engine.call("k45::adr.regions.getCityBounds");
    }
    static listHighways() {
        return engine.call("k45::adr.regions.listHighways");
    }
    static listTrainTracks() {
        return engine.call("k45::adr.regions.listTrainTracks");
    }
    static listUrbanRoads() {
        return engine.call("k45::adr.regions.listUrbanRoads");
    }
    static getCityTerrain(): Promise<string> {
        return engine.call("k45::adr.regions.getCityTerrain");
    }
    static getCityWater(): Promise<string> {
        return engine.call("k45::adr.regions.getCityWater");
    }
    static getCityWaterPollution(): Promise<string> {
        return engine.call("k45::adr.regions.getCityWaterPollution");
    }
    static listAllRegionCities(): Promise<RegionCityEditingDTO[]> {
        return engine.call("k45::adr.regions.listAllRegionCities");
    }
    static saveRegionCity(cityData: RegionCityEditingDTO): Promise<void> {
        return engine.call("k45::adr.regions.saveRegionCity", cityData);
    }
    static removeRegionCity(entity: Entity): Promise<void> {
        return engine.call("k45::adr.regions.removeRegionCity", entity);
    }
    static getLandRegionNeighborhood(): Promise<CityResponseData[]> {
        return engine.call("k45::adr.regions.getLandRegionNeighborhood");
    }
    static getWaterRegionNeighborhood(): Promise<CityResponseData[]> {
        return engine.call("k45::adr.regions.getWaterRegionNeighborhood");
    }
    static getAirRegionNeighborhood(): Promise<CityResponseData[]> {
        return engine.call("k45::adr.regions.getAirRegionNeighborhood");
    }
}
export type CityResponseData = {
    name: string;
    azimuthAngleStart: number;
    azimuthAngleCenter: number;
    azimuthAngleEnd: number;
    reachableByLand: boolean;
    reachableByWater: boolean;
    reachableByAir: boolean;
    mapColor: string;
    entity: Entity;
}

export type RegionCityEditingDTO = {
    entity: Entity;
    name: string;
    centerAzimuth: number;
    degreesLeft: number;
    degreesRight: number;
    reachableByLand: boolean;
    reachableByWater: boolean;
    reachableByAir: boolean;
    mapColor: `#${string}`;
};
