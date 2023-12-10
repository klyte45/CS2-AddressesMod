import { NamesetService } from "#service/NamesetService";
import { ArrayUtils } from "@klyte45/euis-components";
import { translate } from "#utility/translate"
import { Component } from "react";
import NamesetDeletingCmp from "./NamesetDeletingCmp";
import NamesetEditorCmp from "./NamesetEditorCmp";
import NamesetImportingCmp from "./NamesetImportingCmp";
import { ExtendedSimpleNameEntry } from "#service/NamingRulesService";
import { categorizeFiles } from "#utility/categorizeFiles";
import { NamesetCategoryCmp } from "./NamesetCategoryCmp";
import { NamesetLineViewer } from "./NamesetLineViewer";
import NamesetLibrarySelectorCmp from "./NamesetLibrarySelectorCmp";

enum Screen {
    DEFAULT,
    PALETTE_IMPORT_LIB,
    IMPORTING_PALETTE,
    AWAITING_ACTION,
    DELETE_CONFIRM,
    EDIT_PALETTE
}

type State = {
    availableNamesets: NamesetStructureTreeNode,
    currentScreen: Screen,
    namesetBeingImported?: ExtendedSimpleNameEntry,
    namesetBeingDeleted?: ExtendedSimpleNameEntry,
    namesetBeingEdited?: ExtendedSimpleNameEntry,
}

export type NamesetStructureTreeNode = {
    rootContent: ExtendedSimpleNameEntry[],
    subtrees: Record<string, NamesetStructureTreeNode>
}



export default class CityNamesetLibraryCmp extends Component<any, State> {

    constructor(props) {
        super(props);
        this.state = {
            availableNamesets: { subtrees: {}, rootContent: [] },
            currentScreen: Screen.DEFAULT
        }
    }
    componentDidMount() {
        engine.whenReady.then(async () => {
            this.updateNamesets();
            NamesetService.doOnCityNamesetsUpdated(() => this.updateNamesets())
        })
    }
    private async updateNamesets() {
        const namesetsSaved = await NamesetService.listCityNamesets();
        const namesetTree = categorizeFiles(namesetsSaved)
        const root = namesetTree[""]?.rootContent ?? []
        delete namesetTree[""];
        this.setState({
            availableNamesets: {
                rootContent: root,
                subtrees: namesetTree
            }
        });
    }

    render() {
        switch (this.state.currentScreen) {
            case Screen.DEFAULT:
                return <>
                    <h1>{translate("cityNamesetsLibrary.title")}</h1>
                    <h3>{translate("cityNamesetsLibrary.subtitle")}</h3>
                    <section style={{ overflow: "scroll", position: "absolute", bottom: 52, left: 5, right: 5, top: 107 }}>
                        {Object.keys(this.state?.availableNamesets.subtrees ?? {}).length == 0 && !this.state?.availableNamesets.rootContent.length
                            ? <h2>{translate("cityNamesetsLibrary.noNamesetsMsg")} <a onClick={() => this.setState({ currentScreen: Screen.PALETTE_IMPORT_LIB })}>{translate("cityNamesetsLibrary.clickToImport")}</a> <a onClick={() => this.goToEdit()}>{translate("cityNamesetsLibrary.clickToCreate")}</a></h2>
                            : <NamesetCategoryCmp entry={this.state?.availableNamesets} doWithNamesetData={(x, i) => <NamesetLineViewer entry={x} key={i} actionButtons={(y) => this.getActionButtons(y)} />} />}
                    </section>
                    <div style={{ display: "flex", position: "absolute", left: 5, right: 5, bottom: 5, flexDirection: "row-reverse" }}>
                        <button className="positiveBtn " onClick={() => this.setState({ currentScreen: Screen.PALETTE_IMPORT_LIB })}>{translate("cityNamesetsLibrary.importFromLibrary")}</button>
                        <button className="positiveBtn " onClick={() => this.goToEdit()}>{translate("cityNamesetsLibrary.createNewNameset")}</button>
                    </div>
                </>;
            case Screen.PALETTE_IMPORT_LIB:
                return <NamesetLibrarySelectorCmp onBack={() => this.setState({ currentScreen: Screen.DEFAULT })} actionButtons={(p) => <><button className="positiveBtn" onClick={() => this.goToImportDetails(p)}>{translate('cityNamesetsLibrary.copyToCity')}</button></>} />
            case Screen.IMPORTING_PALETTE:
                return <NamesetImportingCmp namesetData={this.state.namesetBeingImported} onBack={() => this.setState({ currentScreen: Screen.PALETTE_IMPORT_LIB })} onOk={(x) => this.doImportNameset(x)} />
            case Screen.AWAITING_ACTION:
                return <div>PLEASE WAIT</div>
            case Screen.DELETE_CONFIRM:
                return <NamesetDeletingCmp onBack={() => this.setState({ currentScreen: Screen.DEFAULT })} onOk={(x) => this.doDelete(x)} namesetData={this.state.namesetBeingDeleted} />
            case Screen.EDIT_PALETTE:
                return <NamesetEditorCmp onBack={() => this.setState({ currentScreen: Screen.DEFAULT })} onOk={(x) => this.doUpdate(x.namesetData)} entryData={this.state.namesetBeingEdited} />
        }
    }
    getActionButtons(x: ExtendedSimpleNameEntry): JSX.Element {
        return <>
            <button className="negativeBtn" onClick={() => this.goToDelete(x)}>{translate("cityNamesetsLibrary.deleteNameset")}</button>
            <button className="neutralBtn" onClick={() => this.goToEdit(x)}>{translate("cityNamesetsLibrary.editNameset")}</button>
        </>
    }
    goToEdit(x?: ExtendedSimpleNameEntry): void {
        this.setState({
            namesetBeingEdited: x ?? { Values: [], Name: "<?>", IdString: null },
            currentScreen: Screen.EDIT_PALETTE
        })
    }
    goToDelete(x: ExtendedSimpleNameEntry): void {
        this.setState({
            namesetBeingDeleted: x,
            currentScreen: Screen.DELETE_CONFIRM
        });
    }

    async doDelete(x: ExtendedSimpleNameEntry) {
        await new Promise((resp) => this.setState({ currentScreen: Screen.AWAITING_ACTION }, () => resp(undefined)));
        await NamesetService.deleteNamesetFromCity(x.IdString);
        this.setState({ currentScreen: Screen.DEFAULT });
    }

    async doImportNameset({ namesetData, namesetNameImport }: { namesetData: ExtendedSimpleNameEntry; namesetNameImport: string; }) {
        await new Promise((resp) => this.setState({ currentScreen: Screen.AWAITING_ACTION }, () => resp(undefined)));
        await NamesetService.sendNamesetForCity(namesetNameImport, namesetData.Values);
        this.setState({ currentScreen: Screen.DEFAULT });
    }

    async doUpdate(namesetData: Omit<ExtendedSimpleNameEntry, "ChecksumString">) {
        await new Promise((resp) => this.setState({ currentScreen: Screen.AWAITING_ACTION }, () => resp(undefined)));
        if (namesetData.IdString) {
            await NamesetService.updateNameset(namesetData.IdString, namesetData.Name, namesetData.Values);
        } else {
            await NamesetService.sendNamesetForCity(namesetData.Name, namesetData.Values)
        }
        this.setState({ currentScreen: Screen.DEFAULT });
    }
    goToImportDetails(p: ExtendedSimpleNameEntry): void {
        this.setState({
            namesetBeingImported: p,
            currentScreen: Screen.IMPORTING_PALETTE
        });

    }
}



