import React, { useContext, useState } from 'react';
import { useHistory } from "react-router-dom";
import LoadingSpinner from '../components/LoadingSpinner';
import { SInput } from 'skippythesnakes-react-components';
import { GlobalContext } from '../GlobalContext';

export default function Login() {
    const { qbittorrentService } = useContext(GlobalContext);
    const [invalidLogin, setInvalidLogin] = useState<boolean>(false);
    const [loading, setLoading] = useState<boolean>(false);

    const history = useHistory();

    const [username, setUsername] = useState<string>("");
    const [password, setPassword] = useState<string>("");

    const attemptLogin = () => {
        setLoading(true);
        qbittorrentService.login(username, password).then((loginSuccessful) => {
            if (loginSuccessful)
                history.push("/dashboard");
            else
                setInvalidLogin(true);
        }).finally(() => {
            setLoading(false);
        });
    }

    return (
        <div className="flex justify-center mt-64">
            <div className="w-96 max-w-lg min-w-max p-8 rounded bg-nord-1 shadow-lg">
                <div className="flex justify-center mb-2">
                    <img className="w-24" src="https://upload.wikimedia.org/wikipedia/commons/6/66/New_qBittorrent_Logo.svg" alt="qBittorrent logo" />
                </div>
                <h1 className="text-3xl text-center text-nord-6 mb-6">qBittorrent Login</h1>

                <div className="flex justify-center">
                    <div>
                        <div className="flex flex-col gap-2 w-64">
                            <SInput type="text" name="username" placeholder="Username" setValue={setUsername} value={username} />
                            <SInput type="password" name="password" placeholder="Password" setValue={setPassword} value={password} />
                        </div>
                        <div className={`flex ${invalidLogin ? '' : 'opacity-0'} text-nord-11 mt-1`}>
                            <p>Invalid username or password</p>
                        </div>
                    </div>
                </div>
                <div className="flex justify-center mt-4">
                    <button className="bg-nord-10 hover:bg-nord-9 text-nord-6 p-2 rounded w-48 disabled:cursor-not-allowed" disabled={loading} onClick={attemptLogin}>
                        <div className="flex justify-center items-center gap-2">
                            {loading &&
                                <div>
                                    <LoadingSpinner />
                                </div>
                            }
                            Log in
                            {loading && <div className="w-100 w-4" />}
                        </div>
                    </button>
                </div>
            </div>
        </div>
    )
}
