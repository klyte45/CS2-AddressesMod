import { AddressesInfoOptionsComponent } from "components/AddressesInfoOptionsComponent";
import { selectedInfo } from "cs2/bindings";

export const AddressesLayoutRegistering = (lastMiddleSection: () => any, onChange?: () => any) => (componentList: any): any => {
    componentList["K45.Addresses"] = () => <><AddressesInfoOptionsComponent entity={selectedInfo.selectedEntity$} entityRef={lastMiddleSection()} onChange={onChange} isEditor={true} /></>
    return componentList as any;
} 