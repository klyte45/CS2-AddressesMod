import { AddressesInfoOptionsComponent } from "components/AddressesInfoOptionsComponent";
import { useModding } from "modding/modding-context";

let currentEntity: any = null;
export const AddressesBindings = () => {
    const bindings = useModding().api.bindings;
    bindings.selectedInfo.selectedEntity$.subscribe((entity) => {
        if (!entity.index) {
            currentEntity = null;
            return entity
        }
        if (currentEntity != entity.index) {
            currentEntity = entity.index
        }
        return entity;
    })
    bindings.selectedInfo.middleSections$.subscribe((val) => {
        if (currentEntity && val.every(x => x?.__Type != "K45.Addresses" as any)) {
            val.push({
                __Type: "K45.Addresses"
            } as any);
        }
        return val;
    })

    return <></>;
}

export const AddressesLayoutRegistering = (componentList: any): any => {
    componentList["K45.Addresses"] = () =><><AddressesInfoOptionsComponent entity={useModding().api.bindings.selectedInfo.selectedEntity$} /></>
    return componentList as any;
} 