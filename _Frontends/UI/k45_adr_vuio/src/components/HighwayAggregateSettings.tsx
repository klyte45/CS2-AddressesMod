import { Entity, HighwayData, HighwayRoutesService, SelectInfoPanelService, SelectedInfoOptions } from "adr";
import { LocElementType, VanillaComponentResolver, VanillaFnResolver, VanillaWidgets } from "@klyte45/vuio-commons";
import { DropdownItem, LocElement } from "cs2/bindings";
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

type Props = {
    response: SelectedInfoOptions;
    onChanged: () => Promise<any>;
    /** Override host entity (defaults to response.entityValue). */
    hostEntity?: Entity;
};

/**
 * Shared ADR highway settings for Addresses SIP and Road Grouping tool options.
 * Expects an ADR name-aggregate host; callers gate on isAdrAggregate.
 */
export const HighwayAggregateSettings = ({ response, onChanged, hostEntity }: Props) => {
    const VR = VanillaComponentResolver.instance;
    const VW = VanillaWidgets.instance;
    const StringDD = VW.DropdownField<string>();
    const NumberDD = VW.DropdownField<number>();
    const editorModule = VW.editorItemModule;
    const loc = VanillaFnResolver.instance.localization.useCachedLocalization();
    const useMiles = loc.unitSettings.unitSystem === UNIT_SYSTEM_FREEDOM;

    const [loading, setLoading] = useState(false);
    const [highways, setHighways] = useState<HighwayData[]>([]);

    // entityValue is already the ADR/vanilla aggregate host from getEntityOptions.
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
        setLoading(true);
        try {
            await action();
            await onChanged();
        } finally {
            setLoading(false);
        }
    };

    if (!VR || !response) return <></>;
    if (loading) return <>Loading...</>;

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

    return (
        <>
            <VR.InfoRow
                left={<>{translate("HighwayAggregateSettings.HighwayRoute")}</>}
                right={
                    <div>
                        <StringDD
                            items={highwayItems}
                            value={currentHighwayId}
                            onChange={(id) =>
                                run(() => SelectInfoPanelService.setHighwayDataId(host, highwayDropdownValue(id)))
                            }
                        />
                    </div>
                }
            />
            {hasHighwayRoute && (
                <>
                    <VR.InfoRow
                        left={<>{translate("HighwayAggregateSettings.RouteDirection")}</>}
                        right={
                            <div>
                                <NumberDD
                                    items={directionItems}
                                    value={routeDirection}
                                    onChange={(dir) => run(() => SelectInfoPanelService.setRouteDirection(host, dir))}
                                />
                            </div>
                        }
                    />
                    <VR.InfoRow
                        left={<>{translate("HighwayAggregateSettings.MileageOverride")}</>}
                        right={
                            <VW.Checkbox
                                checked={mileageOverrideActive}
                                onChange={(active) =>
                                    run(() => SelectInfoPanelService.setMileageOverrideActive(host, active))
                                }
                            />
                        }
                    />
                    {mileageOverrideActive && (
                        <VR.InfoRow
                            subRow={true}
                            left={<>{mileageOverrideLabel}</>}
                            right={
                                <VW.FloatInputStandalone
                                    className={editorModule.input}
                                    value={mileageOverrideDisplay}
                                    fractionDigits={3}
                                    onChange={(displayValue) => {
                                        const meters = displayToMeters(displayValue, useMiles);
                                        return run(() => SelectInfoPanelService.setMileageOverrideMeters(host, meters));
                                    }}
                                    style={{ maxWidth: "150rem", textAlign: "right" }}
                                />
                            }
                        />
                    )}
                    <VR.InfoRow
                        left={<>{translate("HighwayAggregateSettings.ReverseCounting")}</>}
                        right={
                            <VW.Checkbox
                                checked={reverseCounting}
                                onChange={(reverse) =>
                                    run(() => SelectInfoPanelService.setReverseCounting(host, reverse))
                                }
                            />
                        }
                    />
                </>
            )}
        </>
    );
};
