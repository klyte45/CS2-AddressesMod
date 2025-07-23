import { AddressesInfoOptionsComponent } from "components/AddressesInfoOptionsComponent";
import { selectedInfo } from "cs2/bindings";


export const AddressesLayoutRegistering = (onChange?: () => any) => (componentList: any): any => {
    componentList["BelzontAdr.AdrSelectionInfoPanelSystem"] = () => <><AddressesInfoOptionsComponent entity={selectedInfo.selectedEntity$} onChange={onChange} isEditor={true} /></>;
    return componentList as any;
};
