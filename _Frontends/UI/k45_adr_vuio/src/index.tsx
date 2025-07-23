import { selectedInfo, ValueBinding } from "cs2/bindings";
import { ModRegistrar } from "cs2/modding";
import { AddressesLayoutRegistering } from "mods/AddressesLayoutRegistering";
import { EditorBindings } from "mods/EditorBindings";
import { AddressesToolOptions } from "tool/AddressesToolOptions";



const register: ModRegistrar = (moduleRegistry) => {
    moduleRegistry.extend("game-ui/game/components/selected-info-panel/selected-info-sections/selected-info-sections.tsx", 'selectedInfoSectionComponents', AddressesLayoutRegistering(() => { }))
    moduleRegistry.append('Editor', EditorBindings)
    moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", 'MouseToolOptions', AddressesToolOptions);
}

export default register;


