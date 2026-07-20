import React, { useState } from 'react'
import { SInput } from 'skippythesnakes-react-components';
import SortTorrentMenu from '../components/SortTorrentMenu'
import TorrentFilterParams from '../interfaces/TorrentFilterParams'
import { SortAscendingIcon, SortDescendingIcon } from '@heroicons/react/solid'

interface TorrentFiltersProps {
    torrentFilterParams: TorrentFilterParams;
    updateTorrentFilterParams: Function;
    setSearchFilter: Function;
    searchFilter: string;
}

let timeout: NodeJS.Timeout;

export default function TorrentFilters({ torrentFilterParams, updateTorrentFilterParams, setSearchFilter, searchFilter }: TorrentFiltersProps) {
    const [filterText, setFilterText] = useState(searchFilter);

    const setTorrentSort = (newSortValue: string) => {
        torrentFilterParams.sort = newSortValue;
        updateTorrentFilterParams(torrentFilterParams)
    }

    const toggleSortOrder = () => {
        torrentFilterParams.reverse = !torrentFilterParams.reverse;
        updateTorrentFilterParams(torrentFilterParams)
    }

    const updateSearchFilter = (newValue: string) => {
        setFilterText(newValue);
        // We only want to update the search when the user has stopped typing
        clearTimeout(timeout);
        timeout = setTimeout(() => {
            setSearchFilter(newValue)
        }, 300)
    }

    return (
        <div className="flex gap-4 items-end">
            <SInput name="Search" placeholder="Search..." setValue={updateSearchFilter} value={filterText} type="text" />
            <SortTorrentMenu sort={torrentFilterParams.sort} setSort={setTorrentSort} />
            <div className="text-nord-6 cursor-pointer p-2 hover:bg-nord-1 rounded-full" onClick={toggleSortOrder}>
                {torrentFilterParams.reverse && <SortDescendingIcon className="h-8" />}
                {!torrentFilterParams.reverse && <SortAscendingIcon className="h-8" />}
            </div>
            {/* <Toggle defaultValue={torrentFilterParams.reverse ?? false} setter={setReverse} /> */}
        </div>
    )
}
