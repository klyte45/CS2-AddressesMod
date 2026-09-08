import { HighwayData, HighwayRoutesService } from "@klyte45/adr-commons";
import { FormLine, VanillaWidgets } from "@klyte45/vuio-commons";
import { useEffect, useState } from "react";
import { translate } from "../../../translate";
import shared from "../../../styles/regionEditor.module.scss";
import { GenericListing, ListItemData } from "./_GenericListing";

export function HighwayRegisterManagement() {
    const [selectedHw, setSelectedHw] = useState<Partial<HighwayData>>();
    const [reloadToken, setReloadToken] = useState(0);

    if (selectedHw) {
        return (
            <HighwayRegisterForm
                selectedHw={selectedHw}
                setSelectedHw={setSelectedHw}
                onCancel={() => setSelectedHw(undefined)}
                onSave={async () => {
                    await HighwayRoutesService.saveHighwayData(selectedHw as HighwayData);
                    setSelectedHw(undefined);
                    setReloadToken((x) => x + 1);
                }}
            />
        );
    }

    return <HighwayListing onSelectItem={setSelectedHw} reloadToken={reloadToken} />;
}

type HighwayRegisterFormProps = {
    selectedHw: Partial<HighwayData>;
    setSelectedHw: (x: Partial<HighwayData> | undefined) => void;
    onSave: () => void;
    onCancel: () => void;
};

function HighwayRegisterForm({ selectedHw, setSelectedHw, onSave, onCancel }: HighwayRegisterFormProps) {
    const VW = VanillaWidgets.instance;
    const editorModule = VW.editorItemModule;
    const FloatInput = VW.FloatInputStandalone;

    return (
        <div className={`${shared.editorForm} k45_btnHost`}>
            <FormLine title={translate("highwayRegisterEditor.highwayPrefix")}>
                <VW.StringInputField
                    className={editorModule.input}
                    value={selectedHw.prefix ?? ""}
                    onChange={(value) => setSelectedHw({ ...selectedHw, prefix: value })}
                />
            </FormLine>
            <FormLine title={translate("highwayRegisterEditor.highwaySuffix")}>
                <VW.StringInputField
                    className={editorModule.input}
                    value={selectedHw.suffix ?? ""}
                    onChange={(value) => setSelectedHw({ ...selectedHw, suffix: value })}
                />
            </FormLine>
            <FormLine title={translate("highwayRegisterEditor.highwayName")}>
                <VW.StringInputField
                    className={editorModule.input}
                    value={selectedHw.name ?? ""}
                    onChange={(value) => setSelectedHw({ ...selectedHw, name: value })}
                />
            </FormLine>
            <FormLine
                title={translate("highwayRegisterEditor.startPositionReference")}
                subtitle={translate("regionSettings.doubleClickToSelectPoint")}
            >
                <div className={shared.positionRow}>
                    <FloatInput
                        className={`${editorModule.input} ${shared.numberInput}`}
                        value={selectedHw.refStartPoint?.[0] ?? 0}
                        fractionDigits={3}
                        onChange={(value) => setSelectedHw({ ...selectedHw, refStartPoint: [value, selectedHw.refStartPoint?.[1] ?? 0] })}
                    />
                    <FloatInput
                        className={`${editorModule.input} ${shared.numberInput}`}
                        value={selectedHw.refStartPoint?.[1] ?? 0}
                        fractionDigits={3}
                        onChange={(value) => setSelectedHw({ ...selectedHw, refStartPoint: [selectedHw.refStartPoint?.[0] ?? 0, value] })}
                    />
                    <button type="button" className="neutralBtn" disabled={true}>
                        {translate("highwayRegisterEditor.copyFromMapSelection")}
                    </button>
                </div>
            </FormLine>
            <div className={shared.formGap} />
            <div className={shared.bottomActions}>
                <button type="button" className="positiveBtn" onClick={onSave}>{translate("highwayRegisterEditor.save")}</button>
                <button type="button" className="negativeBtn" onClick={onCancel}>{translate("highwayRegisterEditor.cancel")}</button>
            </div>
        </div>
    );
}

type HighwayListingProps = {
    onSelectItem: (x: Partial<HighwayData>) => void;
    reloadToken: number;
};

function HighwayListing({ onSelectItem, reloadToken }: HighwayListingProps) {
    const [highways, setHighways] = useState([] as HighwayData[]);

    useEffect(() => {
        HighwayRoutesService.listHighwaysRegistered().then((items) => setHighways(
            items.sort(
                (a, b) => `${a.prefix} ${a.suffix}`.localeCompare(`${b.prefix} ${b.suffix}`) || a.name.localeCompare(b.name),
            ),
        ));
    }, [reloadToken]);

    const items: ListItemData[] = highways.map((x) => ({
        key: x.Id,
        title: `${x.prefix}-${x.suffix} | ${x.name}`,
        subTitle: `[${x.refStartPoint[0].toFixed(2)} ; ${x.refStartPoint[1].toFixed(2)}]`,
        actions: [
            {
                label: translate("highwayRegisterEditor.editBtn"),
                onClick: () => onSelectItem(x),
            },
        ],
    }));

    return (
        <GenericListing
            title={translate("highwayRegisterEditor.titleList")}
            items={items}
            noItemsMessage={translate("highwayRegisterEditor.noHighwaysRegisteredMessage")}
            onAdd={() => onSelectItem({ refStartPoint: [0, 0] })}
            addBtnLabel={translate("highwayRegisterEditor.addBtn")}
        />
    );
}
