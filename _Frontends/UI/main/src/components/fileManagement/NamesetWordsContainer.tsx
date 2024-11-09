import '#styles/wordContainer.scss';
import { translate } from "#utility/translate";
import { NamesetService } from "@klyte45/adr-commons";
import { GameScrollComponent } from "@klyte45/euis-components";
import { useState } from "react";


type Props = {
  values: string[];
};

export const NamesetWordsContainer = ({ values }: Props) => {

  const [sortedValues, setSortedValues] = useState([] as string[])

  const groupedValues = values.reduce((p, n) => {
    p[n] = (p[n] ?? 0) + 1;
    return p;
  }, {} as Record<string, number>);
  if (sortedValues.length != Object.keys(groupedValues).length) {
    NamesetService.sortValues(Object.keys(groupedValues)).then(res => setSortedValues(res));
    return <></>
  }
  return <>
    <GameScrollComponent contentClass="wordsContainer">{sortedValues.sort((a, b) => groupedValues[b] - groupedValues[a]).map((y, i) => {
      const x = groupedValues[y];
      return <div className="nameToken" key={i}>
        <div className="value">{y}</div>
        {x > 1 && <div className="quantity">{x}</div>}
      </div>;
    })}</GameScrollComponent>
    <div style={{ fontSize: "85%", padding: "4rem" }}>{`${translate("namesetLine.entriesCountLbl")} ${values.length} | ${translate("namesetLine.entriesCountUniqueLbl")} ${sortedValues.length}`}</div>
  </>;

}
