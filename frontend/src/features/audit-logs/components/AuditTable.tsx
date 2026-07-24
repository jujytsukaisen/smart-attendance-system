import { useState } from 'react';
import { format } from 'date-fns';
import { ChevronDown, ChevronUp, ExternalLink } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Badge } from '@/shared/components/ui/badge';
import { Button } from '@/shared/components/ui/button';
import type { AuditLogDto } from '../types';

interface AuditTableProps {
  data: AuditLogDto[];
}

export function AuditTable({ data }: AuditTableProps) {
  const [sortConfig, setSortConfig] = useState<{ key: keyof AuditLogDto; direction: 'asc' | 'desc' } | null>(null);

  const handleSort = (key: keyof AuditLogDto) => {
    let direction: 'asc' | 'desc' = 'asc';
    if (sortConfig && sortConfig.key === key && sortConfig.direction === 'asc') {
      direction = 'desc';
    }
    setSortConfig({ key, direction });
  };

  const sortedData = [...data].sort((a, b) => {
    if (!sortConfig) return 0;
    const { key, direction } = sortConfig;
    if (a[key]! < b[key]!) return direction === 'asc' ? -1 : 1;
    if (a[key]! > b[key]!) return direction === 'asc' ? 1 : -1;
    return 0;
  });

  const getActionColor = (action: string) => {
    switch (action) {
      case 'Create': return 'bg-green-100 text-green-800';
      case 'Update': return 'bg-blue-100 text-blue-800';
      case 'Delete': return 'bg-red-100 text-red-800';
      case 'Login': return 'bg-purple-100 text-purple-800';
      case 'Logout': return 'bg-gray-100 text-gray-800';
      case 'Approve': return 'bg-emerald-100 text-emerald-800';
      case 'Reject': return 'bg-orange-100 text-orange-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getResultColor = (result: string) => {
    return result === 'Success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800';
  };

  return (
    <div className="bg-white shadow overflow-hidden sm:rounded-lg">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer" onClick={() => handleSort('createdAt')}>
                <div className="flex items-center gap-1">Date {sortConfig?.key === 'createdAt' && (sortConfig.direction === 'asc' ? <ChevronUp className="h-3 w-3" /> : <ChevronDown className="h-3 w-3" />)}</div>
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer" onClick={() => handleSort('userName')}>
                <div className="flex items-center gap-1">User {sortConfig?.key === 'userName' && (sortConfig.direction === 'asc' ? <ChevronUp className="h-3 w-3" /> : <ChevronDown className="h-3 w-3" />)}</div>
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer" onClick={() => handleSort('action')}>
                <div className="flex items-center gap-1">Action {sortConfig?.key === 'action' && (sortConfig.direction === 'asc' ? <ChevronUp className="h-3 w-3" /> : <ChevronDown className="h-3 w-3" />)}</div>
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer" onClick={() => handleSort('entityName')}>
                <div className="flex items-center gap-1">Entity {sortConfig?.key === 'entityName' && (sortConfig.direction === 'asc' ? <ChevronUp className="h-3 w-3" /> : <ChevronDown className="h-3 w-3" />)}</div>
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                IP / Browser
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Result
              </th>
              <th scope="col" className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Details
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {sortedData.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-6 py-4 text-center text-sm text-gray-500">
                  No audit logs found.
                </td>
              </tr>
            ) : (
              sortedData.map((log) => (
                <tr key={log.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {format(new Date(log.createdAt), 'MMM dd, yyyy HH:mm:ss')}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">{log.userName}</div>
                    <div className="text-xs text-gray-500">{log.userId}</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <Badge variant="outline" className={`border-transparent ${getActionColor(log.action)}`}>
                      {log.action}
                    </Badge>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">{log.entityName}</div>
                    {log.entityId && <div className="text-xs text-gray-500">{log.entityId}</div>}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-900">{log.ipAddress}</div>
                    <div className="text-xs text-gray-500 truncate max-w-[120px]" title={log.browser}>{log.browser}</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <Badge variant="outline" className={`border-transparent ${getResultColor(log.result)}`}>
                      {log.result}
                    </Badge>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <Link to={`/audit-logs/${log.id}`}>
                      <Button variant="ghost" size="sm">
                        <ExternalLink className="h-4 w-4" />
                      </Button>
                    </Link>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
