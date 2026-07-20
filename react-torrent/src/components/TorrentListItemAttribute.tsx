import React from 'react'

interface TorrentListItemAttributeProps {
    heading: String;
    value?: String;
    smallText?: String;
    children?: React.ReactNode
}

export default function TorrentListItemAttribute({ heading, value, smallText, children }: TorrentListItemAttributeProps) {
    return (
        <div className="flex flex-col gap-1">
            <p className="text-sm">{heading}</p>
            {value && <p className="text-base font-medium">{value}{smallText && <span className="text-xs opacity-50"> {smallText}</span>}</p>}
            {children && children}
        </div>
    )
}
