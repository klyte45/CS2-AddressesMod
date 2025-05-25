import { HighwayData, HighwayRoutesService } from "@klyte45/adr-commons";
import { useEffect, useState } from "react";
import './HighwayListingTab.scss'
import { GameScrollComponent } from "@klyte45/euis-components";
import { translate } from "#utility/translate";

export function HighwayListingTab() {
    const [highways, setHighways] = useState([] as HighwayData[]);

    useEffect(() => {
        HighwayRoutesService.listHighwaysRegistered().then(setHighways);
    }, [])

    return <div className="highwayList">
        <h2>{translate("highwayRegisterEditor.titleList")}</h2>
        {highways.length ?
            <GameScrollComponent parentContainerClass="listContainer" contentClass="listWrapper">{highways.map(x => <div key={x.Id} className="tableItem">
                <div className="data">
                    <div className="title">{`${x.prefix}-${x.suffix} | ${x.name}`}</div>
                    <div className="subTitle">{`[${x.refStartPoint[0].toFixed(1)} ; ${x.refStartPoint[0].toFixed(2)}]`}</div>
                </div>
                <div className="actions">
                    <button className="neutralBtn">{translate("highwayRegisterEditor.editBtn")}</button>
                </div>
            </div>)}
            </GameScrollComponent> : <div className="noHighwaysMessage">
                {translate("highwayRegisterEditor.noHighwaysRegisteredMessage")}
            </div>}
        <div className="bottomActions">
            <button className="positiveBtn">{translate("highwayRegisterEditor.addBtn")}</button>
        </div>
    </div>;
}
