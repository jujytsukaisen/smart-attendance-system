import { useState } from 'react';
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
import { MoreHorizontal, Edit, Trash, Activity, PowerOff, Building } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { DepartmentStatusBadge } from './DepartmentStatusBadge';
import { ActionDialog } from './ActionDialog';
import { useDeleteDepartment, useRestoreDepartment, useActivateDepartment, useDeactivateDepartment } from '../hooks';
import type { DepartmentListDto } from '../types';
import { useTranslation } from '@/shared/i18n';

interface DepartmentTableProps {
  departments: DepartmentListDto[];
  isLoading: boolean;
}

export const DepartmentTable = ({ departments, isLoading }: DepartmentTableProps) => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [selectedDepartment, setSelectedDepartment] = useState<DepartmentListDto | null>(null);
  const [dialogAction, setDialogAction] = useState<'delete' | 'restore' | 'activate' | 'deactivate' | null>(null);

  const deleteMutation = useDeleteDepartment();
  const restoreMutation = useRestoreDepartment();
  const activateMutation = useActivateDepartment();
  const deactivateMutation = useDeactivateDepartment();

  const handleAction = async () => {
    if (!selectedDepartment || !dialogAction) return;

    try {
      if (dialogAction === 'delete') await deleteMutation.mutateAsync(selectedDepartment.id);
      if (dialogAction === 'restore') await restoreMutation.mutateAsync(selectedDepartment.id);
      if (dialogAction === 'activate') await activateMutation.mutateAsync(selectedDepartment.id);
      if (dialogAction === 'deactivate') await deactivateMutation.mutateAsync(selectedDepartment.id);
    } finally {
      closeDialog();
    }
  };

  const closeDialog = () => {
    setDialogAction(null);
    setSelectedDepartment(null);
  };

  const getDialogConfig = () => {
    switch (dialogAction) {
      case 'delete':
        return {
          title: t('departments.deleteTitle'),
          description: t('departments.deleteDesc'),
          confirmText: t('common.delete'),
          variant: 'danger' as const,
          isLoading: deleteMutation.isPending,
        };
      case 'restore':
        return {
          title: t('departments.restoreTitle'),
          description: t('departments.restoreDesc'),
          confirmText: t('employees.restore'),
          variant: 'default' as const,
          isLoading: restoreMutation.isPending,
        };
      case 'activate':
        return {
          title: t('departments.activateTitle'),
          description: t('departments.activateDesc'),
          confirmText: t('employees.activate'),
          variant: 'default' as const,
          isLoading: activateMutation.isPending,
        };
      case 'deactivate':
        return {
          title: t('departments.deactivateTitle'),
          description: t('departments.deactivateDesc'),
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
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t('departments.code')}</TableHead>
              <TableHead>{t('departments.name')}</TableHead>
              <TableHead>{t('departments.company')}</TableHead>
              <TableHead>{t('departments.branch')}</TableHead>
              <TableHead>{t('departments.status')}</TableHead>
              <TableHead className="w-[80px] text-center">{t('departments.actions')}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {[1, 2, 3, 4, 5].map((i) => (
              <TableRow key={i}>
                <TableCell><div className="h-4 w-16 bg-muted animate-pulse rounded" /></TableCell>
                <TableCell><div className="h-4 w-32 bg-muted animate-pulse rounded" /></TableCell>
                <TableCell><div className="h-4 w-32 bg-muted animate-pulse rounded" /></TableCell>
                <TableCell><div className="h-4 w-32 bg-muted animate-pulse rounded" /></TableCell>
                <TableCell><div className="h-5 w-16 bg-muted animate-pulse rounded-full" /></TableCell>
                <TableCell><div className="h-8 w-8 bg-muted animate-pulse rounded" /></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    );
  }

  if (!departments.length) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-center border rounded-md bg-muted/20">
        <Building className="h-10 w-10 text-muted-foreground mb-4" />
        <h3 className="text-lg font-medium">{t('departments.noDepartmentsFound')}</h3>
        <p className="text-sm text-muted-foreground mt-1">
          {t('departments.noDepartmentsHint')}
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
              <TableHead>{t('departments.code')}</TableHead>
              <TableHead>{t('departments.name')}</TableHead>
              <TableHead>{t('departments.company')}</TableHead>
              <TableHead>{t('departments.branch')}</TableHead>
              <TableHead>{t('departments.status')}</TableHead>
              <TableHead className="w-[80px] text-center">{t('departments.actions')}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {departments.map((department) => (
              <TableRow key={department.id} className="cursor-pointer" onClick={() => navigate(`/departments/${department.id}`)}>
                <TableCell className="font-medium">{department.code}</TableCell>
                <TableCell>{department.name}</TableCell>
                <TableCell>{department.companyName || '-'}</TableCell>
                <TableCell>{department.branchName}</TableCell>
                <TableCell onClick={(e) => e.stopPropagation()}>
                  <DepartmentStatusBadge isActive={department.isActive} />
                </TableCell>
                <TableCell onClick={(e) => e.stopPropagation()} className="text-center">
                  <DropdownMenu>
                    <DropdownMenuTrigger className="h-8 w-8 p-0 inline-flex items-center justify-center rounded-md hover:bg-accent hover:text-accent-foreground outline-none">
                      <span className="sr-only">{t('common.actions')}</span>
                      <MoreHorizontal className="h-4 w-4" />
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuLabel>{t('departments.actions')}</DropdownMenuLabel>
                      <DropdownMenuItem onClick={() => navigate(`/departments/${department.id}/edit`)}>
                        <Edit className="h-4 w-4 ltr:mr-2 rtl:ml-2" /> {t('common.edit')}
                      </DropdownMenuItem>
                      <DropdownMenuSeparator />
                      {department.isActive ? (
                        <DropdownMenuItem onClick={() => { setSelectedDepartment(department); setDialogAction('deactivate'); }}>
                          <PowerOff className="h-4 w-4 ltr:mr-2 rtl:ml-2" /> {t('employees.deactivate')}
                        </DropdownMenuItem>
                      ) : (
                        <DropdownMenuItem onClick={() => { setSelectedDepartment(department); setDialogAction('activate'); }}>
                          <Activity className="h-4 w-4 ltr:mr-2 rtl:ml-2" /> {t('employees.activate')}
                        </DropdownMenuItem>
                      )}
                      <DropdownMenuSeparator />
                      <DropdownMenuItem
                        onClick={() => { setSelectedDepartment(department); setDialogAction('delete'); }}
                        className="text-destructive focus:text-destructive"
                      >
                        <Trash className="h-4 w-4 ltr:mr-2 rtl:ml-2" /> {t('common.delete')}
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
