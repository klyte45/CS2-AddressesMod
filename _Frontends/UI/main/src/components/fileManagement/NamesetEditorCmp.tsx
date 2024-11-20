import { translate } from "#utility/translate";
import { ExtendedSimpleNameEntry, SimpleNameEntry } from "@klyte45/adr-commons";
import { DefaultPanelScreen, GameScrollComponent, Input } from "@klyte45/euis-components";
import { CSSProperties, useState } from "react";

type Props = {
    entryData: SimpleNameEntry
    onBack: () => void,
    onOk: (x: Omit<ExtendedSimpleNameEntry, "ChecksumString">) => void
}



export const NamesetEditorCmp = ({ entryData, onBack, onOk }: Props) => {

    const [namesetData, setNamesetData] = useState(entryData);



    const onEditDone = (x: { target: { value: string; }; }) => {
        const newValues = x.target.value.split("\n").map(z => z.split(";").map(y => y.trim()).filter((x: any) => x))
        return setNamesetData(Object.assign(namesetData, { Values: newValues.map(y => y[0]), ValuesAlternative: newValues.map(y => y[1] || y[0]) }));
    };
    return <DefaultPanelScreen title={translate("namesetEditor.title")} subtitle={translate("namesetEditor.subtitle")} buttonsRowContent={<>
        <button className="negativeBtn " onClick={onBack}>{translate("namesetEditor.cancel")}</button>
        <button className="positiveBtn " onClick={() => onOk(namesetData)}>{translate("namesetEditor.save")}</button>
    </>}>
        <div style={{ textAlign: "center", width: "100%", fontSize: "30rem" } as CSSProperties}>{namesetData.Name.split("/").pop()}</div>
        <div className="fullDivider" />
        <div>
            <Input title={translate("namesetsImport.pathToNameset")} getValue={() => namesetData.Name} onValueChanged={(x) => { setNamesetData(Object.assign(namesetData, { Name: x })); return x; }} />
        </div>
        <GameScrollComponent>
            <textarea
                onBlur={onEditDone}
                style={{ width: "100%", height: Math.max(40, 1.315 * namesetData.Values.length) + "em", minHeight: "100%" }}
                defaultValue={namesetData.Values.map((x, i) => namesetData.ValuesAlternative[i] && namesetData.ValuesAlternative[i] != x ? `${x};${namesetData.ValuesAlternative[i]}` : x).join("\n")}
            />
        </GameScrollComponent>
    </DefaultPanelScreen>;

}

