import React from 'react';
import Tippy from '@tippyjs/react';

interface SPopoverProps {
    children: React.ReactElement;
    text: string;
}

export default function SPopover({ children, text }: SPopoverProps) {
    return (
        <div>
            <Tippy className="p-1 px-2 text-snow-2 shadow bg-night-4 rounded" content={text} animation="fade" delay={[700, 0]}>
                <div className="inline-block">
                    {children}
                </div>
            </Tippy>
        </div>
    )
}
