import React, { useState } from "react";
import { SButton } from "../index";

export default {
    title: "SButton"
};

export const Button = () => {
    const [loading, setLoading] = useState(true);

    return (
        <div style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
            <div style={{ display: "flex", gap: "1rem" }}>
                <SButton onClick={() => (setLoading(!loading))}>Dark button</SButton>
                <SButton onClick={() => (setLoading(!loading))}>Long button with long text</SButton>
            </div>
            <SButton loading={loading} onClick={() => (setLoading(true))}>Loading button</SButton>
            <SButton variant="text">Text button</SButton>
            <SButton variant="ghost">Ghost button</SButton>
            <SButton variant="light">Light button</SButton>
            <SButton variant="outline">Outline button</SButton>
            <SButton rounded>Rounded button</SButton>
            <SButton disabled>Disabled button</SButton>
        </div>
    )
};
