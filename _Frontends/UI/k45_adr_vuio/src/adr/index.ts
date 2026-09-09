import engine from "cohtml/cohtml";

export type {
    Entity,
    EnumValueType,
    NameCustom,
    NameFormatted,
    ValuableName,
} from "@klyte45/vuio-commons";
export {
    nameToString,
    replaceArgs,
    toEntityTyped,
} from "@klyte45/vuio-commons";

import type {
    Entity,
    EnumValueType,
    NameCustom,
    NameFormatted,
    ValuableName,
} from "@klyte45/vuio-commons";

export class AdrMainService {
    static async isCityOrEditorLoaded(): Promise<boolean> { return await engine.call("k45::adr.main.isCityOrEditorLoaded"); }
}

export type DistrictListItem = {
    Entity: Entity;
    Name: NameCustom | NameFormatted;
    CurrentValue: string;
};

export class DistrictRelativeService {
    static async listAllDistricts(): Promise<DistrictListItem[]> { return await engine.call("k45::adr.district.listAllDistricts"); }
    static async setRoadNamesFile(district: Entity, guid: string): Promise<void> { await engine.call("k45::adr.district.setRoadNamesFile", district, guid); }
    static onDistrictChanged(x: () => void) { return engine.on("k45::adr.district.onDistrictsChanged", x); }
    static offDistrictChanged() { engine.off("k45::adr.district.onDistrictsChanged"); }
}

export type AdrEntityEditorData = {
    name: ValuableName;
};

export class EditorUISystemService {
    static async getEntityData(x: Entity): Promise<AdrEntityEditorData> { return await engine.call("k45::adr.editorUI.getEntityData", x); }
    static async setEntityCustomName(x: Entity, name?: string): Promise<void> { return await engine.call("k45::adr.editorUI.setEntityCustomName", x, name); }
}

export type GitHubTreeItem = {
    path: string;
    mode: string;
    type: "tree";
    sha: string;
    url: string;
};
export type GitHubFileItem = {
    path: string;
    mode: string;
    type: "blob";
    sha: string;
    url: string;
    size: number;
};
export type GitHubItem = GitHubFileItem | GitHubTreeItem;

type GitHubResponseTree = {
    sha: string;
    url: string;
    tree: GitHubItem[];
    truncated: boolean;
};
type GitHubResponseBlob = {
    sha: string;
    nodeId: string;
    size: number;
    url: string;
    content: string;
    encoding: "base64";
};

export type GitHubResponseContainer<T> = {
    success: boolean;
    data: T | null;
    resetTime?: number | null;
};

export class GitHubAddressesFilesService {
    private static readonly ADR_FILES_REPO_URL = "https://api.github.com/repos/klyte45/AddressesFiles/git/trees/master";

    static async listAtTreePoint(url?: string): Promise<GitHubResponseContainer<GitHubItem[]>> {
        try {
            const result = await new Promise<GitHubResponseTree>((resp, err) => {
                const xhr = new XMLHttpRequest();
                try {
                    xhr.onreadystatechange = () => {
                        if (xhr.readyState === 4) {
                            if (Math.floor(xhr.status / 100) === 2) {
                                resp(JSON.parse(xhr.response));
                            } else {
                                err(xhr);
                            }
                        }
                    };
                    xhr.onerror = (x) => {
                        console.log(x);
                        err(xhr);
                    };
                    xhr.open("GET", url ?? this.ADR_FILES_REPO_URL);
                    xhr.send("");
                } catch {
                    err(xhr);
                }
            });
            return {
                success: true,
                data: result.tree.filter((x) => x.type === "tree" || x.path.endsWith(".txt")),
            };
        } catch (err) {
            const xhr = err as XMLHttpRequest;
            const rateResetTime = xhr.getResponseHeader?.("X-Ratelimit-Reset");
            console.log(xhr.getAllResponseHeaders());
            console.log(rateResetTime);
            return {
                success: false,
                data: null,
                resetTime: rateResetTime != null ? parseInt(rateResetTime) : null,
            };
        }
    }

    static async getBlobData(url: string) {
        const result = await new Promise<GitHubResponseBlob>((resp) => {
            const xhr = new XMLHttpRequest();
            xhr.onreadystatechange = () => {
                if (xhr.readyState === 4) {
                    resp(JSON.parse(xhr.response));
                }
            };
            xhr.open("GET", url);
            xhr.send("");
        });

        return (await engine.call("k45::adr.main.atob", result.content)) as string;
    }

    static async goToRepository(): Promise<string> { return await engine.call("k45::adr.namesets.goToGitHubRepo"); }
}

const HIGHWAY_ROUTES_PREFIX = "k45::adr.highwayRoutes.";
export class HighwayRoutesService {
    static async listHighwaysRegistered(): Promise<HighwayData[]> { return await engine.call(HIGHWAY_ROUTES_PREFIX + "listHighwaysRegistered"); }
    static async saveHighwayData(data: HighwayData): Promise<boolean> { return await engine.call(HIGHWAY_ROUTES_PREFIX + "saveHighwayData", data); }
}

export type HighwayData = {
    Id: string;
    prefix: string;
    suffix: string;
    name: string;
    refStartPoint: [number, number];
};

export class NamesetService {
    static doOnCityNamesetsUpdated(event: () => void) { engine.on("k45::adr.namesets.onCityNamesetsChanged", event); }
    static offCityNamesetsUpdated() { engine.off("k45::adr.namesets.onCityNamesetsChanged"); }

    static async goToDiskSimpleNamesFolder(): Promise<void> { return await engine.call("k45::adr.namesets.goToSimpleNamesFolder"); }
    static async updateNameset(IdString: string, Name: string, Values: string[], ValuesAlternative: string[]) { await engine.call("k45::adr.namesets.updateForCity", IdString, Name, Values, ValuesAlternative); }
    static async deleteNamesetFromCity(GuidString: string) { await engine.call("k45::adr.namesets.deleteFromCity", GuidString); }
    static async sendNamesetForCity(name: string, values: string[], ValuesAlternative: string[]) { await engine.call("k45::adr.namesets.addNamesetToCity", name, values, ValuesAlternative); }

    static async listCityNamesets(): Promise<SimpleNameEntry[]> { return await engine.call("k45::adr.namesets.listCityNamesets"); }
    static async listLibraryNamesets(): Promise<SimpleNameEntry[]> { return await engine.call("k45::adr.namesets.listLibraryNamesets"); }
    static async listNamesetFolderFiles(folder: string, allowedExtension: string): Promise<{ displayName: string; directory: boolean; fullPath: string; }[]> {
        return await engine.call("k45::adr.namesets.file.listFiles", folder, allowedExtension);
    }
    static async readNamesetFromDisk(path: string): Promise<SimpleNameEntry> {
        return await engine.call("k45::adr.namesets.readNamesetFromDisk", path);
    }
    static async reloadLibraryNamesets(): Promise<boolean> { return await engine.call("k45::adr.namesets.reloadLibraryNamesets"); }
    static async isLibraryLoading(): Promise<boolean> { return await engine.call("k45::adr.namesets.isLibraryLoading"); }
    static onLibraryReloaded(event: (namesets: SimpleNameEntry[]) => void) { engine.on("k45::adr.namesets.onLibraryReloaded", event); }
    static offLibraryReloaded() { engine.off("k45::adr.namesets.onLibraryReloaded"); }
    static async exportFromCityToLibrary(IdString: string): Promise<string> { return await engine.call("k45::adr.namesets.exportToLibrary", IdString); }
    static async sortValues(values: string[]): Promise<string[]> { return await engine.call("k45::adr.namesets.sortValues", values); }
}

export type SimpleNameEntry = {
    IdString: string;
    Name: string;
    Values: string[];
    ValuesAlternative: string[];
    _CurrName?: string;
};
export type ExtendedSimpleNameEntry = SimpleNameEntry & {
    _CurrName?: string;
};

enum TrackTypes {
    None = 0,
    Train = 1,
    Tram = 2,
    Subway = 4,
}

export enum RoadFlags {
    EnableZoning = 0x1,
    SeparatedCarriageways = 0x2,
    PreferTrafficLights = 0x4,
    DefaultIsForward = 0x8,
    UseHighwayRules = 0x10,
    DefaultIsBackward = 0x20,
    HasStreetLights = 0x40,
    Train = TrackTypes.Train << 16,
    Tram = TrackTypes.Tram << 16,
    Subway = TrackTypes.Subway << 16,
}

export type AdrRoadPrefixRule = {
    MinSpeedKmh: number;
    MaxSpeedKmh: number;
    RequiredFlagsInt: number | RoadFlags;
    ForbiddenFlagsInt: number | RoadFlags;
    FormatPattern: string;
    FullBridge: 0 | -1 | 1;
    AnyElevated: 0 | -1 | 1;
    MinCarLanes: number;
    MaxCarLanes: number;
    MinWidthM: number;
    MaxWidthM: number;
};

export type AdrRoadPrefixSetting = {
    FallbackRule: AdrRoadPrefixRule;
    AdditionalRules: AdrRoadPrefixRule[];
};

export type AdrCitywideSettings = {
    surnameAtFirst: boolean;
    CitizenMaleNameOverridesStr: string;
    CitizenFemaleNameOverridesStr: string;
    CitizenSurnameOverridesStr: string;
    CitizenDogOverridesStr: string;
    DefaultRoadNameOverridesStr: string;
    DefaultDistrictNameOverridesStr: string;
    roadPrefixSetting: AdrRoadPrefixSetting;
    roadNameAsNameStation: boolean;
    roadNameAsNameCargoStation: boolean;
    districtNameAsNameStation: boolean;
    districtNameAsNameCargoStation: boolean;
    MaximumGeneratedSurnames: number;
    MaximumGeneratedGivenNames: number;
    AddressFormatPattern: string;
};

export class NamingRulesService {
    static onCityDataReloaded(x: () => void) { return engine.on("k45::adr.main.onCurrentCitywideSettingsLoaded", x); }
    static offCityDataReloaded() { engine.off("k45::adr.main.onCurrentCitywideSettingsLoaded"); }

    static async getCurrentCitywideSettings(): Promise<AdrCitywideSettings> { return await engine.call("k45::adr.main.getCurrentCitywideSettings"); }
    static async setSurnameAtFirst(x: boolean): Promise<void> { await engine.call("k45::adr.main.setSurnameAtFirst", x); }
    static async setCitizenMaleNameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setCitizenMaleNameOverridesStr", x); }
    static async setCitizenFemaleNameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setCitizenFemaleNameOverridesStr", x); }
    static async setCitizenSurnameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setCitizenSurnameOverridesStr", x); }
    static async setCitizenDogOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setCitizenDogOverridesStr", x); }
    static async setDefaultRoadNameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setDefaultRoadNameOverridesStr", x); }
    static async setAdrRoadPrefixSetting(x: AdrRoadPrefixSetting): Promise<void> { await engine.call("k45::adr.main.setAdrRoadPrefixSetting", x); }
    static async setDefaultDistrictNameOverridesStr(x: string): Promise<void> { await engine.call("k45::adr.main.setDefaultDistrictNameOverridesStr", x); }
    static async setRoadNameAsNameStation(x: boolean): Promise<void> { await engine.call("k45::adr.main.setRoadNameAsNameStation", x); }
    static async setRoadNameAsNameCargoStation(x: boolean): Promise<void> { await engine.call("k45::adr.main.setRoadNameAsNameCargoStation", x); }
    static async setAddressFormatPattern(x: string): Promise<void> { await engine.call("k45::adr.main.setAddressFormatPattern", x); }

    static async setMaxSurnames(x: number): Promise<string> { return await engine.call("k45::adr.main.setMaxSurnames", x); }
    static async setMaxGivenNames(x: number): Promise<string> { return await engine.call("k45::adr.main.setMaxGivenNames", x); }

    static async exploreToRoadPrefixRulesFileDefault(): Promise<void> { await engine.call("k45::adr.main.exploreToRoadPrefixRulesFileDefault"); }
    static async saveRoadPrefixRulesFileDefault(): Promise<void> { await engine.call("k45::adr.main.saveRoadPrefixRulesFileDefault"); }
    static async loadRoadPrefixRulesFileDefault(): Promise<number> { return await engine.call("k45::adr.main.loadRoadPrefixRulesFileDefault"); }
}

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
};

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

export type SelectedInfoOptions = {
    targetEntityToName: Entity;
    type: EnumValueType<AdrEntityType>;
    entityValue: Entity;
    buildingsOptions: EntityOption[];
    roadAggegateOptions: EntityOption[];
    allowDistrict: boolean;
    districtRef: Entity;
    hasCustomNameList?: boolean;
    customNameListName?: string;
};

export enum AdrEntityType {
    CustomName = -1,
    None = 0,
    PublicTransportStation,
    CargoTransportStation,
    RoadAggregation,
    District,
    Vehicle,
    RoadMark,
    VehicleSource,
}

export type EntityOption = {
    entity: Entity;
    name: ValuableName;
};

export type ADRVehicleData = {
    plateCategory: EnumValueType<VehiclePlateCategory>;
    cityOrigin: Entity;
    serialNumber: number;
    calculatedPlate: string;
    manufactureMonthsFromEpoch: number;
    ownerSerialNumber: number;
    hasOwner: boolean;
};

export enum VehiclePlateCategory {
    Road,
    Air,
    Water,
    Rail,
}

export class SelectInfoPanelService {
    static async getEntityOptions(x: Entity): Promise<SelectedInfoOptions[]> { return await engine.call("k45::adr.selectionPanel.getEntityOptions", x); }
    static async setEntityNamingRef(x: Entity, ref: Entity, origin: Entity): Promise<boolean> { return await engine.call("k45::adr.selectionPanel.setEntityRoadReference", x, ref, origin); }
    static async redrawSeed(x: Entity): Promise<boolean> { return await engine.call("k45::adr.selectionPanel.redrawSeed", x); }
    static async changeSeedByDelta(x: Entity, ref: number): Promise<boolean> { return await engine.call("k45::adr.selectionPanel.changeSeedByDelta", x, ref); }
}

const VEHICLE_PREFIX = "k45::adr.vehicles.";
export class VehicleService {
    static async getAdrData(e: Entity): Promise<ADRVehicleData> { return await engine.call(VEHICLE_PREFIX + "getAdrData", e); }
}
