import React, { useEffect, useState } from 'react';
import { generateUuid } from '../utils';

interface IToggleProps {
    defaultValue: boolean;
    setter: Function;
    disabled?: boolean;
}

export default function Toggle({ defaultValue, setter, disabled }: IToggleProps) {
    const [value, setValue] = useState(defaultValue);
    const [id, setId] = useState<string>();

    useEffect(() => {
        // Genrate unique id
        setId(generateUuid());
    }, []);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setValue(e.target.checked);
        setter(e.target.checked);
    }

    return (
        <div>
            <div>
                <input disabled={disabled} className="absolute opacity-0 cursor-pointer disabled:cursor-not-allowed" id={id} type="checkbox" checked={value} onChange={handleChange} />
                <label className={`flex ${value ? "bg-nord-7" : "bg-nord-2"} rounded-full w-12 h-6 ${disabled ? "opacity-50 cursor-not-allowed" : ""} transition-color duration-300 cursor-pointer`} htmlFor={id}>
                    <div className={`w-6 h-6 bg-nord-6 rounded-full transform ${value ? "translate-x-6" : ""} transition-transform duration-300`}></div>
                </label>
            </div>
        </div>
    )
}
