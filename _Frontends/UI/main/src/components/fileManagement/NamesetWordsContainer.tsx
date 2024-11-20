import '#styles/wordContainer.scss';
import { translate } from "#utility/translate";
import { NamesetService } from "@klyte45/adr-commons";
import { GameScrollComponent } from "@klyte45/euis-components";
import { useState } from "react";


type Props = {
  values: string[];
  valuesAlternative: string[];
};

export const NamesetWordsContainer = ({ values, valuesAlternative }: Props) => {

  const [sortedValues, setSortedValues] = useState([] as string[])

  const targetValues = values.map((x, i) => x == valuesAlternative[i] ? x : `${x}|${valuesAlternative[i]}`)

  const groupedValues = targetValues.reduce((p, n) => {
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
      const valueArr = y.split("|");
      return <div className={["nameToken", valueArr.length > 1 && "double"].join(" ")} key={i}>
        {valueArr.length == 1 ? <div className="value">{y}</div> : <div className="value">{valueArr[0]}<div className='alt'>{valueArr[1]}</div></div>}
        {x > 1 && <div className="quantity">{x}</div>}
      </div>;
    })}</GameScrollComponent>
    <div style={{ fontSize: "85%", padding: "4rem" }}>{`${translate("namesetLine.entriesCountLbl")} ${values.length} | ${translate("namesetLine.entriesCountUniqueLbl")} ${sortedValues.length}`}</div>
  </>;

}
