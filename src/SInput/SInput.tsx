import React from 'react';

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
        <input className="text-snow-3 p-2 rounded bg-night-4 border border-night-4 disabled:cursor-not-allowed disabled:opacity-50" name={name} type={inputType} placeholder={placeholder} disabled={inputDisabled} value={value} onChange={(e) => (setValue(e.target.value))} autoComplete={inputAutocomplete} />
    )
}
