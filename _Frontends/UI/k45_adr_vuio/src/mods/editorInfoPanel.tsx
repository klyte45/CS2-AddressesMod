import { AdrEntityEditorData, EditorUISystemService, nameToString, toEntityTyped } from "@klyte45/adr-commons";
import { AddressesInfoOptionsComponent } from "components/AddressesInfoOptionsComponent";
import { bindValue } from "cs2/api";
import { Panel, PanelSection, Portal } from "cs2/ui";
import { Entity } from "cs2/utils";
import { ChangeEvent, Component, useEffect, useState } from "react";
import { PropsEllipsesTextInput, VanillaComponentResolver } from '@klyte45/vuio-commons'



export const EditorBindings = () => {

    const [entity, setEntity] = useState(undefined as any as Entity)
    const [entityData, setEntityData] = useState(undefined as any as AdrEntityEditorData)
    const [name, setName] = useState(undefined as any as string)
    const [selectedEntity$] = useState(bindValue<Entity>("k45::ADR", "AdrEditorUISystem::selectedEntity"));

    selectedEntity$.subscribe(x => {
        if (x?.index != entity?.index) {
            updateEntityData(x);
        }
    })



    const updateEntityData = (entity: Entity) => {
        if (!entity) {
            setEntity(entity);
            return;
        }
        EditorUISystemService.getEntityData(toEntityTyped(entity)).then(data => {
            setEntity(entity), setEntityData(data), setName(nameToString(data.name) ?? "")
        });
    }
    updateEntityData(selectedEntity$.value);


    const setCustomName = async (x: ChangeEvent<PropsEllipsesTextInput>) => {
        await EditorUISystemService.setEntityCustomName(toEntityTyped(entity), x.target.value);
        return updateEntityData(entity);
    }
    if (!entity?.index) return <Portal>
        <Panel style={{ width: "400rem", position: "absolute", left: "5rem", top: "5rem" }} initialPosition={{ x: 0, y: .0 }}>
            <PanelSection>
                OFF
            </PanelSection>
        </Panel>
    </Portal>;;
    const VR = VanillaComponentResolver.instance;

    const header = <>
        <VR.EllipsisTextInput value={name} maxLength={64}
            onChange={x => setName(x.target.value ?? "")}
            onAbort={x => setName(nameToString(entityData.name) ?? "")}
            onBlur={x => setCustomName(x)}
        />
    </>

    return <Portal>
        <Panel header={header} style={{ width: "400rem", position: "absolute", left: "5rem", top: "5rem" }} initialPosition={{ x: 0, y: .0 }}>
            <PanelSection>
                <AddressesInfoOptionsComponent entity={selectedEntity$} isEditor={true} entityRef={entityData} onChange={() => updateEntityData(selectedEntity$.value)} />
            </PanelSection>
        </Panel>
    </Portal>;

}