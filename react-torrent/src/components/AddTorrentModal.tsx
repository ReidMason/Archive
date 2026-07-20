import { Dialog } from '@headlessui/react';
import React, { useContext, useState } from 'react';
import { GlobalContext } from '../GlobalContext';
import { SButton } from 'skippythesnakes-react-components';
import Toggle from './Toggle';
import { LinkIcon } from '@heroicons/react/solid'


interface AddTorrentModalProps {
    isOpen: boolean;
    setIsOpen: Function;
}

export default function AddTorrentModal({ isOpen, setIsOpen }: AddTorrentModalProps) {
    const { qbittorrentService } = useContext(GlobalContext);
    const [magnetUrls, setMagnetUrls] = useState<Array<string>>([""]);
    const [paused, setPaused] = useState<boolean>(false);
    const [skipChecking, setSkipChecking] = useState<boolean>(false);
    const [autoTMM, setAutoTMM] = useState<boolean>(false);

    const addTorrents = () => {
        qbittorrentService.addTorrentsByMagnet(magnetUrls, paused, skipChecking, autoTMM);
        setIsOpen(false);
    }

    const magnetEdited = (e: React.ChangeEvent<HTMLInputElement>, index: number) => {
        let newMagnetUrls = magnetUrls;
        newMagnetUrls[index] = e.target.value;

        // Remove any magnet urls that are empty strings excluding the alst element in the list
        newMagnetUrls = newMagnetUrls.filter((x, i) => (x.length > 0 || i === newMagnetUrls.length - 1));

        // If the edited manget url is the last one add a new empty one to the end
        // We do this so a new input is automatically added to the iterface
        if (index === magnetUrls.length - 1)
            newMagnetUrls.push("");

        setMagnetUrls([...newMagnetUrls]);
    }

    const handleMagnetInputFocus = (e: React.FocusEvent<HTMLInputElement>) => { e.target.select() };


    return (
        <Dialog open={isOpen} onClose={() => { setIsOpen(false) }} className="fixed inset-0 z-10 overflow-y-auto">
            <Dialog.Overlay className="fixed inset-0 bg-black opacity-40" />

            <div className="min-h-screen px-4 text-center">
                <div className="text-nord-6 inline-block w-full max-w-md my-8 overflow-hidden text-left align-middle transition-all transform bg-nord-3 shadow-xl rounded">
                    <div className="flex flex-col gap-4 p-6 pt-4 gap-2">
                        <Dialog.Title as="h3" className="text-lg font-medium leading-6">Add torrent</Dialog.Title>

                        <div className="flex flex-col gap-2">
                            <p>Magnet urls</p>
                            {magnetUrls.map((url, i) => (
                                <div className="flex flex-col gap-2" key={i}>
                                    <div className="flex items-center gap-x-2">
                                        <LinkIcon className="h-5" />
                                        <input className="bg-nord-2 p-1 rounded border border-nord-1 select-all" type="text" value={url} onFocus={handleMagnetInputFocus} onChange={(e) => magnetEdited(e, i)} />
                                    </div>
                                </div>
                            ))}
                        </div>

                        <hr className="border-nord-4 opacity-40" />
                        <div className="grid grid-cols-2 gap-y-2">
                            <div className="flex flex-col gap-1">
                                <p>Paused</p>
                                <Toggle defaultValue={paused} setter={setPaused} />
                            </div>

                            <div className="flex flex-col gap-1">
                                <p>Skip checking</p>
                                <Toggle defaultValue={skipChecking} setter={setSkipChecking} />
                            </div>

                            <div className="flex flex-col gap-1">
                                <p>Auto torrent mangement</p>
                                <Toggle defaultValue={autoTMM} setter={setAutoTMM} />
                            </div>
                        </div>

                        <div className="flex gap-8 mt-4">
                            <SButton onClick={addTorrents}>Add torrents</SButton>
                            <SButton onClick={() => { setIsOpen(false) }}>Cancel</SButton>
                        </div>
                    </div>
                </div>
            </div>
        </Dialog >
    )
}
