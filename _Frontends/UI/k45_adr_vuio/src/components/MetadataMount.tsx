import { MultiUIValueBinding } from "@klyte45/vuio-commons";
import { ObjectTyped } from "object-typed";
import { useState, useEffect } from "react";
import { AdrFields, AdrFieldData, Condition } from "service/AdrHighwayRoutesSystem";

type MetadataProps = {
    metadata: AdrFields<string>;
    parameters: MultiUIValueBinding<number>[];
    output: React.FC<MountMetadataComponentProps>;
};
export const MetadataMount = ({ metadata, parameters, output: Output }: MetadataProps) => {
    const [storedValues, setStoredValues] = useState<Record<keyof typeof metadata, number>>({});

    function selectParam(fieldData: AdrFieldData<string>) {
        return parameters[fieldData.parameter];
    }
    function paramValueToFieldValue(value: number, fieldData: AdrFieldData<string>) {
        const result = (value >> fieldData.position) & (fieldData.size == 31 ? 0x7fff_ffff : (1 << (fieldData.size)) - 1);
        return result;
    }
    function mergeFieldValueToParam(paramValue: number, fieldValue: number, fieldData: AdrFieldData<string>) {
        const maskRelative = fieldData.size == 31 ? 0x7fff_ffff : ((1 << (fieldData.size)) - 1);
        const relativeValue = fieldValue & maskRelative;
        const maskAbsolute = maskRelative << fieldData.position;
        const absoluteValue = relativeValue << fieldData.position;
        // console.log(paramValue, fieldData, fieldValue, maskRelative, relativeValue, maskAbsolute, absoluteValue);
        return (paramValue & ~maskAbsolute) | absoluteValue;
    }

    function checkConditionsAreMet(field: AdrFieldData<string>) {
        return field.condition == undefined || evaluateCondition(field.condition);
    }

    function evaluateCondition(condition: Condition<string>): boolean {
        const [k, v] = ObjectTyped.entries(condition)[0];
        switch (k) {
            case "or":
                return (v as Condition<string>[]).some(x => evaluateCondition(x));
            case "and":
                return (v as Condition<string>[]).every(x => evaluateCondition(x));
        }
        const valueArray = (v as [string, number]);
        switch (k) {
            case "eq":
                return storedValues[valueArray[0]] == valueArray[1];
            case "ne":
                return storedValues[valueArray[0]] != valueArray[1];
            case "gt":
                return storedValues[valueArray[0]] > valueArray[1];
            case "lt":
                return storedValues[valueArray[0]] < valueArray[1];
        }
        return false;
    }

    useEffect(() => {
        setStoredValues(ObjectTyped.fromEntries(
            ObjectTyped.entries(metadata).map(x => {
                return [x[0], paramValueToFieldValue(selectParam(x[1]).value, x[1])];
            })
        ));
    }, [metadata]);
    useEffect(() => {
        const newParamsValues = ObjectTyped.entries(metadata).filter(x => checkConditionsAreMet(x[1])).map((x) => {
            return [x[1].parameter, mergeFieldValueToParam(0, storedValues[x[0]], x[1])];
        });
        // console.log(newParamsValues, storedValues);
        parameters[0].set(newParamsValues.filter(x => x[0] == 0).reduce((p, n) => p | n[1], 0));
        parameters[1].set(newParamsValues.filter(x => x[0] == 1).reduce((p, n) => p | n[1], 0));
    }, [storedValues]);
    const validOptions = ObjectTyped.entries(metadata).filter(x => checkConditionsAreMet(x[1]));

    return <Output validOptions={validOptions} setStoredValues={(f, v) => setStoredValues({ ...storedValues, [f]: v * 1 })} storedValues={storedValues} />;
};
export type MountMetadataComponentProps = {
    validOptions: [string, AdrFieldData<string>][];
    setStoredValues: (field: string, newValue: number) => any;
    storedValues: Record<string, number>;
};
