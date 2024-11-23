import { useEffect, useState } from "react";
import { MapDiv } from "./MapDiv"
import './regionEditor.scss'
import { Entity, EnumValueType } from "@klyte45/adr-commons";

const listOutsideConnections = () => engine.call("k45::adr.regions.listOutsideConnections");
const getCityBounds = () => engine.call("k45::adr.regions.getCityBounds");

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


export const RegionEditor = () => {

    const [mapSize, setMapSize] = useState([] as number[]);
    const [mapOffset, setMapOffset] = useState([] as number[]);
    const [outsideConnections, setOutsideConnections] = useState([] as ObjectOutsideConnectionResponse[]);

    useEffect(() => {
        getCityBounds().then((x) => {
            setMapSize([x[3] - x[0], x[4] - x[1], x[5] - x[2]])
            setMapOffset([x[0], x[1], x[2]])
        });
        listOutsideConnections().then(setOutsideConnections);
    }, [])
    return <>
        <div className="mapSide">
            <MapDiv>
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
    </>
}