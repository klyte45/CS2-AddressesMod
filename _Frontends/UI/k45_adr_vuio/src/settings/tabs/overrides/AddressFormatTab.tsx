import { AdrCitywideSettings, NamingRulesService } from "@klyte45/adr-commons";
import { FormLine } from "@klyte45/vuio-commons";
import { useRef, useState } from "react";
import { translate } from "../../translate";
import styles from "../../styles/addressFormatTab.module.scss";

type Props = {
    currentSettings: AdrCitywideSettings;
};

const MOCK_NUMBER = "42";
const MOCK_STREET = "Oak Avenue";
const MOCK_DISTRICT = "Downtown";
const MOCK_BRAND = "SuperMart";
const TOKENS = ["{number}", "{street}", "{district}", "{brand}"] as const;

function applyPreview(pattern: string, withBrand: boolean, withDistrict: boolean): string {
    if (!pattern) return "";
    if (!withBrand) {
        pattern = pattern.replace(/([^|}]|^)*\{brand\}([^{|]|$)*/g, "");
    }
    if (!withDistrict) {
        pattern = pattern.replace(/([^|}]|^)*\{district\}([^{|]|$)*/g, "");
    }
    pattern = pattern.replace(/[|]/g, "").replace(/ +/g, " ");
    let result = pattern
        .replace("{number}", MOCK_NUMBER)
        .replace("{street}", MOCK_STREET)
        .replace("{district}", MOCK_DISTRICT)
        .replace("{brand}", MOCK_BRAND);
    while (result.includes("  ")) result = result.replace("  ", " ");
    return result.trim();
}

export const AddressFormatTab = ({ currentSettings }: Props) => {
    const inputRef = useRef<HTMLInputElement>(null);
    const [localValue, setLocalValue] = useState<string | undefined>(undefined);

    const currentPattern = localValue !== undefined ? localValue : (currentSettings?.AddressFormatPattern ?? "");

    const handleChange = (val: string) => {
        setLocalValue(val);
        NamingRulesService.setAddressFormatPattern(val);
    };

    const insertToken = (token: string) => {
        const el = inputRef.current;
        if (!el) {
            handleChange(currentPattern + token);
            return;
        }
        const start = el.selectionStart ?? currentPattern.length;
        const end = el.selectionEnd ?? currentPattern.length;
        const newVal = currentPattern.slice(0, start) + token + currentPattern.slice(end);
        handleChange(newVal);
        requestAnimationFrame(() => {
            el.focus();
            el.setSelectionRange(start + token.length, start + token.length);
        });
    };

    const handleReset = () => {
        setLocalValue("");
        NamingRulesService.setAddressFormatPattern("");
    };

    return <>
        <FormLine title={translate("overrideSettings.addressFormat.patternLabel")}>
            <input
                ref={inputRef}
                type="text"
                className={styles.patternInput}
                value={currentPattern}
                onChange={(e) => handleChange(e.target.value)}
                placeholder={translate("overrideSettings.addressFormat.patternPlaceholder")}
            />
        </FormLine>
        <FormLine title={translate("overrideSettings.addressFormat.tokensLabel")}>
            <div className={styles.tokenRow}>
                {TOKENS.map((token) => (
                    <div key={token} className={styles.tokenChip} onClick={() => insertToken(token)}>
                        {token}
                    </div>
                ))}
            </div>
        </FormLine>
        <FormLine title={translate("overrideSettings.addressFormat.previewLabel")}>
            <div>
                {[...new Array(4)].map((_, i) => {
                    const withBrand = i % 2 === 0;
                    const withDistrict = i < 2;
                    const preview = applyPreview(currentPattern, withBrand, withDistrict);
                    return (
                        <div key={i} className={styles.previewRow}>
                            {preview || translate("overrideSettings.addressFormat.previewEmpty")}
                        </div>
                    );
                })}
            </div>
        </FormLine>
        <FormLine title="">
            <button type="button" className="negativeBtn" onClick={handleReset}>
                {translate("overrideSettings.addressFormat.resetToDefault")}
            </button>
        </FormLine>
    </>;
};
