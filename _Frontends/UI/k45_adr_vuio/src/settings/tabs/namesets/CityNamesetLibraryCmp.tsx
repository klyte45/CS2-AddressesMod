import {
    ExtendedSimpleNameEntry,
    NamesetService,
} from "@klyte45/adr-commons";
import { FilePickerDialog, ListWithContentTab } from "@klyte45/vuio-commons";
import { useCallback, useEffect, useMemo, useState } from "react";
import { translate } from "../../translate";
import styles from "../../styles/namesetLibrary.module.scss";
import { NamesetDeletingCmp } from "./NamesetDeletingCmp";
import { NamesetEditorCmp } from "./NamesetEditorCmp";
import {
    generateGitHubNamesetDataProvider,
    GITHUB_NAMESETS_ROOT,
    GitHubPickerError,
    loadGitHubNamesetFromPickerPath,
} from "./namesetGitHubDataProvider";
import { NamesetGitHubErrorDialog } from "./NamesetGitHubErrorDialog";
import { NamesetImportingCmp } from "./NamesetImportingCmp";
import { NamesetViewingCmp } from "./NamesetViewingCmp";
import { namesetDisplayName } from "./namesetUtils";

enum Screen {
    DEFAULT,
    IMPORTING_NAMESET,
    AWAITING_ACTION,
    DELETE_CONFIRM,
    EDIT_NAMESET,
    VIEWING_NAMESET,
}

export const CityNamesetLibraryCmp = () => {
    const [availableNamesets, setAvailableNamesets] = useState([] as ExtendedSimpleNameEntry[]);
    const [selectedId, setSelectedId] = useState<string | undefined>(undefined);
    const [currentScreen, setCurrentScreen] = useState(Screen.DEFAULT);
    const [namesetBeingImported, setNamesetBeingImported] = useState(undefined! as ExtendedSimpleNameEntry);
    const [namesetBeingDeleted, setNamesetBeingDeleted] = useState(undefined! as ExtendedSimpleNameEntry);
    const [namesetBeingEdited, setNamesetBeingEdited] = useState(undefined! as ExtendedSimpleNameEntry);
    const [lastMessage, setLastMessage] = useState(undefined as string | JSX.Element | undefined);
    const [isExporting, setIsExporting] = useState(false);

    const [diskPickerOpen, setDiskPickerOpen] = useState(false);
    const [githubPickerOpen, setGithubPickerOpen] = useState(false);
    const [githubError, setGithubError] = useState(null as GitHubPickerError | null);
    const [isLibraryLoading, setIsLibraryLoading] = useState(false);

    const updateNamesets = useCallback(() => {
        NamesetService.listCityNamesets().then((namesets) => {
            const sorted = [...namesets].sort((a, b) =>
                a.Name.localeCompare(b.Name, undefined, { sensitivity: "base" }),
            );
            setAvailableNamesets(sorted);
            setSelectedId((prev) => {
                if (prev && sorted.some((n) => n.IdString === prev)) return prev;
                return sorted[0]?.IdString;
            });
        });
    }, []);

    useEffect(() => {
        updateNamesets();
        NamesetService.doOnCityNamesetsUpdated(updateNamesets);
        return () => NamesetService.offCityNamesetsUpdated();
    }, [updateNamesets]);

    const selectedNameset = useMemo(
        () => availableNamesets.find((x) => x.IdString === selectedId),
        [availableNamesets, selectedId],
    );

    const listItems = useMemo(
        () => availableNamesets.map((n) => ({
            displayName: namesetDisplayName(n),
            value: n.IdString,
        })),
        [availableNamesets],
    );

    const goToEdit = (x?: ExtendedSimpleNameEntry) => {
        setNamesetBeingEdited(x ?? { ValuesAlternative: [], Values: [], Name: "<?>", IdString: null as any });
        setCurrentScreen(Screen.EDIT_NAMESET);
    };

    const goToView = (x: ExtendedSimpleNameEntry) => {
        setNamesetBeingEdited(x);
        setCurrentScreen(Screen.VIEWING_NAMESET);
    };

    const goToDelete = (x: ExtendedSimpleNameEntry) => {
        setNamesetBeingDeleted(x);
        setCurrentScreen(Screen.DELETE_CONFIRM);
    };

    const doDelete = async (x: ExtendedSimpleNameEntry) => {
        setCurrentScreen(Screen.AWAITING_ACTION);
        await NamesetService.deleteNamesetFromCity(x.IdString);
        setCurrentScreen(Screen.DEFAULT);
    };

    const doExport = async (x: ExtendedSimpleNameEntry) => {
        setIsExporting(true);
        const exportResult = await NamesetService.exportFromCityToLibrary(x.IdString);
        if (exportResult == null) {
            setIsExporting(false);
            setLastMessage(<div style={{ color: "var(--warningColor)" }}>{translate("cityNamesetsLibrary.anErrorHasOccurredOnExporting")}</div>);
            setTimeout(() => setLastMessage(undefined), 7000);
        } else {
            setIsExporting(false);
            setLastMessage(
                <div>
                    {translate("cityNamesetsLibrary.fileExportedToLocation")}
                    <br />
                    <div style={{ color: "var(--positiveColor)" }}>{exportResult}</div>
                </div>,
            );
            setTimeout(() => setLastMessage(undefined), 7000);
        }
    };

    const doImportNameset = async (namesetData: ExtendedSimpleNameEntry, namesetNameImport: string) => {
        setCurrentScreen(Screen.AWAITING_ACTION);
        await NamesetService.sendNamesetForCity(namesetNameImport, namesetData.Values, namesetData.ValuesAlternative);
        setCurrentScreen(Screen.DEFAULT);
    };

    const doUpdate = async (namesetData: Omit<ExtendedSimpleNameEntry, "ChecksumString">) => {
        setCurrentScreen(Screen.AWAITING_ACTION);
        if (namesetData.IdString) {
            await NamesetService.updateNameset(namesetData.IdString, namesetData.Name, namesetData.Values, namesetData.ValuesAlternative);
        } else {
            await NamesetService.sendNamesetForCity(namesetData.Name, namesetData.Values, namesetData.ValuesAlternative);
        }
        setCurrentScreen(Screen.DEFAULT);
    };

    const handleDiskImportPick = async (path?: string) => {
        if (!path) return;
        setCurrentScreen(Screen.AWAITING_ACTION);
        try {
            const parsed = await NamesetService.readNamesetFromDisk(path);
            setNamesetBeingImported(parsed);
            setCurrentScreen(Screen.IMPORTING_NAMESET);
        } catch {
            setCurrentScreen(Screen.DEFAULT);
        }
    };

    const handleGithubImportPick = async (encodedPath?: string) => {
        if (!encodedPath) return;
        setCurrentScreen(Screen.AWAITING_ACTION);
        try {
            const parsed = await loadGitHubNamesetFromPickerPath(encodedPath);
            setNamesetBeingImported(parsed);
            setCurrentScreen(Screen.IMPORTING_NAMESET);
        } catch {
            setCurrentScreen(Screen.DEFAULT);
        }
    };

    const handleReloadLibrary = async () => {
        const loading = await NamesetService.reloadLibraryNamesets();
        if (loading) {
            setIsLibraryLoading(true);
            NamesetService.onLibraryReloaded(() => {
                setIsLibraryLoading(false);
                NamesetService.offLibraryReloaded();
            });
        }
    };

    const diskDataProvider = useCallback(
        (folder: string, allowedExtension: string) =>
            NamesetService.listNamesetFolderFiles(folder, allowedExtension),
        [],
    );

    const githubDataProvider = useCallback(async (folder: string, allowedExtension: string) => {
        const { items, error } = await generateGitHubNamesetDataProvider(folder, allowedExtension);
        if (error) {
            setGithubError(error);
            setGithubPickerOpen(false);
        }
        return items;
    }, []);

    if (currentScreen === Screen.AWAITING_ACTION) {
        return <div className={styles.loadingMsg}>{translate("main.loadingDataPleaseWait")}</div>;
    }
    if (currentScreen === Screen.IMPORTING_NAMESET) {
        return (
            <NamesetImportingCmp
                namesetData={namesetBeingImported}
                onBack={() => setCurrentScreen(Screen.DEFAULT)}
                onOk={doImportNameset}
            />
        );
    }
    if (currentScreen === Screen.VIEWING_NAMESET) {
        return <NamesetViewingCmp namesetData={namesetBeingEdited} onBack={() => setCurrentScreen(Screen.DEFAULT)} />;
    }
    if (currentScreen === Screen.DELETE_CONFIRM) {
        return (
            <NamesetDeletingCmp
                namesetData={namesetBeingDeleted}
                onBack={() => setCurrentScreen(Screen.DEFAULT)}
                onOk={doDelete}
            />
        );
    }
    if (currentScreen === Screen.EDIT_NAMESET) {
        return (
            <NamesetEditorCmp
                entryData={namesetBeingEdited}
                onBack={() => setCurrentScreen(Screen.DEFAULT)}
                onOk={doUpdate}
            />
        );
    }

    return (
        <div className={`${styles.libraryRoot} k45_btnHost`}>
            <div className={styles.toolbar}>
                <button type="button" className="positiveBtn" onClick={() => setDiskPickerOpen(true)}>{translate("cityNamesetsLibrary.importFromLibrary")}</button>
                <button type="button" className="positiveBtn" onClick={() => setGithubPickerOpen(true)}>{translate("cityNamesetsLibrary.importFromGitHub")}</button>
                <button type="button" className="positiveBtn" onClick={() => goToEdit()}>{translate("cityNamesetsLibrary.createNewNameset")}</button>
                <button type="button" className="neutralBtn" onClick={handleReloadLibrary} disabled={isLibraryLoading}>
                    {isLibraryLoading ? translate("namesetsLibrary.reloadingFiles") : translate("namesetsLibrary.reloadFiles")}
                </button>
                <button type="button" className="neutralBtn" onClick={() => NamesetService.goToDiskSimpleNamesFolder()}>{translate("namesetsLibrary.goToLibraryFolder")}</button>
                <div className={styles.toolbarSpacer} />
                <div className={styles.toolbarMessage}>{lastMessage}</div>
            </div>

            {availableNamesets.length === 0 ? (
                <div className={styles.emptyMsg}>
                    <h2>
                        {translate("cityNamesetsLibrary.noNamesetsMsg")}{" "}
                        <button type="button" className="neutralBtn" onClick={() => setDiskPickerOpen(true)}>{translate("cityNamesetsLibrary.clickToImport")}</button>{" "}
                        <button type="button" className="neutralBtn" onClick={() => goToEdit()}>{translate("cityNamesetsLibrary.clickToCreate")}</button>
                    </h2>
                </div>
            ) : (
                <ListWithContentTab
                    className={styles.libraryNav}
                    listItems={listItems}
                    selectedKey={selectedId ?? null}
                    onChangeSelection={setSelectedId}
                >
                    <div className={styles.libraryBody}>
                        {selectedNameset && <>
                            <div className={styles.entryMeta}>
                                {`${translate("namesetLine.entriesCountLbl")} ${selectedNameset.Values.length}`}
                            </div>
                            <div className={styles.actionRow}>
                                <button type="button" className="negativeBtn" onClick={() => goToDelete(selectedNameset)}>{translate("cityNamesetsLibrary.deleteNameset")}</button>
                                <button type="button" className="neutralBtn" onClick={() => goToEdit(selectedNameset)}>{translate("cityNamesetsLibrary.editNameset")}</button>
                                <button type="button" className="neutralBtn" onClick={() => goToView(selectedNameset)}>{translate("cityNamesetsLibrary.viewNameset")}</button>
                                <button type="button" className="neutralBtn" disabled={isExporting} onClick={() => doExport(selectedNameset)}>{translate("cityNamesetsLibrary.exportNameset")}</button>
                            </div>
                        </>}
                    </div>
                </ListWithContentTab>
            )}

            <FilePickerDialog
                isActive={diskPickerOpen}
                setIsActive={setDiskPickerOpen}
                dialogTitle={translate("namesetsLibrary.title")}
                dialogPromptText={translate("namesetsLibrary.subtitle")}
                allowedExtensions="*.txt"
                initialFolder=""
                translate={translate}
                generateDataProvider={diskDataProvider}
                actionOnSuccess={handleDiskImportPick}
            />

            <FilePickerDialog
                isActive={githubPickerOpen}
                setIsActive={setGithubPickerOpen}
                dialogTitle={translate("githubLibrary.title")}
                dialogPromptText={translate("githubLibrary.subtitle")}
                allowedExtensions="*.txt"
                initialFolder={GITHUB_NAMESETS_ROOT}
                translate={translate}
                generateDataProvider={githubDataProvider}
                actionOnSuccess={handleGithubImportPick}
            />

            <NamesetGitHubErrorDialog error={githubError} onClose={() => setGithubError(null)} />
        </div>
    );
};
