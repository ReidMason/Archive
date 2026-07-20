import { Dialog } from '@headlessui/react';
import React, { useContext, useEffect, useState } from 'react';
import { SButton } from 'skippythesnakes-react-components';
import { GlobalContext } from '../GlobalContext';
import Preferences from '../interfaces/Preferences';
import { AdjustmentsIcon } from '@heroicons/react/outline';

export default function TorrentSettingsModal() {
    const { qbittorrentService } = useContext(GlobalContext);
    const [preferences, setPreferences] = useState<Preferences>({});
    const [isOpen, setIsOpen] = useState(false);

    useEffect(() => {
        qbittorrentService.getPreferences().then((response) => {
            setPreferences(response.data);
        })
    }, [qbittorrentService]);

    const toggleCustomUi = () => {
        preferences.alternative_webui_enabled = !preferences.alternative_webui_enabled
        setPreferences({ ...preferences });
        const changedValue = (({ alternative_webui_enabled }) => ({ alternative_webui_enabled }))(preferences);
        qbittorrentService.setPreferences(changedValue);
    }

    return (
        <div>
            <button className="text-nord-6 bg-nord-2 p-2 rounded hover:bg-nord-9" onClick={() => (setIsOpen(true))}>
                <AdjustmentsIcon className="w-5" />
            </button>

            <Dialog open={isOpen} onClose={() => { setIsOpen(false) }} className="fixed inset-0 z-10 overflow-y-auto">
                <Dialog.Overlay className="fixed inset-0 bg-black opacity-40" />

                <div className="min-h-screen px-4 text-center">
                    <div className="text-nord-6 inline-block w-full max-w-md my-8 overflow-hidden text-left align-middle transition-all transform bg-nord-3 shadow-xl rounded">
                        <div className="flex flex-col gap-4 p-6 pt-4 gap-2">
                            <Dialog.Title as="h3" className="text-lg font-medium leading-6">Add torrent</Dialog.Title>

                            <div>
                                <SButton onClick={toggleCustomUi}>{preferences.alternative_webui_enabled ? "Disable" : "Enable"} custom ui</SButton>
                            </div>

                            <div className="flex gap-8 mt-4">
                                <SButton onClick={() => { setIsOpen(false) }}>Close</SButton>
                            </div>
                        </div>
                    </div>
                </div>
            </Dialog >
        </div>
    )
}
