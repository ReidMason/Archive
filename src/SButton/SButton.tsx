import React from 'react';
import "./SButton.scss";
import SLoadingSpinnder from "../SLoadingSpinner";

interface SButtonProps {
    children: React.ReactNode;
    disabled?: boolean;
    variant?: "dark" | "light" | "outline" | "text" | "ghost";
    rounded?: boolean;
    onClick?: Function;
    loading?: boolean;
}

export default function SButton({ children, disabled, variant, rounded, onClick, loading }: SButtonProps) {
    const variantTheme = `sbutton-${variant ?? "dark"}`;
    const rounding = rounded ? "sbutton-rounded" : "";

    if (loading)
        disabled = true;

    return (
        <button className={`sbutton ${variantTheme} ${rounding}`} disabled={disabled} onClick={(e) => (onClick(e))}>
            {loading && <SLoadingSpinnder />}
            {children}
        </button >
    )
}
