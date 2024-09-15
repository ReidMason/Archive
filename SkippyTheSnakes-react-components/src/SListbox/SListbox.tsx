import React from 'react'
import { Listbox } from '@headlessui/react';

interface SListboxProps {
    value: string | number;
    setValue: Function;
    children: React.ReactElement[];
}

interface ChildProps {
    children: React.ReactNode;
}

interface OptionProps {
    children: React.ReactNode;
    value: string;
}

const Button = ({ }: ChildProps) => null;
const Option = ({ }: OptionProps) => null;

function SListbox({ value, setValue, children }: SListboxProps) {
    var button = children.find(el => el.type === Button);
    var options = children.filter(el => el.type === Option);


    return (
        <Listbox value={value} onChange={(e) => (setValue(e))}>
            <Listbox.Button>{value || "Select something"}</Listbox.Button>
            <Listbox.Options>
                {options.map((option, index) => (
                    <Listbox.Option {...option.props} key={index}>{option.props.children}</Listbox.Option>
                ))}
            </Listbox.Options>
        </Listbox>
    )
}


SListbox.Button = Button;
SListbox.Option = Option;

export default SListbox;