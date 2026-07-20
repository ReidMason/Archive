import React from 'react'

interface ProgressBarProps {
    progress: number;
    unit?: string;
    total: number;
    progressColour?: string;
}

export default function ProgressBar({ progress, unit, total, progressColour }: ProgressBarProps) {
    const width = (progress / total) * 100;
    progressColour = progressColour ?? "bg-nord-14";

    return (
        <div className="bg-nord-1 h-6 w-full text-center rounded-full overflow-hidden relative">
            <span className="absolute left-0 w-full text-center">
                {progress}{unit}
            </span>
            <div className={`${progressColour} h-full transition-all duration-300`} style={{ width: width + "%" }}></div>
        </div>
    )
}
