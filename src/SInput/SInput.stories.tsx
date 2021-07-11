import React, { useState } from "react";
import { SInput } from "../index";

export default {
    title: "SInput"
};

export const Input = () => {
    const [value, setValue] = useState("");
    const [numberValue, setNumberValue] = useState(0);

    return (
        <div className="flex flex-col gap-4 mt-12">
            <SInput value={value} setValue={setValue} placeholder="Username" floatingLabel />
            <SInput disabled value={value} setValue={setValue} placeholder="Disabled input" floatingLabel />
            <SInput type="password" value={value} setValue={setValue} placeholder="Password" />
            <SInput type="number" value={numberValue} setValue={setNumberValue} placeholder="Number" />
        </div>
    )
};
