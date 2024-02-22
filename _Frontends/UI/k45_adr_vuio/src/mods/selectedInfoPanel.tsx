import { useModding } from "modding/modding-context";
import { ModuleRegistry, ModuleRegistryExtend } from "modding/types";

let currentEntity: any = null;
export const AddressesBindings = () => {
    const bindings = useModding().api.bindings;
    const selectedEntity = bindings.selectedInfo.selectedEntity$.value;
    bindings.selectedInfo.middleSections$.subscribe((section) => {
        if (!selectedEntity.index) {
            currentEntity = null;
            return section
        }
        if (currentEntity != selectedEntity.index) {
            currentEntity = selectedEntity.index
            if ((bindings.selectedInfo.titleSection$.value?.name as any)?.k45_addressesSupportGeneration && section.every(x => x?.__Type != "K45.Addresses" as any)) {
                section.push({
                    __Type: "K45.Addresses"
                } as any);
            }
        }
        return section;
    })
    return <></>;
}

export const AddressesLayoutRegistering = (componentList: any): any => {
    componentList["K45.Addresses"] = () => <>Addresses Options goes here =V</>
    return componentList as any;
}