import { NamesetService, NamingRulesService } from "@klyte45/adr-commons";
import { useModding } from "modding/modding-context";
import { ModuleRegistry, ModuleRegistryExtend } from "modding/types";
import { Component, ReactNode } from "react";
import { ValueBinding } from "common/data-binding/binding";
import { Entity } from "common/utils/equality";
import { toEntityTyped } from "@klyte45/adr-commons";
import { AddressesInfoOptionsComponent } from "components/AddressesInfoOptionsComponent";

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
    componentList["K45.Addresses"] = () => <AddressesInfoOptionsComponent entity={useModding().api.bindings.selectedInfo.selectedEntity$} />
    return componentList as any;
} 