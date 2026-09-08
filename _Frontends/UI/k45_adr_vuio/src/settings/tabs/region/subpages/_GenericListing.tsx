import { ReactNode } from "react";
import { VanillaWidgets } from "@klyte45/vuio-commons";
import styles from "../../../styles/genericListing.module.scss";

export type ListItemData = {
    key: string | number;
    title: ReactNode;
    subTitle?: ReactNode;
    color?: string;
    actions: { label: string; className?: string; onClick: () => void }[];
    onMouseEnter?: () => void;
    onMouseLeave?: () => void;
};

type Props = {
    title: ReactNode;
    items: ListItemData[];
    noItemsMessage: string;
    onAdd: () => void;
    addBtnLabel: string;
    titleEnd?: ReactNode;
};

export function GenericListing({
    title,
    items,
    noItemsMessage,
    onAdd,
    addBtnLabel,
    titleEnd,
}: Props) {
    const Scrollable = VanillaWidgets.instance.EditorScrollable;

    return (
        <div className={`${styles.genericList} k45_btnHost`}>
            <div className={styles.header}>
                <h2>{title}</h2>
                {titleEnd}
            </div>
            {items.length ? (
                <Scrollable className={styles.listContainer}>
                    <div className={styles.listWrapper}>
                        {items.map((item) => (
                            <div
                                key={item.key}
                                className={styles.tableItem}
                                onMouseEnter={item.onMouseEnter}
                                onMouseLeave={item.onMouseLeave}
                            >
                                <div className={styles.data}>
                                    {item.color && <div className={styles.color} style={{ backgroundColor: item.color }} />}
                                    <div className={styles.textContainer}>
                                        <div className={styles.title}>{item.title}</div>
                                        {item.subTitle && <div className={styles.subTitle}>{item.subTitle}</div>}
                                    </div>
                                </div>
                                <div className={styles.actions}>
                                    {item.actions.map((action, idx) => (
                                        <button type="button" key={idx} className={action.className ?? "neutralBtn"} onClick={action.onClick}>
                                            {action.label}
                                        </button>
                                    ))}
                                </div>
                            </div>
                        ))}
                    </div>
                </Scrollable>
            ) : (
                <div className={styles.noItemsMessage}>{noItemsMessage}</div>
            )}
            <div className={styles.bottomActions}>
                <button type="button" className="positiveBtn" onClick={onAdd}>
                    {addBtnLabel}
                </button>
            </div>
        </div>
    );
}
