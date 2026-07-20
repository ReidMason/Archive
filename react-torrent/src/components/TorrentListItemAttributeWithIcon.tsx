import React from 'react'
import { SPopover } from 'skippythesnakes-react-components';

interface TorrentListItemAttributeWithIconProps {
    value: String;
    icon: Function;
    iconColour?: string;
    popoverText?: string;
}

export default function AttributeWithIcon({ value, icon, iconColour, popoverText }: TorrentListItemAttributeWithIconProps) {
    const Icon: Function = icon;
    iconColour = iconColour ?? "text-nord-9";

    const component = (
        <div className="flex gap-1 items-center group cursor-default">
            <Icon className={`h-5 ${iconColour}`} />
            <span>{value}</span>
        </div>
    )

    return (
        <div>
            {popoverText ?
                <SPopover text={popoverText}>
                    {component}
                </SPopover>
                : component
            }
        </div>
    )
}
