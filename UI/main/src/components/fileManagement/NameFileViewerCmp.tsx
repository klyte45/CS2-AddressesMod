import { ExtendedSimpleNameEntry, NameFileManagementService, SimpleNameEntry } from "#service/NameFileManagementService";
import "#styles/wordContainer.scss";
import { DefaultPanelScreen, replaceArgs, translate } from "@klyte45/euis-components";
import { ObjectTyped } from "object-typed";
import { Component } from "react";
import { NameFileCategoryCmp } from "./NameFileCategoryCmp";
import { NameFileEntryCmp } from "./NameFileEntryCmp";


export type StructureTreeNode = {
  rootContent: SimpleNameEntry[],
  subtrees: Record<string, StructureTreeNode>
}



export class NameFileViewerCmp extends Component<{}, {
  availableFiles?: StructureTreeNode;
  totalCount: number,
  currentItem?: ExtendedSimpleNameEntry
}> {
  constructor(props) {
    super(props);
    engine.whenReady.then(() => {
      this.listFiles();
    });
  }
  async listFiles(fullReload: boolean = false) {
    const palettesSaved: SimpleNameEntry[] = fullReload ? await NameFileManagementService.reloadDiskSimpleNames() : await NameFileManagementService.listDiskSimpleNames();
    const paletteTree = categorizeFiles(palettesSaved)
    const root = paletteTree[""]?.rootContent ?? []
    delete paletteTree[""];
    this.setState({
      availableFiles: {
        rootContent: root,
        subtrees: paletteTree
      },
      totalCount: palettesSaved.length
    });
  }

  render() {
    if (!this.state) return null;
    if (this.state.currentItem) {
      const item = this.state.currentItem;
      const groupedValues = item.Values.reduce((p, n) => {
        p[n] = (p[n] ?? 0) + 1
        return p;
      }, {} as Record<string, number>)
      const buttons = <>
        <button className="neutralBtn" onClick={() => this.setState({ currentItem: undefined })}>{translate("fileViewer.back")}</button>
      </>
      return <DefaultPanelScreen title={item._CurrName ?? item.Name} subtitle={replaceArgs(translate("fileViewer.fileCounterFmt"), { count: item.Values.length.toFixed() })}
        buttonsRowContent={buttons}>
        <div className="wordsContainer">{
          ObjectTyped.entries(groupedValues).sort((a, b) => (b[1] - a[1]) || a[0].localeCompare(b[0])).map((x, i) => <div className="nameToken" key={i} >
            <div className="value">{x[0]}</div>
            {x[1] > 1 && <div className="quantity">{x[1]}</div>}
          </div>)
        }</div>
      </DefaultPanelScreen>
    } else {
      const buttons = <>
        <button className="neutralBtn" onClick={() => this.listFiles(true)}>{translate("fileViewer.reloadFiles")}</button>
        <button className="neutralBtn" onClick={() => NameFileManagementService.goToDiskSimpleNamesFolder()}>{translate("fileViewer.goToSimpleNamesFolder")}</button>
      </>
      return <DefaultPanelScreen
        title={translate("fileViewer.title")}
        subtitle={replaceArgs(translate("fileViewer.entriesCountFmt"), { count: this.state?.totalCount?.toFixed() })}
        buttonsRowContent={buttons}>
        {Object.keys(this.state?.availableFiles.subtrees ?? {}).length == 0 && !this.state?.availableFiles.rootContent.length
          ? <h2>{translate("fileViewer.noFiles")}</h2>
          : <NameFileCategoryCmp entry={this.state?.availableFiles} doWithPaletteData={(x, i) => <NameFileEntryCmp onView={(x) => this.onView(x)} entry={x} key={i} />} />}
      </DefaultPanelScreen>;
    }
  }

  onView(x: ExtendedSimpleNameEntry): void {
    this.setState({ currentItem: x });
  }
}


export function categorizeFiles(palettesSaved: SimpleNameEntry[], iteration: number = 0): Record<string, StructureTreeNode> {
  return ObjectTyped.fromEntries(ObjectTyped.entries(palettesSaved.reduce((prev, curr) => {
    if (!curr._CurrName) {
      curr._CurrName = curr.Name;
    }

    var splittenName = curr._CurrName.split("/");
    const groupName = splittenName.shift();
    const selfName = splittenName.join("/");
    if (!selfName) {
      prev[""] ??= [];
      prev[""].push(curr);
    } else {
      prev[groupName] ??= [];
      curr._CurrName = selfName;
      prev[groupName].push(curr);
    }
    return prev;
  }, {} as Record<string, SimpleNameEntry[]>)).map(x => {
    return [
      x[0],
      {
        rootContent: x[1].filter(x => x._CurrName.indexOf("/") == -1),
        subtrees: categorizeFiles(x[1].filter(x => x._CurrName.indexOf("/") >= 0), iteration++)
      } as StructureTreeNode
    ] as [string, StructureTreeNode]
  }));
}
