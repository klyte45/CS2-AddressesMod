import { translate } from "#utility/translate";
import { SimpleNameEntry, NamingRulesService, replaceArgs } from "@klyte45/adr-commons";
import { MultiUIValueBinding, ConstructorObjectToInstancesObject, InitializeBindings, Cs2FormLine, Cs2Select, SimpleInput, Input, Cs2Checkbox, Cs2CheckboxWithLine } from "@klyte45/euis-components";
import { useState, useEffect, useRef, MutableRefObject } from "react";
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

const options = [
  { v: 0 }, { v: 1 }, { v: 2 }
]

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

  const setDigitSource = (i: number, type: number) => {
    controllerData.FlagsCarNumber.set(controllerData.FlagsCarNumber.value & ~(type == 2 ? 0 : 1 << i) | (type == 2 ? 1 << i : 0));
    controllerData.FlagsLocal.set(controllerData.FlagsLocal.value & ~(type == 1 ? 0 : 1 << i) | (type == 1 ? 1 << i : 0));
  }

  return !!controllerData && <>
    <Input
      title={translate("vehiclePlate.monthsOffsetForGenerator")}
      subtitle={replaceArgs(translate("vehiclePlate.monthsOffsetForGenerator.subTitleTemplate"), { value: engine.translate("Common.MONTH:" + (controllerData.MonthsFromEpochOffset.value % 12)) + "/" + Math.floor(controllerData.MonthsFromEpochOffset.value / 12) })}
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
        controllerData.LettersAllowed.value?.map((x, i) => {
          const digitFlagId = controllerData.LettersAllowed.value.length - i - 1;
          return <div key={i} className="letterItem">
            <Cs2Select
              options={options}
              getOptionLabel={(x) => translate("vehiclePlate.identifierDigitSource." + x.v)}
              getOptionValue={(x) => x.v.toFixed()}
              value={{ v: (controllerData.FlagsLocal.value & (1 << digitFlagId) ? 1 : controllerData.FlagsCarNumber.value & (1 << digitFlagId) ? 2 : 0) }}
              onChange={x => setDigitSource(digitFlagId, x.v)} />
            <CharsAllowedInput controllerData={controllerData} i={i} />
            <Cs2CheckboxWithLine title={translate("vehiclePlate.allowRandomizeDigitLabel")} isChecked={() => (controllerData.FlagsRandomized.value & (1 << i)) != 0} onValueToggle={(x) => controllerData.FlagsRandomized.set(controllerData.FlagsRandomized.value & ~(x ? 0 : 1 << i) | (x ? 1 << i : 0))} />
          </div>;
        }
        )
      }
    </div>
  </>;
};
const CharsAllowedInput = ({ controllerData, i }: {
  controllerData: ConstructorObjectToInstancesObject<typeof BasePlatesController>,
  i: number
}) => {
  const refTextArea = useRef() as MutableRefObject<HTMLTextAreaElement>;

  useEffect(() => {
    if (refTextArea.current)
      refTextArea.current.value = controllerData.LettersAllowed.value[i];
  }, [refTextArea.current, controllerData.LettersAllowed.value[i]])

  const onSuccessChange = () => {
    const typingText = refTextArea.current.value;
    if (!typingText) {
      onRevert(); return;
    }
    const newArr = controllerData.LettersAllowed.value;
    newArr[i] = Object.keys([...typingText].reduce((p, n) => { p[n] = true; return p; }, {})).join("");
    controllerData.LettersAllowed.set(newArr);
    refTextArea.current.blur()
  }

  const onRevert = () => {
    refTextArea.current.value = controllerData.LettersAllowed.value[i];
  }


  return <textarea
    ref={refTextArea}
    className="lettersSet"
    onBlur={onRevert}
    onKeyDown={(x) => {
      console.log(x)
      if (x.key == "Escape") {
        (x.target as HTMLTextAreaElement).blur();
        x.preventDefault();
        onRevert();
        return;
      } else if (x.key == "Enter") {
        x.preventDefault();
        onSuccessChange();
        return;
      }
    }} />;
}

