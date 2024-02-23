import { NamesetService } from "@klyte45/adr-commons";
import { ArrayUtils, GameScrollComponent } from "@klyte45/euis-components";
import { translate } from "#utility/translate"
import { Component } from "react";
import NamesetDeletingCmp from "./NamesetDeletingCmp";
import NamesetEditorCmp from "./NamesetEditorCmp";
import NamesetImportingCmp from "./NamesetImportingCmp";
import { ExtendedSimpleNameEntry } from "@klyte45/adr-commons";
import { categorizeFiles } from "#utility/categorizeFiles";
import { NamesetCategoryCmp } from "./NamesetCategoryCmp";
import { NamesetLineViewer } from "./NamesetLineViewer";
import NamesetLibrarySelectorCmp from "./NamesetLibrarySelectorCmp";
import { GitHubAddressesFilesSevice, GitHubFileItem } from "@klyte45/adr-commons";
import NamesetGitHubSelectorCmp from "./NamesetGitHubSelectorCmp";

enum Screen {
    DEFAULT,
    NAMESET_IMPORT_LIB,
    IMPORTING_NAMESET,
    AWAITING_ACTION,
    DELETE_CONFIRM,
    EDIT_NAMESET,
    NAMESET_IMPORT_GITHUB,
}

type State = {
    availableNamesets: NamesetStructureTreeNode,
    currentScreen: Screen,
    namesetBeingImported?: ExtendedSimpleNameEntry,
    namesetBeingDeleted?: ExtendedSimpleNameEntry,
    namesetBeingEdited?: ExtendedSimpleNameEntry,
    lastMessage?: string | JSX.Element,
    isExporting?: boolean,
    lastSourceImport: Screen
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
            currentScreen: Screen.DEFAULT,
            lastSourceImport: Screen.DEFAULT
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
        await new Promise((res) => {
            this.setState({
                availableNamesets: {
                    rootContent: root,
                    subtrees: namesetTree
                }
            }, () => res(0))
        });
    }

    render() {
        switch (this.state.currentScreen) {
            case Screen.DEFAULT:
                return <>
                    <h1>{translate("cityNamesetsLibrary.title")}</h1>
                    <h3>{translate("cityNamesetsLibrary.subtitle")}</h3>
                    <section style={{ overflow: "scroll", position: "absolute", bottom: 52, left: 5, right: 5, top: 107 }}>
                        <GameScrollComponent>
                            {Object.keys(this.state?.availableNamesets.subtrees ?? {}).length == 0 && !this.state?.availableNamesets.rootContent.length
                                ? <h2>{translate("cityNamesetsLibrary.noNamesetsMsg")} <a onClick={() => this.setState({ currentScreen: Screen.NAMESET_IMPORT_LIB })}>{translate("cityNamesetsLibrary.clickToImport")}</a> <a onClick={() => this.goToEdit()}>{translate("cityNamesetsLibrary.clickToCreate")}</a></h2>
                                : <NamesetCategoryCmp entry={this.state?.availableNamesets} doWithNamesetData={(x, i) => <NamesetLineViewer entry={x} key={i} actionButtons={(y) => this.getActionButtons(y)} />} />}
                        </GameScrollComponent>
                    </section>
                    <div style={{ display: "flex", position: "absolute", left: 5, right: 5, bottom: 5, flexDirection: "row-reverse" }}>
                        <button className="positiveBtn " onClick={() => this.setState({ currentScreen: Screen.NAMESET_IMPORT_LIB })}>{translate("cityNamesetsLibrary.importFromLibrary")}</button>
                        <button className="positiveBtn " onClick={() => this.setState({ currentScreen: Screen.NAMESET_IMPORT_GITHUB })}>{translate("cityNamesetsLibrary.importFromGitHub")}</button>
                        <button className="positiveBtn " onClick={() => this.goToEdit()}>{translate("cityNamesetsLibrary.createNewNameset")}</button>
                        <div style={{ display: "flex", flex: "5 5" }}></div>
                        <div style={{ display: "flex", alignItems: "center", paddingLeft: "10rem" }}>
                            {this.state.lastMessage}
                        </div>
                    </div>
                </>;
            case Screen.NAMESET_IMPORT_LIB:
                return <NamesetLibrarySelectorCmp onBack={() => this.setState({ currentScreen: Screen.DEFAULT })} actionButtons={(p) => <><button className="positiveBtn" onClick={() => this.goToImportDetails(p)}>{translate('cityNamesetsLibrary.copyToCity')}</button></>} />
            case Screen.NAMESET_IMPORT_GITHUB:
                return <NamesetGitHubSelectorCmp onBack={() => this.setState({ currentScreen: Screen.DEFAULT })} actionButtons={(p) => <><button className="positiveBtn" onClick={() => this.goToImportDetailsGitHub(p)}>{translate('cityNamesetsLibrary.copyToCity')}</button></>} />
            case Screen.IMPORTING_NAMESET:
                return <NamesetImportingCmp namesetData={this.state.namesetBeingImported} onBack={() => this.setState({ currentScreen: this.state.lastSourceImport })} onOk={(x) => this.doImportNameset(x)} />
            case Screen.AWAITING_ACTION:
                return <div>{translate("main.loadingDataPleaseWait")}</div>
            case Screen.DELETE_CONFIRM:
                return <NamesetDeletingCmp onBack={() => this.setState({ currentScreen: Screen.DEFAULT })} onOk={(x) => this.doDelete(x)} namesetData={this.state.namesetBeingDeleted} />
            case Screen.EDIT_NAMESET:
                return <NamesetEditorCmp onBack={() => this.setState({ currentScreen: Screen.DEFAULT })} onOk={(x) => this.doUpdate(x.namesetData)} entryData={this.state.namesetBeingEdited} />
        }
    }
    async goToImportDetailsGitHub(p: GitHubFileItem) {
        await new Promise((resp) => this.setState({ currentScreen: Screen.AWAITING_ACTION }, () => resp(undefined)));
        const fileContents = await GitHubAddressesFilesSevice.getBlobData(p.url)
        this.setState({
            namesetBeingImported: {
                IdString: null,
                Name: `Downloads/${p.path.split("/").reverse()[0].replace(".txt", "")}`,
                Values: fileContents.split("\n").map(x => x.replace("{0}", "").trim())
            },
            currentScreen: Screen.IMPORTING_NAMESET,
            lastSourceImport: Screen.NAMESET_IMPORT_GITHUB
        });
    }
    getActionButtons(x: ExtendedSimpleNameEntry): JSX.Element {
        return <>
            <button className="negativeBtn" onClick={() => this.goToDelete(x)}>{translate("cityNamesetsLibrary.deleteNameset")}</button>
            <button className="neutralBtn" onClick={() => this.goToEdit(x)}>{translate("cityNamesetsLibrary.editNameset")}</button>
            <button className="neutralBtn" disabled={this.state.isExporting} onClick={() => this.doExport(x)}>{translate("cityNamesetsLibrary.exportNameset")}</button>
        </>
    }
    goToEdit(x?: ExtendedSimpleNameEntry): void {
        this.setState({
            namesetBeingEdited: x ?? { Values: [], Name: "<?>", IdString: null },
            currentScreen: Screen.EDIT_NAMESET
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

    async doExport(x: ExtendedSimpleNameEntry) {
        await new Promise((resp) => this.setState({ isExporting: true }, () => resp(undefined)));
        const exportResult = await NamesetService.exportFromCityToLibrary(x.IdString);
        if (exportResult == null) {
            this.setState({
                isExporting: false,
                lastMessage: <div style={{ color: "var(--warningColor)" }}>{translate("cityNamesetsLibrary.anErrorHasOccurredOnExporting")}</div>
            }, () => setTimeout(() => this.setState({ lastMessage: null }), 7000))
        } else {
            await this.updateNamesets();
            this.setState({
                isExporting: false,
                lastMessage: <div>{translate("cityNamesetsLibrary.fileExportedToLocation")}<br
                /><div style={{ color: "var(--positiveColor)" }}>{exportResult}</div></div>
            }, () => setTimeout(() => this.setState({ lastMessage: null }), 7000))

        }

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
            currentScreen: Screen.IMPORTING_NAMESET,
            lastSourceImport: Screen.NAMESET_IMPORT_LIB,
        });

    }
}



