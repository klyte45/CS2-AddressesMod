import { AdrCitywideSettings, NamingRulesService, SimpleNameEntry } from "adr";
import { FormLine, VanillaWidgets } from "@klyte45/vuio-commons";
import { translate } from "../../translate";
import { defaultSetting } from "./overrideDefaults";
import { NamesetSelectLine } from "./NamesetSelectLine";

type Props = {
    simpleFiles: SimpleNameEntry[];
    indexedSimpleFiles: Record<string, SimpleNameEntry>;
    currentSettings: AdrCitywideSettings;
};

export const OverrideCitizenTab = ({ simpleFiles, indexedSimpleFiles, currentSettings }: Props) => {
    const VW = VanillaWidgets.instance;

    return <>
        <NamesetSelectLine
            title={translate("overrideSettings.maleNamesFile")}
            options={simpleFiles}
            value={indexedSimpleFiles[currentSettings.CitizenMaleNameOverridesStr] ?? defaultSetting()}
            onChange={(x) => NamingRulesService.setCitizenMaleNameOverridesStr(x.IdString)}
        />
        <NamesetSelectLine
            title={translate("overrideSettings.femaleNamesFile")}
            options={simpleFiles}
            value={indexedSimpleFiles[currentSettings.CitizenFemaleNameOverridesStr] ?? defaultSetting()}
            onChange={(x) => NamingRulesService.setCitizenFemaleNameOverridesStr(x.IdString)}
        />
        <NamesetSelectLine
            title={translate("overrideSettings.surnamesFile")}
            options={simpleFiles}
            value={indexedSimpleFiles[currentSettings.CitizenSurnameOverridesStr] ?? defaultSetting()}
            onChange={(x) => NamingRulesService.setCitizenSurnameOverridesStr(x.IdString)}
        />
        <FormLine title={translate("overrideSettings.firstNameAtStart")}>
            <VW.Checkbox
                checked={!!currentSettings?.surnameAtFirst}
                onChange={(x) => NamingRulesService.setSurnameAtFirst(x)}
            />
        </FormLine>
        <FormLine title={translate("overrideSettings.maximumGeneratedGivenNames")}>
            <VW.IntSlider
                min={1}
                max={5}
                value={currentSettings.MaximumGeneratedGivenNames ?? 1}
                onChange={(x) => NamingRulesService.setMaxGivenNames(x)}
            />
        </FormLine>
        <FormLine title={translate("overrideSettings.maximumGeneratedSurnames")}>
            <VW.IntSlider
                min={1}
                max={5}
                value={currentSettings.MaximumGeneratedSurnames ?? 1}
                onChange={(x) => NamingRulesService.setMaxSurnames(x)}
            />
        </FormLine>
        <NamesetSelectLine
            title={translate("overrideSettings.dogsFile")}
            options={simpleFiles}
            value={indexedSimpleFiles[currentSettings.CitizenDogOverridesStr] ?? defaultSetting()}
            onChange={(x) => NamingRulesService.setCitizenDogOverridesStr(x.IdString)}
        />
    </>;
};
