import React from 'react';
import SLoadingSpinner from "../SLoadingSpinner";

interface SButtonProps {
    children: React.ReactNode;
    disabled?: boolean;
    variant?: "dark" | "light" | "outline" | "text" | "ghost";
    rounded?: boolean;
    onClick?: Function;
    loading?: boolean;
}

interface variantStyle {
    dark: string,
    light: string,
    outline: string,
    text: string,
    ghost: string
}

const baseStyles: variantStyle = {
    dark: "text-snow-2 bg-frost-4 ",
    light: "text-night-2 bg-snow-2 ",
    outline: "text-snow-2 bg-transparent border border-2 border-snow-2",
    text: "text-snow-2 bg-transparent",
    ghost: "text-snow-2 bg-transparent underline"
}

const hoverStyles: variantStyle = {
    dark: "hover:bg-frost-3",
    light: "hover:bg-snow-3",
    outline: "hover:bg-night-3",
    text: "hover:bg-night-3",
    ghost: ""
}

const baseDisabledStyles = "disabled:opacity-50 disabled:cursor-not-allowed";
const disabledStyles: variantStyle = {
    dark: `${baseDisabledStyles}`,
    light: `${baseDisabledStyles}`,
    outline: `${baseDisabledStyles}`,
    text: `${baseDisabledStyles}`,
    ghost: `${baseDisabledStyles}`
}

const activeStyles: variantStyle = {
    dark: "active:bg-frost-3",
    light: "active:bg-snow-3",
    outline: "active:bg-night-3",
    text: "",
    ghost: ""
}

export default function SButton({ children, disabled, variant, rounded, onClick, loading }: SButtonProps) {
    // Disable the button if it's loading
    if (loading)
        disabled = true;

    const variantTheme = variant ?? "dark";
    const roundingStyle = rounded ? "rounded-full" : "rounded-md";
    const baseStyle = baseStyles[variantTheme];
    // Disabled hover styling when button is disabled
    const hoverStyle = !disabled ? hoverStyles[variantTheme] : "";
    const disabledStyle = disabledStyles[variantTheme];
    const activeStyle = activeStyles[variantTheme];
    const variantStyle = `${baseStyle} ${hoverStyle} ${disabledStyle} ${activeStyle}`;

    const onClickFunction = (e: React.MouseEvent) => {
        if (onClick)
            onClick(e)
    }

    return (
        <div>
            <button className={`w-40 p-2 flex justify-center ${variantStyle} ${roundingStyle}`} disabled={disabled} onClick={onClickFunction}>
                <div className="flex items-center gap-2">
                    {loading &&
                        <SLoadingSpinner />
                    }
                    {children}
                </div>
            </button >
        </div>
    )
}
