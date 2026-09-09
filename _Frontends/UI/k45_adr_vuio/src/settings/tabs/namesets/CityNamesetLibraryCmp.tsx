import {
    ExtendedSimpleNameEntry,
    NamesetService,
} from "adr";
import {
    FilePickerDialog,
    ListActionTypeArray,
    ListWithContentTab,
    VanillaComponentResolver,
} from "@klyte45/vuio-commons";
import { ConfirmationDialog, Portal } from "cs2/ui";
import { useCallback, useEffect, useMemo, useState } from "react";
import { translate } from "../../translate";
import styles from "../../styles/namesetLibrary.module.scss";
import { NamesetEditorCmp } from "./NamesetEditorCmp";
import {
    generateGitHubNamesetDataProvider,
    GITHUB_NAMESETS_ROOT,
    GitHubPickerError,
    loadGitHubNamesetFromPickerPath,
} from "./namesetGitHubDataProvider";
import { NamesetGitHubErrorDialog } from "./NamesetGitHubErrorDialog";
import { NamesetImportingCmp } from "./NamesetImportingCmp";
import { namesetDisplayName } from "./namesetUtils";
import { NamesetWordsContainer } from "./NamesetWordsContainer";
const i_importFromDisk = "coui://uil/Standard/PaperWithArrow.svg";
const i_importFromGitHub = "coui://adr.k45/UI/images/GitHub.svg";
const i_createNameset = "coui://uil/Standard/Plus.svg";
const i_openLibraryFolder = "coui://uil/Standard/Folder.svg";
enum Screen {
    DEFAULT,
    IMPORTING_NAMESET,
    AWAITING_ACTION,
    EDIT_NAMESET,
}
export const CityNamesetLibraryCmp = () => {
    const FocusDisabled = VanillaComponentResolver.instance.FOCUS_DISABLED;
    const buttonClass = VanillaComponentResolver.instance.toolButtonTheme.button;
    const [availableNamesets, setAvailableNamesets] = useState([] as ExtendedSimpleNameEntry[]);
    const [selectedId, setSelectedId] = useState<string | undefined>(undefined);
    const [currentScreen, setCurrentScreen] = useState(Screen.DEFAULT);
    const [namesetBeingImported, setNamesetBeingImported] = useState(undefined! as ExtendedSimpleNameEntry);
    const [namesetBeingDeleted, setNamesetBeingDeleted] = useState(null as ExtendedSimpleNameEntry | null);
    const [namesetBeingEdited, setNamesetBeingEdited] = useState(undefined! as ExtendedSimpleNameEntry);
    const [exportedPath, setExportedPath] = useState(null as string | null);
    const [exportFailed, setExportFailed] = useState(false);
    const [isExporting, setIsExporting] = useState(false);
    const [diskPickerOpen, setDiskPickerOpen] = useState(false);
    const [githubPickerOpen, setGithubPickerOpen] = useState(false);
    const [githubError, setGithubError] = useState(null as GitHubPickerError | null);
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
    const doDelete = async (x: ExtendedSimpleNameEntry) => {
        setNamesetBeingDeleted(null);
        setCurrentScreen(Screen.AWAITING_ACTION);
        await NamesetService.deleteNamesetFromCity(x.IdString);
        setCurrentScreen(Screen.DEFAULT);
    };
    const doExport = async (x: ExtendedSimpleNameEntry) => {
        setIsExporting(true);
        const exportResult = await NamesetService.exportFromCityToLibrary(x.IdString);
        setIsExporting(false);
        if (exportResult == null) {
            setExportFailed(true);
        } else {
            setExportedPath(exportResult);
        }
    };
    const doImportNameset = async (namesetData: ExtendedSimpleNameEntry, namesetNameImport: string) => {
        setCurrentScreen(Screen.AWAITING_ACTION);
        await NamesetService.sendNamesetForCity(namesetNameImport, namesetData.Values, namesetData.ValuesAlternative);
        setCurrentScreen(Screen.DEFAULT);
    };
    const doUpdate = async (namesetData: Omit<ExtendedSimpleNameEntry, "ChecksumString">) => {
        setCurrentScreen(Screen.AWAITING_ACTION);
        const name = namesetData.Name || "<?>";
        if (namesetData.IdString) {
            await NamesetService.updateNameset(namesetData.IdString, name, namesetData.Values, namesetData.ValuesAlternative);
        } else {
            await NamesetService.sendNamesetForCity(name, namesetData.Values, namesetData.ValuesAlternative);
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
    const listActions: ListActionTypeArray = [
        {
            isContext: false,
            onSelect: () => setDiskPickerOpen(true),
            src: i_importFromDisk,
            tooltip: translate("cityNamesetsLibrary.importFromLibrary"),
            focusKey: FocusDisabled,
            className: buttonClass,
        },
        {
            isContext: false,
            onSelect: () => setGithubPickerOpen(true),
            src: i_importFromGitHub,
            tooltip: translate("cityNamesetsLibrary.importFromGitHub"),
            focusKey: FocusDisabled,
            className: buttonClass,
        },
        {
            isContext: false,
            onSelect: () => goToEdit(),
            src: i_createNameset,
            tooltip: translate("cityNamesetsLibrary.createNewNameset"),
            focusKey: FocusDisabled,
            className: buttonClass,
        },
        null,
        {
            isContext: false,
            onSelect: () => { void NamesetService.goToDiskSimpleNamesFolder(); },
            src: i_openLibraryFolder,
            tooltip: translate("namesetsLibrary.goToLibraryFolder"),
            focusKey: FocusDisabled,
            className: buttonClass,
        },
    ];
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
            <div className={styles.libraryContent}>
                <ListWithContentTab
                    className={styles.libraryNav}
                    listItems={listItems}
                    selectedKey={selectedId ?? null}
                    onChangeSelection={setSelectedId}
                    listActions={listActions}
                    emptyListMsg={translate("cityNamesetsLibrary.noNamesetsMsg")}
                >
                    <div className={styles.libraryBody}>
                        {selectedNameset && <>
                            <div className={styles.wordsPreview}>
                                <NamesetWordsContainer
                                    values={selectedNameset.Values}
                                    valuesAlternative={selectedNameset.ValuesAlternative}
                                />
                            </div>
                            <div className={styles.actionRow}>
                                <button type="button" className="negativeBtn" onClick={() => setNamesetBeingDeleted(selectedNameset)}>{translate("cityNamesetsLibrary.deleteNameset")}</button>
                                <button type="button" className="neutralBtn" onClick={() => goToEdit(selectedNameset)}>{translate("cityNamesetsLibrary.editNameset")}</button>
                                <button type="button" className="neutralBtn" disabled={isExporting} onClick={() => doExport(selectedNameset)}>{translate("cityNamesetsLibrary.exportNameset")}</button>
                            </div>
                        </>}
                    </div>
                </ListWithContentTab>
            </div>
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
            {namesetBeingDeleted && (
                <Portal>
                    <ConfirmationDialog
                        title={translate("namesetDelete.title")}
                        message={translate("namesetDelete.subtitle")}
                        confirm={translate("namesetDelete.yes")}
                        cancel={translate("namesetDelete.no")}
                        onConfirm={() => { void doDelete(namesetBeingDeleted); }}
                        onCancel={() => setNamesetBeingDeleted(null)}
                    />
                </Portal>
            )}
            {exportedPath && (
                <Portal>
                    <ConfirmationDialog
                        title={translate("cityNamesetsLibrary.exportNameset")}
                        message={translate("cityNamesetsLibrary.fileExportedToLocation") + "\n" + exportedPath.replaceAll("\\", "\\\\").replaceAll("<", "\\<").replaceAll(">", "\\>")}
                        confirm={translate("namesetsLibrary.goToLibraryFolder")}
                        cancel={engine.translate("Common.OK")}
                        onConfirm={() => {
                            void NamesetService.goToDiskSimpleNamesFolder();
                            setExportedPath(null);
                        }}
                        onCancel={() => setExportedPath(null)}
                    />
                </Portal>
            )}
            {exportFailed && (
                <Portal>
                    <ConfirmationDialog
                        title={translate("cityNamesetsLibrary.exportNameset")}
                        message={translate("cityNamesetsLibrary.anErrorHasOccurredOnExporting")}
                        confirm={engine.translate("Common.OK")}
                        cancellable={false}
                        onConfirm={() => setExportFailed(false)}
                    />
                </Portal>
            )}
        </div>
    );
};
