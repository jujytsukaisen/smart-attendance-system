import { useState } from 'react';
import { format } from 'date-fns';
import { Eye, Trash2 } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/shared/components/ui/button';
import { ActionDialog } from './ActionDialog';
import { AttendanceStatusBadge } from './AttendanceStatusBadge';
import { useDeleteAttendance } from '../hooks';
import type { AttendanceListDto } from '../types';

interface AttendanceTableProps {
  data: AttendanceListDto[];
}

export function AttendanceTable({ data }: AttendanceTableProps) {
  const navigate = useNavigate();
  const deleteMutation = useDeleteAttendance();
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const handleDelete = () => {
    if (deleteId) {
      deleteMutation.mutate(deleteId, {
        onSuccess: () => setDeleteId(null),
      });
    }
  };

  return (
    <>
      <div className="bg-white shadow-sm ring-1 ring-gray-900/5 sm:rounded-lg overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-300">
          <thead className="bg-gray-50">
            <tr>
              <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                Employee ID
              </th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                Check-In Time
              </th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                Check-Out Time
              </th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                Status
              </th>
              <th scope="col" className="relative py-3.5 pl-3 pr-4 sm:pr-6">
                <span className="sr-only">Actions</span>
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {data.map((attendance) => (
              <tr key={attendance.id}>
                <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">
                  {attendance.employeeId.substring(0, 8)}...
                </td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                  {format(new Date(attendance.checkInTime), 'PP p')}
                </td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                  {attendance.checkOutTime ? format(new Date(attendance.checkOutTime), 'PP p') : '-'}
                </td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                  <AttendanceStatusBadge status={attendance.attendanceStatus} />
                </td>
                <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                  <div className="flex justify-end gap-2">
                    <Button variant="ghost" size="icon" onClick={() => navigate(`/attendance/${attendance.id}`)}>
                      <Eye className="h-4 w-4" />
                    </Button>
                    <Button 
                      variant="ghost" 
                      size="icon" 
                      className="text-red-600"
                      onClick={() => setDeleteId(attendance.id)}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <ActionDialog
        isOpen={!!deleteId}
        onClose={() => setDeleteId(null)}
        onConfirm={handleDelete}
        title="Delete Attendance"
        description="Are you sure you want to delete this attendance record? This action cannot be undone."
        confirmText="Delete"
        variant="destructive"
        isLoading={deleteMutation.isPending}
      />
    </>
  );
}
