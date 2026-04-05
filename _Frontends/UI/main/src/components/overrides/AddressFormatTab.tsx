import { translate } from "#utility/translate";
import { AdrCitywideSettings, NamingRulesService } from "@klyte45/adr-commons";
import { Cs2FormLine } from "@klyte45/euis-components";
import { useRef, useState } from "react";

type Props = {
    currentSettings: AdrCitywideSettings
}

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

export const AddressFormatTab = ({ currentSettings }: Props): JSX.Element => {
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
        <Cs2FormLine title={translate("overrideSettings.addressFormat.patternLabel")}>
            <input
                ref={inputRef}
                type="text"
                value={currentPattern}
                onChange={(e) => handleChange(e.target.value)}
                placeholder={translate("overrideSettings.addressFormat.patternPlaceholder")}
                style={{ width: "100%", background: "rgba(0,0,0,0.3)", color: "white", padding: "4rem 8rem", border: "1rem solid rgba(255,255,255,0.2)", borderRadius: "2rem" }}
            />
        </Cs2FormLine>

        <Cs2FormLine title={translate("overrideSettings.addressFormat.tokensLabel")}>
            <div style={{ display: "flex", flexWrap: "wrap", gap: "4rem" }}>
                {TOKENS.map(token => (
                    <div
                        key={token}
                        onClick={() => insertToken(token)}
                        style={{
                            cursor: "pointer",
                            padding: "2rem 8rem",
                            background: "rgba(255,255,255,0.15)",
                            borderRadius: "4rem",
                            fontFamily: "monospace",
                            fontSize: "12rem",
                            userSelect: "none"
                        }}
                    >
                        {token}
                    </div>
                ))}
            </div>
        </Cs2FormLine>

        <Cs2FormLine title={translate("overrideSettings.addressFormat.previewLabel")}>
            {
                [...new Array(4)].map((_, i) => {
                    const withBrand = i % 2 === 0;
                    const withDistrict = i < 2;
                    const preview = applyPreview(currentPattern, withBrand, withDistrict);
                    return <div key={i} style={{ padding: "4rem 8rem", background: "rgba(0,0,0,0.2)", borderRadius: "2rem", opacity: 0.85 }}>
                        {preview || translate("overrideSettings.addressFormat.previewEmpty")}
                    </div>;
                })
            }
        </Cs2FormLine>

        <Cs2FormLine title="">
            <button className="negativeBtn" onClick={handleReset}>
                {translate("overrideSettings.addressFormat.resetToDefault")}
            </button>
        </Cs2FormLine>
    </>;
};
