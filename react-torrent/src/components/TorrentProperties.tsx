import React from 'react'
import Torrent from '../interfaces/Torrent'
import { bytesToSize, secondsToDhms } from '../utils';
import AttributeWithIcon from './TorrentListItemAttributeWithIcon'
import { ArrowDownIcon, ArrowUpIcon, TrendingUpIcon, UserGroupIcon, ClockIcon, DocumentIcon, CollectionIcon } from '@heroicons/react/solid'
import { CloudUploadIcon, CloudDownloadIcon } from '@heroicons/react/outline';

interface TorrentPropertiesProps {
    torrent: Torrent;
}

export default function TorrentProperties({ torrent }: TorrentPropertiesProps) {
    const downloadSpeed: String = bytesToSize(torrent.dlspeed);
    const uploadSpeed: String = bytesToSize(torrent.upspeed);
    const downloadSize: String = bytesToSize(torrent.total_size);
    const downloaded: String = bytesToSize(torrent.downloaded);
    const uploaded: String = bytesToSize(torrent.uploaded);
    const eta: String = secondsToDhms(torrent.eta >= 8640000 ? 0 : torrent.eta);

    return (
        <div className="grid grid-cols-3 gap-x-6 gap-y-1">
            {/* Row 1 */}
            <AttributeWithIcon value={eta} icon={ClockIcon} popoverText="eta" />
            <AttributeWithIcon value={`${downloadSpeed}/s`} icon={ArrowDownIcon} iconColour="text-nord-8" popoverText="Download speed" />
            <AttributeWithIcon value={`${uploadSpeed}/s`} icon={ArrowUpIcon} iconColour="text-nord-14" popoverText="Upload speed" />

            {/* Row 2 */}
            <AttributeWithIcon value={downloadSize} icon={DocumentIcon} popoverText="Downlod size" />
            <AttributeWithIcon value={downloaded} icon={CloudDownloadIcon} popoverText="Downloaded" />
            <AttributeWithIcon value={uploaded} icon={CloudUploadIcon} popoverText="Uploaded" />

            {/* Row 3 */}
            {torrent.category ?
                <AttributeWithIcon value={torrent.category} icon={CollectionIcon} popoverText="Category" />
                : <div></div>
            }
            <AttributeWithIcon value={torrent.num_leechs.toString()} icon={UserGroupIcon} popoverText="Leeches" />
            <AttributeWithIcon value={torrent.num_seeds.toString()} icon={TrendingUpIcon} popoverText="Seeds" />
        </div>
    )
}
