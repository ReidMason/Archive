import React, { useState } from "react";
import { SToggle } from "../index";

export default {
    title: "SToggle"
};

export const Popover = () => {
    const [value, setValue] = useState(true);

    return (
        <div className="flex flex-col">
            <h1>{value ? "Enabled" : "Disabled"}</h1>
            <SToggle checked={value} setChecked={setValue} />
            <SToggle checked={value} setChecked={setValue} variant="light" />
            <SToggle checked={value} setChecked={setValue} variant="outline" />
            <SToggle disabled checked={value} setChecked={setValue} />
        </div>
    )
};
