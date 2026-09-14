import { CityResponseData, Entity, EnumValueType, RegionService } from "adr";
import { VanillaComponentResolver } from "@klyte45/vuio-commons";
import { useEffect, useMemo, useRef, useState, type CSSProperties } from "react";
import { translate } from "../../translate";
import styles from "../../styles/regionEditor.module.scss";
import { MapDiv } from "./MapDiv";
import { HighwayRegisterManagement } from "./subpages/HighwayRegisterManagement";
import { RegionCitiesManagement } from "./subpages/RegionCitiesManagement";

enum OutsideConnectionType {
    Road,
    Rail,
    Pipe,
    Electricity,
    Waterway,
    Airway,
}

enum RegionMapType {
    None,
    Land,
    Water,
    Air,
}

type RegionTabKey = "highways" | "cities";

interface ObjectOutsideConnectionResponse {
    entity: Entity;
    name: string;
    position: number[];
    azimuthDirection: number;
    netEntity: Entity;
    netName: string;
    outsideConnectionType: EnumValueType<OutsideConnectionType>;
}

interface AggregationData {
    entity: Entity;
    name: string;
    curves: [[number, number, number], [number, number, number], [number, number, number], [number, number, number]][];
    nodes: Entity[];
    /** Registry highway id from ADR aggregate; empty/default when not associated. */
    highwayId: string;
}

export type ExtraMapPoint = {
    x: number;
    y: number;
    classNames?: string[];
    id: string;
};

type PointerSnapshot = {
    clientX: number;
    clientY: number;
};

const DEFAULT_MAP_ZOOM = 5 / 7;

export const RegionEditor = () => {
    const [currentTab, setCurrentTab] = useState<RegionTabKey>("highways");
    const [buildIdx, setBuildIdx] = useState(0);
    const [isLoading, setIsLoading] = useState(false);
    const [mapSize, setMapSize] = useState<number[]>([]);
    const [mapOffset, setMapOffset] = useState<number[]>([]);
    const [outsideConnections, setOutsideConnections] = useState<ObjectOutsideConnectionResponse[]>([]);
    const [highways, setHighways] = useState<AggregationData[]>([]);
    const [trainTracks, setTrainTracks] = useState<AggregationData[]>([]);
    const [urbanRoads, setUrbanRoads] = useState<AggregationData[]>([]);
    const [terrainMap, setTerrainMap] = useState<string | null>(null);
    const [waterMap, setWaterMap] = useState<string | null>(null);
    const [regionLand, setRegionLand] = useState<CityResponseData[] | null>(null);
    const [regionWater, setRegionWater] = useState<CityResponseData[] | null>(null);
    const [regionAir, setRegionAir] = useState<CityResponseData[] | null>(null);
    const [selectedRegionMapType, setSelectedRegionMapType] = useState<RegionMapType>(RegionMapType.None);
    const [zoom, setZoom] = useState(DEFAULT_MAP_ZOOM);
    const [position, setPosition] = useState([0, 0]);
    const [mouseInfo, setMouseInfo] = useState<PointerSnapshot>();
    const [mapPointSelectionInfo, setMapPointSelectionInfo] = useState<PointerSnapshot>();
    const [extraMapPoints, setExtraMapPoints] = useState<ExtraMapPoint[]>([]);
    const mapRef = useRef<HTMLDivElement>(null);

    const loadMapData = async () => {
        setIsLoading(true);
        await Promise.all([
            RegionService.getCityBounds().then((x) => {
                setMapSize([x[3] - x[0], x[4] - x[1], x[5] - x[2]]);
                setMapOffset([x[0], x[1], x[2]]);
            }),
            RegionService.listHighways().then(setHighways),
            RegionService.listTrainTracks().then(setTrainTracks),
            RegionService.listUrbanRoads().then(setUrbanRoads),
            RegionService.listOutsideConnections().then(setOutsideConnections),
            RegionService.getCityTerrain().then(setTerrainMap),
            RegionService.getCityWater().then(setWaterMap),
            RegionService.getLandRegionNeighborhood().then(setRegionLand),
            RegionService.getWaterRegionNeighborhood().then(setRegionWater),
            RegionService.getAirRegionNeighborhood().then(setRegionAir),
        ]);
        setBuildIdx((x) => x + 1);
        setIsLoading(false);
    };

    const reloadNeighborhoods = async () => {
        setIsLoading(true);
        await Promise.all([
            RegionService.getLandRegionNeighborhood().then(setRegionLand),
            RegionService.getWaterRegionNeighborhood().then(setRegionWater),
            RegionService.getAirRegionNeighborhood().then(setRegionAir),
        ]);
        setBuildIdx((x) => x + 1);
        setIsLoading(false);
    };

    useEffect(() => {
        void loadMapData();
    }, []);

    const onWheel = (event: React.WheelEvent<HTMLDivElement>) => {
        setZoom((prev) => Math.max(0.25, Math.min(20, prev - event.deltaY * 0.01)));
    };

    const onMouseMove = (event: React.MouseEvent<HTMLDivElement>) => {
        setMouseInfo({ clientX: event.clientX, clientY: event.clientY });
        if (!event.buttons || (!event.movementY && !event.movementX)) {
            return;
        }
        if (event.buttons === 1) {
            const { offsetWidth, offsetHeight } = event.currentTarget;
            setPosition((prev) => [
                Math.max(mapOffset[0], Math.min(mapSize[0] + mapOffset[0], prev[0] + event.movementX * (mapSize[0] / offsetWidth) / zoom)),
                Math.max(mapOffset[2], Math.min(mapSize[2] + mapOffset[2], prev[1] + event.movementY * (mapSize[2] / offsetHeight) / zoom)),
            ]);
        }
    };

    const mapBeyondBordersSizeMultiplier = 4;
    const svgNeighborsDraw = useMemo(
        () => getNeighborhoodMap(mapBeyondBordersSizeMultiplier, selectedRegionMapType, regionLand, regionWater, regionAir, mapSize),
        [buildIdx, mapSize, regionAir, regionLand, regionWater, selectedRegionMapType],
    );

    const currentMouseHoverPosition = useMemo(() => {
        if (mapRef.current && mouseInfo) {
            return mousePositionToWorldPosition(mouseInfo, mapRef.current, mapOffset, mapSize);
        }
        return null;
    }, [mapOffset, mapSize, mouseInfo]);

    const selectionPosition = useMemo(() => {
        if (mapRef.current && mapPointSelectionInfo) {
            return mousePositionToWorldPosition(mapPointSelectionInfo, mapRef.current, mapOffset, mapSize);
        }
        return null;
    }, [mapOffset, mapPointSelectionInfo, mapSize]);

    const listItems = useMemo(() => [
        { displayName: translate("highwayRegisterEditor.tabTitle"), value: "highways" as RegionTabKey },
        { displayName: translate("regionCityEditor.tabTitle"), value: "cities" as RegionTabKey },
    ], []);
    const VR = VanillaComponentResolver.instance;
    const TabBar = VR.TabBar;
    const Tab = VR.Tab;

    if (buildIdx < 1 || isLoading) {
        return <div className={styles.loadmapWait}>{translate("regionSettings.loadmap")}</div>;
    }

    const mapTranslationX = position[0] / mapSize[0] * 100;
    const mapTranslationY = position[1] / mapSize[2] * 100;
    const effZoom = 0.6666 + zoom / 1.5;

    const content = currentTab === "highways"
        ? (
            <HighwayRegisterManagement
                selectionPosition={selectionPosition}
                mapSizeY={mapSize[2]}
                setExtraPoints={setExtraMapPoints}
            />
        )
        : <RegionCitiesManagement onCitiesChanged={() => void reloadNeighborhoods()} />;

    return (
        <div className={styles.regionEditor}>
            <div
                className={`${styles.mapSide} k45_btnHost`}
                style={{ "--currentZoom": effZoom } as CSSProperties}
                onWheel={onWheel}
                onMouseLeave={() => setMouseInfo(undefined)}
                onDoubleClick={(event) => setMapPointSelectionInfo({ clientX: event.clientX, clientY: event.clientY })}
                onMouseMove={onMouseMove}
            >
                <MapDiv
                    ref={mapRef}
                    waterMap={waterMap}
                    cityMap={terrainMap}
                    style={{ transform: `translate(-50%, -50%) scale(${zoom / 20}) translate(${mapTranslationX}%, ${mapTranslationY}%)` }}
                    beforeAnyLayer={svgNeighborsDraw.length > 0 && (
                        <svg
                            viewBox={[mapOffset[0] * mapBeyondBordersSizeMultiplier, mapOffset[2] * mapBeyondBordersSizeMultiplier, mapSize[0] * mapBeyondBordersSizeMultiplier, mapSize[2] * mapBeyondBordersSizeMultiplier].join(" ")}
                            width={`${mapBeyondBordersSizeMultiplier * 100}%`}
                            height={`${mapBeyondBordersSizeMultiplier * 100}%`}
                            style={{ transform: `translate(-${50 - 50 / mapBeyondBordersSizeMultiplier}%, -${50 - 50 / mapBeyondBordersSizeMultiplier}%)` }}
                        >
                            {svgNeighborsDraw}
                        </svg>
                    )}
                >
                    <svg
                        viewBox={[mapOffset[0] * mapBeyondBordersSizeMultiplier, mapOffset[2] * mapBeyondBordersSizeMultiplier, mapSize[0] * mapBeyondBordersSizeMultiplier, mapSize[2] * mapBeyondBordersSizeMultiplier].join(" ")}
                        width={`${mapBeyondBordersSizeMultiplier * 100}%`}
                        height={`${mapBeyondBordersSizeMultiplier * 100}%`}
                        className={styles.pathsOverlay}
                        style={{ transform: `translate(-${50 - 50 / mapBeyondBordersSizeMultiplier}%, -${50 - 50 / mapBeyondBordersSizeMultiplier}%) scaleY(-1)` }}
                    >
                        {urbanRoads.map((item, i) => <path key={`urban_${i}`} d={item.curves.map((curve) => `M ${curve[0][0]} ${curve[0][2]} C ${curve[1][0]} ${curve[1][2]}, ${curve[2][0]} ${curve[2][2]}, ${curve[3][0]} ${curve[3][2]}`).join(" ")} className={[styles.mapUrbanRoads, `hwId_${item.highwayId}`].join(" ")} id={`rd_${item.entity.Index}_${item.entity.Version}`} />)}
                        {trainTracks.map((item, i) => <path key={`train_${i}`} d={item.curves.map((curve) => `M ${curve[0][0]} ${curve[0][2]} C ${curve[1][0]} ${curve[1][2]}, ${curve[2][0]} ${curve[2][2]}, ${curve[3][0]} ${curve[3][2]}`).join(" ")} className={[styles.mapTrainTrack, `hwId_${item.highwayId}`].join(" ")} id={`hw_${item.entity.Index}_${item.entity.Version}`} />)}
                        {highways.map((item, i) => <path key={`highway_${i}`} d={item.curves.map((curve) => `M ${curve[0][0]} ${curve[0][2]} C ${curve[1][0]} ${curve[1][2]}, ${curve[2][0]} ${curve[2][2]}, ${curve[3][0]} ${curve[3][2]}`).join(" ")} className={[styles.mapHighway, `hwId_${item.highwayId}`].join(" ")} id={`tr_${item.entity.Index}_${item.entity.Version}`} />)}
                    </svg>
                    {extraMapPoints.map((item, i) => {
                        const { bottom, left } = asRelativePosition(item, mapOffset, mapSize);
                        return (
                            <div
                                key={`${i}_${item.id}`}
                                className={[styles.extraMapPoint, ...(item.classNames ?? [])].join(" ")}
                                style={{ left: `${left}%`, bottom: `${100 - bottom}%` }}
                            >
                                <div className={styles.extraMapPointLabel}>★</div>
                            </div>
                        );
                    })}
                    {selectionPosition && (() => {
                        const { bottom, left } = asRelativePosition(selectionPosition, mapOffset, mapSize);
                        return <div className={styles.selectedPoint} style={{ left: `${left}%`, bottom: `${100 - bottom}%` }} />;
                    })()}
                    {outsideConnections
                        .filter((item) => ![OutsideConnectionType.Pipe, OutsideConnectionType.Electricity].includes(item.outsideConnectionType.value__))
                        .map((item, i) => {
                            const { bottom, left } = asRelativePosition({ x: item.position[0], y: -item.position[2] }, mapOffset, mapSize);
                            const nameRotationDeg = bottom < 1 ? 45 : bottom > 99 ? -45 : 0;
                            const isAtLeft = left < 1;

                            return (
                                <div
                                    key={`outside_${i}`}
                                    className={[styles.outsideConnectionPoint, styles[OutsideConnectionType[item.outsideConnectionType.value__]]].join(" ")}
                                    style={{ left: `${left}%`, bottom: `${bottom}%` }}
                                >
                                    <div
                                        className={[styles.name, isAtLeft ? styles.leftName : ""].join(" ")}
                                        style={{
                                            transformOrigin: "center left",
                                            transform: `translate(0, ${nameRotationDeg < 22.5 ? "-50%" : "0"}) rotate(${nameRotationDeg}deg)`,
                                        }}
                                    >
                                        {item.name}
                                    </div>
                                </div>
                            );
                        })}
                </MapDiv>
                {currentMouseHoverPosition && <div className={styles.hoverPositionBox}>{`(${currentMouseHoverPosition.x.toFixed(1)} ; ${currentMouseHoverPosition.y.toFixed(1)})`}</div>}
                <div className={styles.selectedPositionBox}>
                    <div className={styles.circleLegend} />
                    {selectionPosition
                        ? `(${selectionPosition.x.toFixed(1)} ; ${selectionPosition.y.toFixed(1)}) | Δ (0;0) = ${Math.sqrt(selectionPosition.x * selectionPosition.x + selectionPosition.y * selectionPosition.y).toFixed(2)}m`
                        : translate("regionSettings.doubleClickToSelectPoint")}
                </div>
                <button type="button" onClick={() => void loadMapData()} className={["neutralBtn", styles.reloadButton].join(" ")}>{translate("regionSettings.reloadMapButton")}</button>
                <button type="button" onClick={() => setSelectedRegionMapType((selectedRegionMapType + 1) % 4)} className={["neutralBtn", styles.toggleMapMode].join(" ")}>{translate(`regionSettings.mapModeName.${selectedRegionMapType}`)}</button>
            </div>
            <div className={styles.dataSide}>
                <div className={styles.regionNav}>
                    <TabBar className={styles.rightPaneTabs}>
                        {listItems.map((item) => (
                            <Tab
                                key={item.value}
                                id={item.value}
                                selectedId={currentTab}
                                className={styles.rightPaneTab}
                                onSelect={(key) => {
                                    setExtraMapPoints([]);
                                    setCurrentTab(key as RegionTabKey);
                                }}
                            >
                                {item.displayName}
                            </Tab>
                        ))}
                    </TabBar>
                    <div className={styles.regionBody}>
                        {content}
                    </div>
                </div>
            </div>
        </div>
    );
};

function mousePositionToWorldPosition(info: PointerSnapshot, mapElement: HTMLDivElement, mapOffset: number[], mapSize: number[]) {
    const bounds = mapElement.getBoundingClientRect();
    const relativeX = (info.clientX - bounds.left) / bounds.width;
    const relativeY = (info.clientY - bounds.top) / bounds.height;
    return {
        x: mapOffset[0] + relativeX * mapSize[0],
        y: mapOffset[2] + (1 - relativeY) * mapSize[2],
    };
}

function getNeighborhoodMap(
    mapBeyondBordersSizeMultiplier: number,
    selectedRegionMapType: RegionMapType,
    regionLand: CityResponseData[] | null,
    regionWater: CityResponseData[] | null,
    regionAir: CityResponseData[] | null,
    mapSize: number[],
) {
    if (!mapSize || mapSize.length < 3) {
        return [];
    }

    let entries: CityResponseData[];
    switch (selectedRegionMapType) {
        case RegionMapType.Land:
            entries = regionLand || [];
            break;
        case RegionMapType.Water:
            entries = regionWater || [];
            break;
        case RegionMapType.Air:
            entries = regionAir || [];
            break;
        default:
            return [];
    }

    entries.sort((a, b) => (a.azimuthAngleEnd % 360) - (b.azimuthAngleEnd % 360));
    const svgPaths = [] as JSX.Element[];
    for (let i = 0; i < entries.length; i++) {
        const entry = entries[i];
        const p1 = pointAtAngle({ x: 0, y: 0 }, entry.azimuthAngleStart - 90, mapSize[0] * 1.1);
        const p2 = pointAtAngle({ x: 0, y: 0 }, entry.azimuthAngleCenter - 90, 100_000);
        const p3 = pointAtAngle({ x: 0, y: 0 }, entry.azimuthAngleEnd - 90, mapSize[0] * 1.1);
        const p1_5 = entry.azimuthAngleStart / 45 % 2 === 1
            ? pointAtAngle({ x: 0, y: 0 }, entry.azimuthAngleStart - 90, mapSize[0] * 6)
            : entry.azimuthAngleStart > 315 || entry.azimuthAngleStart < 45 || (entry.azimuthAngleStart > 135 && entry.azimuthAngleStart < 225)
                ? { x: p1.x, y: p2.y }
                : { x: p2.x, y: p1.y };
        const p2_5 = entry.azimuthAngleEnd / 45 % 2 === 1
            ? pointAtAngle({ x: 0, y: 0 }, entry.azimuthAngleEnd - 90, mapSize[0] * 6)
            : entry.azimuthAngleEnd > 315 || entry.azimuthAngleEnd < 45 || (entry.azimuthAngleEnd > 135 && entry.azimuthAngleEnd < 225)
                ? { x: p3.x, y: p2.y }
                : { x: p2.x, y: p3.y };

        const textPosition = pointAtAngle(
            { x: 0, y: 0 },
            ((entry.azimuthAngleEnd + entry.azimuthAngleStart + (entry.azimuthAngleEnd > entry.azimuthAngleStart ? 0 : 360)) / 2) - 90,
            mapSize[0] / 1.5,
        );

        if (Math.abs(textPosition.x) < mapSize[0] / 2 && Math.abs(textPosition.y) < mapSize[2] / 2) {
            textPosition.y = mapSize[2] * 0.6 * Math.sign(textPosition.y);
            textPosition.x = mapSize[0] * 0.6 * Math.sign(textPosition.x);
        }

        let dominantBaseline = "central";
        if (textPosition.y < -mapSize[2] / 2) {
            dominantBaseline = "hanging";
        } else if (textPosition.y > mapSize[2] / 2) {
            dominantBaseline = "text-before-edge";
        }

        const anchor = p1.x < mapSize[0] / -2 ? "end" : p1.x > mapSize[0] / 2 ? "start" : "middle";

        svgPaths.unshift(
            <path
                key={`${entry.entity.Index}_AREA_${selectedRegionMapType}`}
                d={`M0,0 L ${p1.x} ${p1.y} L ${p1_5.x} ${p1_5.y} L ${p2.x} ${p2.y} L ${p2_5.x} ${p2_5.y} L ${p3.x} ${p3.y} Z`}
                fill={clampRGB({ value: entry.mapColor, min: 0x22, max: 0xcc })}
                className={styles.cityArea}
                strokeWidth={10}
                stroke="black"
            />,
        );
        svgPaths.push(
            <text
                key={`${entry.entity.Index}_NAME_${selectedRegionMapType}`}
                x={textPosition.x}
                y={textPosition.y}
                textAnchor={anchor as "start" | "middle" | "end"}
                dominantBaseline={dominantBaseline as any}
                className={styles.cityName}
            >
                {entry.name}
            </text>,
        );
    }
    return svgPaths;
}

function clampRGB({ value, min = 0, max = 255 }: { value: string; min?: number; max?: number; }) {
    if (value.length !== 7 || value[0] !== "#") {
        throw new Error("Invalid hex color format");
    }
    const factor = (max - min) / 255;
    const r = Math.round(parseInt(value.slice(1, 3), 16) * factor) + min;
    const g = Math.round(parseInt(value.slice(3, 5), 16) * factor) + min;
    const b = Math.round(parseInt(value.slice(5, 7), 16) * factor) + min;

    return `#${r.toString(16).padStart(2, "0")}${g.toString(16).padStart(2, "0")}${b.toString(16).padStart(2, "0")}`;
}

function pointAtAngle(refPoint: { x: number; y: number }, angle: number, distance: number) {
    const radians = angle * Math.PI / 180;
    return {
        x: refPoint.x + distance * Math.cos(radians),
        y: refPoint.y + distance * Math.sin(radians),
    };
}

function asRelativePosition(point2d: { x: number; y: number }, mapOffset: number[], mapSize: number[]) {
    const left = (point2d.x - mapOffset[0]) / mapSize[0] * 100;
    const bottom = 100 - (100 * (point2d.y - mapOffset[2]) / mapSize[2]);
    return { bottom, left };
}
