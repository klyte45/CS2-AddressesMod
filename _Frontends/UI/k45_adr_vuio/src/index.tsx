import { ModRegistrar } from "cs2/modding";
import { AddressesLayoutRegistering } from "mods/AddressesLayoutRegistering";
import { EditorBindings } from "mods/EditorBindings";
import { AdrSettingsPanel, AdrSettingsPanelId } from "settings/AdrSettingsPanel";
import { AdrUniversalModButton } from "settings/AdrUniversalModButton";
import { AddressesToolOptions } from "tool/AddressesToolOptions";

const extendGamePanelType = (gamePanelType: Record<string, string>) => {
    gamePanelType["K45_ADR"] = AdrSettingsPanelId;
    return gamePanelType;
};

const extendGamePanelComponents = (components: Record<string, unknown>) => {
    components[AdrSettingsPanelId] = AdrSettingsPanel;
    return components;
};

const register: ModRegistrar = (moduleRegistry) => {
    moduleRegistry.extend(
        "game-ui/game/components/selected-info-panel/selected-info-sections/selected-info-sections.tsx",
        "selectedInfoSectionComponents",
        AddressesLayoutRegistering(() => { })
    );
    moduleRegistry.append("Editor", EditorBindings);
    moduleRegistry.extend(
        "game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx",
        "MouseToolOptions",
        AddressesToolOptions
    );

    moduleRegistry.extend(
        "game-ui/game/data-binding/game-bindings.ts",
        "GamePanelType",
        extendGamePanelType as any
    );
    moduleRegistry.extend(
        "game-ui/game/components/game-panel-renderer.tsx",
        "gamePanelComponents",
        extendGamePanelComponents as any
    );
    moduleRegistry.append("UniversalModMenu", AdrUniversalModButton);
};

export default register;
