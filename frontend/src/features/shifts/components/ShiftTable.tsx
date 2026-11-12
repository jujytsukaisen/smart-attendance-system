import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import { Badge } from '@/shared/components/ui/badge';
import { MoreHorizontal, Edit, Trash, Activity, PowerOff, Clock, Eye } from 'lucide-react';
import { ActionDialog } from './ActionDialog';
import { useDeleteShift, useRestoreShift, useActivateShift, useDeactivateShift } from '../hooks';
import { ShiftTypeLabels, ShiftTypeArabicLabels } from '../types';
import type { ShiftListDto } from '../types';
import { useTranslation } from '@/shared/i18n';

interface ShiftTableProps {
  shifts: ShiftListDto[];
  isLoading: boolean;
}

export const ShiftTable = ({ shifts, isLoading }: ShiftTableProps) => {
  const navigate = useNavigate();
  const { isRtl } = useTranslation();
  const [selectedShift, setSelectedShift] = useState<ShiftListDto | null>(null);
  const [dialogAction, setDialogAction] = useState<'delete' | 'restore' | 'activate' | 'deactivate' | null>(null);

  const deleteMutation = useDeleteShift();
  const restoreMutation = useRestoreShift();
  const activateMutation = useActivateShift();
  const deactivateMutation = useDeactivateShift();

  const handleAction = async () => {
    if (!selectedShift || !dialogAction) return;

    try {
      if (dialogAction === 'delete') await deleteMutation.mutateAsync(selectedShift.id);
      if (dialogAction === 'restore') await restoreMutation.mutateAsync(selectedShift.id);
      if (dialogAction === 'activate') await activateMutation.mutateAsync(selectedShift.id);
      if (dialogAction === 'deactivate') await deactivateMutation.mutateAsync(selectedShift.id);
    } finally {
      closeDialog();
    }
  };

  const closeDialog = () => {
    setDialogAction(null);
    setSelectedShift(null);
  };

  const getDialogConfig = () => {
    switch (dialogAction) {
      case 'delete':
        return {
          title: isRtl ? 'حذف الوردية' : 'Delete Shift',
          description: isRtl
            ? `هل أنت متأكد من حذف وردية "${selectedShift?.name}"؟`
            : `Are you sure you want to delete "${selectedShift?.name}"?`,
          confirmText: isRtl ? 'حذف' : 'Delete',
          variant: 'danger' as const,
          isLoading: deleteMutation.isPending,
        };
      case 'activate':
        return {
          title: isRtl ? 'تفعيل الوردية' : 'Activate Shift',
          description: isRtl
            ? `هل أنت متأكد من تفعيل وردية "${selectedShift?.name}"؟`
            : `Are you sure you want to activate "${selectedShift?.name}"?`,
          confirmText: isRtl ? 'تفعيل' : 'Activate',
          variant: 'default' as const,
          isLoading: activateMutation.isPending,
        };
      case 'deactivate':
        return {
          title: isRtl ? 'إلغاء تفعيل الوردية' : 'Deactivate Shift',
          description: isRtl
            ? `هل أنت متأكد من إلغاء تفعيل وردية "${selectedShift?.name}"؟`
            : `Are you sure you want to deactivate "${selectedShift?.name}"?`,
          confirmText: isRtl ? 'إلغاء التفعيل' : 'Deactivate',
          variant: 'danger' as const,
          isLoading: deactivateMutation.isPending,
        };
      default:
        return null;
    }
  };

  const dialogConfig = getDialogConfig();

  const getShiftTypeBadge = (type: number) => {
    const label = isRtl ? (ShiftTypeArabicLabels[type] || 'أخرى') : (ShiftTypeLabels[type] || 'Other');
    switch (type) {
      case 1: // Morning
        return <Badge variant="default" className="bg-amber-500/15 text-amber-700 hover:bg-amber-500/25 border-amber-300">{label}</Badge>;
      case 2: // Afternoon
        return <Badge variant="default" className="bg-blue-500/15 text-blue-700 hover:bg-blue-500/25 border-blue-300">{label}</Badge>;
      case 3: // Night
        return <Badge variant="default" className="bg-indigo-500/15 text-indigo-700 hover:bg-indigo-500/25 border-indigo-300">{label}</Badge>;
      case 4: // FullDay
        return <Badge variant="default" className="bg-emerald-500/15 text-emerald-700 hover:bg-emerald-500/25 border-emerald-300">{label}</Badge>;
      case 5: // Flexible
        return <Badge variant="secondary">{label}</Badge>;
      default:
        return <Badge variant="outline">{label}</Badge>;
    }
  };

  if (isLoading) {
    return (
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{isRtl ? 'الكود' : 'Code'}</TableHead>
              <TableHead>{isRtl ? 'اسم الوردية' : 'Name'}</TableHead>
              <TableHead>{isRtl ? 'نوع الوردية' : 'Type'}</TableHead>
              <TableHead>{isRtl ? 'أوقات العمل' : 'Working Hours'}</TableHead>
              <TableHead>{isRtl ? 'الحالة' : 'Status'}</TableHead>
              <TableHead className="w-[80px]">{isRtl ? 'الإجراءات' : 'Actions'}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {[1, 2, 3, 4, 5].map((i) => (
              <TableRow key={i}>
                <TableCell><div className="h-4 w-16 bg-muted animate-pulse rounded" /></TableCell>
                <TableCell><div className="h-4 w-32 bg-muted animate-pulse rounded" /></TableCell>
                <TableCell><div className="h-5 w-20 bg-muted animate-pulse rounded-full" /></TableCell>
                <TableCell><div className="h-4 w-28 bg-muted animate-pulse rounded" /></TableCell>
                <TableCell><div className="h-5 w-16 bg-muted animate-pulse rounded-full" /></TableCell>
                <TableCell><div className="h-8 w-8 bg-muted animate-pulse rounded" /></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    );
  }

  if (!shifts.length) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-center border rounded-md bg-muted/20">
        <Clock className="h-10 w-10 text-muted-foreground mb-4" />
        <h3 className="text-lg font-medium">{isRtl ? 'لا توجد ورديات عمل' : 'No shifts found'}</h3>
        <p className="text-sm text-muted-foreground mt-1">
          {isRtl ? 'ابدأ بإضافة وردية عمل جديدة للنظام.' : 'Get started by creating a new work shift.'}
        </p>
      </div>
    );
  }

  return (
    <>
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{isRtl ? 'الكود' : 'Code'}</TableHead>
              <TableHead>{isRtl ? 'اسم الوردية' : 'Name'}</TableHead>
              <TableHead>{isRtl ? 'نوع الوردية' : 'Type'}</TableHead>
              <TableHead>{isRtl ? 'أوقات العمل' : 'Working Hours'}</TableHead>
              <TableHead>{isRtl ? 'الحالة' : 'Status'}</TableHead>
              <TableHead className="w-[80px]">{isRtl ? 'الإجراءات' : 'Actions'}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {shifts.map((shift) => (
              <TableRow
                key={shift.id}
                className="cursor-pointer"
                onClick={() => navigate(`/shifts/${shift.id}`)}
              >
                <TableCell className="font-medium font-mono text-xs">{shift.code}</TableCell>
                <TableCell className="font-medium">{shift.name}</TableCell>
                <TableCell onClick={(e) => e.stopPropagation()}>
                  {getShiftTypeBadge(shift.shiftType)}
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-1.5 text-sm text-muted-foreground font-mono">
                    <Clock className="h-3.5 w-3.5" />
                    <span>{shift.startTime} - {shift.endTime}</span>
                  </div>
                </TableCell>
                <TableCell onClick={(e) => e.stopPropagation()}>
                  {shift.isActive ? (
                    <Badge variant="outline" className="bg-emerald-500/10 text-emerald-600 border-emerald-200">
                      {isRtl ? 'نشط' : 'Active'}
                    </Badge>
                  ) : (
                    <Badge variant="outline" className="bg-muted text-muted-foreground">
                      {isRtl ? 'غير نشط' : 'Inactive'}
                    </Badge>
                  )}
                </TableCell>
                <TableCell onClick={(e) => e.stopPropagation()}>
                  <DropdownMenu>
                    <DropdownMenuTrigger className="h-8 w-8 p-0 flex items-center justify-center rounded-md hover:bg-accent hover:text-accent-foreground outline-none ml-auto">
                      <span className="sr-only">Open menu</span>
                      <MoreHorizontal className="h-4 w-4" />
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuLabel>{isRtl ? 'الإجراءات' : 'Actions'}</DropdownMenuLabel>
                      <DropdownMenuItem onClick={() => navigate(`/shifts/${shift.id}`)}>
                        <Eye className="mr-2 h-4 w-4" /> {isRtl ? 'عرض التفاصيل' : 'View Details'}
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => navigate(`/shifts/${shift.id}/edit`)}>
                        <Edit className="mr-2 h-4 w-4" /> {isRtl ? 'تعديل' : 'Edit'}
                      </DropdownMenuItem>
                      <DropdownMenuSeparator />
                      {shift.isActive ? (
                        <DropdownMenuItem onClick={() => { setSelectedShift(shift); setDialogAction('deactivate'); }}>
                          <PowerOff className="mr-2 h-4 w-4" /> {isRtl ? 'إلغاء التفعيل' : 'Deactivate'}
                        </DropdownMenuItem>
                      ) : (
                        <DropdownMenuItem onClick={() => { setSelectedShift(shift); setDialogAction('activate'); }}>
                          <Activity className="mr-2 h-4 w-4" /> {isRtl ? 'تفعيل' : 'Activate'}
                        </DropdownMenuItem>
                      )}
                      <DropdownMenuSeparator />
                      <DropdownMenuItem
                        onClick={() => { setSelectedShift(shift); setDialogAction('delete'); }}
                        className="text-destructive focus:text-destructive"
                      >
                        <Trash className="mr-2 h-4 w-4" /> {isRtl ? 'حذف' : 'Delete'}
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {dialogConfig && (
        <ActionDialog
          isOpen={!!dialogAction}
          onClose={closeDialog}
          onConfirm={handleAction}
          title={dialogConfig.title}
          description={dialogConfig.description}
          confirmText={dialogConfig.confirmText}
          variant={dialogConfig.variant}
          isLoading={dialogConfig.isLoading}
        />
      )}
    </>
  );
};
