import { Component } from "react";

import "#styles/cs2-side-tabs-menu.scss";
import translate from "#utility/translate";
import { Tabs, TabList, Tab, TabPanel } from "react-tabs";


export class Cs2SideTabs<X extends string> extends Component<{
    componentsMapViewer: Record<X, JSX.Element>,
    clickableTabs?: X[]
    tabsOrder: (X | undefined)[]
    currentTab: X
    i18nTitlePrefix: string
    onSetCurrentTab: (newTab: X) => void
}, {}> {
    constructor(props) {
        super(props);
        this.state = {};
    }
    render() {
        return (
            <>
                <Tabs selectedIndex={this.props.tabsOrder.filter(x => x != undefined).indexOf(this.props.currentTab)} onSelect={y => this.props.onSetCurrentTab(this.props.tabsOrder.filter(x => x != undefined)[y])} className={"cs2-side-tab"}>
                    <TabList className="sideNav" >
                        {this.props.tabsOrder.map((x, i) => !x
                            ? <div className="space" key={i}></div>
                            : <Tab key={i} disabled={Array.isArray(this.props.clickableTabs) && !this.props.clickableTabs.includes(x)}>{translate(this.props.i18nTitlePrefix + x)}</Tab>
                        )}
                    </TabList>
                    <div className="dataPanel">
                        {this.props.tabsOrder.map((x, i) => x && <TabPanel key={i}>{this.props.componentsMapViewer[x]}</TabPanel>)}
                    </div>
                </Tabs>
            </>
        );
    }
}
