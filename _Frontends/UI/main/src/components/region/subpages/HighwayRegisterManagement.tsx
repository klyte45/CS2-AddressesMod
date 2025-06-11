import { HighwayData, HighwayRoutesService, RegionCityEditingDTO, RegionService } from "@klyte45/adr-commons";
import { useEffect, useMemo, useState } from "react";
import './HighwayListingTab.scss'
import { Cs2FormBoundaries, Cs2FormLine, Input, SimpleInput } from "@klyte45/euis-components";
import { translate } from "#utility/translate";
import { ListItemData, _GenericListing } from "./_GenericListing";
import { createPortal } from "react-dom";
import { getStarPathD } from "#utility/svgutils";

type MainProps = {
    getSelectionPosition: { x: number, y: number }
    mapSizeY: number
}

export function HighwayRegisterManagement({ getSelectionPosition, mapSizeY }: MainProps) {
    const [selectedHw, setSelectedHw] = useState<Partial<HighwayData>>()


    useEffect(() => {
        return () => document.querySelectorAll(".pathsOverlay .hwId_hover").forEach(x => x.classList.remove("hwId_hover"))
    }, [])
    if (!selectedHw) {
        return <HighwayListing onSelectItem={setSelectedHw} mapSizeY={mapSizeY} />
    }

    return <HighwayRegisterForm
        mapSizeY={mapSizeY}
        getSelectionPosition={getSelectionPosition}
        selectedHw={selectedHw}
        setSelectedHw={setSelectedHw}
        onCancel={() => setSelectedHw(undefined)}
        onSave={() => HighwayRoutesService.saveHighwayData(selectedHw as HighwayData).then(() => setSelectedHw(undefined))}
    />
}

type HighwayRegisterFormProps = {
    selectedHw: Partial<HighwayData>;
    setSelectedHw: (x: Partial<HighwayData>) => any;
    onSave: () => any;
    onCancel: () => any,
    getSelectionPosition: { x: number, y: number },
    mapSizeY: number
};


function HighwayRegisterForm({ selectedHw, setSelectedHw, onSave, onCancel, getSelectionPosition, mapSizeY }: HighwayRegisterFormProps) {



    return <><Cs2FormBoundaries className="hwEditingForm">
        <Input title={translate("highwayRegisterEditor.highwayPrefix")}
            getValue={() => selectedHw.prefix} onValueChanged={(x) => {
                setSelectedHw({ ...selectedHw, prefix: x });
                return x;
            }} />
        <Input title={translate("highwayRegisterEditor.highwaySuffix")}
            getValue={() => selectedHw.suffix} onValueChanged={(x) => {
                setSelectedHw({ ...selectedHw, suffix: x });
                return x;
            }} />
        <Input title={translate("highwayRegisterEditor.highwayName")}
            getValue={() => selectedHw.name} onValueChanged={(x) => {
                setSelectedHw({ ...selectedHw, name: x });
                return x;
            }} />
        <Cs2FormLine title={translate("highwayRegisterEditor.startPositionReference")}>
            <SimpleInput isValid={(x) => !isNaN(parseFloat(x?.replace(",", ".")))} getValue={() => selectedHw.refStartPoint?.[0].toFixed(3) ?? "0.00"} onValueChanged={(x) => setSelectedHw({ ...selectedHw, refStartPoint: [parseFloat(x.replace(",", ".")), selectedHw.refStartPoint?.[1] ?? 0] })} />
            <SimpleInput isValid={(x) => !isNaN(parseFloat(x?.replace(",", ".")))} getValue={() => selectedHw.refStartPoint?.[1].toFixed(3) ?? "0.00"} onValueChanged={(x) => setSelectedHw({ ...selectedHw, refStartPoint: [selectedHw.refStartPoint?.[0] ?? 0, parseFloat(x.replace(",", "."))] })} />
            <button className="neutralBtn" disabled={!getSelectionPosition} onClick={() => setSelectedHw({ ...selectedHw, refStartPoint: [getSelectionPosition.x, getSelectionPosition.y] })}>{translate("highwayRegisterEditor.copyFromMapSelection")}</button>
        </Cs2FormLine>
        <div className="formGap" />
        <div className="bottomActions">
            <button className="positiveBtn" onClick={onSave}>{translate("highwayRegisterEditor.save")}</button>
            <button className="negativeBtn" onClick={onCancel}>{translate("highwayRegisterEditor.cancel")}</button>
        </div>
    </Cs2FormBoundaries>
        {createPortal(
            <path key={"HW-" + selectedHw.Id} className="starHw" stroke-width={mapSizeY * .001}
                d={getStarPathD(selectedHw.refStartPoint?.[0] ?? 0, selectedHw.refStartPoint?.[1] ?? 0, mapSizeY * .025)} />,
            document.querySelector("svg.pathsOverlay")!
        )}
    </>;
}


type HighwayListingProps = {
    onSelectItem: (x: Partial<HighwayData>) => any;
    mapSizeY: number;
};

function HighwayListing({ onSelectItem, mapSizeY }: HighwayListingProps) {
    const [highways, setHighways] = useState([] as HighwayData[]);
    const [hoveredHw, setHoveredHw] = useState<HighwayData>();


    useEffect(() => {
        HighwayRoutesService.listHighwaysRegistered().then((x) =>
            setHighways(
                x.sort(
                    (a, b) =>
                        `${a.prefix} ${a.suffix}`.localeCompare(`${b.prefix} ${b.suffix}`) ||
                        a.name.localeCompare(b.name)
                )
            )
        );
        document.querySelectorAll(".pathsOverlay .hwId_hover").forEach((x) => x.classList.remove("hwId_hover"));
    }, []);
    const isReady = useMemo(() => !!document.querySelector("svg.pathsOverlay"), [document.querySelector("svg.pathsOverlay")]);

    const items: ListItemData<HighwayData>[] = highways.map((x) => ({
        key: x.Id,
        title: `${x.prefix}-${x.suffix} | ${x.name}`,
        subTitle: `[${x.refStartPoint[0].toFixed(2)} ; ${x.refStartPoint[1].toFixed(2)}]`,
        actions: [
            {
                label: translate("highwayRegisterEditor.editBtn"),
                onClick: () => onSelectItem(x),
            },
        ],
        onMouseEnter: () => {
            document
                .querySelectorAll(".pathsOverlay .hwId_" + x.Id)
                .forEach((el) => el.classList.add("hwId_hover"))
            setHoveredHw(x);
        },
        onMouseLeave: () => {
            document
                .querySelectorAll(".pathsOverlay .hwId_" + x.Id)
                .forEach((el) => el.classList.remove("hwId_hover")),
                setHoveredHw(undefined);
        }
    }));

    return <>
        <_GenericListing
            title={translate("highwayRegisterEditor.titleList")}
            items={items}
            noItemsMessage={translate("highwayRegisterEditor.noHighwaysRegisteredMessage")}
            onAdd={() => onSelectItem({})}
            addBtnLabel={translate("highwayRegisterEditor.addBtn")}
        />
        {isReady && createPortal(<>
            {hoveredHw && <path key={"HW-" + hoveredHw.Id} className="starHw" stroke-width={mapSizeY * .001} d={getStarPathD(hoveredHw.refStartPoint?.[0] ?? 0, hoveredHw.refStartPoint?.[1] ?? 0, mapSizeY * .025)} />}
        </>,
            document.querySelector("svg.pathsOverlay")!
        )}
    </>
}
