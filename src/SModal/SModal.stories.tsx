import React, { useState } from "react";
import { SModal, SButton, SInput } from "../index";

export default {
    title: "Smodal"
};

export const Modal = () => {
    const [open, setOpen] = useState(true);
    const [text, setText] = useState("");

    return (
        <div>
            <h1>Modal</h1>
            <SButton onClick={() => setOpen(true)}>Open modal</SButton>
            <SModal open={open} setOpen={setOpen}>
                <SModal.Title>
                    <div>
                        <p>This is in the header</p>
                        <SInput placeholder="Placeholder" value={text} setValue={setText} />
                    </div>
                </SModal.Title>

                <SModal.Body>
                    <p>This is in the body</p>
                </SModal.Body>

                <SModal.Footer>
                    <SButton className="mt-4" onClick={() => setOpen(false)}>Close</SButton>
                </SModal.Footer>
            </SModal>
        </div>
    )
};
