import { HighwayData, HighwayRoutesService } from "@klyte45/adr-commons";
import { useEffect, useState } from "react";
import './HighwayListingTab.scss'
import { Cs2FormLine, GameScrollComponent, Input, SimpleInput } from "@klyte45/euis-components";
import { translate } from "#utility/translate";

type HighwayListingProps = {
    onSelectItem: (x: Partial<HighwayData>) => any
}

type MainProps = {
    getSelectionPosition: { x: number, y: number }
}



export function HighwayRegisterManagement({ getSelectionPosition }: MainProps) {
    const [selectedHw, setSelectedHw] = useState<Partial<HighwayData>>()

    useEffect(() => {
        return () => document.querySelectorAll(".pathsOverlay .hwId_hover").forEach(x => x.classList.remove("hwId_hover"))
    }, [])
    if (!selectedHw) {
        return <HighwayListing onSelectItem={setSelectedHw} />
    }

    return <HighwayRegisterForm
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
    getSelectionPosition: { x: number, y: number }
};


function HighwayRegisterForm({ selectedHw, setSelectedHw, onSave, onCancel, getSelectionPosition }: HighwayRegisterFormProps) {
    console.log(selectedHw);
    return <div className="hwEditingForm">
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
            <SimpleInput isValid={(x) => !isNaN(parseFloat(x.replace(",", ".")))} getValue={() => selectedHw.refStartPoint?.[0].toFixed(3) ?? "0.00"} onValueChanged={(x) => setSelectedHw({ ...selectedHw, refStartPoint: [parseFloat(x.replace(",", ".")), selectedHw.refStartPoint?.[1] ?? 0] })} />
            <SimpleInput isValid={(x) => !isNaN(parseFloat(x.replace(",", ".")))} getValue={() => selectedHw.refStartPoint?.[1].toFixed(3) ?? "0.00"} onValueChanged={(x) => setSelectedHw({ ...selectedHw, refStartPoint: [selectedHw.refStartPoint?.[0] ?? 0, parseFloat(x.replace(",", "."))] })} />
            <button className="neutralBtn" disabled={!getSelectionPosition} onClick={() => setSelectedHw({ ...selectedHw, refStartPoint: [getSelectionPosition.x, getSelectionPosition.y] })}>{translate("highwayRegisterEditor.copyFromMapSelection")}</button>
        </Cs2FormLine>
        <div className="hwFormGap" />
        <div className="bottomActions">
            <button className="positiveBtn" onClick={onSave}>{translate("highwayRegisterEditor.save")}</button>
            <button className="negativeBtn" onClick={onCancel}>{translate("highwayRegisterEditor.cancel")}</button>
        </div>
    </div>;
}

function HighwayListing({ onSelectItem }: HighwayListingProps) {
    const [highways, setHighways] = useState([] as HighwayData[]);

    useEffect(() => {
        HighwayRoutesService.listHighwaysRegistered().then((x) => setHighways(x.sort((a, b) => `${a.prefix} ${a.suffix}`.localeCompare(`${b.prefix} ${b.suffix}`) || a.name.localeCompare(b.name))));
        document.querySelectorAll(".pathsOverlay .hwId_hover").forEach(x => x.classList.remove("hwId_hover"))
    }, [])

    return <div className="highwayList">
        <h2>{translate("highwayRegisterEditor.titleList")}</h2>
        {highways.length ?
            <GameScrollComponent parentContainerClass="listContainer" contentClass="listWrapper">{highways.map(x =>
                <div key={x.Id} className="tableItem" onMouseEnter={() => document.querySelectorAll(".pathsOverlay .hwId_" + x.Id).forEach(x => x.classList.add("hwId_hover"))}
                    onMouseLeave={() => document.querySelectorAll(".pathsOverlay .hwId_" + x.Id).forEach(x => x.classList.remove("hwId_hover"))}
                >
                    <div className="data">
                        <div className="title">{`${x.prefix}-${x.suffix} | ${x.name}`}</div>
                        <div className="subTitle">{`[${x.refStartPoint[0].toFixed(2)} ; ${x.refStartPoint[1].toFixed(2)}]`}</div>
                    </div>
                    <div className="actions">
                        <button className="neutralBtn" onClick={() => onSelectItem(x)}>{translate("highwayRegisterEditor.editBtn")}</button>
                    </div>
                </div>)}
            </GameScrollComponent> : <div className="noHighwaysMessage">
                {translate("highwayRegisterEditor.noHighwaysRegisteredMessage")}
            </div>}
        <div className="bottomActions">
            <button className="positiveBtn" onClick={() => onSelectItem({})}>{translate("highwayRegisterEditor.addBtn")}</button>
        </div>
    </div>;
}
