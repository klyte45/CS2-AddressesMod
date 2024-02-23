import {  NamingRulesService, toEntityTyped } from "@klyte45/adr-commons";
import { ValueBinding } from "common/data-binding/binding";
import { Entity } from "common/utils/equality";
import { Component, ReactNode } from "react";

type Props = { entity: ValueBinding<Entity> };
type State = { optionsResult: any }
export class AddressesInfoOptionsComponent extends Component<Props, State> {
    constructor(props: Props) {
        super(props)
        NamingRulesService.getEntityOptions(toEntityTyped(props.entity.value)).then((x) => this.setState({ optionsResult: x }))
    }

    render(): ReactNode {
        return <>Addresses Options goes here =V {JSON.stringify(this.state?.optionsResult)}</>
    }
}