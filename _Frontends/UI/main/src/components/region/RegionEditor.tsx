import { useEffect, useState } from "react";
import { MapDiv } from "./MapDiv"
import './regionEditor.scss'
import { Entity, EnumValueType } from "@klyte45/adr-commons";

const listOutsideConnections = () => engine.call("k45::adr.regions.listOutsideConnections");
const getCityBounds = () => engine.call("k45::adr.regions.getCityBounds");
const listHighways = () => engine.call("k45::adr.regions.listHighways");
const listTrainTracks = () => engine.call("k45::adr.regions.listTrainTracks");
const listUrbanRoads = () => engine.call("k45::adr.regions.listUrbanRoads");

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

    const [mapSize, setMapSize] = useState([] as number[]);
    const [mapOffset, setMapOffset] = useState([] as number[]);
    const [outsideConnections, setOutsideConnections] = useState([] as ObjectOutsideConnectionResponse[]);
    const [highways, setHighways] = useState([] as AggregationData[]);
    const [trainTracks, setTrainTracks] = useState([] as AggregationData[]);
    const [urbanRoads, setUrbanRoads] = useState([] as AggregationData[]);

    useEffect(() => {
        getCityBounds().then((x) => {
            setMapSize([x[3] - x[0], x[4] - x[1], x[5] - x[2]])
            setMapOffset([x[0], x[1], x[2]])
        });
        listHighways().then(setHighways);
        listTrainTracks().then(setTrainTracks);
        listUrbanRoads().then(setUrbanRoads);
        listOutsideConnections().then(setOutsideConnections);
    }, [])
    return <div className="regionEditor">
        <div className="mapSide">{/*translate(-50%, -50%) scale(10) translate(-21%, 23%)*/}
            <MapDiv>
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
        </div>
        <div className="dataSide">

        </div>
    </div>
}