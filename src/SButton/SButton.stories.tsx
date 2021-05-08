import React from "react";
import SButton from "./SButton";

export default {
    title: "SButton"
};

export const Button1 = () => (
    <div style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
        <SButton onClick={() => { console.log("Hello World!") }}>Dark button</SButton>
        <SButton loading>Loading button</SButton>
        <SButton variant="text">Text button</SButton>
        <SButton variant="ghost">Ghost button</SButton>
        <SButton variant="light">Light button</SButton>
        <SButton variant="outline">Outline button</SButton>
        <SButton rounded>Rounded button</SButton>
        <SButton disabled>Disabled button</SButton>
    </div>
);
