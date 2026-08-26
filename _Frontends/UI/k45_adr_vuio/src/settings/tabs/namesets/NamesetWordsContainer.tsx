import { NamesetService } from "@klyte45/adr-commons";
import { VanillaWidgets } from "@klyte45/vuio-commons";
import { useEffect, useState } from "react";
import { translate } from "../../translate";
import styles from "../../styles/namesetWordsContainer.module.scss";

type Props = {
    values: string[];
    valuesAlternative: string[];
};

export const NamesetWordsContainer = ({ values, valuesAlternative }: Props) => {
    const Scrollable = VanillaWidgets.instance.EditorScrollable;
    const [sortedValues, setSortedValues] = useState([] as string[]);

    const targetValues = values.map((x, i) =>
        x == valuesAlternative[i] ? x : `${x}|${valuesAlternative[i]}`,
    );

    const groupedValues = targetValues.reduce((p, n) => {
        p[n] = (p[n] ?? 0) + 1;
        return p;
    }, {} as Record<string, number>);

    useEffect(() => {
        const keys = Object.keys(groupedValues);
        if (!keys.length) {
            setSortedValues([]);
            return;
        }
        NamesetService.sortValues(keys).then(setSortedValues);
    }, [values, valuesAlternative]);

    if (sortedValues.length != Object.keys(groupedValues).length) {
        return <></>;
    }

    return <>
        <Scrollable className={styles.wordsContainer}>
            {sortedValues.sort((a, b) => groupedValues[b] - groupedValues[a]).map((y, i) => {
                const count = groupedValues[y];
                const valueArr = y.split("|");
                return (
                    <div className={[styles.nameToken, valueArr.length > 1 ? styles.double : ""].filter(Boolean).join(" ")} key={i}>
                        {valueArr.length == 1
                            ? <div className={styles.value}>{y}</div>
                            : <div className={styles.value}>{valueArr[0]}<div className={styles.alt}>{valueArr[1]}</div></div>}
                        {count > 1 && <div className={styles.quantity}>{count}</div>}
                    </div>
                );
            })}
        </Scrollable>
        <div className={styles.footerStats}>
            {`${translate("namesetLine.entriesCountLbl")} ${values.length} | ${translate("namesetLine.entriesCountUniqueLbl")} ${sortedValues.length}`}
        </div>
    </>;
};
