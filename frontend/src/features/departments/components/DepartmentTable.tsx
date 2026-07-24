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

interface DepartmentTableProps {
  departments: DepartmentListDto[];
  isLoading: boolean;
}

export const DepartmentTable = ({ departments, isLoading }: DepartmentTableProps) => {
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
          title: 'Delete Department',
          description: `Are you sure you want to delete ${selectedDepartment?.name}? This action can be undone later by restoring it.`,
          confirmText: 'Delete',
          variant: 'danger' as const,
          isLoading: deleteMutation.isPending,
        };
      case 'restore':
        return {
          title: 'Restore Department',
          description: `Are you sure you want to restore ${selectedDepartment?.name}?`,
          confirmText: 'Restore',
          variant: 'default' as const,
          isLoading: restoreMutation.isPending,
        };
      case 'activate':
        return {
          title: 'Activate Department',
          description: `Are you sure you want to activate ${selectedDepartment?.name}?`,
          confirmText: 'Activate',
          variant: 'default' as const,
          isLoading: activateMutation.isPending,
        };
      case 'deactivate':
        return {
          title: 'Deactivate Department',
          description: `Are you sure you want to deactivate ${selectedDepartment?.name}? Users won't be able to interact with it.`,
          confirmText: 'Deactivate',
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
              <TableHead>Code</TableHead>
              <TableHead>Name</TableHead>
              <TableHead>Branch</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="w-[80px]">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {[1, 2, 3, 4, 5].map((i) => (
              <TableRow key={i}>
                <TableCell><div className="h-4 w-16 bg-muted animate-pulse rounded" /></TableCell>
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
        <h3 className="text-lg font-medium">No departments found</h3>
        <p className="text-sm text-muted-foreground mt-1">
          Get started by creating a new department.
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
              <TableHead>Code</TableHead>
              <TableHead>Name</TableHead>
              <TableHead>Branch</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="w-[80px]">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {departments.map((department) => (
              <TableRow key={department.id} className="cursor-pointer" onClick={() => navigate(`/departments/${department.id}`)}>
                <TableCell className="font-medium">{department.code}</TableCell>
                <TableCell>{department.name}</TableCell>
                <TableCell>{department.branchName}</TableCell>
                <TableCell onClick={(e) => e.stopPropagation()}>
                  <DepartmentStatusBadge isActive={department.isActive} />
                </TableCell>
                <TableCell onClick={(e) => e.stopPropagation()}>
                  <DropdownMenu>
                    <DropdownMenuTrigger className="h-8 w-8 p-0 flex items-center justify-center rounded-md hover:bg-accent hover:text-accent-foreground outline-none ml-auto">
                      <span className="sr-only">Open menu</span>
                      <MoreHorizontal className="h-4 w-4" />
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuLabel>Actions</DropdownMenuLabel>
                      <DropdownMenuItem onClick={() => navigate(`/departments/${department.id}/edit`)}>
                        <Edit className="mr-2 h-4 w-4" /> Edit
                      </DropdownMenuItem>
                      <DropdownMenuSeparator />
                      {department.isActive ? (
                        <DropdownMenuItem onClick={() => { setSelectedDepartment(department); setDialogAction('deactivate'); }}>
                          <PowerOff className="mr-2 h-4 w-4" /> Deactivate
                        </DropdownMenuItem>
                      ) : (
                        <DropdownMenuItem onClick={() => { setSelectedDepartment(department); setDialogAction('activate'); }}>
                          <Activity className="mr-2 h-4 w-4" /> Activate
                        </DropdownMenuItem>
                      )}
                      <DropdownMenuSeparator />
                      <DropdownMenuItem
                        onClick={() => { setSelectedDepartment(department); setDialogAction('delete'); }}
                        className="text-destructive focus:text-destructive"
                      >
                        <Trash className="mr-2 h-4 w-4" /> Delete
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
