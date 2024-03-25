import { AddressesInfoOptionsComponent } from "components/AddressesInfoOptionsComponent";
import { selectedInfo } from "cs2/bindings";

let currentEntity: any = null;
const selectedEntity$ = selectedInfo.selectedEntity$;
const middleSections$ = selectedInfo.middleSections$;
let lastMiddleSection: any = null;
export const AddressesBindings = () => {
    selectedEntity$.subscribe((entity) => {
        if (!entity.index) {
            currentEntity = null;
            return entity
        }
        if (currentEntity != entity.index) {
            currentEntity = entity.index
        }
        return entity;
    })
    middleSections$.subscribe((val) => {
        if (currentEntity && val.every(x => x?.__Type != "K45.Addresses" as any)) {
            val.push({
                __Type: "K45.Addresses"
            } as any);
        }
        return lastMiddleSection = val;
    })
    return <></>;
}

export const AddressesLayoutRegistering = (componentList: any): any => {
    componentList["K45.Addresses"] = () => <><AddressesInfoOptionsComponent entity={selectedEntity$} entityRef={lastMiddleSection} isEditor={true} /></>
    return componentList as any;
} 