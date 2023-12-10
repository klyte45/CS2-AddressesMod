import { GameScrollComponent } from "@klyte45/euis-components";
import { ObjectTyped } from "object-typed";
import { Component } from "react";
import '#styles/wordContainer.scss'


export class NamesetWordsContainer extends Component<{
  values: string[];
}> {

  render() {
    const groupedValues = this.props.values.reduce((p, n) => {
      p[n] = (p[n] ?? 0) + 1;
      return p;
    }, {} as Record<string, number>);
    return < >
      <GameScrollComponent contentClass="wordsContainer">{ObjectTyped.entries(groupedValues).sort((a, b) => (b[1] - a[1]) || a[0].localeCompare(b[0], "en", { sensitivity: "base" })).map((x, i) => <div className="nameToken" key={i}>
        <div className="value">{x[0]}</div>
        {x[1] > 1 && <div className="quantity">{x[1]}</div>}
      </div>)}</GameScrollComponent>
    </>;
  }
}
