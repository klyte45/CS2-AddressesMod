import { translate } from "#utility/translate";
import { replaceArgs } from "@klyte45/adr-commons";
import { ConstructorObjectToInstancesObject, Cs2CheckboxWithLine, Cs2FormLine, Cs2Select, InitializeBindings, Input, MultiUIValueBinding } from "@klyte45/euis-components";
import { MutableRefObject, useEffect, useRef, useState } from "react";
import "./vehiclePlateController.scss";
import Select from "react-select/dist/declarations/src/Select";

const BaseSerialController = {
  LettersAllowed: (MultiUIValueBinding<string[]>),
  FlagsOwnSerial: (MultiUIValueBinding<number>),
  FlagsCarNumber: (MultiUIValueBinding<number>),
  BuildingIdOnStart: (MultiUIValueBinding<number>),
};
type Props = {
  type: string;
};

const options = [
  { v: 0 }, { v: 1 }
]

export const VehicleSerialControllerComponent = ({ type }: Props) => {

  const [controllerData, setControllerData] = useState(undefined as ConstructorObjectToInstancesObject<typeof BaseSerialController>);
  const [_, setBuildIdx] = useState(0 as any);


  useEffect(() => {
    const tmp = InitializeBindings({
      _prefix: "k45::adr.vehicleSerial." + type,
      ...BaseSerialController
    });
    setControllerData(tmp);
    Object.entries(tmp).forEach(([x, y]) => y.subscribe(async (z) => { setBuildIdx(x + JSON.stringify(z)); }));
    return () => {
      controllerData && Object.values(controllerData).forEach(x => x.dispose());
    };
  }, []);

  const setDigitSource = (i: number, type: number) => {
    controllerData.FlagsCarNumber.set(controllerData.FlagsCarNumber.value & ~(type == 1 ? 0 : 1 << i) | (type == 1 ? 1 << i : 0));
    controllerData.FlagsOwnSerial.set(controllerData.FlagsOwnSerial.value & ~(type == 0 ? 0 : 1 << i) | (type == 0 ? 1 << i : 0));
  }

  const optionsBuildingId = [
    { label: translate("vehicleSerial.buildingCityIdPosition.start"), value: 1 },
    { label: translate("vehicleSerial.buildingCityIdPosition.end"), value: 0 },
    { label: translate("vehicleSerial.buildingCityIdPosition.none"), value: -1 }
  ]

  return !!controllerData && <>
    <Cs2FormLine title={translate("vehicleSerial.buildingCityIdPosition")}>
      <Cs2Select
        options={optionsBuildingId}
        getOptionLabel={(x) => x?.label}
        getOptionValue={(x) => x?.value.toFixed()}
        onChange={(x) => controllerData.BuildingIdOnStart.set(x.value)}
        value={optionsBuildingId[1 - Math.sign(controllerData.BuildingIdOnStart.value)]}
      />
    </Cs2FormLine>
    <Input
      title={translate("vehiclePlate.digitsQuantity")}
      getValue={() => controllerData.LettersAllowed.value?.length.toFixed(0)}
      maxLength={8} isValid={(x) => parseInt(x) > 0 && parseInt(x) <= 9}
      onValueChanged={async (x) => {
        const newSize = [...new Array(parseInt(x))].map((y, i) => controllerData.LettersAllowed.value[i] || "0123456789");
        controllerData.LettersAllowed.set(newSize); return controllerData.LettersAllowed.value.length.toFixed(0)
      }}
    />
    <h2>{translate("vehicleSerial.identifierCharSettingsTitle." + type)}</h2>
    <div className="vehiclePlateLettersEditorContainer">
      {
        controllerData.LettersAllowed.value?.map((x, i) => {
          const digitFlagId = controllerData.LettersAllowed.value.length - i - 1;
          return <div key={i} className="letterItem">
            <Cs2Select
              options={options}
              getOptionLabel={(x) => translate("vehicleSerial.identifierDigitSource." + x.v)}
              getOptionValue={(x) => x.v.toFixed()}
              value={{ v: (controllerData.FlagsOwnSerial.value & (1 << digitFlagId) ? 0 : controllerData.FlagsCarNumber.value & (1 << digitFlagId) ? 1 : -1) }}
              onChange={x => setDigitSource(digitFlagId, x.v)} />
            <CharsAllowedInput controllerData={controllerData} i={i} />
          </div>;
        }
        )
      }
    </div>
  </>;
};
const CharsAllowedInput = ({ controllerData, i }: {
  controllerData: ConstructorObjectToInstancesObject<typeof BaseSerialController>,
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

