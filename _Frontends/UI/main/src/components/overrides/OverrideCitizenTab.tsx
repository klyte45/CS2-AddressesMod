import { translate } from "#utility/translate";
import { AdrCitywideSettings, NamingRulesService, SimpleNameEntry } from "@klyte45/adr-commons";
import { Cs2CheckboxWithLine, Cs2FormLine, Cs2Select, Input } from "@klyte45/euis-components";
import { defaultSetting } from "./OverrideSettingsCmp";

type Props = {
  simpleFiles: SimpleNameEntry[], indexedSimpleFiles: Record<string, SimpleNameEntry>, currentSettings: AdrCitywideSettings
}

export const OverrideCitizenTab = ({ simpleFiles, indexedSimpleFiles, currentSettings }: Props): JSX.Element => {
  return <>
    <Cs2FormLine title={translate("overrideSettings.maleNamesFile")}>
      <Cs2Select
        options={Object.values(simpleFiles)}
        getOptionLabel={(x: SimpleNameEntry) => x?.Name}
        getOptionValue={(x: SimpleNameEntry) => x?.IdString}
        onChange={(x) => NamingRulesService.setCitizenMaleNameOverridesStr(x.IdString)}
        value={indexedSimpleFiles[currentSettings.CitizenMaleNameOverridesStr]}
        defaultValue={defaultSetting()} />
    </Cs2FormLine>
    <Cs2FormLine title={translate("overrideSettings.femaleNamesFile")}>
      <Cs2Select
        options={Object.values(simpleFiles)}
        getOptionLabel={(x: SimpleNameEntry) => x?.Name}
        getOptionValue={(x: SimpleNameEntry) => x?.IdString}
        onChange={(x) => NamingRulesService.setCitizenFemaleNameOverridesStr(x.IdString)}
        value={indexedSimpleFiles[currentSettings.CitizenFemaleNameOverridesStr]}
        defaultValue={defaultSetting()} />
    </Cs2FormLine>
    <Cs2FormLine title={translate("overrideSettings.surnamesFile")}>
      <Cs2Select
        options={Object.values(simpleFiles)}
        getOptionLabel={(x: SimpleNameEntry) => x?.Name}
        getOptionValue={(x: SimpleNameEntry) => x?.IdString}
        onChange={(x) => NamingRulesService.setCitizenSurnameOverridesStr(x.IdString)}
        value={indexedSimpleFiles[currentSettings.CitizenSurnameOverridesStr]}
        defaultValue={defaultSetting()} />
    </Cs2FormLine>

    <Cs2CheckboxWithLine title={translate("overrideSettings.firstNameAtStart")} isChecked={() => currentSettings?.surnameAtFirst} onValueToggle={(x) => NamingRulesService.setSurnameAtFirst(x)} />

    <Input title={translate("overrideSettings.maximumGeneratedGivenNames")} getValue={() => currentSettings.MaximumGeneratedGivenNames?.toFixed(0)} maxLength={1} isValid={(x) => parseInt(x) >= 1 && parseInt(x) <= 5} onValueChanged={async (x) => NamingRulesService.setMaxGivenNames(parseInt(x))} />
    <Input title={translate("overrideSettings.maximumGeneratedSurnames")} getValue={() => currentSettings.MaximumGeneratedSurnames?.toFixed(0)} maxLength={1} isValid={(x) => parseInt(x) >= 1 && parseInt(x) <= 5} onValueChanged={async (x) => NamingRulesService.setMaxSurnames(parseInt(x))} />
    <Cs2FormLine title={translate("overrideSettings.dogsFile")}>
      <Cs2Select
        options={Object.values(simpleFiles)}
        getOptionLabel={(x: SimpleNameEntry) => x?.Name}
        getOptionValue={(x: SimpleNameEntry) => x?.IdString}
        onChange={(x) => NamingRulesService.setCitizenDogOverridesStr(x.IdString)}
        value={indexedSimpleFiles[currentSettings.CitizenDogOverridesStr]}
        defaultValue={defaultSetting()} />
    </Cs2FormLine>
  </>;
}
