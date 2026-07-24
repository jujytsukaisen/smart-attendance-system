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
import { MoreHorizontal, Edit, Trash, Activity, PowerOff, Users } from 'lucide-react';
import { EmployeeStatusBadge } from './EmployeeStatusBadge';
import { ActionDialog } from './ActionDialog';
import { useDeleteEmployee, useRestoreEmployee, useActivateEmployee, useDeactivateEmployee } from '../hooks';
import type { EmployeeListDto } from '../types';

interface EmployeeTableProps {
  employees: EmployeeListDto[];
  isLoading: boolean;
}

export const EmployeeTable = ({ employees, isLoading }: EmployeeTableProps) => {
  const navigate = useNavigate();
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
          title: 'Delete Employee',
          description: `Are you sure you want to delete ${selectedEmployee?.firstName} ${selectedEmployee?.lastName}? This action can be undone later by restoring.`,
          confirmText: 'Delete',
          variant: 'danger' as const,
          isLoading: deleteMutation.isPending,
        };
      case 'restore':
        return {
          title: 'Restore Employee',
          description: `Are you sure you want to restore ${selectedEmployee?.firstName} ${selectedEmployee?.lastName}?`,
          confirmText: 'Restore',
          variant: 'default' as const,
          isLoading: restoreMutation.isPending,
        };
      case 'activate':
        return {
          title: 'Activate Employee',
          description: `Are you sure you want to activate ${selectedEmployee?.firstName} ${selectedEmployee?.lastName}?`,
          confirmText: 'Activate',
          variant: 'default' as const,
          isLoading: activateMutation.isPending,
        };
      case 'deactivate':
        return {
          title: 'Deactivate Employee',
          description: `Are you sure you want to deactivate ${selectedEmployee?.firstName} ${selectedEmployee?.lastName}? They won't be able to log in.`,
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
              <TableHead>Emp No.</TableHead>
              <TableHead>Name</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Job Title</TableHead>
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
      <div className="flex flex-col items-center justify-center p-8 text-center border rounded-md bg-muted/20">
        <Users className="h-10 w-10 text-muted-foreground mb-4" />
        <h3 className="text-lg font-medium">No employees found</h3>
        <p className="text-sm text-muted-foreground mt-1">
          Get started by adding a new employee.
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
              <TableHead>Emp No.</TableHead>
              <TableHead>Name</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Job Title</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="w-[80px]">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {employees.map((employee) => (
              <TableRow key={employee.id} className="cursor-pointer" onClick={() => navigate(`/employees/${employee.id}`)}>
                <TableCell className="font-medium">{employee.employeeNumber}</TableCell>
                <TableCell>{employee.firstName} {employee.lastName}</TableCell>
                <TableCell>{employee.email}</TableCell>
                <TableCell>{employee.jobTitle}</TableCell>
                <TableCell onClick={(e) => e.stopPropagation()}>
                  <EmployeeStatusBadge status={employee.employeeStatus} />
                </TableCell>
                <TableCell onClick={(e) => e.stopPropagation()}>
                  <DropdownMenu>
                    <DropdownMenuTrigger className="h-8 w-8 p-0 flex items-center justify-center rounded-md hover:bg-accent hover:text-accent-foreground outline-none ml-auto">
                      <span className="sr-only">Open menu</span>
                      <MoreHorizontal className="h-4 w-4" />
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuLabel>Actions</DropdownMenuLabel>
                      <DropdownMenuItem onClick={() => navigate(`/employees/${employee.id}/edit`)}>
                        <Edit className="mr-2 h-4 w-4" /> Edit
                      </DropdownMenuItem>
                      <DropdownMenuSeparator />
                      {employee.employeeStatus === 0 ? (
                        <DropdownMenuItem onClick={() => { setSelectedEmployee(employee); setDialogAction('deactivate'); }}>
                          <PowerOff className="mr-2 h-4 w-4" /> Deactivate
                        </DropdownMenuItem>
                      ) : (
                        <DropdownMenuItem onClick={() => { setSelectedEmployee(employee); setDialogAction('activate'); }}>
                          <Activity className="mr-2 h-4 w-4" /> Activate
                        </DropdownMenuItem>
                      )}
                      <DropdownMenuSeparator />
                      <DropdownMenuItem
                        onClick={() => { setSelectedEmployee(employee); setDialogAction('delete'); }}
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
