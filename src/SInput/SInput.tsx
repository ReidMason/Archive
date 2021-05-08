import React from 'react';
import "./SInput.scss";

interface SInputProps {
    name?: string;
    placeholder?: string;
    value: string | number;
    setValue: Function;
    type?: "text" | "password" | "number";
    disabled?: boolean;
    autoComplete?: string;
}

export default function SInput({ name, placeholder, value, setValue, type, disabled, autoComplete }: SInputProps) {
    const inputType = type ?? "text";
    const inputDisabled = disabled ?? false
    const inputAutocomplete = autoComplete ?? "off";

    return (
        <input className="s-input" name={name} type={inputType} placeholder={placeholder} disabled={inputDisabled} value={value} onChange={(e) => (setValue(e.target.value))} autoComplete={inputAutocomplete} />
    )
}
