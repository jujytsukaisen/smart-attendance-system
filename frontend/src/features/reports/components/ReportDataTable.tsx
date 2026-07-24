import { useState } from 'react';
import { ChevronDown, ChevronUp } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';

export interface ColumnDef<T> {
  header: string;
  accessorKey: keyof T;
  cell?: (item: T) => React.ReactNode;
  sortable?: boolean;
}

interface ReportDataTableProps<T> {
  data: T[];
  columns: ColumnDef<T>[];
}

export function ReportDataTable<T>({ data, columns }: ReportDataTableProps<T>) {
  const [sortConfig, setSortConfig] = useState<{ key: keyof T; direction: 'asc' | 'desc' } | null>(null);

  const handleSort = (key: keyof T) => {
    let direction: 'asc' | 'desc' = 'asc';
    if (sortConfig && sortConfig.key === key && sortConfig.direction === 'asc') {
      direction = 'desc';
    }
    setSortConfig({ key, direction });
  };

  const sortedData = [...data].sort((a, b) => {
    if (!sortConfig) return 0;
    const { key, direction } = sortConfig;
    if (a[key] < b[key]) return direction === 'asc' ? -1 : 1;
    if (a[key] > b[key]) return direction === 'asc' ? 1 : -1;
    return 0;
  });

  return (
    <div className="bg-white shadow overflow-hidden sm:rounded-lg">
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              {columns.map((col, idx) => (
                <th 
                  key={idx}
                  scope="col" 
                  className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                >
                  <div 
                    className={`flex items-center gap-1 ${col.sortable ? 'cursor-pointer select-none hover:text-gray-700' : ''}`}
                    onClick={() => col.sortable && handleSort(col.accessorKey)}
                  >
                    {col.header}
                    {col.sortable && sortConfig?.key === col.accessorKey && (
                      sortConfig.direction === 'asc' ? <ChevronUp className="h-3 w-3" /> : <ChevronDown className="h-3 w-3" />
                    )}
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {sortedData.length === 0 ? (
              <tr>
                <td colSpan={columns.length} className="px-6 py-4 text-center text-sm text-gray-500">
                  No data available.
                </td>
              </tr>
            ) : (
              sortedData.map((item, rowIdx) => (
                <tr key={rowIdx} className="hover:bg-gray-50">
                  {columns.map((col, colIdx) => (
                    <td key={colIdx} className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {col.cell ? col.cell(item) : String(item[col.accessorKey] || '')}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
      
      {/* Simple Pagination Footer Mock */}
      <div className="bg-white px-4 py-3 border-t border-gray-200 flex items-center justify-between sm:px-6">
        <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
          <div>
            <p className="text-sm text-gray-700">
              Showing <span className="font-medium">1</span> to <span className="font-medium">{sortedData.length}</span> of <span className="font-medium">{sortedData.length}</span> results
            </p>
          </div>
          <div>
            <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
              <Button variant="outline" size="sm" disabled className="rounded-l-md">Previous</Button>
              <Button variant="outline" size="sm" disabled className="rounded-r-md">Next</Button>
            </nav>
          </div>
        </div>
      </div>
    </div>
  );
}
