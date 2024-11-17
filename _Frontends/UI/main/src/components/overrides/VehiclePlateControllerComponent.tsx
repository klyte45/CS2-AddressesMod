import { translate } from "#utility/translate";
import { SimpleNameEntry, NamingRulesService, replaceArgs } from "@klyte45/adr-commons";
import { MultiUIValueBinding, ConstructorObjectToInstancesObject, InitializeBindings, Cs2FormLine, Cs2Select, SimpleInput, Input } from "@klyte45/euis-components";
import { useState, useEffect } from "react";
import { defaultSetting } from "./OverrideSettingsCmp";
import "./vehiclePlateController.scss"

const BasePlatesController = {
  LettersAllowed: (MultiUIValueBinding<string[]>),
  FlagsLocal: (MultiUIValueBinding<number>),
  FlagsCarNumber: (MultiUIValueBinding<number>),
  FlagsRandomized: (MultiUIValueBinding<number>),
  MonthsFromEpochOffset: (MultiUIValueBinding<number>),
  SerialIncrementEachMonth: (MultiUIValueBinding<number>),
};
type Props = {
  type: string;
};
export const VehiclePlateControllerComponent = ({ type }: Props) => {

  const [controllerData, setControllerData] = useState(undefined as ConstructorObjectToInstancesObject<typeof BasePlatesController>);
  const [_, setBuildIdx] = useState(0 as any);

  useEffect(() => {
    const tmp = InitializeBindings({
      _prefix: "k45::adr.vehiclePlate." + type,
      ...BasePlatesController
    });
    setControllerData(tmp);
    Object.entries(tmp).forEach(([x, y]) => y.subscribe(async (z) => { setBuildIdx(x + JSON.stringify(z)); }));
    return () => {
      controllerData && Object.values(controllerData).forEach(x => x.dispose());
    };
  }, []);

  return !!controllerData && <>
    <Input
      title={translate("vehiclePlate.monthsOffsetForGenerator")}
      subtitle={replaceArgs(translate("vehiclePlate.monthsOffsetForGenerator.subTitleTemplate"), {value: engine.translate("Common.MONTH:" + (controllerData.MonthsFromEpochOffset.value % 12)) + "/" + Math.floor(controllerData.MonthsFromEpochOffset.value / 12)})}
      getValue={() => controllerData.MonthsFromEpochOffset.value?.toFixed(0)}
      maxLength={8} isValid={(x) => isFinite(parseInt(x))}
      onValueChanged={async (x) => { controllerData.MonthsFromEpochOffset.set(parseInt(x)); return controllerData.MonthsFromEpochOffset.value.toFixed(0) }}
    />
    <Input
      title={translate("vehiclePlate.serialIncrementEachMonth")}
      getValue={() => controllerData.SerialIncrementEachMonth.value?.toFixed(0)}
      maxLength={8} isValid={(x) => parseInt(x) >= 0}
      onValueChanged={async (x) => { controllerData.SerialIncrementEachMonth.set(parseInt(x)); return controllerData.SerialIncrementEachMonth.value.toFixed(0) }}
    />
    <Input
      title={translate("vehiclePlate.digitsQuantity")}
      getValue={() => controllerData.LettersAllowed.value?.length.toFixed(0)}
      maxLength={8} isValid={(x) => parseInt(x) > 0 && parseInt(x) <= 9}
      onValueChanged={async (x) => {
        const newSize = [...new Array(parseInt(x))].map((y, i) => controllerData.LettersAllowed.value[i] || "0123456789");
        controllerData.LettersAllowed.set(newSize); return controllerData.LettersAllowed.value.length.toFixed(0)
      }}
    />
    <h2>{translate("vehiclePlate.identifierCharSettingsTitle." + type)}</h2>
    <div className="vehiclePlateLettersEditorContainer">
      {
        controllerData.LettersAllowed.value?.map((x, i) =>
          <div key={i} className="letterItem">
            <Cs2Select

            />
          </div>
        )
      }
    </div>

    {Object.entries(controllerData || {}).map((x, i) => {
      return <div key={i}>{`${x[0]} => ${JSON.stringify(x[1].value)}`}</div>;
    })}
  </>;
};
