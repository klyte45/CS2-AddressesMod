import { forwardRef, type CSSProperties, type HTMLAttributes, type ReactNode } from "react";
import styles from "../../styles/regionEditor.module.scss";

type Props = {
    cityMap: string | null;
    waterMap: string | null;
    beforeAnyLayer?: ReactNode;
    children?: ReactNode;
};

export const MapDiv = forwardRef<HTMLDivElement, Props & HTMLAttributes<HTMLDivElement>>(({
    cityMap,
    waterMap,
    beforeAnyLayer,
    children,
    className,
    style,
    ...rest
}, ref) => (
    <div
        {...rest}
        ref={ref}
        className={[styles.cityMapTopographic, className].filter(Boolean).join(" ")}
        style={cityMap ? style : ({ display: "none" } as CSSProperties)}
    >
        {beforeAnyLayer}
        <div
            className={styles.cityMapTopographic}
            style={cityMap ? ({ backgroundImage: `url(${cityMap})` } as CSSProperties) : undefined}
        />
        <div
            className={styles.waterLayer}
            style={waterMap ? ({ backgroundImage: `url(${waterMap})` } as CSSProperties) : undefined}
        />
        {children}
    </div>
));

MapDiv.displayName = "MapDiv";
