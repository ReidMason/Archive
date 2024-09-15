import React, { useState } from "react";
import { SListbox } from "../index";

export default {
    title: "SListbox"
};

export const Listbox = () => {
    const [value, setValue] = useState("");

    return (
        <div>
            <h1>Testing</h1>
            <SListbox value={value} setValue={setValue}>
                <SListbox.Button>Select something</SListbox.Button>
                <SListbox.Option value="Testing">Testing</SListbox.Option>
                <SListbox.Option value="Testing2">Testing2</SListbox.Option>
            </SListbox>
        </div>
    )
};
