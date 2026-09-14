import { Entity, HighwayData, HighwayRoutesService, SelectInfoPanelService, SelectedInfoOptions } from "adr";
import { LocElementType, VanillaComponentResolver, VanillaFnResolver, VanillaWidgets, VectorSectionEditable } from "@klyte45/vuio-commons";
import { DropdownItem, LocElement } from "cs2/bindings";
import { FocusDisabled } from "cs2/input";
import { ObjectTyped } from "object-typed";
import { useEffect, useState } from "react";
import { translate } from "utility/translate";

/** Matches ADRNameAggregateHighwayData.RouteDirection ordinals. */
export enum AdrHighwayRouteDirection {
    NONE = 0,
    NORTH = 1,
    NORTHEAST = 2,
    EAST = 3,
    SOUTHEAST = 4,
    SOUTH = 5,
    SOUTHWEST = 6,
    WEST = 7,
    NORTHWEST = 8,
    INTERNAL = 9,
    EXTERNAL = 10,
}

/** Same boolean affordance as AddressesToolOptions road-marker double-pole toggle. */
const i_ToggleOn = "coui://uil/Standard/Checkmark.svg";
const i_ToggleOff = "coui://uil/Standard/XClose.svg";

/** Matches cs2/l10n UnitSystem.Freedom (Metric = 0). */
const UNIT_SYSTEM_FREEDOM = 1;

/** Component stores meters; Metric UI uses km, Freedom UI uses mi. */
const METERS_PER_KM = 1000;
const METERS_PER_MILE = 1609.344;

/** Hash128.ToString() is 32 hex chars with no dashes (not Guid format). */
const DEFAULT_HIGHWAY_ID = "00000000000000000000000000000000";

/** Dropdown values must be non-empty — empty string does not select in vanilla DropdownField. */
function highwayDropdownValue(id?: string): string {
    if (!id || id === DEFAULT_HIGHWAY_ID || id === "00000000-0000-0000-0000-000000000000") {
        return DEFAULT_HIGHWAY_ID;
    }
    return id;
}

function metersToDisplay(meters: number, useMiles: boolean): number {
    return useMiles ? meters / METERS_PER_MILE : meters / METERS_PER_KM;
}

function displayToMeters(display: number, useMiles: boolean): number {
    const meters = useMiles ? display * METERS_PER_MILE : display * METERS_PER_KM;
    return Math.round(meters);
}

type Surface = "sip" | "tool";

type Props = {
    response: SelectedInfoOptions;
    onChanged: () => Promise<any>;
    /** Override host entity (defaults to response.entityValue). */
    hostEntity?: Entity;
    /**
     * `sip` — InfoRow + Checkbox (RoadMarkSettings).
     * `tool` — Section + ToolButton (AddressesToolOptions road-marker tool).
     */
    surface?: Surface;
};

type HighwayUiState = {
    host: Entity;
    busy: boolean;
    currentHighwayId: string;
    hasHighwayRoute: boolean;
    routeDirection: number;
    mileageOverrideActive: boolean;
    mileageOverrideMeters: number;
    mileageDraft: number;
    setMileageDraft: (v: number) => void;
    reverseCounting: boolean;
    mileageOverrideLabel: string;
    highwayItems: DropdownItem<string>[];
    directionItems: DropdownItem<number>[];
    run: (action: () => Promise<boolean>) => Promise<void>;
    commitMileageDraft: (v?: number) => Promise<void>;
};

function useHighwayAggregateUi(
    response: SelectedInfoOptions,
    onChanged: () => Promise<any>,
    hostEntity?: Entity,
): HighwayUiState | null {
    const loc = VanillaFnResolver.instance.localization.useCachedLocalization();
    const useMiles = loc.unitSettings.unitSystem === UNIT_SYSTEM_FREEDOM;

    const [busy, setBusy] = useState(false);
    const [highways, setHighways] = useState<HighwayData[]>([]);

    const host = hostEntity ?? response.entityValue;
    const currentHighwayId = highwayDropdownValue(response.highwayDataId);
    const hasHighwayRoute = currentHighwayId !== DEFAULT_HIGHWAY_ID;
    const routeDirection = response.routeDirection ?? AdrHighwayRouteDirection.NONE;
    const mileageOverrideActive = !!response.mileageOverrideActive;
    const mileageOverrideMeters = response.mileageOverrideMeters ?? 0;
    const mileageOverrideDisplay = metersToDisplay(mileageOverrideMeters, useMiles);
    const reverseCounting = !!response.reverseCounting;
    const mileageOverrideLabel = useMiles
        ? translate("HighwayAggregateSettings.MileageOverrideMi")
        : translate("HighwayAggregateSettings.MileageOverrideKm");

    const [mileageDraft, setMileageDraft] = useState(mileageOverrideDisplay);
    useEffect(() => {
        setMileageDraft(mileageOverrideDisplay);
    }, [mileageOverrideMeters, useMiles, host?.Index, host?.Version]);

    useEffect(() => {
        HighwayRoutesService.listHighwaysRegistered().then((items) =>
            setHighways(
                [...items].sort(
                    (a, b) =>
                        `${a.prefix} ${a.suffix}`.localeCompare(`${b.prefix} ${b.suffix}`) ||
                        a.name.localeCompare(b.name),
                ),
            ),
        );
    }, []);

    const run = async (action: () => Promise<boolean>) => {
        setBusy(true);
        try {
            await action();
            await onChanged();
        } finally {
            setBusy(false);
        }
    };

    const commitMileageDraft = async (newVal?: number) => {
        const effectiveValue = newVal ?? mileageDraft;
        if (newVal !== undefined) setMileageDraft(newVal);
        const meters = displayToMeters(effectiveValue, useMiles);
        if (meters === mileageOverrideMeters) return;
        await SelectInfoPanelService.setMileageOverrideMeters(host, meters);
        await onChanged();
    };

    if (!response) return null;

    const highwayItems: DropdownItem<string>[] = [
        {
            value: DEFAULT_HIGHWAY_ID,
            displayName: { __Type: LocElementType.String, value: translate("HighwayAggregateSettings.NotAHighway") } as LocElement,
        },
        ...highways.map((hw) => ({
            value: hw.Id,
            displayName: {
                __Type: LocElementType.String,
                value: `${hw.prefix}-${hw.suffix} | ${hw.name}`,
            } as LocElement,
        })),
    ];

    const directionItems: DropdownItem<number>[] = ObjectTyped.entries(AdrHighwayRouteDirection)
        .filter((x) => typeof x[1] === "number")
        .map((x: [string, number]) => ({
            value: x[1],
            displayName: { __Type: LocElementType.String, value: translate("RouteDirection." + x[0]) } as LocElement,
        }));

    return {
        host,
        busy,
        currentHighwayId,
        hasHighwayRoute,
        routeDirection,
        mileageOverrideActive,
        mileageOverrideMeters,
        mileageDraft,
        setMileageDraft,
        reverseCounting,
        mileageOverrideLabel,
        highwayItems,
        directionItems,
        run,
        commitMileageDraft,
    };
}

/** SIP: InfoRow + Checkbox / FloatInputStandalone — matches RoadMarkSettings. */
const HighwayAggregateSettingsSip = ({ ui }: { ui: HighwayUiState }) => {
    const VR = VanillaComponentResolver.instance;
    const VW = VanillaWidgets.instance;
    const StringDD = VW.DropdownField<string>();
    const NumberDD = VW.DropdownField<number>();
    const editorModule = VW.editorItemModule;

    return (
        <>
            <VR.InfoRow
                left={<>{translate("HighwayAggregateSettings.HighwayRoute")}</>}
                right={
                    <div>
                        <StringDD
                            items={ui.highwayItems}
                            value={ui.currentHighwayId}
                            disabled={ui.busy}
                            onChange={(id) =>
                                ui.run(() => SelectInfoPanelService.setHighwayDataId(ui.host, highwayDropdownValue(id)))
                            }
                        />
                    </div>
                }
            />
            {ui.hasHighwayRoute && (
                <>
                    <VR.InfoRow
                        left={<>{translate("HighwayAggregateSettings.RouteDirection")}</>}
                        right={
                            <div>
                                <NumberDD
                                    items={ui.directionItems}
                                    value={ui.routeDirection}
                                    disabled={ui.busy}
                                    onChange={(dir) => ui.run(() => SelectInfoPanelService.setRouteDirection(ui.host, dir))}
                                />
                            </div>
                        }
                    />
                    <VR.InfoRow left={<>{translate("HighwayAggregateSettings.MileageOverride")}</>} right={<VW.Checkbox
                        onChange={(active) =>
                            ui.run(() => SelectInfoPanelService.setMileageOverrideActive(ui.host, active))
                        }
                        checked={ui.mileageOverrideActive}
                    />} />
                    {ui.mileageOverrideActive && (
                        <VR.InfoRow
                            subRow={true}
                            left={<>{ui.mileageOverrideLabel}</>}
                            right={
                                <VW.FloatInputStandalone
                                    className={editorModule.input}
                                    value={ui.mileageDraft}
                                    fractionDigits={3}
                                    disabled={ui.busy}
                                    onChange={ui.setMileageDraft}
                                    onBlur={() => { void ui.commitMileageDraft(); }}
                                    style={{ maxWidth: "150rem", textAlign: "right" }}
                                />
                            }
                        />
                    )}
                    <VR.InfoRow left={<>{translate("HighwayAggregateSettings.ReverseCounting")}</>} right={<VW.Checkbox
                        onChange={(reverse) =>
                            ui.run(() => SelectInfoPanelService.setReverseCounting(ui.host, reverse))
                        }
                        checked={ui.reverseCounting}
                    />} />
                </>
            )}
        </>
    );
};

/** Tool: Section + ToolButton / FloatInputField — matches AddressesToolOptions road-marker tool. */
const HighwayAggregateSettingsTool = ({ ui }: { ui: HighwayUiState }) => {
    const VR = VanillaComponentResolver.instance;
    const VW = VanillaWidgets.instance;
    const StringDD = VW.DropdownField<string>();
    const NumberDD = VW.DropdownField<number>();

    return (
        <>
            <VR.Section title={translate("HighwayAggregateSettings.HighwayRoute")}>
                <StringDD
                    items={ui.highwayItems}
                    value={ui.currentHighwayId}
                    disabled={ui.busy}
                    onChange={(id) =>
                        ui.run(() => SelectInfoPanelService.setHighwayDataId(ui.host, highwayDropdownValue(id)))
                    }
                />
            </VR.Section>
            {ui.hasHighwayRoute && (
                <>
                    <VR.Section title={translate("HighwayAggregateSettings.RouteDirection")}>
                        <NumberDD
                            items={ui.directionItems}
                            value={ui.routeDirection}
                            disabled={ui.busy}
                            onChange={(dir) => ui.run(() => SelectInfoPanelService.setRouteDirection(ui.host, dir))}
                        />
                    </VR.Section>
                    <VR.Section title={translate("HighwayAggregateSettings.MileageOverride")}>
                        <FocusDisabled>
                            <VR.ToolButton
                                selected={ui.mileageOverrideActive}
                                multiSelect
                                disabled={ui.busy}
                                onSelect={() =>
                                    ui.run(() =>
                                        SelectInfoPanelService.setMileageOverrideActive(ui.host, !ui.mileageOverrideActive),
                                    )
                                }
                                src={ui.mileageOverrideActive ? i_ToggleOn : i_ToggleOff}
                                focusKey={VR.FOCUS_DISABLED}
                                className={VR.toolButtonTheme.button}
                            />
                        </FocusDisabled>
                    </VR.Section>
                    {ui.mileageOverrideActive && (
                        <VectorSectionEditable
                            onValueChanged={async (_, numberStr) => {
                                await ui.commitMileageDraft(parseFloat(numberStr));
                            }}

                            title={ui.mileageOverrideLabel}
                            valueGetter={() => [ui.mileageDraft.toFixed(3)]}
                            valueGetterFormatted={() => [VanillaFnResolver.instance.localizedNumber.useNumberFormat(VanillaFnResolver.instance.unit.Unit.Length, false)(ui.mileageDraft * 1000)]}
                        />
                    )}
                    <VR.Section title={translate("HighwayAggregateSettings.ReverseCounting")}>
                        <FocusDisabled>
                            <VR.ToolButton
                                selected={ui.reverseCounting}
                                multiSelect
                                disabled={ui.busy}
                                onSelect={() =>
                                    ui.run(() => SelectInfoPanelService.setReverseCounting(ui.host, !ui.reverseCounting))
                                }
                                src={ui.reverseCounting ? i_ToggleOn : i_ToggleOff}
                                focusKey={VR.FOCUS_DISABLED}
                                className={VR.toolButtonTheme.button}
                            />
                        </FocusDisabled>
                    </VR.Section>
                </>
            )}
        </>
    );
};

/**
 * ADR highway settings. Pass `surface="tool"` from Road Grouping tool options;
 * SIP (Addresses) keeps the default InfoRow + Checkbox layout.
 */
export const HighwayAggregateSettings = ({ response, onChanged, hostEntity, surface = "sip" }: Props) => {
    const ui = useHighwayAggregateUi(response, onChanged, hostEntity);
    if (!VanillaComponentResolver.instance || !ui) return <></>;
    return surface === "tool"
        ? <HighwayAggregateSettingsTool ui={ui} />
        : <HighwayAggregateSettingsSip ui={ui} />;
};
