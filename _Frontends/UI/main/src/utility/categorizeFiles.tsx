import { SimpleNameEntry } from "@klyte45/adr-commons";
import { ObjectTyped } from "object-typed";


export type StructureTreeNode = {
  rootContent: SimpleNameEntry[],
  subtrees: Record<string, StructureTreeNode>
}

export function categorizeFiles(namesetsSaved: SimpleNameEntry[], iteration: number = 0): Record<string, StructureTreeNode> {
  return ObjectTyped.fromEntries(ObjectTyped.entries(namesetsSaved.reduce((prev, curr) => {
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
    ] as [string, StructureTreeNode];
  }));
}
