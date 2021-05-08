import React, { useState } from "react";
import { SInput } from "../index";

export default {
    title: "SInput"
};

export const Input = () => {
    const [value, setValue] = useState("");
    const [numberValue, setNumberValue] = useState(0);

    return (
        <div style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
            <SInput value={value} setValue={setValue} placeholder="Username" />
            <SInput disabled value={value} setValue={setValue} placeholder="Disabled input" />
            <SInput type="password" value={value} setValue={setValue} placeholder="Password" />
            <SInput type="number" value={numberValue} setValue={setNumberValue} placeholder="Number" />
        </div>
    )
};
