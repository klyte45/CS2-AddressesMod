import { AdrEntityType, SelectInfoPanelService, SelectedInfoOptions, toEntityTyped } from "@klyte45/adr-commons";
import { ValueBinding } from "common/data-binding/binding";
import { Entity } from "common/utils/equality";
import { Component, ReactNode } from "react";
import { StationBuildingOptionsComponent } from "./StationBuildingOptions";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver";

type Props = { entity: ValueBinding<Entity> };
type State = { optionsResult: SelectedInfoOptions }
let lastEntity: any = null;
export class AddressesInfoOptionsComponent extends Component<Props, State> {
    constructor(props: Props) {
        super(props)
        this.state = {} as any
        this.loadOptions(props.entity.value)
    }

    private async loadOptions(entity: Entity) {
        if (lastEntity != entity) {
            lastEntity = entity;
            const result = await SelectInfoPanelService.getEntityOptions(toEntityTyped(entity)); 
            console.log(result)
            this.setState({ optionsResult: result })
        }
    }


    render(): ReactNode {
        const VanillaResolver = VanillaComponentResolver.instance;
        return VanillaResolver && this.state.optionsResult &&
            <VanillaResolver.InfoSection disableFocus={true}><></>
                <VanillaResolver.InfoRow uppercase={true} icon="coui://adr.k45/UI/images/ADR.svg" left={<>Naming Reference</>} right={<>{AdrEntityType[this.state.optionsResult.type?.value__]}</>} tooltip={<></>} />
                {this.getSubRows()}
            </VanillaResolver.InfoSection>
    }

    private getSubRows() {
        switch (this.state.optionsResult.type?.value__) {
            case AdrEntityType.PublicTransportStation:
                return <StationBuildingOptionsComponent entity={toEntityTyped(this.props.entity.value)} response={this.state.optionsResult} />
            default:
                return <></>
        }
    }
}
