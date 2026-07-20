import axios, { AxiosResponse } from 'axios';
import Preferences from '../interfaces/Preferences';
import TorrentFilterParams from '../interfaces/TorrentFilterParams';


class QbittorrentService {
    axios;
    setAuthenticated: Function;

    constructor(setAuthenticated: Function) {
        this.setAuthenticated = setAuthenticated;
        // Remove http://localhost:5000 for live
        this.axios = axios.create({ baseURL: "/api/v2" })
        this.axios.defaults.headers.post['Content-Type'] = 'application/x-www-form-urlencoded'

        // Add inteceptor to invalidate authentication any time a 403 reponse is received
        this.axios.interceptors.response.use((response) => { return response; },
            (error) => {
                if (error.response.status === 403)
                    this.setAuthenticated(false);
                return error.response;
            })
    }

    validateAuthentication() {
        return new Promise<boolean>((resolve) => {
            // Send a test request to try and get the version
            this.getVersion().then((response) => {
                const isAuthenticated: boolean = response.status === 200;
                this.setAuthenticated(isAuthenticated);
                resolve(isAuthenticated);
            }).catch((error) => {
                this.setAuthenticated(false);
                resolve(false);
            })
        })
    }

    login(username: string, password: string) {
        const payload = new URLSearchParams({ username, password })
        return new Promise((resolve, reject) => {
            this.axios.post('/auth/login', payload).then((response: AxiosResponse) => {
                const loginSuccess = response.data === 'Ok.'
                this.setAuthenticated(loginSuccess);
                resolve(loginSuccess);

            }).catch((response) => {
                this.setAuthenticated(false);
                reject(response);
            });
        })
    }

    logout() {
        this.setAuthenticated(false);
        return this.axios.post('/auth/logout')
    }

    getVersion() {
        return this.axios.get('/app/version');
    }

    getGlobalTransferInfo() {
        return this.axios.get('/transfer/info');
    }

    getMaindata(rid: number) {
        return this.axios.get(`/sync/maindata?rid=${rid}`);
    }

    getTorrents(params: TorrentFilterParams): Promise<AxiosResponse> {
        const formattedParams: Record<string, string> = {
            sort: params.sort ?? "state",
            reverse: params.reverse ? params.reverse.toString() : 'false',
            hashes: params.hashes ? params.hashes.join('|') : "",
            filter: params.filter ? params.filter : "",
            category: params.category ? params.category : ""
        }

        // Remove params that weren't passed
        Object.keys(formattedParams).forEach((key: string) => {
            if (formattedParams[key] === "") {
                delete formattedParams[key]
            }
        });

        const data = new URLSearchParams(formattedParams);

        return this.axios.get(`/torrents/info?${data.toString()}`)
    }

    addTorrentsByMagnet(urls: Array<string>, paused: boolean, skipChecking: boolean, autoTMM: boolean) {
        const payload = new URLSearchParams(
            {
                urls: urls.join('\n'),
                paused: paused.toString(),
                skip_checking: skipChecking.toString(),
                autoTMM: autoTMM.toString()
            })

        return this.axios.post('/torrents/add', payload)
    }

    pauseTorrents(hashes: Array<string>) {
        const payload = new URLSearchParams({ hashes: hashes.join("|") });
        return this.axios.post("/torrents/pause", payload);
    }

    resumeTorrents(hashes: Array<string>) {
        const payload = new URLSearchParams({ hashes: hashes.join("|") });
        return this.axios.post("/torrents/resume", payload);
    }

    deleteTorrents(hashes: Array<string>, deleteFiles: boolean) {
        const payload = new URLSearchParams({ hashes: hashes.join("|"), deleteFiles: deleteFiles.toString() });
        return this.axios.post("/torrents/delete", payload);
    }

    setPreferences(params: Preferences) {
        const payload = new URLSearchParams({
            json: JSON.stringify(params)
        })

        return this.axios.post('/app/setPreferences', payload)
    }

    getPreferences() {
        return this.axios.get('/app/preferences')
    }
}

export default QbittorrentService;