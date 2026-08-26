import { NamesetService } from "@klyte45/adr-commons";
import { VanillaWidgets } from "@klyte45/vuio-commons";
import { useEffect, useMemo, useState } from "react";
import { translate } from "../../translate";
import styles from "../../styles/namesetWordsContainer.module.scss";

type Props = {
    values: string[];
    valuesAlternative: string[];
};

export const NamesetWordsContainer = ({ values, valuesAlternative }: Props) => {
    const Scrollable = VanillaWidgets.instance.EditorScrollable;
    const [sortedValues, setSortedValues] = useState([] as string[]);

    const alt = valuesAlternative?.length ? valuesAlternative : values;

    const groupedValues = useMemo(() => {
        const targetValues = values.map((x, i) =>
            x === alt[i] ? x : `${x}|${alt[i]}`,
        );
        return targetValues.reduce((p, n) => {
            p[n] = (p[n] ?? 0) + 1;
            return p;
        }, {} as Record<string, number>);
    }, [values, alt]);

    const uniqueKeys = useMemo(() => Object.keys(groupedValues), [groupedValues]);
    const keysSignature = uniqueKeys.join("\0");

    useEffect(() => {
        const keys = keysSignature ? keysSignature.split("\0") : [];
        if (!keys.length) {
            setSortedValues([]);
            return;
        }
        let active = true;
        NamesetService.sortValues(keys)
            .then((res) => { if (active) setSortedValues(res); })
            .catch(() => { if (active) setSortedValues([...keys].sort()); });
        return () => { active = false; };
    }, [keysSignature]);

    if (sortedValues.length !== uniqueKeys.length) {
        return null;
    }

    return (
        <div className={styles.wordsRoot}>
            <Scrollable>
                <div className={styles.wordsContainer}>
                    {sortedValues.sort((a, b) => groupedValues[b] - groupedValues[a]).map((y, i) => {
                        const count = groupedValues[y];
                        const valueArr = y.split("|");
                        return (
                            <div className={[styles.nameToken, valueArr.length > 1 ? styles.double : ""].filter(Boolean).join(" ")} key={i}>
                                {valueArr.length === 1
                                    ? <div className={styles.value}>{y}</div>
                                    : <div className={styles.value}>{valueArr[0]}<div className={styles.alt}>{valueArr[1]}</div></div>}
                                {count > 1 && <div className={styles.quantity}>{count}</div>}
                            </div>
                        );
                    })}
                </div>
            </Scrollable>
            <div className={styles.footerStats}>
                {`${translate("namesetLine.entriesCountLbl")} ${values.length} | ${translate("namesetLine.entriesCountUniqueLbl")} ${sortedValues.length}`}
            </div>
        </div>
    );
};
