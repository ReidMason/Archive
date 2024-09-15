import React, { useState } from "react";
import { SButton, SPopover } from "../index";

export default {
    title: "SPopover"
};

export const Popover = () => {
    const [value, setValue] = useState("");
    const [numberValue, setNumberValue] = useState(0);

    return (
        <div className="flex flex-col mt-24">
            <SPopover text="Message here with lots of text">
                <SButton>Button with popover</SButton>
            </SPopover>
        </div>
    )
};
