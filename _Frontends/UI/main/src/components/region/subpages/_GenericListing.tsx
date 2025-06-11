import { GameScrollComponent } from "@klyte45/euis-components";

export type ListItemData<T> = {
    key: string | number;
    title: React.ReactNode;
    subTitle?: React.ReactNode;
    color?: string;
    actions: { label: string; className?: string; onClick: () => void; }[];
    onMouseEnter?: () => void;
    onMouseLeave?: () => void;
};
type GenericListProps<T> = {
    title: React.ReactNode;
    items: ListItemData<T>[];
    noItemsMessage: string;
    onAdd: () => void;
    addBtnLabel: string;
    parentContainerClass?: string;
    contentClass?: string;
};
export function _GenericListing<T>({
    title, items, noItemsMessage, onAdd, addBtnLabel, parentContainerClass = "listContainer", contentClass = "listWrapper",
}: GenericListProps<T>) {
    return <div className="genericList">
        <h2>{title}</h2>
        {items.length ? (
            <GameScrollComponent parentContainerClass={parentContainerClass} contentClass={contentClass}>
                {items.map((item) => (
                    <div
                        key={item.key}
                        className="tableItem"
                        onMouseEnter={item.onMouseEnter}
                        onMouseLeave={item.onMouseLeave}
                    >
                        <div className="data">
                            {item.color && <div className="color" style={{ backgroundColor: item.color }} />}
                            <div className="textContainer">
                                <div className="title">{item.title}</div>
                                {item.subTitle && <div className="subTitle">{item.subTitle}</div>}
                            </div>
                        </div>
                        <div className="actions">
                            {item.actions.map((action, idx) => (
                                <button key={idx} className={action.className ?? "neutralBtn"} onClick={action.onClick}>
                                    {action.label}
                                </button>
                            ))}
                        </div>
                    </div>
                ))}
            </GameScrollComponent>
        ) : (
            <div className="noItemsMessage">{noItemsMessage}</div>
        )}
        <div className="bottomActions">
            <button className="positiveBtn" onClick={onAdd}>
                {addBtnLabel}
            </button>
        </div>
    </div>;
}
