import React from 'react'
import { Menu } from '@headlessui/react'
import { DotsVerticalIcon } from '@heroicons/react/solid';

export default function TorrentContextMenu() {
    return (
        <Menu as="div" className="relative">
            <Menu.Button>
                <DotsVerticalIcon className="h-5 mt-2 opacity-40 hover:opacity-100 cursor-pointer" />
            </Menu.Button>
            <Menu.Items className="absolute flex flex-col right-0 w-56 mt-2 origin-top-right">
                <Menu.Item>
                    {({ active }) => (
                        <a
                            className={`${active && "bg-blue-500"}`}
                            href="/account-settings"
                        >
                            Account settings
                        </a>
                    )}
                </Menu.Item>
                <Menu.Item>
                    {({ active }) => (
                        <a
                            className={`${active && "bg-blue-500"}`}
                            href="/account-settings"
                        >
                            Documentation
                        </a>
                    )}
                </Menu.Item>
            </Menu.Items>
        </Menu>
    )
}
