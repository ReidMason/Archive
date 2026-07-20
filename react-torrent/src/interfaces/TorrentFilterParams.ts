export default interface TorrentFilterParams {
    sort?: string;
    reverse: boolean;
    hashes?: Array<string>;
    filter?: string;
    category?: string;
}