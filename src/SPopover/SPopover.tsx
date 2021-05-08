import React from 'react'
import { usePopperTooltip } from "react-popper-tooltip";

interface SPopoverProps {
    children: React.ReactNode;
    text: string;
}

export default function SPopover({ children, text }: SPopoverProps) {
    const {
        getTooltipProps,
        setTooltipRef,
        setTriggerRef,
        visible
    } = usePopperTooltip({
        placement: "top",
        delayShow: 700
    });


    const opacityStyle = visible ? "opacity-100" : "opacity-0";

    return (
        <div className="mt-24">
            <button type="button" ref={setTriggerRef}>{children}</button>
            <div ref={setTooltipRef} {...getTooltipProps({ className: "tooltip-container" })}>
                <div className={`p-1 px-2 text-snow-2 ${opacityStyle} shadow bg-night-4 rounded transition-all duration-300`}>
                    {text}
                </div>
            </div>
        </div>
    )
}
