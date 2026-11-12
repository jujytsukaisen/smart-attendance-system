import { useState } from 'react';
import { Eye, Trash2 } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { Button } from '@/shared/components/ui/button';
import { ActionDialog } from './ActionDialog';
import { AttendanceStatusBadge } from './AttendanceStatusBadge';
import { useDeleteAttendance } from '../hooks';
import type { AttendanceListDto } from '../types';
import { useTranslation } from '@/shared/i18n';
import { useSystemFormatters } from '@/shared/hooks';

interface AttendanceTableProps {
  data: AttendanceListDto[];
  isEmployeeView?: boolean;
}

export function AttendanceTable({ data, isEmployeeView }: AttendanceTableProps) {
  const navigate = useNavigate();
  const { t, isRtl } = useTranslation();
  const { formatDateTime } = useSystemFormatters();
  const deleteMutation = useDeleteAttendance();
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const handleDelete = () => {
    if (deleteId) {
      deleteMutation.mutate(deleteId, {
        onSuccess: () => {
          toast.success(t('attendance.deleteSuccess'));
          setDeleteId(null);
        },
        onError: (err: any) => {
          toast.error(err.response?.data?.message || err.message || t('common.error'));
        },
      });
    }
  };

  return (
    <>
      <div className="bg-card text-card-foreground shadow-sm border border-border sm:rounded-lg overflow-x-auto">
        <table className="min-w-full divide-y divide-border">
          <thead className="bg-muted/50">
            <tr>
              {!isEmployeeView && (
                <th scope="col" className="py-3.5 px-4 text-start text-sm font-semibold text-foreground sm:px-6">
                  {t('attendance.employee')}
                </th>
              )}
              <th scope="col" className="px-3 py-3.5 text-start text-sm font-semibold text-foreground">
                {t('attendance.checkIn')}
              </th>
              <th scope="col" className="px-3 py-3.5 text-start text-sm font-semibold text-foreground">
                {t('attendance.checkOut')}
              </th>
              <th scope="col" className="px-3 py-3.5 text-start text-sm font-semibold text-foreground">
                {t('common.status')}
              </th>
              <th scope="col" className="relative py-3.5 px-4 text-end sm:px-6">
                <span className="sr-only">{t('common.actions')}</span>
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-border">
            {data.map((attendance) => (
              <tr key={attendance.id} className="hover:bg-muted/30 transition-colors">
                {!isEmployeeView && (
                  <td className="whitespace-nowrap py-4 px-4 text-sm sm:px-6">
                    <div className="flex flex-col">
                      <span className="font-medium text-foreground">
                        {attendance.employeeName || (isRtl ? 'موظف غير معروف' : 'Unknown Employee')}
                      </span>
                      <span className="text-xs text-muted-foreground font-mono">
                        {attendance.employeeNumber || (attendance.employeeId ? `${attendance.employeeId.substring(0, 8)}...` : '')}
                      </span>
                    </div>
                  </td>
                )}
                <td className="whitespace-nowrap px-3 py-4 text-sm text-muted-foreground">
                  {formatDateTime(attendance.checkInTime)}
                </td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-muted-foreground">
                  {formatDateTime(attendance.checkOutTime)}
                </td>
                <td className="whitespace-nowrap px-3 py-4 text-sm">
                  <AttendanceStatusBadge status={attendance.attendanceStatus} />
                </td>
                <td className="relative whitespace-nowrap py-4 px-4 text-end text-sm font-medium sm:px-6">
                  <div className="flex justify-end gap-2">
                    <Button variant="ghost" size="icon" onClick={() => navigate(`/attendance/${attendance.id}`)}>
                      <Eye className="h-4 w-4" />
                    </Button>
                    {!isEmployeeView && (
                      <Button 
                        variant="ghost" 
                        size="icon" 
                        className="text-destructive hover:text-destructive hover:bg-destructive/10"
                        onClick={() => setDeleteId(attendance.id)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    )}
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
        title={t('attendance.deleteTitle')}
        description={t('attendance.deleteDesc')}
        confirmText={t('attendance.deleteConfirm')}
        variant="destructive"
        isLoading={deleteMutation.isPending}
      />
    </>
  );
}
