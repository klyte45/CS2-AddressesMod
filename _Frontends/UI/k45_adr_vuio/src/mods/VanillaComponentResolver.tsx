import { ActiveFocusDivProps, PassiveFocusDivProps } from "common/focus/focus-div";
import { UniqueFocusKey, useUniqueFocusKey } from "common/focus/focus-key";
import { Theme } from "game/data-binding/prefab/prefab-bindings";
import { ModuleRegistry } from "modding/types";
import { DropdownField } from "widgets/data-binding/widget-bindings";

type PropsToggleField = {
    "value": any,
    "disabled"?: boolean,
    "onChange"?: (x: any) => any
}

type PropsRadioToggle = {
    focusKey?: UniqueFocusKey | null
    checked: boolean
    disabled?: boolean
    theme?: Theme | any
    style?: CSSStyleRule
    className?: string
}

type PropsRadioGroupToggleField = {
    value: any,
    groupValue: any,
    disabled?: boolean,
    onChange?: (x: any) => any,
    onToggleSelected?: (x: any) => any,
}

type PropsInfoSection = {
    focusKey?: UniqueFocusKey | null
    tooltip?: JSX.Element
    disableFocus?: boolean
    className?: string
    children: JSX.Element | JSX.Element[] | string;
}

type PropsInfoRow = {
    icon?: string
    left: JSX.Element
    right?: JSX.Element
    tooltip?: JSX.Element
    link?: string
    uppercase?: boolean
    subRow?: string
    className?: string
    disableFocus?: boolean
}

type PropsTooltipRow = {

}

type PropsDropdown = {
    theme?: any,
    children?: JSX.Element | JSX.Element[] | string,
    disableFocus?: boolean,
    focusKey?: UniqueFocusKey | null
}

type PropsDropdownToggle = {
    theme?: any,
    children?: JSX.Element | JSX.Element[] | string,
    disableFocus?: boolean,
    focusKey?: UniqueFocusKey | null
}
type PropsDropdownItem = {
    focusKey?: UniqueFocusKey | null
    value?: any
    selected: boolean
    theme?: any
    sounds?: any
    className?: string
    onChange?: (x: any) => any
    onToggleSelected?: (x: any) => any
    closeOnSelect?: boolean
    children?: JSX.Element | JSX.Element[] | string,
}


const registryIndex = {
    RadioToggle: ["game-ui/common/input/toggle/radio-toggle/radio-toggle.tsx", "RadioToggle"],
    ToggleField: ["game-ui/menu/components/shared/game-options/toggle-field/toggle-field.tsx", "ToggleField"],
    RadioGroupToggleField: ["game-ui/menu/components/shared/game-options/toggle-field/toggle-field.tsx", "RadioGroupToggleField"],
    InfoSection: ["game-ui/game/components/selected-info-panel/shared-components/info-section/info-section.tsx", "InfoSection"],
    InfoRow: ["game-ui/game/components/selected-info-panel/shared-components/info-row/info-row.tsx", "InfoRow"],
    TooltipRow: ["game-ui/game/components/selected-info-panel/shared-components/info-row/info-row.tsx", "TooltipRow"],
    ActiveFocusDiv: ["game-ui/common/focus/focus-div.tsx", "ActiveFocusDiv"],
    PassiveFocusDiv: ["game-ui/common/focus/focus-div.tsx", "PassiveFocusDiv"],
    themeToggleLine: ["game-ui/game/components/selected-info-panel/selected-info-sections/lines-section/lines-section.module.scss", "classes"],
    FOCUS_DISABLED: ["game-ui/common/focus/focus-key.ts", "FOCUS_DISABLED"],
    FOCUS_AUTO: ["game-ui/common/focus/focus-key.ts", "FOCUS_AUTO"],
    useUniqueFocusKey: ["game-ui/common/focus/focus-key.ts", "useUniqueFocusKey"],
    Dropdown: ["game-ui/common/input/dropdown/dropdown.tsx", "Dropdown"],
    themeDropdown: ["game-ui/menu/widgets/dropdown-field/dropdown-field.module.scss", "classes"],
    DropdownItem: ["game-ui/common/input/dropdown/items/dropdown-item.tsx", "DropdownItem"],
    DropdownToggle: ["game-ui/common/input/dropdown/dropdown-toggle.tsx", "DropdownToggle"],
}



export class VanillaComponentResolver {
    public static get instance(): VanillaComponentResolver { return this._instance!! }
    private static _instance?: VanillaComponentResolver

    public static setRegistry(in_registry: ModuleRegistry) { this._instance = new VanillaComponentResolver(in_registry); }
    private registryData: ModuleRegistry;

    constructor(in_registry: ModuleRegistry) {
        this.registryData = in_registry;
    }

    private cachedData: Partial<Record<keyof typeof registryIndex, any>> = {}
    private updateCache(entry: keyof typeof registryIndex) {
        const entryData = registryIndex[entry];
        return this.cachedData[entry] = this.registryData.registry.get(entryData[0])!![entryData[1]]
    }

    public get RadioToggle(): (props: PropsRadioToggle) => JSX.Element { return this.cachedData["RadioToggle"] ?? this.updateCache("RadioToggle") }
    public get ToggleField(): (props: PropsToggleField) => JSX.Element { return this.cachedData["ToggleField"] ?? this.updateCache("ToggleField") }
    public get RadioGroupToggleField(): (props: PropsRadioGroupToggleField) => JSX.Element { return this.cachedData["RadioGroupToggleField"] ?? this.updateCache("RadioGroupToggleField") }
    public get InfoSection(): (props: PropsInfoSection) => JSX.Element { return this.cachedData["InfoSection"] ?? this.updateCache("InfoSection") }
    public get InfoRow(): (props: PropsInfoRow) => JSX.Element { return this.cachedData["InfoRow"] ?? this.updateCache("InfoRow") }
    public get TooltipRow(): (props: PropsTooltipRow) => JSX.Element { return this.cachedData["TooltipRow"] ?? this.updateCache("TooltipRow") }
    public get ActiveFocusDiv(): (props: ActiveFocusDivProps) => JSX.Element { return this.cachedData["ActiveFocusDiv"] ?? this.updateCache("ActiveFocusDiv") }
    public get PassiveFocusDiv(): (props: PassiveFocusDivProps) => JSX.Element { return this.cachedData["PassiveFocusDiv"] ?? this.updateCache("PassiveFocusDiv") }
    public get Dropdown(): (props: PropsDropdown) => JSX.Element { return this.cachedData["Dropdown"] ?? this.updateCache("Dropdown") }
    public get DropdownItem(): (props: PropsDropdownItem) => JSX.Element { return this.cachedData["DropdownItem"] ?? this.updateCache("DropdownItem") }
    public get DropdownToggle(): (props: PropsDropdownToggle) => JSX.Element { return this.cachedData["DropdownToggle"] ?? this.updateCache("DropdownToggle") }


    public get themeToggleLine(): Theme | any { return this.cachedData["themeToggleLine"] ?? this.updateCache("themeToggleLine") }
    public get themeDropdown(): Theme | any { return this.cachedData["themeDropdown"] ?? this.updateCache("themeDropdown") }


    public get FOCUS_DISABLED(): UniqueFocusKey { return this.cachedData["FOCUS_DISABLED"] ?? this.updateCache("FOCUS_DISABLED") }
    public get FOCUS_AUTO(): UniqueFocusKey { return this.cachedData["FOCUS_AUTO"] ?? this.updateCache("FOCUS_AUTO") }
    public get useUniqueFocusKey(): typeof useUniqueFocusKey { return this.cachedData["useUniqueFocusKey"] ?? this.updateCache("useUniqueFocusKey") }


}