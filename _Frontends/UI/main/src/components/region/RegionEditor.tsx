import { translate } from "#utility/translate";
import { CityResponseData, Entity, EnumValueType, RegionService } from "@klyte45/adr-commons";
import { DefaultPanelScreen, MenuItem } from "@klyte45/euis-components";
import { useEffect, useMemo, useRef, useState } from "react";
import { Tab, TabList, TabPanel, Tabs } from "react-tabs";
import { MapDiv } from "./MapDiv";
import './regionEditor.scss';
import { HighwayRegisterManagement } from "./subpages/HighwayRegisterManagement";
import RegionCitiesManagement from "./subpages/RegionCitiesMangement";


enum OutsideConnectionType {
    Road,
    Rail,
    Pipe,
    Electricity,
    Waterway,
    Airway
}

enum RegionMapType {
    None,
    Land,
    Water,
    Air
}

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
    highwayId: string
}

export type ExtraSvgPath = {
    d: string;
    classNames?: string[];
    style?: React.CSSProperties;
    id: string;
}

export const RegionEditor = () => {

    const [buildIdx, setBuildIdx] = useState(0)
    const [isLoading, setIsLoading] = useState(false)
    const [mapSize, setMapSize] = useState([] as number[]);
    const [mapOffset, setMapOffset] = useState([] as number[]);
    const [outsideConnections, setOutsideConnections] = useState([] as ObjectOutsideConnectionResponse[]);
    const [highways, setHighways] = useState([] as AggregationData[]);
    const [trainTracks, setTrainTracks] = useState([] as AggregationData[]);
    const [urbanRoads, setUrbanRoads] = useState([] as AggregationData[]);
    const [terrainMap, setTerrainMap] = useState(null as string);
    const [waterMap, setWaterMap] = useState(null as string);

    const [regionLand, setRegionLand] = useState(null as CityResponseData[]);
    const [regionWater, setRegionWater] = useState(null as CityResponseData[]);
    const [regionAir, setRegionAir] = useState(null as CityResponseData[]);

    const [selectedRegionMapType, setSelectedRegionMapType] = useState<RegionMapType>(RegionMapType.None);

    const [zoom, setZoom] = useState(1);
    const [position, setPostion] = useState([0, 0]);

    const [mouseInfo, setMouseInfo] = useState<React.MouseEvent>()
    const [mapPointSelectionInfo, setMapPointSelectionInfo] = useState<React.MouseEvent>()
    const [selectedTab, setSelectedTab] = useState(0);
    const [extraSvgPaths, setExtraSvgPaths] = useState<ExtraSvgPath[]>([]);
    const refDiv = useRef<HTMLDivElement>()

    useEffect(() => {
        if (buildIdx == 0) return;
        setIsLoading(true);
        Promise.all([
            RegionService.getCityBounds().then((x) => {
                setMapSize([x[3] - x[0], x[4] - x[1], x[5] - x[2]])
                setMapOffset([x[0], x[1], x[2]])
            }),
            RegionService.listHighways().then(setHighways),
            RegionService.listTrainTracks().then(setTrainTracks),
            RegionService.listUrbanRoads().then(setUrbanRoads),
            RegionService.listOutsideConnections().then(setOutsideConnections),
            RegionService.getCityTerrain().then(setTerrainMap),
            RegionService.getCityWater().then(setWaterMap),
            RegionService.getLandRegionNeighborhood().then(setRegionLand),
            RegionService.getWaterRegionNeighborhood().then(setRegionWater),
            RegionService.getAirRegionNeighborhood().then(setRegionAir)
        ]).then(() => {
            setBuildIdx(buildIdx + 1)
            setIsLoading(false);
        })
    }, [buildIdx == 0])


    const onCitiesChanged = () => {
        setIsLoading(true);
        Promise.all([
            RegionService.getLandRegionNeighborhood().then(setRegionLand),
            RegionService.getWaterRegionNeighborhood().then(setRegionWater),
            RegionService.getAirRegionNeighborhood().then(setRegionAir)
        ]).then(() => {
            setBuildIdx(buildIdx + 1)
            setIsLoading(false);
        })
    };


    const doOnWheel = (x) => {
        setZoom(Math.max(.25, Math.min(20, zoom - x.deltaY * .01)))
    }
    const doOnMouseMove = (x) => {
        setMouseInfo(x);
        if (!x.buttons || (!x.movementY && !x.movementX)) return;
        if (x.buttons == 1) {
            setPostion(
                [
                    Math.max(mapOffset[0], Math.min(mapSize[0] + mapOffset[0], position[0] + x.movementX * (mapSize[0] / x.currentTarget.offsetWidth) / zoom)),
                    Math.max(mapOffset[2], Math.min(mapSize[2] + mapOffset[2], position[1] + x.movementY * (mapSize[2] / x.currentTarget.offsetHeight) / zoom))
                ]
            )
        }
    }
    const mapBeyondBordersSizeMultiplier = 4; //1== no beyond borders, 2 == 2x map size, 4 == 4x map size
    const svgNeighborsDraw = useMemo(() => getNeighborhoodMap(mapBeyondBordersSizeMultiplier), [isLoading, selectedRegionMapType, regionLand, regionWater, regionAir]);

    const currentMouseHoverPosition = useMemo(() => {
        if (refDiv.current && mouseInfo) {
            return mousePositionToWorldPosition(mouseInfo);
        }
        return null;

    }, [mouseInfo, refDiv.current])

    const selectionPosition = useMemo(() => {
        if (refDiv.current && mapPointSelectionInfo) {
            return mousePositionToWorldPosition(mapPointSelectionInfo);
        }
        return null;

    }, [mapPointSelectionInfo, refDiv.current])

    function mousePositionToWorldPosition(info: React.MouseEvent) {
        const currentHoverPosition = { x: 0, y: 0 };
        const bounds = refDiv.current.getBoundingClientRect();
        const posX = (info.clientX - bounds.x - bounds.width * .5);
        const posY = (info.clientY - bounds.y - bounds.height * .5);
        currentHoverPosition.x = posX / 800 / zoom * mapSize[0] - position[0];
        currentHoverPosition.y = -(posY / 800 / zoom * mapSize[2] - position[1]);
        return currentHoverPosition;
    }

    if (buildIdx < 2 || isLoading) {
        if (buildIdx == 0) setTimeout(() => setBuildIdx(buildIdx + 1), 500);
        return <div className="loadmapWait">{translate("regionSettings.loadmap")}</div>;
    }

    const mapTranslationX = position[0] / mapSize[0] * 100;
    const mapTranslationY = position[1] / mapSize[2] * 100;
    const effZoom = .6666 + zoom / 1.5;

    return <div className="regionEditor">
        <div style={{ ["--currentZoom"]: effZoom } as any} className="mapSide" onWheel={doOnWheel} onMouseLeave={() => setMouseInfo(undefined)} onDoubleClick={(x) => setMapPointSelectionInfo(x)}
            onMouseMove={doOnMouseMove} ref={refDiv}>
            <MapDiv waterMap={waterMap} cityMap={terrainMap}
                style={{ transform: `translate(-50%, -50%) scale(${zoom / 20}) translate(${mapTranslationX}%, ${mapTranslationY}%)` }}
                beforeAnyLayer={svgNeighborsDraw.length
                    && <svg viewBox={[mapOffset[0] * mapBeyondBordersSizeMultiplier, mapOffset[2] * mapBeyondBordersSizeMultiplier, mapSize[0] * mapBeyondBordersSizeMultiplier, mapSize[2] * mapBeyondBordersSizeMultiplier].join(" ")}
                        width={mapBeyondBordersSizeMultiplier * 100 + "%"} height={mapBeyondBordersSizeMultiplier * 100 + "%"}
                        style={{ transform: `translate(-${50 - 50 / mapBeyondBordersSizeMultiplier}%, -${50 - 50 / mapBeyondBordersSizeMultiplier}%)` }}>
                        {svgNeighborsDraw}
                    </svg>}
            >

                <svg viewBox={[mapOffset[0] * mapBeyondBordersSizeMultiplier, mapOffset[2] * mapBeyondBordersSizeMultiplier, mapSize[0] * mapBeyondBordersSizeMultiplier, mapSize[2] * mapBeyondBordersSizeMultiplier].join(" ")}
                    width={mapBeyondBordersSizeMultiplier * 100 + "%"} height={mapBeyondBordersSizeMultiplier * 100 + "%"} className="pathsOverlay"
                    style={{ transform: `translate(-${50 - 50 / mapBeyondBordersSizeMultiplier}%, -${50 - 50 / mapBeyondBordersSizeMultiplier}%) scaleY(-1)` }}>
                    {urbanRoads.map((x, i) => <path key={i} d={x.curves.map(x => `M ${x[0][0]} ${x[0][2]} C ${x[1][0]} ${x[1][2]}, ${x[2][0]} ${x[2][2]}, ${x[3][0]} ${x[3][2]}`).join(" ")} className={["mapUrbanRoads", "hwId_" + x.highwayId].join(" ")} id={`rd_${x.entity.Index}_${x.entity.Version}`} />)}
                    {trainTracks.map((x, i) => <path key={i} d={x.curves.map(x => `M ${x[0][0]} ${x[0][2]} C ${x[1][0]} ${x[1][2]}, ${x[2][0]} ${x[2][2]}, ${x[3][0]} ${x[3][2]}`).join(" ")} className={["mapTrainTrack", "hwId_" + x.highwayId].join(" ")} id={`hw_${x.entity.Index}_${x.entity.Version}`} />)}
                    {highways.map((x, i) => <path key={i} d={x.curves.map(x => `M ${x[0][0]} ${x[0][2]} C ${x[1][0]} ${x[1][2]}, ${x[2][0]} ${x[2][2]}, ${x[3][0]} ${x[3][2]}`).join(" ")} className={["mapHighway", "hwId_" + x.highwayId].join(" ")} id={`tr_${x.entity.Index}_${x.entity.Version}`} />)}
                    {extraSvgPaths.map((x, i) => <path key={i + x.id} d={x.d} className={x.classNames?.join(" ")} id={x.id} style={x.style} />)}
                </svg>
                {selectionPosition && (() => {
                    const { bottom, left } = AsRelativePosition(selectionPosition, mapOffset, mapSize);
                    return <div className="selectedPoint"
                        style={{
                            left: left + "%",
                            bottom: 100 - bottom + "%",
                        }} ></div>
                })()}
                {outsideConnections.filter(x => ![OutsideConnectionType.Pipe, OutsideConnectionType.Electricity].includes(x.outsideConnectionType.value__)).map((x, i) => {
                    const { bottom, left } = AsRelativePosition({ x: x.position[0], y: -x.position[2] }, mapOffset, mapSize);

                    const nameRotationDeg = bottom < 1 ? 45 : bottom > 99 ? -45 : 0;
                    const isAtLeft = left < 1;

                    return <div key={i} className={["outsideConnectionPoint", OutsideConnectionType[x.outsideConnectionType.value__]].join(" ")}
                        style={{
                            left: left + "%",
                            bottom: bottom + "%",
                        }} >
                        <div className={["name", isAtLeft && "leftName"].join(" ")} style={{
                            transformOrigin: "center left",
                            transform: "translate(0, " + (nameRotationDeg < 22.5 ? "-50%" : '0') + ") rotate(" + nameRotationDeg + "deg)"
                        }}>{x.name}</div>
                    </div>
                })}
            </MapDiv>
            {currentMouseHoverPosition && <div className="hoverPositionBox">{`(${currentMouseHoverPosition.x.toFixed(1)} ; ${currentMouseHoverPosition.y.toFixed(1)})`}</div>}
            {<div className="selectedPositionBox"><div className="circleLegend" />{selectionPosition ? `(${selectionPosition.x.toFixed(1)} ; ${selectionPosition.y.toFixed(1)}) | Δ (0;0) = ${Math.sqrt(selectionPosition.x * selectionPosition.x + selectionPosition.y * selectionPosition.y).toFixed(2)}m` : translate("regionSettings.doubleClickToSelectPoint")}</div>}
            <button onClick={() => setBuildIdx(0)} className={["neutralBtn", "reloadButton"].join(" ")}>{translate("regionSettings.reloadMapButton")}</button>
            <button onClick={() => setSelectedRegionMapType((selectedRegionMapType + 1) % 4)} className={["neutralBtn", "toggleMapMode"].join(" ")}>{translate("regionSettings.mapModeName." + selectedRegionMapType)}</button>
        </div>
        <div className="dataSide">
            <DefaultPanelScreen title={translate("regionSettings.title")} subtitle={translate("regionSettings.subtitle")} scrollable={false}>
                <RegionalEditorContent getSelectionPosition={selectionPosition} onCitiesChanged={onCitiesChanged} selectedTab={selectedTab} setSelectedTab={setSelectedTab} mapSizeY={mapSize[2]} setExtraPaths={x => setExtraSvgPaths(x?.length ? x : [{ d: "M 0 0", id: "dummy" }])} />
            </DefaultPanelScreen>
        </div>
    </div>

    function getNeighborhoodMap(mapBeyondBordersSizeMultiplier: number) {
        if (!mapSize || mapSize.length < 3) return [];
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
        const svgPaths = [];
        for (let i = 0; i < entries.length; i++) {
            const entry = entries[i];
            const p1 = pointAtAngle({ x: 0, y: 0 }, entry.azimuthAngleStart - 90, mapSize[0] * 1.1);
            const p2 = pointAtAngle({ x: 0, y: 0 }, entry.azimuthAngleCenter - 90, 100_000);
            const p3 = pointAtAngle({ x: 0, y: 0 }, entry.azimuthAngleEnd - 90, mapSize[0] * 1.1);

            const p1_5 = entry.azimuthAngleStart / 45 % 2 === 1 ? pointAtAngle({ x: 0, y: 0 }, entry.azimuthAngleStart - 90, mapSize[0] * 6) : entry.azimuthAngleStart > 315 || entry.azimuthAngleStart < 45 || (entry.azimuthAngleStart > 135 && entry.azimuthAngleStart < 225) ? { x: p1.x, y: p2.y } : { x: p2.x, y: p1.y };
            const p2_5 = entry.azimuthAngleEnd / 45 % 2 === 1 ? pointAtAngle({ x: 0, y: 0 }, entry.azimuthAngleEnd - 90, mapSize[0] * 6) : entry.azimuthAngleEnd > 315 || entry.azimuthAngleEnd < 45 || (entry.azimuthAngleEnd > 135 && entry.azimuthAngleEnd < 225) ? { x: p3.x, y: p2.y } : { x: p2.x, y: p3.y };

            const textPosition = pointAtAngle({ x: 0, y: 0 }, ((entry.azimuthAngleEnd + entry.azimuthAngleStart + (entry.azimuthAngleEnd > entry.azimuthAngleStart ? 0 : 360)) / 2) - 90, mapSize[0] / 1.5)




            if (Math.abs(textPosition.x) < mapSize[0] / 2 && Math.abs(textPosition.y) < mapSize[2] / 2) {
                // if (Math.abs(textPosition.x) / mapSize[0] > Math.abs(textPosition.y) / mapSize[2]) {
                textPosition.y = mapSize[2] * .6 * Math.sign(textPosition.y);
                // } else {
                textPosition.x = mapSize[0] * .6 * Math.sign(textPosition.x);
                // }
            }
            let dominantBaseline = "central";
            if (textPosition.y < -mapSize[2] / 2) {
                dominantBaseline = "hanging";
            } else if (textPosition.y > mapSize[2] / 2) {
                dominantBaseline = "text-before-edge";
            }

            const anchor = p1.x < mapSize[0] / -2 ? "end" : p1.x > mapSize[0] / 2 ? "start" : "middle";

            svgPaths.unshift(<path key={entry.entity.Index + "_AREA_" + selectedRegionMapType} d={`M0,0 L ${p1.x} ${p1.y} L ${p1_5.x} ${p1_5.y} L ${p2.x} ${p2.y} L ${p2_5.x} ${p2_5.y} L ${p3.x} ${p3.y} Z`} fill={clampRGB({ value: entry.mapColor, min: 0x22, max: 0xcc })} className="cityArea" stroke-width="10" stroke="black" />);
            svgPaths.push(<text key={entry.entity.Index + "_NAME_" + selectedRegionMapType} x={textPosition.x} y={textPosition.y} textAnchor={anchor} dominantBaseline={dominantBaseline} className="cityName">{entry.name}</text>);

            {/* 
                <div>
                    <div className={["prev", endPos > 90 ? "after180" : ""].join(" ")} style={{ left: left + "%" }}>
                        {entries[i].name}
                    </div>
                    <div className={["next", endPos > 90 ? "after180" : ""].join(" ")} style={{ left: left + "%" }}>
                        {entries[(i + 1) % entries.length].name}
                    </div>
                </div> 
                */}
        }
        return svgPaths;
    }

}

function clampRGB({ value, min = 0, max = 255 }: { value: string; min?: number; max?: number; }) {
    if (value.length !== 7 || value[0] !== '#') {
        throw new Error("Invalid hex color format");
    }
    const factor = (max - min) / 255;
    const r = Math.round(parseInt(value.slice(1, 3), 16) * factor) + min;
    const g = Math.round(parseInt(value.slice(3, 5), 16) * factor) + min;
    const b = Math.round(parseInt(value.slice(5, 7), 16) * factor) + min;

    return `#${r.toString(16).padStart(2, '0')}${g.toString(16).padStart(2, '0')}${b.toString(16).padStart(2, '0')}`;
}

function lerpPosition(start: { x: number, y: number }, end: { x: number, y: number }, t: number) {
    return {
        x: start.x + (end.x - start.x) * t,
        y: start.y + (end.y - start.y) * t
    };
}

function saturatePosition(position: { x: number, y: number }, mapSize: number[], multiplier: number) {
    return {
        x: Math.max(mapSize[0] * multiplier / -2, Math.min(mapSize[0] * multiplier / 2, position.x)),
        y: Math.max(mapSize[2] * multiplier / -2, Math.min(mapSize[2] * multiplier / 2, position.y))
    };
}

function pointAtAngle(refPoint: { x: number, y: number }, angle: number, distance: number) {
    const radians = angle * Math.PI / 180;
    const newX = refPoint.x + distance * Math.cos(radians);
    const newY = refPoint.y + distance * Math.sin(radians);
    return { x: newX, y: newY };
}
type RegionalEditorContentProps = {
    getSelectionPosition: { x: number, y: number }
    onCitiesChanged: () => void;
    setSelectedTab: (x: number) => void;
    selectedTab: number;
    mapSizeY: number;
    setExtraPaths: (x: ExtraSvgPath[]) => any;
}

function AsRelativePosition(point2d: { x: number, y: number }, mapOffset: number[], mapSize: number[]) {
    const left = ((point2d.x - mapOffset[0]) / mapSize[0] * 100);
    const bottom = 100 - (100 * (point2d.y - mapOffset[2]) / mapSize[2]);
    return { bottom, left };
}

function RegionalEditorContent({ getSelectionPosition, onCitiesChanged, selectedTab, setSelectedTab, mapSizeY, setExtraPaths }: RegionalEditorContentProps) {

    const menus: Omit<MenuItem, 'iconUrl'>[] = [
        {
            name: translate("highwayRegisterEditor.tabTitle"),
            panelContent: <HighwayRegisterManagement getSelectionPosition={getSelectionPosition} mapSizeY={mapSizeY} setExtraPaths={setExtraPaths} />
        },
        {
            name: translate("regionCityEditor.tabTitle"),
            panelContent: <RegionCitiesManagement onCitiesChanged={onCitiesChanged} />
        },
    ]

    return <Tabs className="tabsContainer" onSelect={(x) => {
        setExtraPaths(undefined);
        return setSelectedTab(x);
    }} selectedIndex={selectedTab}>
        <TabList className="horizontalTabStrip">
            {menus.map((x, i) =>
                <Tab key={i} className="horizontalTabStripTab">
                    {x.name}
                </Tab>)}
        </TabList>
        {menus.map((x, i) => <TabPanel className="tabContent" key={i}>{x.panelContent}</TabPanel>)}
    </Tabs>;
}