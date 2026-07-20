import React from 'react';
import AddTorrents from '../modules/AddTorrents';
import TorrentSettingsModal from '../modules/TorrentSettingsModal';
import TorrentsList from "../modules/TorrentsList";

export default function Dashboard() {
    // const history = useHistory();
    //
    // const logOut = () => {
    //     qbittorrentService.logout();
    //     history.push('/login');
    // }

    return (
        <div>
            <div className="flex justify-end pt-4 pr-4">
                <TorrentSettingsModal />
            </div>

            <div>
                <AddTorrents />
            </div>

            <div className="flex">
                <div className="m-4 min-w-0 w-full">
                    <TorrentsList />
                </div>
            </div>
        </div>
    )
}
