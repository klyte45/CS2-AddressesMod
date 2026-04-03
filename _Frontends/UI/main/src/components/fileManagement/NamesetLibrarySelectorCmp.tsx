import { categorizeFiles } from "#utility/categorizeFiles";
import { translate } from "#utility/translate";
import { ExtendedSimpleNameEntry, NamesetService, SimpleNameEntry } from "@klyte45/adr-commons";
import { DefaultPanelScreen } from "@klyte45/euis-components";
import { useEffect, useState } from "react";
import { NamesetCategoryCmp } from "./NamesetCategoryCmp";
import { NamesetLineViewer } from "./NamesetLineViewer";

type Props = {
    actionButtons?: (nameset: ExtendedSimpleNameEntry) => JSX.Element,
    onBack?: () => void
}

export const NamesetLibrarySelectorCmp = ({ actionButtons, onBack }: Props) => {

    const [availableNamesets, setAvailableNamesets] = useState({ subtrees: {}, rootContent: [] });
    const [isLibraryLoading, setIsLibraryLoading] = useState(false);

    useEffect(() => {
        updateNamesets();
        return () => { NamesetService.offLibraryReloaded(); };
    }, [])

    const updateNamesets = async () => {
        const namesetsSaved = await NamesetService.listLibraryNamesets();
        applyNamesets(namesetsSaved);
    }

    const applyNamesets = (namesetsSaved: SimpleNameEntry[]) => {
        const namesetTree = categorizeFiles(namesetsSaved)
        const root = namesetTree[""]?.rootContent ?? []
        delete namesetTree[""];
        setAvailableNamesets({
            rootContent: root,
            subtrees: namesetTree
        });
    }

    const handleReload = async () => {
        const loading = await NamesetService.reloadLibraryNamesets();
        if (loading) {
            setIsLibraryLoading(true);
            NamesetService.onLibraryReloaded((namesets: SimpleNameEntry[]) => {
                setIsLibraryLoading(false);
                applyNamesets(namesets);
            });
        } else {
            await updateNamesets();
        }
    }

    const buttonsRowContent = <>
        <button className="negativeBtn" onClick={onBack}>{translate("namesetsLibrary.back")}</button>
        <button className="neutralBtn" onClick={handleReload} disabled={isLibraryLoading}>
            {isLibraryLoading ? translate("namesetsLibrary.reloadingFiles") : translate("namesetsLibrary.reloadFiles")}
        </button>
        <button className="neutralBtn" onClick={() => NamesetService.goToDiskSimpleNamesFolder()}>{translate("namesetsLibrary.goToLibraryFolder")}</button>
    </>
    return <>
        <DefaultPanelScreen title={translate("namesetsLibrary.title")} subtitle={translate("namesetsLibrary.subtitle")} buttonsRowContent={buttonsRowContent} scrollable>
            <NamesetCategoryCmp entry={availableNamesets} doWithNamesetData={(x, i) => <NamesetLineViewer entry={x} key={i} actionButtons={actionButtons} />} />
        </DefaultPanelScreen>
    </>;

}


