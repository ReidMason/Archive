import React from 'react';

interface SInputProps {
    name?: string;
    placeholder?: string;
    value: string | number;
    setValue: Function;
    type?: "text" | "password" | "number";
    disabled?: boolean;
    autoComplete?: string;
    className?: string;
    floatingLabel?: boolean;
}

export default function SInput({ name, placeholder, value, setValue, type, disabled, autoComplete, className, floatingLabel }: SInputProps) {
    const inputType = type ?? "text";
    const inputDisabled = disabled ?? false
    const inputAutocomplete = autoComplete ?? "off";
    const floatingLabelStyles = floatingLabel ? "focus:placeholder-transparent" : "";

    return (
        <div className="flex flex-col-reverse gap-2">
            <input className={`${className} ${floatingLabelStyles} peer z-10 text-snow-3 p-2 rounded bg-night-4 border border-night-4 disabled:cursor-not-allowed disabled:opacity-50`} name={name} type={inputType} placeholder={placeholder} disabled={inputDisabled} value={value} onChange={(e) => (setValue(e.target.value))} autoComplete={inputAutocomplete} />
            <label className={`${floatingLabel ? "" : "hidden"} ml-1 text-snow-1 transition-all transform translate-y-0 opacity-100 peer-placeholder-shown:opacity-0 peer-placeholder-shown:translate-y-10 peer-focus:opacity-100 peer-focus:translate-y-0`}>{placeholder}</label>
        </div>
    )
}
