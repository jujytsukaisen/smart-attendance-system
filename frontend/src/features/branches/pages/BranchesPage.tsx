import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useBranches, useDeleteBranch, useRestoreBranch, useActivateBranch, useDeactivateBranch } from '../hooks';
import { BranchTable } from '../components/BranchTable';
import { BranchFilters } from '../components/BranchFilters';
import { ActionDialog } from '../components/ActionDialog';
import { Button } from '@/shared/components/ui/button';
import { PlusCircle, ChevronLeft, ChevronRight } from 'lucide-react';
import { toast } from 'sonner';
import type { BranchListDto, GetBranchesQuery } from '../types';

export const BranchesPage = () => {
  const navigate = useNavigate();
  const [query, setQuery] = useState<GetBranchesQuery>({
    pageNumber: 1,
    pageSize: 10,
    name: '',
    code: '',
    isActive: undefined,
  });

  const { data, isLoading, isError, error } = useBranches(query);
  const deleteMutation = useDeleteBranch();
  const restoreMutation = useRestoreBranch();
  const activateMutation = useActivateBranch();
  const deactivateMutation = useDeactivateBranch();

  const [dialogState, setDialogState] = useState<{
    isOpen: boolean;
    type: 'delete' | 'restore';
    branchId: string | null;
  }>({
    isOpen: false,
    type: 'delete',
    branchId: null,
  });

  const handleSearchChange = (value: string) => {
    setQuery(prev => ({ ...prev, name: value, code: value, pageNumber: 1 }));
  };

  const handleStatusChange = (value: string) => {
    const isActive = value === 'all' ? undefined : value === 'active';
    setQuery(prev => ({ ...prev, isActive, pageNumber: 1 }));
  };

  const handlePageChange = (newPage: number) => {
    setQuery(prev => ({ ...prev, pageNumber: newPage }));
  };

  const openDialog = (type: 'delete' | 'restore', id: string) => {
    setDialogState({ isOpen: true, type, branchId: id });
  };

  const handleConfirmAction = () => {
    if (!dialogState.branchId) return;
    
    if (dialogState.type === 'delete') {
      deleteMutation.mutate(dialogState.branchId, {
        onSuccess: () => toast.success('Branch deleted successfully.'),
        onError: (err) => toast.error(`Failed to delete: ${err.message}`),
      });
    } else {
      restoreMutation.mutate(dialogState.branchId, {
        onSuccess: () => toast.success('Branch restored successfully.'),
        onError: (err) => toast.error(`Failed to restore: ${err.message}`),
      });
    }
  };

  const handleActivateToggle = (branch: BranchListDto) => {
    if (branch.isActive) {
      deactivateMutation.mutate(branch.id, {
        onSuccess: () => toast.success('Branch deactivated.'),
        onError: (err) => toast.error(`Failed to deactivate: ${err.message}`),
      });
    } else {
      activateMutation.mutate(branch.id, {
        onSuccess: () => toast.success('Branch activated.'),
        onError: (err) => toast.error(`Failed to activate: ${err.message}`),
      });
    }
  };

  if (isError) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-center text-destructive">
        <h2 className="text-xl font-semibold mb-2">Error loading branches</h2>
        <p>{error?.message}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h2 className="text-2xl font-bold tracking-tight">Branches</h2>
          <p className="text-muted-foreground">Manage branches for your companies.</p>
        </div>
        <Button onClick={() => navigate('/branches/new')}>
          <PlusCircle className="mr-2 h-4 w-4" />
          Add Branch
        </Button>
      </div>

      <BranchFilters
        searchQuery={query.name || ''}
        onSearchChange={handleSearchChange}
        statusFilter={query.isActive === undefined ? 'all' : query.isActive ? 'active' : 'inactive'}
        onStatusChange={handleStatusChange}
      />

      <BranchTable
        data={data?.items || []}
        isLoading={isLoading}
        onDeleteClick={(id) => openDialog('delete', id)}
        onRestoreClick={(id) => openDialog('restore', id)}
        onActivateToggle={handleActivateToggle}
      />

      {data?.pagination && (
        <div className="flex items-center justify-between px-2">
          <p className="text-sm text-muted-foreground">
            Showing page {data.pagination.pageNumber} of {data.pagination.totalPages} ({data.pagination.totalCount} total)
          </p>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={!data.pagination.hasPreviousPage}
              onClick={() => handlePageChange(data.pagination.pageNumber - 1)}
            >
              <ChevronLeft className="h-4 w-4 mr-1" /> Previous
            </Button>
            <Button
              variant="outline"
              size="sm"
              disabled={!data.pagination.hasNextPage}
              onClick={() => handlePageChange(data.pagination.pageNumber + 1)}
            >
              Next <ChevronRight className="h-4 w-4 ml-1" />
            </Button>
          </div>
        </div>
      )}

      <ActionDialog
        isOpen={dialogState.isOpen}
        onOpenChange={(isOpen) => setDialogState(prev => ({ ...prev, isOpen }))}
        onConfirm={handleConfirmAction}
        title={dialogState.type === 'delete' ? 'Delete Branch' : 'Restore Branch'}
        description={
          dialogState.type === 'delete'
            ? 'Are you sure you want to delete this branch? This action may impact associated departments and employees.'
            : 'Are you sure you want to restore this branch?'
        }
        confirmText={dialogState.type === 'delete' ? 'Delete' : 'Restore'}
        isDestructive={dialogState.type === 'delete'}
      />
    </div>
  );
};
