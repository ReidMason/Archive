import React, { useState } from "react";
import { SToggle } from "../index";

export default {
    title: "SToggle"
};

export const Popover = () => {
    const [value, setValue] = useState(false);

    return (
        <div className="flex flex-col gap-2 w-32">
            <h1 className="text-snow-1 text-xl bg-night-2 p-2 rounded text-center">{value ? "Enabled" : "Disabled"}</h1>
            <SToggle checked={value} setChecked={setValue} />
            <SToggle checked={value} setChecked={setValue} variant="light" />
            <SToggle checked={value} setChecked={setValue} variant="outline" />
            <SToggle disabled checked={value} setChecked={setValue} />
        </div>
    )
};
