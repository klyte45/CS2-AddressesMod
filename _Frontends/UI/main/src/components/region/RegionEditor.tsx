import { translate } from "#utility/translate";
import { Entity, EnumValueType } from "@klyte45/adr-commons";
import { DefaultPanelScreen, MenuItem } from "@klyte45/euis-components";
import { useCallback, useEffect, useRef, useState } from "react";
import { Tab, TabList, TabPanel, Tabs } from "react-tabs";
import { HighwayRegisterManagement } from "./HighwayRegisterManagement";
import { MapDiv } from "./MapDiv";
import './regionEditor.scss';

const listOutsideConnections = () => engine.call("k45::adr.regions.listOutsideConnections");
const getCityBounds = () => engine.call("k45::adr.regions.getCityBounds");
const listHighways = () => engine.call("k45::adr.regions.listHighways");
const listTrainTracks = () => engine.call("k45::adr.regions.listTrainTracks");
const listUrbanRoads = () => engine.call("k45::adr.regions.listUrbanRoads");
const getCityTerrain = (): Promise<string> => engine.call("k45::adr.regions.getCityTerrain");
const getCityWater = (): Promise<string> => engine.call("k45::adr.regions.getCityWater");
const getCityWaterPollution = (): Promise<string> => engine.call("k45::adr.regions.getCityWaterPollution");

enum OutsideConnectionType {
    Road,
    Rail,
    Pipe,
    Electricity,
    Waterway,
    Airway
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

    const [zoom, setZoom] = useState(1);
    const [position, setPostion] = useState([0, 0]);

    const [mouseInfo, setMouseInfo] = useState<any>()
    const refDiv = useRef<HTMLDivElement>()

    useEffect(() => {
        if (buildIdx == 0) return;
        setIsLoading(true);
        Promise.all([
            // getCityBounds().then((x) => {
            //     setMapSize([x[3] - x[0], x[4] - x[1], x[5] - x[2]])
            //     setMapOffset([x[0], x[1], x[2]])
            // }),
            // listHighways().then(setHighways),
            // listTrainTracks().then(setTrainTracks),
            // listUrbanRoads().then(setUrbanRoads),
            // listOutsideConnections().then(setOutsideConnections),
            // getCityTerrain().then(setTerrainMap),
            // getCityWater().then(setWaterMap)
        ]).then(() => {
            setBuildIdx(buildIdx + 1)
            setIsLoading(false);
        })
    }, [buildIdx == 0])


    const doOnWheel = (x) => {
        setZoom(Math.max(1, Math.min(20, zoom - x.deltaY * .01)))
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

    const getCurrentMouseHoverPosition = useCallback(() => {
        if (refDiv.current && mouseInfo) {
            const currentHoverPosition = { x: 0, y: 0 }
            const bounds = refDiv.current.getBoundingClientRect();
            const posX = (mouseInfo.clientX - bounds.x - bounds.width * .5);
            const posY = (mouseInfo.clientY - bounds.y - bounds.height * .5);
            currentHoverPosition.x = posX / 800 / zoom * mapSize[0] - position[0];//(mouseInfo.clientX - bounds.x - bounds.width * .5 + position[0]) * (mapSize[0] / bounds.width) / zoomValue
            currentHoverPosition.y = posY / 800 / zoom * mapSize[2] - position[1];//(mouseInfo.clientY - bounds.y - bounds.height * .5 + position[1]) * (mapSize[2] / bounds.height) / zoomValue
            return currentHoverPosition;
        }
        return null;
    }, [mouseInfo, refDiv.current])


    if (buildIdx < 2 || isLoading) {
        if (buildIdx == 0) setTimeout(() => setBuildIdx(buildIdx + 1), 500);
        return <div>Loading...</div>;
    }




    return <div className="regionEditor">
        <div className="mapSide" onWheel={doOnWheel} onMouseOut={() => setMouseInfo(undefined)} onMouseMove={doOnMouseMove} style={{ ["--currentZoom"]: .6666 + zoom / 1.5 } as any} ref={refDiv}>
            <MapDiv waterMap={waterMap} cityMap={terrainMap}
                style={{ transform: `translate(-50%, -50%) scale(${zoom / 20}) translate(${position[0] / mapSize[0] * 100}%, ${position[1] / mapSize[2] * 100}%)` }}
            >
                <svg viewBox={[mapOffset[0], mapOffset[2], mapSize[0], mapSize[2]].join(" ")} className="pathsOverlay" width="100%" height="100%" >
                    {urbanRoads.map((x, i) => <path key={i} d={x.curves.map(x => `M ${x[0][0]} ${x[0][2]} C ${x[1][0]} ${x[1][2]}, ${x[2][0]} ${x[2][2]}, ${x[3][0]} ${x[3][2]}`).join(" ")} className="mapUrbanRoads" id={`rd_${x.entity.Index}_${x.entity.Version}`} />)}
                    {trainTracks.map((x, i) => <path key={i} d={x.curves.map(x => `M ${x[0][0]} ${x[0][2]} C ${x[1][0]} ${x[1][2]}, ${x[2][0]} ${x[2][2]}, ${x[3][0]} ${x[3][2]}`).join(" ")} className="mapTrainTrack" id={`hw_${x.entity.Index}_${x.entity.Version}`} />)}
                    {highways.map((x, i) => <path key={i} d={x.curves.map(x => `M ${x[0][0]} ${x[0][2]} C ${x[1][0]} ${x[1][2]}, ${x[2][0]} ${x[2][2]}, ${x[3][0]} ${x[3][2]}`).join(" ")} className="mapHighway" id={`tr_${x.entity.Index}_${x.entity.Version}`} />)}
                </svg>
                {outsideConnections.filter(x => ![OutsideConnectionType.Pipe, OutsideConnectionType.Electricity].includes(x.outsideConnectionType.value__)).map((x, i) => {
                    const left = ((x.position[0] - mapOffset[0]) / mapSize[0] * 100);
                    const bottom = (100 * (x.position[2] - mapOffset[2]) / mapSize[2]);

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
            {getCurrentMouseHoverPosition() && <div className="hoverPositionBox">{`(${getCurrentMouseHoverPosition().x.toFixed(1)} ; ${getCurrentMouseHoverPosition().y.toFixed(1)})`}</div>}
        </div>
        <div className="dataSide">
            <DefaultPanelScreen title={translate("regionSettings.title")} subtitle={translate("regionSettings.subtitle")} scrollable={false}>
                <RegionalEditorContent />
            </DefaultPanelScreen>
        </div>
    </div>
}

function RegionalEditorContent() {

    const menus: Omit<MenuItem, 'iconUrl'>[] = [
        {
            name: translate("highwayRegisterEditor.tabTitle"),
            panelContent: <HighwayRegisterManagement />
        },

    ]

    return <Tabs className="tabsContainer">
        <TabList className="horizontalTabStrip">
            {menus.map((x, i) =>
                <Tab key={i} className="horizontalTabStripTab">
                    {x.name}
                </Tab>)}
        </TabList>
        {menus.map((x, i) => <TabPanel className="tabContent" key={i}>{x.panelContent}</TabPanel>)}
    </Tabs>;
}