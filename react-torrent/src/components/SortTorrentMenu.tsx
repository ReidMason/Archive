import { Listbox, Transition } from '@headlessui/react';
import React, { Fragment, useState } from 'react';
import { SelectorIcon } from '@heroicons/react/solid';

interface SortTorrentMenuProps {
    sort?: string;
    setSort: Function;
}

interface Option {
    name: string;
    value: string;
}

export default function SortTorrentMenu({ setSort, sort }: SortTorrentMenuProps) {
    const options: Array<Option> = [{ name: "State", value: "state" }, { name: "Added on", value: "added_on" }, { name: "Name", value: "name" }]

    const [selectedSort, setSelectedSort] = useState<Option>(options.find(x => x.value === sort) ?? options[0]);

    const updateSelectedSort = (selectedSort: Option) => {
        setSelectedSort(selectedSort);
        setSort(selectedSort.value);
    }

    return (
        <div className="text-nord-6">
            <p>Sort</p>
            <Listbox value={selectedSort} onChange={updateSelectedSort}>
                {({ open }) => (
                    <div className="relative">
                        <Listbox.Button className="text-left bg-nord-10 p-2 w-32 border-nord-1 hover:bg-nord-9 focus:outline-none focus:border-opacity-50 border-opacity-0 border border-nord-4 rounded">
                            <span className="block truncate">{selectedSort.name}</span>
                            <span className="absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
                                <SelectorIcon
                                    className="w-5 h-5"
                                    aria-hidden="true"
                                />
                            </span>
                        </Listbox.Button>
                        <Transition
                            show={open}
                            as={Fragment}
                            leave="transition ease-in duration-100"
                            leaveFrom="opacity-100"
                            leaveTo="opacity-0"
                        >
                            <Listbox.Options static className="bg-nord-2 mt-1 w-full rounded absolute max-h-60 z-50">
                                {options.map((option) => (
                                    <Listbox.Option className="border-t p-1 border-nord-1 hover:bg-nord-1 cursor-pointer" key={option.name} value={option}>
                                        {option.name}
                                    </Listbox.Option>
                                ))}
                            </Listbox.Options>
                        </Transition>
                    </div>
                )}
            </Listbox>
        </div>
    )
}
