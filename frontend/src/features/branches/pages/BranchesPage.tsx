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
import { useTranslation } from '@/shared/i18n';

export const BranchesPage = () => {
  const { t } = useTranslation();
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
        onSuccess: () => toast.success(t('common.success')),
        onError: (err) => toast.error(`${t('common.error')}: ${err.message}`),
      });
    } else {
      restoreMutation.mutate(dialogState.branchId, {
        onSuccess: () => toast.success(t('common.success')),
        onError: (err) => toast.error(`${t('common.error')}: ${err.message}`),
      });
    }
  };

  const handleActivateToggle = (branch: BranchListDto) => {
    if (branch.isActive) {
      deactivateMutation.mutate(branch.id, {
        onSuccess: () => toast.success(t('common.success')),
        onError: (err) => toast.error(`${t('common.error')}: ${err.message}`),
      });
    } else {
      activateMutation.mutate(branch.id, {
        onSuccess: () => toast.success(t('common.success')),
        onError: (err) => toast.error(`${t('common.error')}: ${err.message}`),
      });
    }
  };

  if (isError) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-center text-destructive">
        <h2 className="text-xl font-semibold mb-2">{t('common.error')}</h2>
        <p>{error?.message}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h2 className="text-2xl font-bold tracking-tight">{t('branches.title')}</h2>
          <p className="text-muted-foreground">{t('branches.subtitle')}</p>
        </div>
        <Button onClick={() => navigate('/branches/new')} className="gap-2">
          <PlusCircle className="h-4 w-4" /> {t('branches.addBranch')}
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

      {data && data.pagination.totalPages > 1 && (
        <div className="flex items-center justify-between">
          <div className="text-sm text-muted-foreground">
            {t('attendance.showingPage')} {data.pagination.pageNumber} {t('attendance.of')} {data.pagination.totalPages}
          </div>
          <div className="flex items-center space-x-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => handlePageChange(query.pageNumber - 1)}
              disabled={!data.pagination.hasPreviousPage}
            >
              <ChevronLeft className="h-4 w-4 mr-1 rtl:rotate-180" />
              {t('attendance.previous')}
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => handlePageChange(query.pageNumber + 1)}
              disabled={!data.pagination.hasNextPage}
            >
              {t('attendance.next')}
              <ChevronRight className="h-4 w-4 ml-1 rtl:rotate-180" />
            </Button>
          </div>
        </div>
      )}

      <ActionDialog
        isOpen={dialogState.isOpen}
        onClose={() => setDialogState(prev => ({ ...prev, isOpen: false }))}
        onConfirm={handleConfirmAction}
        title={dialogState.type === 'delete' ? t('branches.deleteTitle') : t('branches.restoreTitle')}
        description={dialogState.type === 'delete' ? t('branches.deleteDesc') : t('branches.restoreDesc')}
        confirmText={dialogState.type === 'delete' ? t('common.delete') : t('employees.restore')}
        variant={dialogState.type === 'delete' ? 'danger' : 'default'}
        isLoading={deleteMutation.isPending || restoreMutation.isPending}
      />
    </div>
  );
};
