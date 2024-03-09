import { AdrEntityType, SelectInfoPanelService, SelectedInfoOptions, toEntityTyped } from "@klyte45/adr-commons";
import { Component, ReactNode } from "react";
import { StationBuildingOptionsComponent } from "./StationBuildingOptions";
import { InfoSection, InfoRow, MenuButton } from "cs2/ui";
import { translate } from "utility/translate";
import { SeedManagementOptionsComponent } from "./SeedManagementOptions";
import { Entity } from "cs2/utils";
import { ValueBinding } from "cs2/api";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver";

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
            const call = SelectInfoPanelService.getEntityOptions(toEntityTyped(entity));
            const result = await call
            this.setState({ optionsResult: result })
        }
    }


    render(): ReactNode {
        if (!this.state.optionsResult) return <></>;
        const valueType = AdrEntityType[this.state.optionsResult?.type?.value__];
        const VR = VanillaComponentResolver.instance;
        return <VR.InfoSection disableFocus={true}  >
            <VR.InfoRow uppercase={true} icon="coui://adr.k45/UI/images/ADR.svg" left={<>{translate("AddressesInfoOptions.NamingReference")}</>} right={<>{translate("AdrEntityType." + valueType, valueType)}</>} tooltip={<>{translate("AddressesInfoOptions.Tooltip." + valueType)}</>} />
            {this.getSubRows()}
        </VR.InfoSection>
    }

    private getSubRows() {
        switch (this.state.optionsResult?.type?.value__) {
            case AdrEntityType.PublicTransportStation:
            case AdrEntityType.CargoTransportStation:
                return <StationBuildingOptionsComponent onChanged={() => this.loadOptions(this.props.entity.value, true)} entity={toEntityTyped(this.props.entity.value)} response={this.state.optionsResult} />
            case AdrEntityType.RoadAggregation:
            case AdrEntityType.District:
                return <SeedManagementOptionsComponent onChanged={() => this.loadOptions(this.props.entity.value, true)} entity={toEntityTyped(this.props.entity.value)} response={this.state.optionsResult} />
            default:
                return <></>
        }
    }
}
