import React, { Fragment } from 'react'
import { Dialog, Transition } from '@headlessui/react';
import { SButton } from '..';

interface SModalProps {
    open: boolean;
    setOpen: Function;
}

export default function SModal({ open, setOpen }: SModalProps) {
    const close = () => {
        setOpen(false);
    }

    return (
        <Transition show={open} as={Fragment}>
            <Dialog static as="div" open={open} onClose={close} className="fixed inset-0 z-10 overflow-y-auto">
                <div className="min-h-screen px-4 text-center">
                    <Transition.Child
                        as={Fragment}
                        enter="ease-out duration-150"
                        enterFrom="bg-opacity-0"
                        enterTo="bg-opacity-30"
                        leave="ease-in duration-150"
                        leaveFrom="opacity-30"
                        leaveTo="opacity-0"
                    >
                        <div>
                            <Dialog.Overlay className="fixed inset-0 bg-black bg-opacity-30" />
                        </div>
                    </Transition.Child>

                    {/* This element is to trick the browser into centering the modal contents. */}
                    <span
                        className="inline-block h-screen align-middle"
                        aria-hidden="true"
                    >
                        &#8203;
                    </span>

                    <Transition.Child
                        as={Fragment}
                        enter="ease-out duration-150"
                        enterFrom="opacity-0 scale-95"
                        enterTo="opacity-100 scale-100"
                        leave="ease-in duration-150"
                        leaveFrom="opacity-100 scale-100"
                        leaveTo="opacity-0 scale-95"
                    >
                        <div className="inline-block opacity-100 w-full max-w-md p-6 my-8 overflow-hidden text-left align-middle transition-all transform bg-white shadow-xl rounded-lg">
                            <Dialog.Title>Title</Dialog.Title>
                            <Dialog.Description>
                                Description
                    </Dialog.Description>
                        Content
                    <SButton onClick={close}>Close</SButton>
                        </div>
                    </Transition.Child>
                </div>
            </Dialog>
        </Transition>
    )
}
