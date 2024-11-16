import { MultiUIValueBinding, ConstructorObjectToInstancesObject, InitializeBindings } from "@klyte45/euis-components";
import { useState, useEffect } from "react";

const BasePlatesController = {
  LettersAllowed: (MultiUIValueBinding<string[]>),
  FlagsLocal: (MultiUIValueBinding<number>),
  FlagsCarNumber: (MultiUIValueBinding<number>),
  FlagsRandomized: (MultiUIValueBinding<number>),
  MonthsFromEpochOffset: (MultiUIValueBinding<number>),
  SerialIncrementEachMonth: (MultiUIValueBinding<number>),
};
type Props = {
  prefix: string;
};
export const VehiclePlateControllerComponent = ({ prefix }: Props) => {

  const [controllerData, setControllerData] = useState(undefined as ConstructorObjectToInstancesObject<typeof BasePlatesController>);
  const [_, setBuildIdx] = useState(0 as any);

  useEffect(() => {
    const tmp = InitializeBindings({
      _prefix: prefix,
      ...BasePlatesController
    });
    setControllerData(tmp);
    Object.entries(tmp).forEach(([x, y]) => y.subscribe(async (z) => { setBuildIdx(x + JSON.stringify(z)); }));
    return () => {
      controllerData && Object.values(controllerData).forEach(x => x.dispose());
    };
  }, []);

  return <>
    {Object.entries(controllerData || {}).map((x, i) => {
      return <div key={i}>{`${x[0]} => ${JSON.stringify(x[1].value)}`}</div>;
    })}
  </>;
};
