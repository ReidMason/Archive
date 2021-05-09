import React, { useState } from "react";
import { SModal, SButton } from "../index";

export default {
    title: "Smodal"
};

export const Modal = () => {
    const [open, setOpen] = useState(true);

    return (
        <div>
            <h1>Modal</h1>
            <SButton onClick={() => setOpen(true)}>Open modal</SButton>
            <SModal open={open} setOpen={setOpen} />
        </div>
    )
};
