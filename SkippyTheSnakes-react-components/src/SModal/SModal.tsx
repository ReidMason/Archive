import React, { Fragment } from 'react';
import { Dialog, Transition } from '@headlessui/react';
import { SButton } from '..';

interface SModalProps {
    open: boolean;
    setOpen: Function;
    children: React.ReactElement | React.ReactElement[];
}

interface ChildProps {
    children: JSX.Element | JSX.Element[];
}

const Title = ({ }: ChildProps) => null;
const Body = ({ }: ChildProps) => null;
const Footer = ({ }: ChildProps) => null;

function SModal({ open, setOpen, children }: SModalProps) {
    const singleChild = (children as React.ReactElement[]).length === undefined;

    if (!singleChild) {
        children = children as React.ReactElement[];
        var title = children.find(el => el.type === Title);
        var body = children.find(el => el.type === Body);
        var footer = children.find(el => el.type === Footer);
    } else {
        var body = children as React.ReactElement;
    }

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
                        <div className="bg-night-2 inline-block opacity-100 w-full max-w-md p-6 my-8 overflow-hidden text-left align-middle transition-all transform shadow-xl rounded-lg">
                            <Dialog.Title>
                                <div>
                                    {title && title.props.children}
                                </div>
                            </Dialog.Title>
                            <div>
                                {body && body.props.children}
                            </div>

                            <div>
                                {footer && footer.props.children}
                            </div>
                        </div>
                    </Transition.Child>
                </div>
            </Dialog>
        </Transition>
    )
}

SModal.Title = Title;
SModal.Body = Body;
SModal.Footer = Footer;

export default SModal;