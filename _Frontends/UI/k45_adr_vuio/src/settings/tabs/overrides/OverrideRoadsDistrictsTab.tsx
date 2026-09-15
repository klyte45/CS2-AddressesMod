import { AdrBulkConvertResult, AdrCitywideSettings, DistrictListItem, DistrictRelativeService, NamingRulesService, SimpleNameEntry, nameToString, replaceArgs } from "adr";
import { FormLine, LocElementType, VanillaWidgets } from "@klyte45/vuio-commons";
import { DropdownItem } from "cs2/bindings";
import { ConfirmationDialog, Portal } from "cs2/ui";
import { useEffect, useMemo, useState } from "react";
import { translate } from "../../translate";
import styles from "../../styles/overrideSettings.module.scss";
import { defaultSetting, defaultSettingRoadByDistrict } from "./overrideDefaults";
import { NamesetSelectLine } from "./NamesetSelectLine";

type Props = {
    simpleFiles: SimpleNameEntry[];
    indexedSimpleFiles: Record<string, SimpleNameEntry>;
    currentSettings: AdrCitywideSettings;
    innerContextSimpleFiles: SimpleNameEntry[];
    districts: DistrictListItem[];
};

/** Mirrors AdrNetKind ordinals used as bit positions in kindMask (None=0 unused). */
const BULK_CONVERT_KINDS = [
    { kind: 1, key: "netKind.Road" },
    { kind: 2, key: "netKind.Pathway" },
    { kind: 3, key: "netKind.Train" },
    { kind: 4, key: "netKind.Tram" },
    { kind: 5, key: "netKind.Subway" },
    { kind: 6, key: "netKind.Waterway" },
    { kind: 7, key: "netKind.Taxiway" },
] as const;

const kindBit = (kind: number) => 1 << kind;

const emptyResult = (): AdrBulkConvertResult => ({
    AggregatesConverted: 0,
    AggregatesCreatedPartial: 0,
    SkippedAlreadyAdr: 0,
    SkippedNoMatch: 0,
    Failed: 0,
});

export const OverrideRoadsDistrictsTab = ({
    simpleFiles,
    indexedSimpleFiles,
    currentSettings,
    innerContextSimpleFiles,
    districts,
}: Props) => {
    const VW = VanillaWidgets.instance;
    const StringDD = VW.DropdownField<string>();
    const [selectedDistrict, setSelectedDistrict] = useState<DistrictListItem | undefined>(undefined);
    const [confirmOpen, setConfirmOpen] = useState(false);
    const [busy, setBusy] = useState(false);
    const [result, setResult] = useState<AdrBulkConvertResult | null>(null);
    // Default: Road on, others off.
    const [kindMask, setKindMask] = useState(() => kindBit(0));

    useEffect(() => {
        setSelectedDistrict((prev) => (prev ? districts.find((x) => x.Entity.Index == prev.Entity.Index) : undefined));
    }, [districts]);

    const districtItems: DropdownItem<string>[] = districts.map((d) => ({
        value: d.Entity.Index.toString(),
        displayName: { __Type: LocElementType.String, value: nameToString(d.Name) ?? "" },
    }));

    const anyKindChecked = useMemo(() => kindMask !== 0, [kindMask]);

    const toggleKind = (kind: number) => {
        const bit = kindBit(kind);
        setKindMask((prev) => ((prev & bit) !== 0 ? prev & ~bit : prev | bit));
    };

    const runConvert = async () => {
        if (!anyKindChecked || busy) return;
        setBusy(true);
        try {
            const r = await NamingRulesService.convertAllToAdrAggregates(kindMask);
            setResult(r ?? emptyResult());
        } catch {
            setResult({ ...emptyResult(), Failed: 1 });
        } finally {
            setBusy(false);
            setConfirmOpen(false);
        }
    };

    return <>
        <NamesetSelectLine
            title={translate("overrideSettings.districtsFile")}
            options={simpleFiles}
            value={indexedSimpleFiles[currentSettings.DefaultDistrictNameOverridesStr] ?? defaultSetting()}
            onChange={(x) => NamingRulesService.setDefaultDistrictNameOverridesStr(x.IdString)}
        />
        <NamesetSelectLine
            title={translate("overrideSettings.roadsFile")}
            options={simpleFiles}
            value={indexedSimpleFiles[currentSettings.DefaultRoadNameOverridesStr] ?? defaultSetting()}
            onChange={(x) => NamingRulesService.setDefaultRoadNameOverridesStr(x.IdString)}
        />
        <FormLine title={translate("overrideSettings.useRoadNameAsStationName")}>
            <VW.Checkbox
                checked={!!currentSettings?.roadNameAsNameStation}
                onChange={(x) => NamingRulesService.setRoadNameAsNameStation(x)}
            />
        </FormLine>
        <FormLine title={translate("overrideSettings.useRoadNameAsCargoStationName")}>
            <VW.Checkbox
                checked={!!currentSettings?.roadNameAsNameCargoStation}
                onChange={(x) => NamingRulesService.setRoadNameAsNameCargoStation(x)}
            />
        </FormLine>

        <FormLine title={""}>
            <button
                type="button"
                className="neutralBtn"
                disabled={busy}
                onClick={() => {
                    setResult(null);
                    setConfirmOpen(true);
                }}
            >
                {translate("bulkConvert.button")}
            </button>
        </FormLine>

        <h2>{translate("overrideSettings.perDistrictRoadsFile")}</h2>
        <FormLine title={translate("overrideSettings.perDistrictRoadsFile")}>
            <StringDD
                items={districtItems}
                value={selectedDistrict?.Entity.Index.toString()}
                onChange={(id) => setSelectedDistrict(districts.find((d) => d.Entity.Index.toString() === id))}
            />
        </FormLine>
        {selectedDistrict && (
            <NamesetSelectLine
                title={translate("overrideSettings.roadsFile")}
                options={innerContextSimpleFiles}
                value={
                    innerContextSimpleFiles.find((x) => x.IdString == selectedDistrict.CurrentValue)
                    ?? defaultSettingRoadByDistrict()
                }
                onChange={async (x) => {
                    await DistrictRelativeService.setRoadNamesFile(selectedDistrict.Entity, x.IdString);
                }}
            />
        )}

        {confirmOpen && (
            <Portal>
                {/* message must be text-only (FormattedText); interactive nodes go in children */}
                <ConfirmationDialog
                    title={translate("bulkConvert.confirmTitle")}
                    message={translate("bulkConvert.confirmBody")}
                    confirm={translate("bulkConvert.confirm")}
                    cancel={translate("bulkConvert.cancel")}
                    disableContinue={!anyKindChecked || busy}
                    onConfirm={() => {
                        if (!anyKindChecked) {
                            return;
                        }
                        void runConvert();
                    }}
                    onCancel={() => setConfirmOpen(false)}
                >
                    <div className={`k45_btnHost ${styles.bulkConvertKindRow}`}>
                        {BULK_CONVERT_KINDS.map(({ kind, key }) => {
                            const on = (kindMask & kindBit(kind)) !== 0;
                            return (
                                <button
                                    key={kind}
                                    type="button"
                                    className={on ? "positiveBtn" : "blackBtn"}
                                    onClick={() => toggleKind(kind)}
                                >
                                    {translate(key)}
                                </button>
                            );
                        })}
                    </div>
                </ConfirmationDialog>
            </Portal>
        )}

        {result && (
            <Portal>
                <ConfirmationDialog
                    title={translate("bulkConvert.resultTitle")}
                    message={replaceArgs(translate("bulkConvert.resultBody"), {
                        converted: String(result.AggregatesConverted ?? 0),
                        partial: String(result.AggregatesCreatedPartial ?? 0),
                        alreadyAdr: String(result.SkippedAlreadyAdr ?? 0),
                        noMatch: String(result.SkippedNoMatch ?? 0),
                        failed: String(result.Failed ?? 0),
                    })}
                    confirm={engine.translate("Common.OK")}
                    cancellable={false}
                    onConfirm={() => setResult(null)}
                />
            </Portal>
        )}
    </>;
};
