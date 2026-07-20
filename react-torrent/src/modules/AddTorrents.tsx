import React, { useState } from 'react'
import AddTorrentModal from '../components/AddTorrentModal';
import { SButton } from 'skippythesnakes-react-components';

export default function AddTorrents() {
    const [addTorrentModalOpen, setAddTorrentModalOpen] = useState<boolean>(false);

    return (
        <div>
            <SButton onClick={() => { setAddTorrentModalOpen(!addTorrentModalOpen) }}>Add torrent</SButton>
            {addTorrentModalOpen && <AddTorrentModal isOpen={addTorrentModalOpen} setIsOpen={setAddTorrentModalOpen} />}
        </div>
    )
}
