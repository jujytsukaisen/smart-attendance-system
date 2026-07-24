import { format } from 'date-fns';
import { ShieldAlert, Info, AlertTriangle } from 'lucide-react';
import type { DeviceLogDto } from '../types';

interface DeviceLogsTableProps {
  logs: DeviceLogDto[];
}

export function DeviceLogsTable({ logs }: DeviceLogsTableProps) {
  const getIcon = (level: string) => {
    switch (level) {
      case 'Info':
        return <Info className="h-4 w-4 text-blue-500" />;
      case 'Warning':
        return <AlertTriangle className="h-4 w-4 text-yellow-500" />;
      case 'Error':
        return <ShieldAlert className="h-4 w-4 text-red-500" />;
      default:
        return <Info className="h-4 w-4 text-gray-500" />;
    }
  };

  return (
    <div className="bg-white shadow overflow-hidden sm:rounded-lg">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50">
          <tr>
            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Level</th>
            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Message</th>
            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Timestamp</th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {logs.length === 0 ? (
            <tr>
              <td colSpan={3} className="px-6 py-4 text-center text-sm text-gray-500">
                No logs available.
              </td>
            </tr>
          ) : (
            logs.map((log) => (
              <tr key={log.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="flex items-center gap-2">
                    {getIcon(log.level)}
                    <span className="text-sm font-medium text-gray-900">{log.level}</span>
                  </div>
                </td>
                <td className="px-6 py-4">
                  <div className="text-sm text-gray-500">{log.message}</div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm text-gray-500">
                    {format(new Date(log.timestamp), 'MMM dd, yyyy HH:mm:ss')}
                  </div>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
