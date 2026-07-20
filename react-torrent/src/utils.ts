import { v4 as uuidv4 } from 'uuid';

export function bytesToSize(bytes: number) {
    var sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    if (bytes <= 0 || isNaN(bytes))
        return '0 B';

    var i: number = parseInt(Math.floor(Math.log(bytes) / Math.log(1000)).toString());
    return (bytes / Math.pow(1000, i)).toFixed(2) + ' ' + sizes[i];
}

export function secondsToDhms(seconds: number) {
    seconds = Number(seconds);

    if (seconds <= 0)
        return "∞";

    var d: Number = Math.floor(seconds / (3600 * 24));
    var h: Number = Math.floor(seconds % (3600 * 24) / 3600);
    var m: Number = Math.floor(seconds % 3600 / 60);
    var s: Number = Math.floor(seconds % 60);

    if (seconds >= 86400)
        return `${d}d ${h}h`;

    if (seconds >= 3600)
        return `${h}h ${m}m`;

    if (seconds >= 60)
        return `${m}m ${s}s`;

    return `${s}s`;
}

export function formatState(state: String) {
    /* States:
        Downloading
        Completed
        Metadata
        Seeding
        Paused
        Queued
        Checking
        Moving
        Failed
        Missing Files
        Stalled
    */

    switch (state) {
        case 'forcedDL':
        case 'downloading':
            return 'Downloading'
        case 'metaDL':
            return 'Metadata'
        case 'forcedUP':
        case 'uploading':
        case 'stalledUP':
            return 'Seeding'
        case 'pausedDL':
            return 'Paused'
        case 'pausedUP':
            return 'Completed'
        case 'queuedDL':
        case 'queuedUP':
            return 'Queued'
        case 'allocating':
        case 'checkingDL':
        case 'checkingUP':
        case 'checkingResumeData':
            return 'Checking'
        case 'moving':
            return 'Moving'
        case 'unknown':
            return 'Failed'
        case 'missingFiles':
            return 'Missing Files'
        case 'stalledDL':
            return 'Stalled'
        default:
            return 'Failed'
    }
}

export function generateUuid(): string {
    return uuidv4();
}