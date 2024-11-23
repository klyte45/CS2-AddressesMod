import { ReactNode } from 'react'
import './map.scss'
type Props = {
    children?: ReactNode
}
export const MapDiv = ({ children }: Props) => {
    return <div className='cityMapTopographic'>
        <div className='waterLayer' />
        {children}
    </div>
}