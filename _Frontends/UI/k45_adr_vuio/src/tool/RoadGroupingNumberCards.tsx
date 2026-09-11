import { Entity } from "@klyte45/vuio-commons";
import { useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { ModuleRegistryExtend } from "cs2/modding";
import { useEffect, useMemo, useState } from "react";
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

type Side = "above" | "below";

type Rect = { l: number; r: number; t: number; b: number };

type PlacedCard = {
    key: string;
    label: string;
    anchorX: number;
    anchorY: number;
    cardX: number;
    cardY: number;
    side: Side;
    w: number;
    h: number;
};

const CARD_H = 52;
const BASE_GAP = 48;
const GAP_STEP = 12;
const X_STEP = 20;
const PAD = 8;
const ANCHOR_R = 5;

const estimateCardWidth = (label: string) => Math.max(64, label.length * 11 + 36);

const toRect = (cx: number, cy: number, w: number, h: number): Rect => ({
    l: cx - w / 2,
    r: cx + w / 2,
    t: cy - h / 2,
    b: cy + h / 2,
});

const overlaps = (a: Rect, b: Rect) =>
    !(a.r + PAD < b.l || a.l - PAD > b.r || a.b + PAD < b.t || a.t - PAD > b.b);

const layoutCards = (
    cards: RoadGroupingSegmentCard[],
    screenW: number,
    screenH: number
): PlacedCard[] => {
    const placed: PlacedCard[] = [];
    const rects: Rect[] = [];

    for (const card of cards) {
        if (!isCardVisible(card)) {
            continue;
        }

        const anchorX = card.viewportX * screenW;
        const anchorY = (1 - card.viewportY) * screenH;
        const label = formatRange(card.numberLow, card.numberHigh);
        const w = estimateCardWidth(label);
        const h = CARD_H;
        const key = `${card.edge.Index}:${card.edge.Version}:${card.sortKey}`;

        let chosen: PlacedCard | null = null;

        outer: for (let ring = 0; ring < 36; ring++) {
            const gap = BASE_GAP + ring * GAP_STEP;
            const xShifts = [0];
            for (let k = 1; k <= ring; k++) {
                xShifts.push(k * X_STEP, -k * X_STEP);
            }
            for (const side of ["above", "below"] as Side[]) {
                for (const dx of xShifts) {
                    const cardX = anchorX + dx;
                    const cardY = side === "above" ? anchorY - gap - h / 2 : anchorY + gap + h / 2;
                    const rect = toRect(cardX, cardY, w, h);
                    if (rect.t < 4 || rect.b > screenH - 4 || rect.l < 4 || rect.r > screenW - 4) {
                        continue;
                    }
                    if (rects.some((r) => overlaps(rect, r))) {
                        continue;
                    }
                    chosen = { key, label, anchorX, anchorY, cardX, cardY, side, w, h };
                    break outer;
                }
            }
        }

        if (!chosen) {
            // Last resort: place above without collision check.
            chosen = {
                key,
                label,
                anchorX,
                anchorY,
                cardX: anchorX,
                cardY: anchorY - BASE_GAP - h / 2,
                side: "above",
                w,
                h,
            };
        }

        placed.push(chosen);
        rects.push(toRect(chosen.cardX, chosen.cardY, chosen.w, chosen.h));
    }

    return placed;
};

/** Line end at the midpoint of the card edge facing the anchor (AABB hit). */
const connectorEnd = (p: PlacedCard) => {
    const dx = p.cardX - p.anchorX;
    const dy = p.cardY - p.anchorY;
    const dist = Math.hypot(dx, dy);
    if (dist < 1e-3) {
        return { x: p.cardX, y: p.cardY };
    }
    // Distance from card center to the facing edge along the connector ray.
    const nx = dx / dist;
    const ny = dy / dist;
    const hx = p.w / 2;
    const hy = p.h / 2;
    const tx = Math.abs(nx) > 1e-6 ? hx / Math.abs(nx) : Number.POSITIVE_INFINITY;
    const ty = Math.abs(ny) > 1e-6 ? hy / Math.abs(ny) : Number.POSITIVE_INFINITY;
    const edgeDist = Math.min(tx, ty);
    const stop = Math.max(0, dist - edgeDist);
    return { x: p.anchorX + nx * stop, y: p.anchorY + ny * stop };
};

export const RoadGroupingNumberCardsOverlay = () => {
    const toolActive = useValue(tool.activeTool$).id === ROAD_GROUPING_TOOL_ID;
    const [cards, setCards] = useState<RoadGroupingSegmentCard[]>([]);
    const [viewport, setViewport] = useState({ w: window.innerWidth, h: window.innerHeight });

    useEffect(() => {
        const onCards = async (value: unknown) => {
            setCards(normalizeCards(value ?? RoadGroupingService.SegmentCards.value));
        };
        RoadGroupingService.SegmentCards.reactivate();
        RoadGroupingService.SegmentCards.subscribe(onCards);

        const onResize = () => setViewport({ w: window.innerWidth, h: window.innerHeight });
        window.addEventListener("resize", onResize);
        return () => window.removeEventListener("resize", onResize);
    }, []);

    const placed = useMemo(
        () => (toolActive ? layoutCards(cards, viewport.w, viewport.h) : []),
        [toolActive, cards, viewport.w, viewport.h]
    );

    if (!toolActive || !placed.length) {
        return null;
    }

    return (
        <div className={styles.root} aria-hidden>
            <svg className={styles.connectors} width={viewport.w} height={viewport.h}>
                {placed.map((p) => {
                    const end = connectorEnd(p);
                    return (
                        <g key={`c-${p.key}`}>
                            <line
                                className={styles.connectorLine}
                                x1={p.anchorX}
                                y1={p.anchorY}
                                x2={end.x}
                                y2={end.y}
                            />
                            <circle
                                className={styles.anchorDot}
                                cx={p.anchorX}
                                cy={p.anchorY}
                                r={ANCHOR_R}
                            />
                        </g>
                    );
                })}
            </svg>
            {placed.map((p) => (
                <div
                    key={p.key}
                    className={styles.card}
                    style={{
                        left: p.cardX,
                        top: p.cardY,
                        minWidth: p.w,
                    }}
                >
                    {p.label}
                </div>
            ))}
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
