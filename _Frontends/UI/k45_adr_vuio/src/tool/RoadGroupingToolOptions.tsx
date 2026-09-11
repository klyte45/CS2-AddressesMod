import { Entity, MultiUIValueBinding, MultiUIValueBindingTools, VanillaComponentResolver } from "@klyte45/vuio-commons";
import { useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { FocusDisabled } from "cs2/input";
import { ModuleRegistryExtend } from "cs2/modding";
import { useEffect, useState } from "react";
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
    const [, setTick] = useState(0);

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

    const body = hasSelection
        ? `${translate("roadGrouping.toolOptions.selected")} #${selected!.Index}`
        : translate("roadGrouping.toolOptions.pickPrompt");

    return (
        <>
            <VCR.Section title={translate("roadGrouping.toolOptions.title")}>
                {body}
            </VCR.Section>
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
