import { formatDistanceToNow } from 'date-fns';
import { SyncStatusBadge } from './DeviceStatusBadge';
import type { SyncResultDto } from '../types';

interface SyncHistoryTimelineProps {
  history: SyncResultDto[];
}

export function SyncHistoryTimeline({ history }: SyncHistoryTimelineProps) {
  if (history.length === 0) {
    return (
      <div className="text-sm text-gray-500 py-4 text-center">
        No synchronization history available.
      </div>
    );
  }

  return (
    <div className="flow-root">
      <ul className="-mb-8">
        {history.map((sync, itemIdx) => (
          <li key={sync.id}>
            <div className="relative pb-8">
              {itemIdx !== history.length - 1 ? (
                <span className="absolute top-4 left-4 -ml-px h-full w-0.5 bg-gray-200" aria-hidden="true" />
              ) : null}
              <div className="relative flex space-x-3">
                <div>
                  <span className="h-8 w-8 rounded-full bg-gray-100 flex items-center justify-center ring-8 ring-white">
                    <span className="text-xs font-medium text-gray-500">{sync.recordsSynced}</span>
                  </span>
                </div>
                <div className="flex min-w-0 flex-1 justify-between space-x-4 pt-1.5">
                  <div>
                    <p className="text-sm text-gray-500">
                      {sync.message || 'Synchronization task processed'} 
                    </p>
                    <div className="mt-1">
                      <SyncStatusBadge status={sync.status} />
                    </div>
                  </div>
                  <div className="whitespace-nowrap text-right text-sm text-gray-500">
                    {formatDistanceToNow(new Date(sync.syncTime), { addSuffix: true })}
                  </div>
                </div>
              </div>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
