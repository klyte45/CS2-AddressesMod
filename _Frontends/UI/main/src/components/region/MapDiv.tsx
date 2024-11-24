import { ReactNode } from 'react'
import './map.scss'
type Props = {
    children?: ReactNode
}
export const MapDiv = (props: Props & any) => {
    const children = props.children;
    return <div {...props} className='cityMapTopographic'>
        <div className='waterLayer' />
        {children}
    </div>
}