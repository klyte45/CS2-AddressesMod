import { selectedInfo, ValueBinding } from "cs2/bindings";
import { ModRegistrar } from "cs2/modding";
import { EditorBindings } from "mods/editorInfoPanel";
import { AddressesLayoutRegistering } from "mods/selectedInfoPanel";



const register: ModRegistrar = (moduleRegistry) => {
    const selectedEntity$ = selectedInfo.selectedEntity$;
    const middleSections$ = selectedInfo.middleSections$;

    let currentEntity = null as any;
    let lastMiddleSection = null as any;
    selectedEntity$.subscribe((entity) => {
        if (!entity.index) {
            currentEntity = (null);
            return entity
        }
        if (currentEntity != entity.index) {
            currentEntity = (entity.index)
        }
        return entity;
    })
    const onChangeSelection = (val: (typeof middleSections$ extends ValueBinding<infer X> ? X : never)) => {
        if (currentEntity && val.every(x => x?.__Type != "K45.Addresses" as any)) {
            val.push({
                __Type: "K45.Addresses"
            } as any);
        }
        lastMiddleSection = val;
        return lastMiddleSection;
    };
    middleSections$.subscribe(onChangeSelection)



    // moduleRegistry.append('Game', AddressesBindings)
    moduleRegistry.extend("game-ui/game/components/selected-info-panel/selected-info-sections/selected-info-sections.tsx", 'selectedInfoSectionComponents', AddressesLayoutRegistering(() => lastMiddleSection, () => { }))
    moduleRegistry.append('Editor', EditorBindings)
    //moduleRegistry.find([].concat(...moduleRegistry.find(/.*\/selected-info.*\.tsx/) as any[]).sort((a: any, b) => a.lo caleCompare(b))
    //console.log( moduleRegistry.registry.get("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx"))
}

export default register;


