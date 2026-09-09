import { AdrEntityEditorData, EditorUISystemService, toEntityTyped, nameToString } from "adr";
import { VanillaComponentResolver } from "@klyte45/vuio-commons";
import { AddressesInfoOptionsComponent } from "components/AddressesInfoOptionsComponent";
import { bindValue } from "cs2/api";
import { Panel, PanelSection, Portal } from "cs2/ui";
import { Entity } from "cs2/utils";
import { useEffect, useState } from "react";

export const EditorBindings = () => {
    const [entity, setEntity] = useState<Entity>();
    const [entityData, setEntityData] = useState<AdrEntityEditorData>();
    const [name, setName] = useState<string>();
    const [selectedEntity$] = useState(bindValue<Entity>("k45::ADR", "AdrEditorUISystem::selectedEntity"));

    selectedEntity$.subscribe((x) => {
        if (x?.index != entity?.index) {
            updateEntityData(x);
        }
    });

    const updateEntityData = (selected?: Entity) => {
        if (!selected) {
            setEntity(selected);
            return;
        }

        EditorUISystemService.getEntityData(toEntityTyped(selected)).then((data) => {
            setEntity(selected);
            setEntityData(data);
            setName(nameToString(data.name) ?? "");
        });
    };

    useEffect(() => {
        updateEntityData(selectedEntity$.value);
    }, []);

    const setCustomName = async (x: { target: { value: string } }) => {
        if (entity) {
            await EditorUISystemService.setEntityCustomName(toEntityTyped(entity), x.target.value);
        }
        return updateEntityData(entity);
    };

    const header = (
        <>
            <VanillaComponentResolver.instance.EllipsisTextInput
                value={name}
                maxLength={64}
                onChange={(x) => setName(x.target.value ?? "")}
                onAbort={() => entityData && setName(nameToString(entityData.name) ?? "")}
                onBlur={(x) => setCustomName(x)}
            />
        </>
    );

    return (
        <Portal>
            {entity?.index && (
                <Panel
                    header={header}
                    style={{ width: "400rem", position: "absolute", left: "5rem", top: "5rem" }}
                    initialPosition={{ x: 0, y: 0 }}
                >
                    <PanelSection>
                        <AddressesInfoOptionsComponent isEditor={true} onChange={() => updateEntityData(selectedEntity$.value)} />
                    </PanelSection>
                </Panel>
            )}
        </Portal>
    );
};
