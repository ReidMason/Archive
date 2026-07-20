import React from 'react'
import { bytesToSize } from '../utils';
import AttributeWithIcon from './TorrentListItemAttributeWithIcon';
import { ArrowDownIcon, ArrowUpIcon, ServerIcon } from '@heroicons/react/solid'
import { DownloadIcon, UploadIcon } from '@heroicons/react/outline'
import Maindata from '../interfaces/Maindata';


interface GlobalTranserInfoDisplayProps {
    maindata: Maindata;
}

export default function GlobalTranserInfoDisplay({ maindata }: GlobalTranserInfoDisplayProps) {
    return (
        <div className="grid grid-cols-3 gap-4 text-nord-6">
            <AttributeWithIcon value={bytesToSize(maindata?.server_state.dl_info_speed)} iconColour="text-nord-8" icon={ArrowDownIcon} popoverText="Total download speed" />
            <div className="col-span-2">
                <AttributeWithIcon value={bytesToSize(maindata?.server_state.up_info_speed)} iconColour="text-nord-14" icon={ArrowUpIcon} popoverText="Total upload speed" />
            </div>

            <AttributeWithIcon value={bytesToSize(maindata?.server_state.free_space_on_disk)} icon={ServerIcon} popoverText="Free space" />
            <AttributeWithIcon value={bytesToSize(maindata?.server_state.alltime_dl)} iconColour="text-nord-8" icon={DownloadIcon} popoverText="All time download" />
            <AttributeWithIcon value={bytesToSize(maindata?.server_state.alltime_ul)} iconColour="text-nord-14" icon={UploadIcon} popoverText="All time upload" />
        </div>
    )
}
