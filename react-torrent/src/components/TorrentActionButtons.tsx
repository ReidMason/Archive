import React, { useContext, useState } from 'react';
import { GlobalContext } from '../GlobalContext';
import RemoveTorrentModal from './RemoveTorrentModal';
// import { PlayIcon, TrashIcon, PauseIcon } from '@heroicons/react/solid'
// import { PlayIcon, TrashIcon, PauseIcon } from '@heroicons/react/outline'


interface TorrentActionButtonsProps {
    torrentHashes: Array<string>;
}

export default function TorrentActionButtons({ torrentHashes }: TorrentActionButtonsProps) {
    const { qbittorrentService } = useContext(GlobalContext);

    const [removeTorrentModalOpen, setRemoveTorrentModalOpen] = useState<boolean>(false)

    const deleteTorrent = () => {
        setRemoveTorrentModalOpen(true);
    }

    const pauseTorrent = () => {
        qbittorrentService.pauseTorrents(torrentHashes);

    }

    const resumeTorrent = () => {
        qbittorrentService.resumeTorrents(torrentHashes);
    }

    return (
        <div>
            <button className="focus:outline-none hover:bg-nord-2 transition-color duration-100 rounded text-nord-14 active:text-nord-10 p-0.5" onClick={resumeTorrent}>
                {/* <PlayIcon className="h-8" /> */}
                <span className="iconify" data-icon="carbon-play-filled-alt"></span>
            </button>
            <button className="focus:outline-none hover:bg-nord-2 transition-color duration-100 rounded text-nord-7 active:text-nord-10 p-0.5" onClick={pauseTorrent}>
                {/* <PauseIcon className="h-8" /> */}
                <span className="iconify" data-icon="carbon-pause-filled"></span>
            </button>
            <button className="focus:outline-none hover:bg-nord-2 transition-color duration-100 rounded text-nord-11 active:text-nord-10 p-0.5" onClick={deleteTorrent}>
                {/* <TrashIcon className="h-8" /> */}
                <span className="iconify" data-icon="bx-bxs-trash-alt"></span>
            </button>

            {/* Delete torrent modal */}
            <RemoveTorrentModal torrentHashes={torrentHashes} isOpen={removeTorrentModalOpen} setIsOpen={setRemoveTorrentModalOpen} />
        </div>
    )
}
