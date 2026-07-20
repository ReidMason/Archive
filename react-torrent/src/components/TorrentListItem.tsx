import React from 'react';
import TorrentListItemAttribute from '../components/TorrentListItemAttribute';
import Torrent from '../interfaces/Torrent';
import { formatState } from '../utils';
import Checkbox from './Checkbox';
import ProgressBar from './ProgressBar';
import TorrentActionButtons from './TorrentActionButtons';
import TorrentProperties from './TorrentProperties';
import TorrentContextMenu from './TorrentContextMenu';

export interface TorrentListItemProps {
    torrent: Torrent;
    setTorrent: Function;
}

const getStateBorderColour = (state: string) => {
    if (["Queued", "Metadata", "Checking"].includes(state))
        return "border-nord-15"; // Purple

    if (["Downloading", "Seeding"].includes(state))
        return "border-green-light"; // Bright green

    if (state === "Completed")
        return "border-nord-14"; // Green

    if (["Paused", "Moving"].includes(state))
        return "border-nord-8"; // Light blue

    if (state === "Stalled")
        return "border-nord-12"; // Orange

    if (["Missing Files", "Failed"].includes(state))
        return "border-nord-11"; // Red

    return "border-gray-400";
}

const getStateBackgroundColour = (state: string) => {
    if (["Queued", "Metadata", "Checking"].includes(state))
        return "bg-nord-15"; // Purple

    if (["Downloading", "Seeding"].includes(state))
        return "bg-green-light"; // Bright green

    if (state === "Completed")
        return "bg-nord-14"; // Green

    if (["Paused", "Moving"].includes(state))
        return "bg-nord-8"; // Light blue

    if (state === "Stalled")
        return "bg-nord-12"; // Orange

    if (["Missing Files", "Failed"].includes(state))
        return "bg-nord-11"; // Red

    return "bg-gray-400";
}

export default function TorrentListItem({ torrent, setTorrent }: TorrentListItemProps) {

    const progressPercentage: number = Math.floor(torrent.progress * 100);


    var formattedState: string = formatState(torrent.state);
    const bgColour = getStateBackgroundColour(formattedState);
    const borderColour = getStateBorderColour(formattedState);

    const setSelected = (selected: boolean) => {
        torrent.selected = selected;
        setTorrent({ ...torrent })
    }

    return (
        <div className={`bg-nord-2 hover:bg-nord-3 test transition-all transition-color duration-100 p-4 pt-2 text-nord-6 border-l-8 ${borderColour} rounded shadow`}>
            <div className="flex flex-col gap-4">
                <div className="flex flex-col flex-shrink gap-2">
                    <div className="relative">
                        <div className="flex absolute right-0">
                            <div className="flex flex-col items-center mt-1">
                                <Checkbox id={torrent.hash} defaultValue={!!torrent.selected} setter={setSelected} />
                            </div>
                            <TorrentContextMenu />
                        </div>
                        <h3 className="text-lg font-medium truncate">{torrent.name}</h3>
                    </div>
                    <div className="flex flex-wrap gap-4 ml-1">
                        <div className="flex flex-col gap-2">
                            <TorrentListItemAttribute heading={formattedState}>
                                <ProgressBar total={100} progress={progressPercentage} unit="%" progressColour={bgColour} />
                            </TorrentListItemAttribute>
                            <div className="flex text-3xl text-nord-6 mt-auto">
                                <TorrentActionButtons torrentHashes={[torrent.hash]} />
                            </div>
                        </div>

                        <TorrentProperties torrent={torrent} />
                    </div>
                </div>
            </div>
        </div>
    )
}
