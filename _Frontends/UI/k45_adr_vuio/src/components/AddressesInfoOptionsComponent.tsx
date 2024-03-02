import { AdrEntityType, SelectInfoPanelService, SelectedInfoOptions, toEntityTyped } from "@klyte45/adr-commons";
import { ValueBinding } from "common/data-binding/binding";
import { Entity } from "common/utils/equality";
import { Component, ReactNode } from "react";
import { StationBuildingOptionsComponent } from "./StationBuildingOptions";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver";
import { translate } from "utility/translate";
import { SeedManagementOptionsComponent } from "./SeedManagementOptions";

type Props = { entity: ValueBinding<Entity> };
type State = { optionsResult: SelectedInfoOptions }
let lastEntity: any = null;
export class AddressesInfoOptionsComponent extends Component<Props, State> {
    constructor(props: Props) {
        super(props)
        this.state = {} as any
        this.loadOptions(props.entity.value, false)
    }

    private async loadOptions(entity: Entity, force: boolean) {
        if (force || lastEntity != entity) {
            lastEntity = entity;
            const result = await SelectInfoPanelService.getEntityOptions(toEntityTyped(entity));            
            this.setState({ optionsResult: result })
        }
    }


    render(): ReactNode {
        if (!this.state.optionsResult) return <></>;
        const VanillaResolver = VanillaComponentResolver.instance;
        const valueType = AdrEntityType[this.state.optionsResult?.type?.value__];
        return VanillaResolver &&
            <VanillaResolver.InfoSection disableFocus={true}><></>
                <VanillaResolver.InfoRow uppercase={true} icon="coui://adr.k45/UI/images/ADR.svg" left={<>{translate("AddressesInfoOptions.NamingReference")}</>} right={<>{translate("AdrEntityType." + valueType, valueType)}</>} tooltip={<>{translate("AddressesInfoOptions.Tooltip." + valueType)}</>} />
                {this.getSubRows()}
            </VanillaResolver.InfoSection>
    }

    private getSubRows() {
        switch (this.state.optionsResult.type?.value__) {
            case AdrEntityType.PublicTransportStation:
            case AdrEntityType.CargoTransportStation:
                return <StationBuildingOptionsComponent onChanged={() => this.loadOptions(this.props.entity.value, true)} entity={toEntityTyped(this.props.entity.value)} response={this.state.optionsResult} />
            case AdrEntityType.RoadAggregation:
                return <SeedManagementOptionsComponent onChanged={() => this.loadOptions(this.props.entity.value, true)} entity={toEntityTyped(this.props.entity.value)} response={this.state.optionsResult} />
            default:
                return <></>
        }
    }
}
