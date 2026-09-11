import { Entity } from "@klyte45/vuio-commons";
import { useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { ModuleRegistryExtend } from "cs2/modding";
import { useEffect, useState } from "react";
import {
    ROAD_GROUPING_TOOL_ID,
    RoadGroupingSegmentCard,
    RoadGroupingService,
} from "tool/RoadGroupingToolOptions";
import styles from "./RoadGroupingNumberCards.module.scss";

const formatRange = (low: number, high: number) =>
    low === high ? `${low}` : `${low}–${high}`;

const isCardVisible = (card: RoadGroupingSegmentCard) => {
    if (typeof card.visible === "boolean") {
        return card.visible;
    }
    const x = Number(card.viewportX);
    const y = Number(card.viewportY);
    return x > -0.1 && x < 1.1 && y > -0.1 && y < 1.1;
};

const normalizeCards = (raw: unknown): RoadGroupingSegmentCard[] => {
    if (!Array.isArray(raw)) {
        return [];
    }
    return [...raw]
        .map((item: any) => ({
            edge: {
                Index: Number((item.edge ?? item.Edge)?.Index ?? 0),
                Version: Number((item.edge ?? item.Edge)?.Version ?? 0),
            } as Entity,
            sortKey: Number(item.sortKey ?? item.SortKey ?? 0),
            numberLow: Number(item.numberLow ?? item.NumberLow ?? 0),
            numberHigh: Number(item.numberHigh ?? item.NumberHigh ?? 0),
            midX: Number(item.midX ?? item.MidX ?? 0),
            midY: Number(item.midY ?? item.MidY ?? 0),
            midZ: Number(item.midZ ?? item.MidZ ?? 0),
            viewportX: Number(item.viewportX ?? item.ViewportX ?? 0.5),
            viewportY: Number(item.viewportY ?? item.ViewportY ?? 0.5),
            visible: item.visible ?? item.Visible,
        }))
        .sort((a, b) => a.sortKey - b.sortKey);
};

export const RoadGroupingNumberCardsOverlay = () => {
    const toolActive = useValue(tool.activeTool$).id === ROAD_GROUPING_TOOL_ID;
    const [cards, setCards] = useState<RoadGroupingSegmentCard[]>([]);

    useEffect(() => {
        // Drive visibility from SegmentCards pushes only. Tool options also
        // reactivate ShowNumberCards and would wipe a shared subscription list.
        const onCards = async (value: unknown) => {
            setCards(normalizeCards(value ?? RoadGroupingService.SegmentCards.value));
        };
        RoadGroupingService.SegmentCards.reactivate();
        RoadGroupingService.SegmentCards.subscribe(onCards);
    }, []);

    // C# clears SegmentCards when the toggle is off / tool inactive.
    if (!toolActive || !cards.length) {
        return null;
    }

    return (
        <div className={styles.root} aria-hidden>
            {cards.map((card) => {
                if (!isCardVisible(card)) {
                    return null;
                }
                const key = `${card.edge.Index}:${card.edge.Version}:${card.sortKey}`;
                return (
                    <div
                        key={key}
                        className={styles.card}
                        style={{
                            left: `${card.viewportX * 100}%`,
                            top: `${(1 - card.viewportY) * 100}%`,
                        }}
                    >
                        {formatRange(card.numberLow, card.numberHigh)}
                    </div>
                );
            })}
        </div>
    );
};

export const RoadGroupingNumberCardsHost: ModuleRegistryExtend = (Component: any) => {
    return (props: any) => (
        <>
            <Component {...props} />
            <RoadGroupingNumberCardsOverlay />
        </>
    );
};
