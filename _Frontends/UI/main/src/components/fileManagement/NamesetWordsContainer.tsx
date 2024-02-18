import { GameScrollComponent } from "@klyte45/euis-components";
import { ObjectTyped } from "object-typed";
import { Component } from "react";
import '#styles/wordContainer.scss'
import { NamesetService } from "#service/NamesetService";
import { translate } from "#utility/translate";


type Props = {
  values: string[];
};

export class NamesetWordsContainer extends Component<Props, { sortedValues: string[] }> {

  constructor(props: Props) {
    super(props)
    this.state = {
      sortedValues: []
    }
  }

  render() {
    const groupedValues = this.props.values.reduce((p, n) => {
      p[n] = (p[n] ?? 0) + 1;
      return p;
    }, {} as Record<string, number>);
    if (this.state.sortedValues.length != Object.keys(groupedValues).length) {
      NamesetService.sortValues(Object.keys(groupedValues)).then(res => this.setState({ sortedValues: res }));
      return <></>
    }
    return < >
      <GameScrollComponent contentClass="wordsContainer">{this.state.sortedValues.sort((a, b) => groupedValues[b] - groupedValues[a]).map((y, i) => {
        const x = groupedValues[y];
        return <div className="nameToken" key={i}>
          <div className="value">{y}</div>
          {x > 1 && <div className="quantity">{x}</div>}
        </div>;
      })}</GameScrollComponent>
      <div style={{ fontSize: "85%", padding: "4rem" }}>{`${translate("namesetLine.entriesCountLbl")} ${this.props.values.length} | ${translate("namesetLine.entriesCountUniqueLbl")} ${this.state.sortedValues.length}`}</div>
    </>;
  }
}
