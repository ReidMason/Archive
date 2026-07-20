import React from 'react'
import { SButton } from 'skippythesnakes-react-components';
import TorrentActionButtons from './TorrentActionButtons'

interface BulkTorrentActionsProps {
    torrentHashes: Array<string>;
    deselectAllTorrents: Function;
}

export default function BulkTorrentActions({ torrentHashes, deselectAllTorrents }: BulkTorrentActionsProps) {
    return (
        <div className="flex items-end gap-4 text-nord-6">
            <SButton onClick={deselectAllTorrents}>Deselect all</SButton>
            <div className="flex flex-col">
                <p>Selected torrents: {torrentHashes.length}</p>
                <div className="text-3xl">
                    <TorrentActionButtons torrentHashes={torrentHashes} />
                </div>
            </div>
        </div>
    )
}
