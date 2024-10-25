import { ModRegistrar } from "cs2/modding";
import { EditorBindings } from "mods/editorInfoPanel";
import { AddressesBindings, AddressesLayoutRegistering } from "mods/selectedInfoPanel";



const register: ModRegistrar = (moduleRegistry) => {    
    moduleRegistry.append('Game', AddressesBindings)
    moduleRegistry.extend("game-ui/game/components/selected-info-panel/selected-info-sections/selected-info-sections.tsx", 'selectedInfoSectionComponents', AddressesLayoutRegistering)
    moduleRegistry.append('Editor', EditorBindings)
    //moduleRegistry.find([].concat(...moduleRegistry.find(/.*\/selected-info.*\.tsx/) as any[]).sort((a: any, b) => a.lo caleCompare(b))
    //console.log( moduleRegistry.registry.get("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx"))
}

export default register;


