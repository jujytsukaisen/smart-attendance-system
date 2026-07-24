import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCompanies, useDeleteCompany, useRestoreCompany, useActivateCompany, useDeactivateCompany } from '../hooks';
import { CompanyTable } from '../components/CompanyTable';
import { CompanyFilters } from '../components/CompanyFilters';
import { ActionDialog } from '../components/ActionDialog';
import { Button } from '@/shared/components/ui/button';
import { PlusCircle, ChevronLeft, ChevronRight } from 'lucide-react';
import { toast } from 'sonner';
import type { CompanyListDto, GetCompaniesQuery } from '../types';

export const CompaniesPage = () => {
  const navigate = useNavigate();
  const [query, setQuery] = useState<GetCompaniesQuery>({
    pageNumber: 1,
    pageSize: 10,
    name: '',
    isActive: undefined,
  });

  const { data, isLoading, isError, error } = useCompanies(query);
  const deleteMutation = useDeleteCompany();
  const restoreMutation = useRestoreCompany();
  const activateMutation = useActivateCompany();
  const deactivateMutation = useDeactivateCompany();

  const [dialogState, setDialogState] = useState<{
    isOpen: boolean;
    type: 'delete' | 'restore';
    companyId: string | null;
  }>({
    isOpen: false,
    type: 'delete',
    companyId: null,
  });

  const handleSearchChange = (value: string) => {
    setQuery(prev => ({ ...prev, name: value, pageNumber: 1 }));
  };

  const handleStatusChange = (value: string) => {
    const isActive = value === 'all' ? undefined : value === 'active';
    setQuery(prev => ({ ...prev, isActive, pageNumber: 1 }));
  };

  const handlePageChange = (newPage: number) => {
    setQuery(prev => ({ ...prev, pageNumber: newPage }));
  };

  const openDialog = (type: 'delete' | 'restore', id: string) => {
    setDialogState({ isOpen: true, type, companyId: id });
  };

  const handleConfirmAction = () => {
    if (!dialogState.companyId) return;
    
    if (dialogState.type === 'delete') {
      deleteMutation.mutate(dialogState.companyId, {
        onSuccess: () => toast.success('Company deleted successfully.'),
        onError: (err) => toast.error(`Failed to delete: ${err.message}`),
      });
    } else {
      restoreMutation.mutate(dialogState.companyId, {
        onSuccess: () => toast.success('Company restored successfully.'),
        onError: (err) => toast.error(`Failed to restore: ${err.message}`),
      });
    }
  };

  const handleActivateToggle = (company: CompanyListDto) => {
    if (company.isActive) {
      deactivateMutation.mutate(company.id, {
        onSuccess: () => toast.success('Company deactivated.'),
        onError: (err) => toast.error(`Failed to deactivate: ${err.message}`),
      });
    } else {
      activateMutation.mutate(company.id, {
        onSuccess: () => toast.success('Company activated.'),
        onError: (err) => toast.error(`Failed to activate: ${err.message}`),
      });
    }
  };

  if (isError) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-center text-destructive">
        <h2 className="text-xl font-semibold mb-2">Error loading companies</h2>
        <p>{error?.message}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h2 className="text-2xl font-bold tracking-tight">Companies</h2>
          <p className="text-muted-foreground">Manage all companies and subsidiaries.</p>
        </div>
        <Button onClick={() => navigate('/companies/new')}>
          <PlusCircle className="mr-2 h-4 w-4" />
          Add Company
        </Button>
      </div>

      <CompanyFilters
        searchQuery={query.name || ''}
        onSearchChange={handleSearchChange}
        statusFilter={query.isActive === undefined ? 'all' : query.isActive ? 'active' : 'inactive'}
        onStatusChange={handleStatusChange}
      />

      <CompanyTable
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
        title={dialogState.type === 'delete' ? 'Delete Company' : 'Restore Company'}
        description={
          dialogState.type === 'delete'
            ? 'Are you sure you want to delete this company? This action may impact associated branches and employees.'
            : 'Are you sure you want to restore this company?'
        }
        confirmText={dialogState.type === 'delete' ? 'Delete' : 'Restore'}
        isDestructive={dialogState.type === 'delete'}
      />
    </div>
  );
};
