import { AdrEntityEditorData, EditorUISystemService, nameToString, toEntityTyped } from "@klyte45/adr-commons";
import { AddressesInfoOptionsComponent } from "components/AddressesInfoOptionsComponent";
import { bindValue } from "cs2/api";
import { Panel, PanelSection, Portal } from "cs2/ui";
import { Entity } from "cs2/utils";
import { ChangeEvent, Component } from "react";
import { PropsEllipsesTextInput, VanillaComponentResolver } from "./VanillaComponentResolver";

const selectedEntity$ = bindValue<Entity>("k45::ADR", "AdrEditorUISystem::selectedEntity");
type State = {
    entity: Entity,
    entityData: AdrEntityEditorData,
    editor: {
        name: string
    }
}

export class EditorBindings extends Component<{}, State> {

    constructor(props: {}) {
        super(props)
        selectedEntity$.subscribe((entity) => {
            if (entity.index != this.state.entity?.index) {
                this.updateEntityData(entity);
            }
            return entity;
        })
        this.updateEntityData(selectedEntity$.value)
    }

    private updateEntityData(entity: Entity) {
        EditorUISystemService.getEntityData(toEntityTyped(entity)).then(data => this.setState({
            entity, entityData: data, editor: {
                name: nameToString(data.name) ?? ""
            }
        }));
    }

    private async setCustomName(x: ChangeEvent<PropsEllipsesTextInput>) {
        await EditorUISystemService.setEntityCustomName(toEntityTyped(this.state.entity), x.target.value);
        return this.updateEntityData(this.state.entity);
    }

    render() {
        if (!this.state?.entity?.index) return <></>;
        const VR = VanillaComponentResolver.instance;

        const name = this.state.editor.name;

        const header = <>
            <VR.EllipsisTextInput value={name} maxLength={64}
                onChange={x => this.setState({ editor: { name: x.target.value ?? "" } })}
                onAbort={x => this.setState({ editor: { name: nameToString(this.state.entityData.name) ?? "" } })}
                onBlur={x => this.setCustomName(x)}
            />
        </>

        return <Portal>
            <Panel header={header} style={{ width: "400rem", position: "absolute", left: "5rem", top: "5rem" }} initialPosition={{ x: 0, y: .0 }}>
                <PanelSection>
                    <AddressesInfoOptionsComponent entity={selectedEntity$} isEditor={true} entityRef={this.state.entityData} onChange={() => this.updateEntityData(selectedEntity$.value)} />
                </PanelSection>
            </Panel>
        </Portal>;
    }
}