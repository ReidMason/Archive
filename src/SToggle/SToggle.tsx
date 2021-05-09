import React from 'react'
import { Switch } from "@headlessui/react";
import SToggleBall from './SToggleBall';

interface SToggleProps {
    disabled?: boolean;
    checked: boolean;
    setChecked: Function;
    variant?: "dark" | "light" | "outline";
    srText?: string;
}

export interface VariantStyle {
    dark: string,
    light: string,
    outline: string,
}

const baseStyles: VariantStyle = {
    dark: "text-snow-2",
    light: "text-night-2",
    outline: "bg-transparent border border-2 border-snow-2",
}

const uncheckedStyles: VariantStyle = {
    dark: "bg-night-4",
    light: "bg-snow-1",
    outline: "",
}

const checkedStyles: VariantStyle = {
    dark: "bg-frost-3",
    light: "bg-frost-3",
    outline: "bg-transparent",
}

const baseDisabledStyles = "disabled:opacity-50 disabled:cursor-not-allowed";
const disabledStyles: VariantStyle = {
    dark: `${baseDisabledStyles}`,
    light: `${baseDisabledStyles}`,
    outline: `${baseDisabledStyles}`,
}

export default function SToggle({ disabled, checked, setChecked, variant, srText }: SToggleProps) {
    const variantTheme = variant ?? "dark";
    const baseStyle = baseStyles[variantTheme];
    const disabledStyle = disabled ? disabledStyles[variantTheme] : "";
    const checkedStyle = checked ? checkedStyles[variantTheme] : uncheckedStyles[variantTheme];

    const variantStyle = `${baseStyle} ${checkedStyle} ${disabledStyle}`;

    return (
        <Switch className={`${variantStyle} h-7 w-14 relative inline-flex items-center rounded-full transition-colors ease-in-out duration-200`}
            disabled={disabled}
            checked={checked}
            onChange={(x) => { setChecked(x) }}
        >
            <SToggleBall checked={checked} variant={variantTheme} srText={srText} />
        </Switch>
    )
}
