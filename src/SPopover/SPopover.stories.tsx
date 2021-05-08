import React, { useState } from "react";
import { SPopover } from "../index";

export default {
    title: "SPopover"
};

export const Popover = () => {
    const [value, setValue] = useState("");
    const [numberValue, setNumberValue] = useState(0);

    return (
        <div style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
            <SPopover text="Message here with lots of text">
                <p>Popover!</p>
            </SPopover>
        </div>
    )
};
