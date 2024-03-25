import { AdrEntityType, SelectInfoPanelService, SelectedInfoOptions, toEntityTyped } from "@klyte45/adr-commons";
import { ValueBinding } from "cs2/api";
import { Entity } from "cs2/utils";
import { VanillaComponentResolver } from "mods/VanillaComponentResolver";
import { Component, ReactNode } from "react";
import { translate } from "utility/translate";
import { SeedManagementOptionsComponent } from "./SeedManagementOptions";
import { StationBuildingOptionsComponent } from "./StationBuildingOptions";
import { EventBinding } from "cs2/bindings";

type Props = { entity: ValueBinding<Entity>, entityRef?: any, isEditor?: boolean, onChange?: () => any };
type State = { optionsResult: SelectedInfoOptions }
let lastEntityRef: any = null;
export class AddressesInfoOptionsComponent extends Component<Props, State> {
    constructor(props: Props) {
        super(props)
        this.state = {} as any
        this.props.entity.subscribe(x => this.loadOptions(x))
        lastEntityRef = this.props.entityRef;
        this.loadOptions(props.entity.value)
    }

    private async loadOptions(entity: Entity) {
        const call = SelectInfoPanelService.getEntityOptions(toEntityTyped(entity));
        const result = await call
        this.setState({ optionsResult: result })
    }

    private async onValueChanged() {
        await this.props.onChange?.();
        await this.loadOptions(this.props.entity.value)
    }

    render(): ReactNode {
        if (this.props.entityRef != lastEntityRef) this.loadOptions(this.props.entity.value)
        if (!this.state.optionsResult) return <></>;
        const valueType = AdrEntityType[this.state.optionsResult?.type?.value__];
        const VR = VanillaComponentResolver.instance;
        return <VR.InfoSection disableFocus={true}  >
            <VR.InfoRow uppercase={true} icon="coui://adr.k45/UI/images/ADR.svg" left={<>{translate("AddressesInfoOptions.NamingReference")}</>} right={<div style={{ whiteSpace: "pre-wrap" }}>{translate("AdrEntityType." + valueType, valueType)}</div>} tooltip={<>{translate("AddressesInfoOptions.Tooltip." + valueType)}</>} />
            {this.getSubRows()}
        </VR.InfoSection>
    }

    private getSubRows() {
        switch (this.state.optionsResult?.type?.value__) {
            case AdrEntityType.PublicTransportStation:
            case AdrEntityType.CargoTransportStation:
                return <StationBuildingOptionsComponent onChanged={() => this.onValueChanged()} entity={toEntityTyped(this.props.entity.value)} response={this.state.optionsResult} />
            case AdrEntityType.RoadAggregation:
            case AdrEntityType.District:
                return <SeedManagementOptionsComponent onChanged={() => this.onValueChanged()} entity={toEntityTyped(this.props.entity.value)} response={this.state.optionsResult} />
            default:
                return <></>
        }
    }
}
