import { ReactNode } from 'react'
import './map.scss'
type Props = {
    cityMap: string | null,
    waterMap: string | null,
    children?: ReactNode
}
export const MapDiv = (props: Props & React.HTMLAttributes<HTMLDivElement>) => {
    const children = props.children;
    const cityMap = props.cityMap;
    const waterMap = props.waterMap;
    return <div {...Object.fromEntries(Object.entries(props).filter(x => !['children', 'cityMap', "waterMap"].includes(x[0])))} className='cityMapTopographic' style={cityMap != null ? { ...props.style, backgroundImage: `url(${cityMap})` } : { display: "none" }}>
        <div className='waterLayer' style={waterMap != null ? { backgroundImage: `url(${waterMap})` } : {}} />
        {children}
    </div>
}