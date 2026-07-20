import { Dialog } from '@headlessui/react';
import React, { useContext } from 'react';
import { GlobalContext } from '../GlobalContext';
import { SButton } from 'skippythesnakes-react-components';

interface RemoveTorrentModalProps {
    isOpen: boolean;
    setIsOpen: Function;
    torrentHashes: Array<string>;
}

export default function RemoveTorrentModal({ isOpen, setIsOpen, torrentHashes }: RemoveTorrentModalProps) {
    const { qbittorrentService } = useContext(GlobalContext);

    const removeTorrent = () => {
        qbittorrentService.deleteTorrents(torrentHashes, false);
        setIsOpen(false);
    }

    const removeTorrentWithFiles = () => {
        qbittorrentService.deleteTorrents(torrentHashes, true);
        setIsOpen(false);
    }

    return (
        <Dialog open={isOpen} onClose={() => setIsOpen(false)} className="fixed inset-0 z-10 overflow-y-auto">
            <Dialog.Overlay className="fixed inset-0 bg-black opacity-40" />

            <div className="min-h-screen px-4 text-center">

                <div className="inline-block w-full max-w-2xl p-6 my-8 overflow-hidden text-left align-middle transition-all transform bg-nord-3 shadow-xl rounded">
                    <div className="flex flex-col gap-4">
                        <Dialog.Title as="h3" className="text-lg font-medium leading-6 text-nord-6">Remove torrent</Dialog.Title>
                        <Dialog.Description className="text-nord-6">
                            Are you sure you want to delete the selected torrents from the transfer list?
                    </Dialog.Description>

                        <div className="flex flex-wrap items-center gap-8 mt-4">
                            <SButton onClick={() => setIsOpen(false)}>Cancel</SButton>
                            <SButton onClick={removeTorrent}>Delete</SButton>
                            <SButton onClick={removeTorrentWithFiles}>Delete with files</SButton>
                        </div>
                    </div>
                </div>
            </div>
        </Dialog>
    )
}
