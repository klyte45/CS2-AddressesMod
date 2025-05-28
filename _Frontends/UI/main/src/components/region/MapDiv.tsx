import { ReactNode } from 'react'
import './map.scss'
type Props = {
    cityMap: string | null,
    waterMap: string | null,
    beforeAnyLayer?: ReactNode,
    children?: ReactNode
}
export const MapDiv = (props: Props & React.HTMLAttributes<HTMLDivElement>) => {
    const children = props.children;
    const cityMap = props.cityMap;
    const waterMap = props.waterMap;
    const beforeWaterLayer = props.beforeAnyLayer;
    return <>
        <div {...Object.fromEntries(Object.entries(props).filter(x => !['children', 'cityMap', "waterMap"].includes(x[0])))} className='cityMapTopographic' style={cityMap != null ? { ...props.style } : { display: "none" }}>
            {beforeWaterLayer}
            <div className='cityMapTopographic' style={cityMap != null ? { backgroundImage: `url(${cityMap})` } : {}} />
            <div className='waterLayer' style={waterMap != null ? { backgroundImage: `url(${waterMap})` } : {}} />
            {children}
        </div></>
}