import { SelectInfoPanelService, toEntityTyped } from "@klyte45/adr-commons";
import { ValueBinding } from "common/data-binding/binding";
import { Entity } from "common/utils/equality";
import { Component, ReactNode } from "react";

type Props = { entity: ValueBinding<Entity> };
type State = { optionsResult: any }
let lastEntity: any = null;
export class AddressesInfoOptionsComponent extends Component<Props, State> {
    constructor(props: Props) {
        super(props)
        this.loadOptions(props.entity.value)
    }

    private async loadOptions(entity: Entity) {

        if (lastEntity != entity) {
            lastEntity = entity;

            const result = await SelectInfoPanelService.getEntityOptions(toEntityTyped(entity));
            console.log(result);
            this.setState({ optionsResult: result })
        }
    }


    render(): ReactNode {
        return <>Addresses Options goes here =V {JSON.stringify(this.state?.optionsResult)}</>
    }
}