import { WhatsNewHost } from "@klyte45/vuio-commons";
import { ModRegistrar } from "cs2/modding";
import { AdrEditorToolbar } from "mods/AdrEditorToolbar";
import { AddressesLayoutRegistering } from "mods/AddressesLayoutRegistering";
import { EditorBindings } from "mods/EditorBindings";
import { AdrSettingsPanel, AdrSettingsPanelId } from "settings/AdrSettingsPanel";
import { AdrUniversalModButton } from "settings/AdrUniversalModButton";
import { AddressesToolOptions } from "tool/AddressesToolOptions";
import "./style/whatsNew.scss";

const extendGamePanelType = (gamePanelType: Record<string, string>) => {
    gamePanelType["K45_ADR"] = AdrSettingsPanelId;
    return gamePanelType;
};

const extendGamePanelComponents = (components: Record<string, unknown>) => {
    components[AdrSettingsPanelId] = AdrSettingsPanel;
    return components;
};

const AdrWhatsNewShell = (Component: any): any => {
    return (props: any) => (
        <>
            <Component {...props} />
            <WhatsNewHost acronym="adr" />
        </>
    );
};

const register: ModRegistrar = (moduleRegistry) => {
    moduleRegistry.extend(
        "game-ui/game/components/selected-info-panel/selected-info-sections/selected-info-sections.tsx",
        "selectedInfoSectionComponents",
        AddressesLayoutRegistering(() => { })
    );
    moduleRegistry.extend("game-ui/editor/components/toolbar/toolbar.tsx", "Toolbar", AdrEditorToolbar as any);
    moduleRegistry.extend("game-ui/editor/components/editor-ui.tsx", "EditorUI", AdrWhatsNewShell);
    moduleRegistry.extend("game-ui/menu/components/menu-ui.tsx", "MenuUI", AdrWhatsNewShell);
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
    moduleRegistry.extend(
        "game-ui/game/components/game-main-screen.tsx",
        "GameMainScreen",
        AdrWhatsNewShell,
    );
    moduleRegistry.append("UniversalModMenu", AdrUniversalModButton);
};

export default register;
