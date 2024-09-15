import React from 'react'
import { VariantStyle } from "./SToggle";

interface SToggleBallProps {
    checked: boolean;
    variant: "dark" | "light" | "outline";
    srText?: string;
}

const baseStyles: VariantStyle = {
    dark: "bg-snow-1",
    light: "bg-night-4",
    outline: "bg-snow-3",
}

const baseCheckedStyles = "translate-x-8";
const checkedStyles: VariantStyle = {
    dark: `${baseCheckedStyles}`,
    light: `${baseCheckedStyles}`,
    outline: `translate-x-7`,
}

export default function SToggleBall({ checked, variant, srText }: SToggleBallProps) {
    const baseStyle = baseStyles[variant];
    const checkedStyle = checked ? checkedStyles[variant] : "translate-x-1";

    const variantStyle = `${baseStyle} ${checkedStyle}`;

    return (
        <div className="flex items-center">
            {srText && <span className="sr-only">{srText}</span>}
            <span className={`${variantStyle} shadow-xl inline-block w-5 h-5 transform rounded-full transition ease-in-out duration-200`} />
        </div>
    )
}
