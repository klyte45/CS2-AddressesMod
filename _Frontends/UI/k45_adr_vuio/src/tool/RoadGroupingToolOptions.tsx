import { AdrEntityType, EditorUISystemService, Entity, SelectInfoPanelService, SelectedInfoOptions, nameToString } from "adr";
import { MultiUIValueBinding, MultiUIValueBindingTools, VanillaComponentResolver, VanillaWidgets } from "@klyte45/vuio-commons";
import { useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { FocusDisabled } from "cs2/input";
import { ModuleRegistryExtend } from "cs2/modding";
import { useCallback, useEffect, useState } from "react";
import { HighwayAggregateSettings } from "components/HighwayAggregateSettings";
import { SeedManagementOptionsComponent } from "components/SeedManagementOptions";
import { translate } from "utility/translate";

export const ROAD_GROUPING_TOOL_ID = "K45_ADR_AdrRoadGroupingTool";

export type RoadGroupingSegmentCard = {
    edge: Entity;
    sortKey: number;
    numberLow: number;
    numberHigh: number;
    label?: string;
    midX: number;
    midY: number;
    midZ: number;
    viewportX: number;
    viewportY: number;
    visible?: boolean;
};

export const isNullEntity = (e?: Entity | null) =>
    !e || (e.Index === 0 && e.Version === 0);

export const entityEquals = (a?: Entity | null, b?: Entity | null) =>
    !!a && !!b && a.Index === b.Index && a.Version === b.Version;

const i_ShowNumberCards = "coui://uil/Standard/GridTextView.svg";
const i_PickAnother = "coui://uil/Standard/PickerPipette.svg";

const RoadGroupingBindings = {
    _prefix: "k45::adr.roadGrouping",
    SelectedAggregation: MultiUIValueBinding<Entity>,
    ShowNumberCards: MultiUIValueBinding<boolean>,
    SegmentCards: MultiUIValueBinding<RoadGroupingSegmentCard[]>,
};

export const RoadGroupingService = {
    ...MultiUIValueBindingTools.InitializeBindings(RoadGroupingBindings),
    enableTool: () => engine.call(`${RoadGroupingBindings._prefix}.enableTool`),
    disableTool: () => engine.call(`${RoadGroupingBindings._prefix}.disableTool`),
};

export const RoadGroupingToolOptionsVisibility: ModuleRegistryExtend = (Component: any) => {
    return () => Component() || tool.activeTool$.value.id === ROAD_GROUPING_TOOL_ID;
};

export const RoadGroupingToolOptions: ModuleRegistryExtend = (Component: any) => {
    return () => {
        const toolActive = useValue(tool.activeTool$).id === ROAD_GROUPING_TOOL_ID;
        const result = Component();
        if (toolActive) {
            result.props.children ??= [];
            result.props.children.unshift(<RoadGroupingToolPanel />);
        }
        return result;
    };
};

const RoadGroupingToolPanel = () => {
    const VCR = VanillaComponentResolver.instance;
    const VW = VanillaWidgets.instance;
    const editorModule = VW.editorItemModule;
    const [, setTick] = useState(0);
    const [options, setOptions] = useState<SelectedInfoOptions[]>([]);
    const [customName, setCustomName] = useState("");
    const [effectiveName, setEffectiveName] = useState("");
    const [loadingOptions, setLoadingOptions] = useState(false);

    useEffect(() => {
        const bump = async () => setTick((x) => x + 1);
        // Subscribe after reactivate — reactivate() clears the subscription list.
        RoadGroupingService.SelectedAggregation.reactivate();
        RoadGroupingService.ShowNumberCards.reactivate();
        RoadGroupingService.SelectedAggregation.subscribe(bump);
        RoadGroupingService.ShowNumberCards.subscribe(bump);
    }, []);

    const selected = RoadGroupingService.SelectedAggregation.value;
    const showCards = !!RoadGroupingService.ShowNumberCards.value;
    const hasSelection = !isNullEntity(selected);

    const reloadSelectionUi = useCallback(async (entity: Entity | null | undefined) => {
        if (isNullEntity(entity)) {
            setOptions([]);
            setCustomName("");
            setEffectiveName("");
            return;
        }
        setLoadingOptions(true);
        try {
            const [opts, name, entityData] = await Promise.all([
                SelectInfoPanelService.getEntityOptions(entity!),
                SelectInfoPanelService.getAggregateCustomName(entity!),
                // Same NameSystem.GetName path SIP title uses (editorUI.getEntityData).
                EditorUISystemService.getEntityData(entity!),
            ]);
            setOptions(opts ?? []);
            setCustomName(name ?? "");
            setEffectiveName(nameToString(entityData?.name) ?? "");
        } finally {
            setLoadingOptions(false);
        }
    }, []);

    useEffect(() => {
        reloadSelectionUi(selected);
    }, [selected?.Index, selected?.Version, reloadSelectionUi]);

    const onSettingsChanged = useCallback(async () => {
        await reloadSelectionUi(selected);
        setTick((x) => x + 1);
    }, [reloadSelectionUi, selected]);

    const commitCustomName = useCallback(async () => {
        if (isNullEntity(selected)) return;
        await SelectInfoPanelService.setAggregateCustomName(selected!, customName);
        await onSettingsChanged();
    }, [selected, customName, onSettingsChanged]);

    const seedOption = options.find((o) => o?.type?.value__ === AdrEntityType.RoadAggregation);
    const highwayOption = options.find((o) => o?.type?.value__ === AdrEntityType.HighwayAggregate);
    const isAdr = !!highwayOption || !!seedOption?.isAdrAggregate;

    return (
        <>
            <VCR.Section title={translate("roadGrouping.toolOptions.title")}>
                {!hasSelection && translate("roadGrouping.toolOptions.pickPrompt")}
                {hasSelection && loadingOptions && <>...</>}
                {hasSelection && !loadingOptions && (effectiveName || translate("roadGrouping.toolOptions.selected"))}
            </VCR.Section>
            {hasSelection && !loadingOptions && (
                <>
                    <VCR.InfoRow
                        left={<>{translate("roadGrouping.toolOptions.customName")}</>}
                        right={
                            <VW.StringInputField
                                className={editorModule.input}
                                value={customName}
                                onChange={setCustomName}
                                onChangeEnd={() => { void commitCustomName(); }}
                            />
                        }
                    />
                    {seedOption && !customName && (
                        <SeedManagementOptionsComponent
                            response={seedOption}
                            seedEntity={seedOption.entityValue ?? selected!}
                            onChanged={onSettingsChanged}
                            showNamingSource={false}
                        />
                    )}
                    {isAdr && highwayOption && (
                        <HighwayAggregateSettings
                            response={highwayOption}
                            hostEntity={highwayOption.entityValue ?? selected!}
                            onChanged={onSettingsChanged}
                        />
                    )}
                    {!isAdr && !seedOption && (
                        <>{translate("roadGrouping.toolOptions.pickPrompt")}</>
                    )}
                </>
            )}
            {hasSelection && (
                <VCR.Section title={translate("roadGrouping.toolOptions.actions")}>
                    <FocusDisabled>
                        <VCR.ToolButton
                            selected={showCards}
                            multiSelect
                            onSelect={() => RoadGroupingService.ShowNumberCards.set(!showCards)}
                            src={i_ShowNumberCards}
                            focusKey={VCR.FOCUS_DISABLED}
                            className={VCR.toolButtonTheme.button}
                            tooltip={translate("roadGrouping.toolOptions.showNumberCards")}
                        />
                        <div style={{ width: "10rem" }} />
                        <VCR.ToolButton
                            onSelect={() => RoadGroupingService.SelectedAggregation.set({ Index: 0, Version: 0 })}
                            src={i_PickAnother}
                            focusKey={VCR.FOCUS_DISABLED}
                            className={VCR.toolButtonTheme.button}
                            tooltip={translate("roadGrouping.toolOptions.pickAnother")}
                        />
                    </FocusDisabled>
                </VCR.Section>
            )}
        </>
    );
};
