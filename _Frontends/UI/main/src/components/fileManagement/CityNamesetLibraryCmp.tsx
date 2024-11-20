import { categorizeFiles } from "#utility/categorizeFiles";
import { translate } from "#utility/translate";
import { ExtendedSimpleNameEntry, GitHubAddressesFilesSevice, GitHubFileItem, NamesetService, SimpleNameEntry } from "@klyte45/adr-commons";
import { DefaultPanelScreen } from "@klyte45/euis-components";
import { useEffect, useState } from "react";
import { NamesetCategoryCmp } from "./NamesetCategoryCmp";
import { NamesetDeletingCmp } from "./NamesetDeletingCmp";
import { NamesetEditorCmp } from "./NamesetEditorCmp";
import { NamesetGitHubSelectorCmp } from "./NamesetGitHubSelectorCmp";
import { NamesetImportingCmp } from "./NamesetImportingCmp";
import { NamesetLibrarySelectorCmp } from "./NamesetLibrarySelectorCmp";
import { NamesetLineViewer } from "./NamesetLineViewer";
import { NamesetViewingCmp } from "./NamesetViewingCmp";

enum Screen {
    DEFAULT,
    NAMESET_IMPORT_LIB,
    IMPORTING_NAMESET,
    AWAITING_ACTION,
    DELETE_CONFIRM,
    EDIT_NAMESET,
    NAMESET_IMPORT_GITHUB,
    VIEWING_NAMESET,
}

export type NamesetStructureTreeNode = {
    rootContent: ExtendedSimpleNameEntry[],
    subtrees: Record<string, NamesetStructureTreeNode>
}


export const CityNamesetLibraryCmp = () => {

    const [availableNamesets, setAvailableNamesets] = useState({ subtrees: {}, rootContent: [] })
    const [currentScreen, setCurrentScreen] = useState(Screen.DEFAULT)
    const [lastSourceImport, setLastSourceImport] = useState(Screen.DEFAULT)
    const [namesetBeingImported, setNamesetBeingImported] = useState(undefined as ExtendedSimpleNameEntry)
    const [namesetBeingDeleted, setNamesetBeingDeleted] = useState(undefined as ExtendedSimpleNameEntry)
    const [namesetBeingEdited, setNamesetBeingEdited] = useState(undefined as ExtendedSimpleNameEntry)
    const [lastMessage, setLastMessage] = useState(undefined as string | JSX.Element)
    const [isExporting, setIsExporting] = useState(undefined as boolean)

    NamesetService.doOnCityNamesetsUpdated(() => updateNamesets());

    const updateNamesets = () => {
        NamesetService.listCityNamesets().then(namesets => {
            const namesetTree = categorizeFiles(namesets);
            const root = namesetTree[""]?.rootContent ?? [];
            delete namesetTree[""];
            setAvailableNamesets({
                rootContent: root,
                subtrees: namesetTree
            });
        });
        return () => NamesetService.offCityNamesetsUpdated();
    };
    useEffect(updateNamesets, []);



    const goToImportDetailsGitHub = async (p: GitHubFileItem) => {
        setCurrentScreen(Screen.AWAITING_ACTION);
        const fileContents = await GitHubAddressesFilesSevice.getBlobData(p.url)
        const lines = fileContents.split("\n").map(x => x.split(";")).map(x => x[1] ??= x[0]);


        setNamesetBeingImported({
            IdString: null,
            Name: `Downloads/${p.path.split("/").reverse()[0].replace(".txt", "")}`,
            Values: lines.map(x => x[0].replace("{0}", "").trim()),
            ValuesAlternative: lines.map(x => x[1].replace("{0}", "").trim())
        }),
            setCurrentScreen(Screen.IMPORTING_NAMESET),
            setLastSourceImport(Screen.NAMESET_IMPORT_GITHUB)
    };

    const getActionButtons = (x: ExtendedSimpleNameEntry): JSX.Element => {
        return <>
            <button className="negativeBtn" onClick={() => goToDelete(x)}>{translate("cityNamesetsLibrary.deleteNameset")}</button>
            <button className="neutralBtn" onClick={() => goToEdit(x)}>{translate("cityNamesetsLibrary.editNameset")}</button>
            <button className="neutralBtn" onClick={() => goToView(x)}>{translate("cityNamesetsLibrary.viewNameset")}</button>
            <button className="neutralBtn" disabled={isExporting} onClick={() => doExport(x)}>{translate("cityNamesetsLibrary.exportNameset")}</button>
        </>
    }
    const goToEdit = (x?: ExtendedSimpleNameEntry): void => {
        setNamesetBeingEdited(x ?? { ValuesAlternative: [], Values: [], Name: "<?>", IdString: null })
        setCurrentScreen(Screen.EDIT_NAMESET)
    }
    const goToView = (x?: ExtendedSimpleNameEntry): void => {
        setNamesetBeingEdited(x ?? { ValuesAlternative: [], Values: [], Name: "<?>", IdString: null })
        setCurrentScreen(Screen.VIEWING_NAMESET)
    }
    const goToDelete = (x: ExtendedSimpleNameEntry): void => {
        setNamesetBeingDeleted(x)
        setCurrentScreen(Screen.DELETE_CONFIRM)
    }

    const doDelete = async (x: ExtendedSimpleNameEntry) => {
        setCurrentScreen(Screen.AWAITING_ACTION);
        await NamesetService.deleteNamesetFromCity(x.IdString);
        setCurrentScreen(Screen.DEFAULT);
    }

    const doExport = async (x: ExtendedSimpleNameEntry) => {
        setIsExporting(true)
        const exportResult = await NamesetService.exportFromCityToLibrary(x.IdString);
        if (exportResult == null) {
            setIsExporting(false)
            setLastMessage(<div style={{ color: "var(--warningColor)" }}>{translate("cityNamesetsLibrary.anErrorHasOccurredOnExporting")}</div>)
            setTimeout(() => setLastMessage(null), 7000);
        } else {
            setIsExporting(false)
            setLastMessage(<div>{translate("cityNamesetsLibrary.fileExportedToLocation")}<br /><div style={{ color: "var(--positiveColor)" }}>{exportResult}</div></div>);
            setTimeout(() => setLastMessage(null), 7000);
        }
    }

    const doImportNameset = async (namesetData: ExtendedSimpleNameEntry, namesetNameImport: string) => {
        setCurrentScreen(Screen.AWAITING_ACTION);
        await NamesetService.sendNamesetForCity(namesetNameImport, namesetData.Values, namesetData.ValuesAlternative);
        setCurrentScreen(Screen.DEFAULT);
    }

    const doUpdate = async (namesetData: Omit<ExtendedSimpleNameEntry, "ChecksumString">) => {
        setCurrentScreen(Screen.AWAITING_ACTION);
        if (namesetData.IdString) {
            await NamesetService.updateNameset(namesetData.IdString, namesetData.Name, namesetData.Values, namesetData.ValuesAlternative);
        } else {
            await NamesetService.sendNamesetForCity(namesetData.Name, namesetData.Values, namesetData.ValuesAlternative)
        }
        setCurrentScreen(Screen.DEFAULT);
    }
    const goToImportDetails = (p: ExtendedSimpleNameEntry): void => {
        setNamesetBeingImported(p)
        setCurrentScreen(Screen.IMPORTING_NAMESET)
        setLastSourceImport(Screen.NAMESET_IMPORT_LIB)
    }


    switch (currentScreen) {
        case Screen.DEFAULT:
            const buttonsRowContent = <>
                <button className="positiveBtn " onClick={() => setCurrentScreen(Screen.NAMESET_IMPORT_LIB)}>{translate("cityNamesetsLibrary.importFromLibrary")}</button>
                <button className="positiveBtn " onClick={() => setCurrentScreen(Screen.NAMESET_IMPORT_GITHUB)}>{translate("cityNamesetsLibrary.importFromGitHub")}</button>
                <button className="positiveBtn " onClick={() => goToEdit()}>{translate("cityNamesetsLibrary.createNewNameset")}</button>
                <div style={{ display: "flex", flex: "5 5" }}></div>
                <div style={{ display: "flex", alignItems: "center", paddingLeft: "10rem" }}>
                    {lastMessage}
                </div>
            </>
            return <DefaultPanelScreen title={translate("cityNamesetsLibrary.title")} subtitle={translate("cityNamesetsLibrary.subtitle")} buttonsRowContent={buttonsRowContent} scrollable>
                {Object.keys(availableNamesets.subtrees ?? {}).length == 0 && !availableNamesets.rootContent.length
                    ? <h2>{translate("cityNamesetsLibrary.noNamesetsMsg")} <a onClick={() => setCurrentScreen(Screen.NAMESET_IMPORT_LIB)}>{translate("cityNamesetsLibrary.clickToImport")}</a> <a onClick={() => goToEdit()}>{translate("cityNamesetsLibrary.clickToCreate")}</a></h2>
                    : <NamesetCategoryCmp entry={availableNamesets} doWithNamesetData={(x, i) => <NamesetLineViewer entry={x} key={i} actionButtons={(y) => getActionButtons(y)} />} />}
            </DefaultPanelScreen>
        case Screen.NAMESET_IMPORT_LIB:
            return <NamesetLibrarySelectorCmp onBack={() => setCurrentScreen(Screen.DEFAULT)} actionButtons={(p) => <><button className="positiveBtn" onClick={() => goToImportDetails(p)}>{translate('cityNamesetsLibrary.copyToCity')}</button></>} />
        case Screen.NAMESET_IMPORT_GITHUB:
            return <NamesetGitHubSelectorCmp onBack={() => setCurrentScreen(Screen.DEFAULT)} actionButtons={(p) => <><button className="positiveBtn" onClick={() => goToImportDetailsGitHub(p)}>{translate('cityNamesetsLibrary.copyToCity')}</button></>} />
        case Screen.IMPORTING_NAMESET:
            return <NamesetImportingCmp namesetData={namesetBeingImported} onBack={() => setCurrentScreen(lastSourceImport)} onOk={(x, y) => doImportNameset(x, y)} />
        case Screen.VIEWING_NAMESET:
            return <NamesetViewingCmp namesetData={namesetBeingEdited} onBack={() => setCurrentScreen(Screen.DEFAULT)} />
        case Screen.AWAITING_ACTION:
            return <div>{translate("main.loadingDataPleaseWait")}</div>
        case Screen.DELETE_CONFIRM:
            return <NamesetDeletingCmp onBack={() => setCurrentScreen(Screen.DEFAULT)} onOk={(x) => doDelete(x)} namesetData={namesetBeingDeleted} />
        case Screen.EDIT_NAMESET:
            return <NamesetEditorCmp onBack={() => setCurrentScreen(Screen.DEFAULT)} onOk={(x) => doUpdate(x)} entryData={namesetBeingEdited} />
    }

}



