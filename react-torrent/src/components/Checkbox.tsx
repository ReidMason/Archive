import React from 'react';
import { CheckIcon } from "@heroicons/react/solid";

interface CheckboxProps {
    defaultValue: boolean;
    setter: Function;
    disabled?: boolean;
    id: string;
}

export default function Checkbox({ defaultValue, setter, disabled, id }: CheckboxProps) {
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setter(e.target.checked);
    }

    return (
        <div>
            <div>
                <input disabled={disabled} className="absolute opacity-0 cursor-pointer disabled:cursor-not-allowed" id={id} type="checkbox" checked={defaultValue} onChange={handleChange} />
                <label className={`flex items-center justify-center ${defaultValue ? "bg-nord-9" : ""} border-2 border-nord-1 transition-colors duration-300 rounded w-6 h-6 ${disabled ? "opacity-50 cursor-not-allowed" : ""} cursor-pointer`} htmlFor={id}>
                    <div className={`${defaultValue ? "" : "opacity-0"}`}>
                        <CheckIcon className="h-5" />
                    </div>
                </label>
            </div>
        </div >
    )
}
