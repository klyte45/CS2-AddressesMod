import { ExtendedSimpleNameEntry, SimpleNameEntry } from '#service/NamingRulesService';
import { translate } from "#utility/translate";
import { GameScrollComponent, Input } from "@klyte45/euis-components";
import { CSSProperties, Component } from "react";

type State = {
    namesetData: Mutable<Omit<ExtendedSimpleNameEntry, "ChecksumString">>,
    editingIndex: number
}
type Mutable<Type> = {
    -readonly [Key in keyof Type]: Type[Key];
};

type Props = {
    entryData: SimpleNameEntry
    onBack: () => void,
    onOk: (x: State) => void
}



export default class NamesetEditorCmp extends Component<Props, State> {

    constructor(props: Props | Readonly<Props>) {
        super(props);
        this.state = {
            namesetData: {
                IdString: props.entryData.IdString,
                Values: [].concat(props.entryData.Values),
                Name: props.entryData.Name,
            },
            editingIndex: -1
        }
    }

    render() {
        return <>
            <h1>{translate("namesetEditor.title")}</h1>
            <h3>{translate("namesetEditor.subtitle")}</h3>
            <section style={{ overflow: "hidden", position: "absolute", bottom: this.props.onBack ? 52 : 0, left: 5, right: 5, top: 107 }}>
                <div style={{ textAlign: "center", width: "100%", fontSize: "30rem" } as CSSProperties}>{this.state.namesetData.Name.split("/").pop()}</div>
                <div className="fullDivider" />
                <div>
                    <Input title={translate("namesetsImport.pathToNameset")} getValue={() => this.state.namesetData.Name} onValueChanged={(x) => { this.setState({ namesetData: Object.assign(this.state.namesetData, { Name: x }) }); return x; }} />
                </div>
                <GameScrollComponent>
                    <textarea
                        onBlur={(x) => this.setState({ namesetData: Object.assign(this.state.namesetData, { Values: x.target.value.split("\n").map(x => x.trim()) }) })}
                        style={{ width: "100%", height: Math.max(40, 1.315 * this.state.namesetData.Values.length) + "em", minHeight: "100%" }}
                        defaultValue={this.state.namesetData.Values.join("\n")}
                    />
                </GameScrollComponent>
            </section>
            <div style={{ display: "flex", position: "absolute", left: 5, right: 5, bottom: 5, flexDirection: "row-reverse" }}>
                <button className="negativeBtn " onClick={this.props.onBack}>{translate("namesetEditor.cancel")}</button>
                <button className="positiveBtn " onClick={() => this.props.onOk(this.state)}>{translate("namesetEditor.save")}</button>
            </div>

        </>;
    }
}

