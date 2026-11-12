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
import { MoreHorizontal, Edit, Trash, Activity, PowerOff, Users, ScanFace } from 'lucide-react';
import { EmployeeStatusBadge } from './EmployeeStatusBadge';
import { ActionDialog } from './ActionDialog';
import { useDeleteEmployee, useRestoreEmployee, useActivateEmployee, useDeactivateEmployee } from '../hooks';
import type { EmployeeListDto } from '../types';
import { useTranslation } from '@/shared/i18n';

interface EmployeeTableProps {
  employees: EmployeeListDto[];
  isLoading: boolean;
}

export const EmployeeTable = ({ employees, isLoading }: EmployeeTableProps) => {
  const navigate = useNavigate();
  const { t, isRtl } = useTranslation();
  const [selectedEmployee, setSelectedEmployee] = useState<EmployeeListDto | null>(null);
  const [dialogAction, setDialogAction] = useState<'delete' | 'restore' | 'activate' | 'deactivate' | null>(null);

  const deleteMutation = useDeleteEmployee();
  const restoreMutation = useRestoreEmployee();
  const activateMutation = useActivateEmployee();
  const deactivateMutation = useDeactivateEmployee();

  const handleAction = async () => {
    if (!selectedEmployee || !dialogAction) return;

    try {
      if (dialogAction === 'delete') await deleteMutation.mutateAsync(selectedEmployee.id);
      if (dialogAction === 'restore') await restoreMutation.mutateAsync(selectedEmployee.id);
      if (dialogAction === 'activate') await activateMutation.mutateAsync(selectedEmployee.id);
      if (dialogAction === 'deactivate') await deactivateMutation.mutateAsync(selectedEmployee.id);
    } finally {
      closeDialog();
    }
  };

  const closeDialog = () => {
    setDialogAction(null);
    setSelectedEmployee(null);
  };

  const getDialogConfig = () => {
    switch (dialogAction) {
      case 'delete':
        return {
          title: t('employees.deleteEmployee'),
          description: isRtl
            ? `هل أنت متأكد من رغبتك في حذف الموظف ${selectedEmployee?.firstName} ${selectedEmployee?.lastName}؟ يمكن التراجع عن هذا الإجراء لاحقاً.`
            : `Are you sure you want to delete ${selectedEmployee?.firstName} ${selectedEmployee?.lastName}? This action can be undone later by restoring.`,
          confirmText: t('common.delete'),
          variant: 'danger' as const,
          isLoading: deleteMutation.isPending,
        };
      case 'restore':
        return {
          title: t('employees.restoreEmployee'),
          description: isRtl
            ? `هل أنت متأكد من رغبتك في استعادة الموظف ${selectedEmployee?.firstName} ${selectedEmployee?.lastName}؟`
            : `Are you sure you want to restore ${selectedEmployee?.firstName} ${selectedEmployee?.lastName}?`,
          confirmText: t('employees.restore'),
          variant: 'default' as const,
          isLoading: restoreMutation.isPending,
        };
      case 'activate':
        return {
          title: t('employees.activateEmployee'),
          description: isRtl
            ? `هل أنت متأكد من رغبتك في تفعيل الموظف ${selectedEmployee?.firstName} ${selectedEmployee?.lastName}؟`
            : `Are you sure you want to activate ${selectedEmployee?.firstName} ${selectedEmployee?.lastName}?`,
          confirmText: t('employees.activate'),
          variant: 'default' as const,
          isLoading: activateMutation.isPending,
        };
      case 'deactivate':
        return {
          title: t('employees.deactivateEmployee'),
          description: isRtl
            ? `هل أنت متأكد من رغبتك في تعطيل الموظف ${selectedEmployee?.firstName} ${selectedEmployee?.lastName}؟ لن يتمكن من تسجيل الدخول.`
            : `Are you sure you want to deactivate ${selectedEmployee?.firstName} ${selectedEmployee?.lastName}? They won't be able to log in.`,
          confirmText: t('employees.deactivate'),
          variant: 'danger' as const,
          isLoading: deactivateMutation.isPending,
        };
      default:
        return null;
    }
  };

  const dialogConfig = getDialogConfig();

  if (isLoading) {
    return (
      <div className="rounded-md border border-border bg-card">
        <Table>
          <TableHeader className="bg-muted/50">
            <TableRow>
              <TableHead className="text-start">{t('employees.empNo')}</TableHead>
              <TableHead className="text-start">{t('employees.name')}</TableHead>
              <TableHead className="text-start">{t('employees.email')}</TableHead>
              <TableHead className="text-start">{t('employees.jobTitle')}</TableHead>
              <TableHead className="text-start">{t('employees.status')}</TableHead>
              <TableHead className="w-[80px] text-end">{t('common.actions')}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {[1, 2, 3, 4, 5].map((i) => (
              <TableRow key={i}>
                <TableCell><div className="h-4 w-16 bg-muted animate-pulse rounded" /></TableCell>
                <TableCell><div className="h-4 w-32 bg-muted animate-pulse rounded" /></TableCell>
                <TableCell><div className="h-4 w-32 bg-muted animate-pulse rounded" /></TableCell>
                <TableCell><div className="h-4 w-24 bg-muted animate-pulse rounded" /></TableCell>
                <TableCell><div className="h-5 w-16 bg-muted animate-pulse rounded-full" /></TableCell>
                <TableCell><div className="h-8 w-8 bg-muted animate-pulse rounded" /></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    );
  }

  if (!employees.length) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-center border border-border rounded-md bg-card text-card-foreground">
        <Users className="h-10 w-10 text-muted-foreground mb-4" />
        <h3 className="text-lg font-medium">{t('employees.noEmployeesFound')}</h3>
        <p className="text-sm text-muted-foreground mt-1">
          {t('employees.noEmployeesHint')}
        </p>
      </div>
    );
  }

  return (
    <>
      <div className="rounded-md border border-border bg-card text-card-foreground overflow-x-auto shadow-sm">
        <Table>
          <TableHeader className="bg-muted/50">
            <TableRow>
              <TableHead className="text-start">{t('employees.empNo')}</TableHead>
              <TableHead className="text-start">{t('employees.name')}</TableHead>
              <TableHead className="text-start">{t('employees.email')}</TableHead>
              <TableHead className="text-start">{t('employees.jobTitle')}</TableHead>
              <TableHead className="text-start">{t('employees.status')}</TableHead>
              <TableHead className="w-[80px] text-end">{t('common.actions')}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {employees.map((employee) => (
              <TableRow key={employee.id} className="cursor-pointer hover:bg-muted/30 transition-colors" onClick={() => navigate(`/employees/${employee.id}`)}>
                <TableCell className="font-medium font-mono text-foreground">{employee.employeeNumber}</TableCell>
                <TableCell className="font-medium text-foreground">{employee.firstName} {employee.lastName}</TableCell>
                <TableCell className="text-muted-foreground font-mono text-xs">{employee.email}</TableCell>
                <TableCell className="text-muted-foreground">{employee.jobTitle || '—'}</TableCell>
                <TableCell onClick={(e) => e.stopPropagation()}>
                  <EmployeeStatusBadge status={employee.employeeStatus} />
                </TableCell>
                <TableCell onClick={(e) => e.stopPropagation()} className="text-end">
                  <DropdownMenu>
                    <DropdownMenuTrigger className="h-8 w-8 p-0 inline-flex items-center justify-center rounded-md hover:bg-muted text-muted-foreground hover:text-foreground outline-none">
                      <span className="sr-only">{t('common.actions')}</span>
                      <MoreHorizontal className="h-4 w-4" />
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align={isRtl ? 'start' : 'end'}>
                      <DropdownMenuLabel>{t('common.actions')}</DropdownMenuLabel>
                      <DropdownMenuItem onClick={() => navigate(`/employees/${employee.id}`)} className="gap-2 cursor-pointer">
                        <ScanFace className="h-4 w-4 text-primary" /> <span>{isRtl ? 'بصمة الوجه' : 'Face Biometrics'}</span>
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => navigate(`/employees/${employee.id}/edit`)} className="gap-2 cursor-pointer">
                        <Edit className="h-4 w-4" /> <span>{t('common.edit')}</span>
                      </DropdownMenuItem>
                      <DropdownMenuSeparator />
                      {employee.employeeStatus === 'Active' ? (
                        <DropdownMenuItem onClick={() => { setSelectedEmployee(employee); setDialogAction('deactivate'); }} className="gap-2 cursor-pointer">
                          <PowerOff className="h-4 w-4" /> <span>{t('employees.deactivate')}</span>
                        </DropdownMenuItem>
                      ) : (
                        <DropdownMenuItem onClick={() => { setSelectedEmployee(employee); setDialogAction('activate'); }} className="gap-2 cursor-pointer">
                          <Activity className="h-4 w-4" /> <span>{t('employees.activate')}</span>
                        </DropdownMenuItem>
                      )}
                      <DropdownMenuSeparator />
                      <DropdownMenuItem
                        onClick={() => { setSelectedEmployee(employee); setDialogAction('delete'); }}
                        className="text-destructive focus:text-destructive gap-2 cursor-pointer"
                      >
                        <Trash className="h-4 w-4" /> <span>{t('common.delete')}</span>
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