import { Badge } from '@/shared/components/ui/badge';
import { DeviceStatus, ConnectionStatus, DeviceType, SyncStatus } from '../types';

export function DeviceStatusBadge({ status }: { status: DeviceStatus }) {
  switch (status) {
    case DeviceStatus.Approved:
      return <Badge variant="default" className="bg-green-100 text-green-800 hover:bg-green-100">Approved</Badge>;
    case DeviceStatus.PendingApproval:
      return <Badge variant="secondary" className="bg-yellow-100 text-yellow-800 hover:bg-yellow-100">Pending</Badge>;
    case DeviceStatus.Revoked:
      return <Badge variant="destructive" className="bg-red-100 text-red-800 hover:bg-red-100">Revoked</Badge>;
    case DeviceStatus.Maintenance:
      return <Badge variant="secondary" className="bg-blue-100 text-blue-800 hover:bg-blue-100">Maintenance</Badge>;
    case DeviceStatus.Retired:
      return <Badge variant="secondary" className="bg-gray-100 text-gray-800 hover:bg-gray-100">Retired</Badge>;
    default:
      return <Badge variant="outline">Unknown</Badge>;
  }
}

export function ConnectionStatusBadge({ status }: { status: ConnectionStatus }) {
  switch (status) {
    case ConnectionStatus.Online:
      return (
        <span className="inline-flex items-center gap-1.5 py-1 px-2 rounded-md text-xs font-medium bg-green-50 text-green-700 ring-1 ring-inset ring-green-600/20">
          <span className="w-1.5 h-1.5 rounded-full bg-green-500 animate-pulse"></span>
          Online
        </span>
      );
    case ConnectionStatus.Offline:
      return (
        <span className="inline-flex items-center gap-1.5 py-1 px-2 rounded-md text-xs font-medium bg-red-50 text-red-700 ring-1 ring-inset ring-red-600/10">
          <span className="w-1.5 h-1.5 rounded-full bg-red-500"></span>
          Offline
        </span>
      );
    default:
      return null;
  }
}

export function DeviceTypeBadge({ type }: { type: DeviceType }) {
  const typeMap: Record<string, string> = {
    [DeviceType.WallMounted]: 'Wall Mounted',
    [DeviceType.Mobile]: 'Mobile',
    [DeviceType.Desktop]: 'Desktop',
    [DeviceType.Web]: 'Web',
    [DeviceType.Kiosk]: 'Kiosk',
  };
  return <Badge variant="outline">{typeMap[type] || 'Unknown'}</Badge>;
}

export function SyncStatusBadge({ status }: { status: SyncStatus }) {
  switch (status) {
    case SyncStatus.Completed:
      return <Badge variant="default" className="bg-green-100 text-green-800 hover:bg-green-100">Completed</Badge>;
    case SyncStatus.InProgress:
      return <Badge variant="secondary" className="bg-blue-100 text-blue-800 hover:bg-blue-100">In Progress</Badge>;
    case SyncStatus.Failed:
      return <Badge variant="destructive" className="bg-red-100 text-red-800 hover:bg-red-100">Failed</Badge>;
    case SyncStatus.Pending:
      return <Badge variant="secondary" className="bg-yellow-100 text-yellow-800 hover:bg-yellow-100">Pending</Badge>;
    default:
      return <Badge variant="outline">Unknown</Badge>;
  }
}
