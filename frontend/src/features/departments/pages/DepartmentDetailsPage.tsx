import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, Building2, Calendar, FileText, Hash, MapPin, Edit, Activity, PowerOff, Trash } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { useDepartment, useActivateDepartment, useDeactivateDepartment, useDeleteDepartment } from '../hooks';
import { DepartmentStatusBadge } from '../components/DepartmentStatusBadge';
import { ActionDialog } from '../components/ActionDialog';
import { useState } from 'react';

export const DepartmentDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: department, isLoading, isError, error } = useDepartment(id!);

  const [dialogAction, setDialogAction] = useState<'delete' | 'activate' | 'deactivate' | null>(null);

  const deleteMutation = useDeleteDepartment();
  const activateMutation = useActivateDepartment();
  const deactivateMutation = useDeactivateDepartment();

  const handleAction = async () => {
    if (!department || !dialogAction) return;

    try {
      if (dialogAction === 'delete') {
        await deleteMutation.mutateAsync(department.id);
        navigate('/departments');
      }
      if (dialogAction === 'activate') await activateMutation.mutateAsync(department.id);
      if (dialogAction === 'deactivate') await deactivateMutation.mutateAsync(department.id);
    } finally {
      closeDialog();
    }
  };

  const closeDialog = () => setDialogAction(null);

  if (isError) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {error instanceof Error && !error.message.toLowerCase().includes('internal server error') && !error.message.includes('status code')
            ? error.message
            : "Failed to load"}
        </p>
      </div>
    );
  }

  if (isLoading) {


    return (
      <div className="flex items-center justify-center h-[50vh]">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (!department) {
    return (
      <div className="flex flex-col items-center justify-center h-[50vh] gap-4">
        <h2 className="text-2xl font-bold">Department not found</h2>
        <Button onClick={() => navigate('/departments')}>Back to Departments</Button>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6 p-6 max-w-5xl mx-auto w-full">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div className="flex items-center gap-4">
          <Button variant="outline" size="icon" onClick={() => navigate('/departments')}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-3xl font-bold tracking-tight">{department.name}</h1>
              <DepartmentStatusBadge isActive={department.isActive} />
            </div>
            <p className="text-muted-foreground mt-1 flex items-center gap-2">
              <Building2 className="h-4 w-4" />
              {department.branchName}
            </p>
          </div>
        </div>

        <div className="flex gap-2">
          <Button variant="outline" onClick={() => navigate(`/departments/${department.id}/edit`)}>
            <Edit className="mr-2 h-4 w-4" /> Edit
          </Button>
          {department.isActive ? (
            <Button variant="outline" onClick={() => setDialogAction('deactivate')}>
              <PowerOff className="mr-2 h-4 w-4" /> Deactivate
            </Button>
          ) : (
            <Button variant="outline" onClick={() => setDialogAction('activate')}>
              <Activity className="mr-2 h-4 w-4" /> Activate
            </Button>
          )}
          <Button variant="destructive" onClick={() => setDialogAction('delete')}>
            <Trash className="mr-2 h-4 w-4" /> Delete
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-card border rounded-lg p-6 flex flex-col gap-4">
          <h3 className="text-lg font-semibold flex items-center gap-2">
            <FileText className="h-5 w-5 text-muted-foreground" />
            General Information
          </h3>
          <div className="grid grid-cols-2 gap-y-4 text-sm">
            <div>
              <p className="text-muted-foreground mb-1">Code</p>
              <p className="font-medium flex items-center gap-1">
                <Hash className="h-3 w-3" /> {department.code}
              </p>
            </div>
            <div>
              <p className="text-muted-foreground mb-1">Branch</p>
              <p className="font-medium flex items-center gap-1">
                <MapPin className="h-3 w-3" /> {department.branchName}
              </p>
            </div>
            <div className="col-span-2">
              <p className="text-muted-foreground mb-1">Description</p>
              <p className="font-medium">{department.description || 'No description provided.'}</p>
            </div>
          </div>
        </div>

        <div className="bg-card border rounded-lg p-6 flex flex-col gap-4">
          <h3 className="text-lg font-semibold flex items-center gap-2">
            <Calendar className="h-5 w-5 text-muted-foreground" />
            System Metadata
          </h3>
          <div className="grid grid-cols-1 gap-y-4 text-sm">
            <div>
              <p className="text-muted-foreground mb-1">Created At</p>
              <p className="font-medium">{new Date(department.createdAt).toLocaleString()}</p>
            </div>
            <div>
              <p className="text-muted-foreground mb-1">Last Updated</p>
              <p className="font-medium">{department.updatedAt ? new Date(department.updatedAt).toLocaleString() : 'Never'}</p>
            </div>
          </div>
        </div>
      </div>

      <ActionDialog
        isOpen={!!dialogAction}
        onClose={closeDialog}
        onConfirm={handleAction}
        title={
          dialogAction === 'delete' ? 'Delete Department' :
            dialogAction === 'activate' ? 'Activate Department' :
              'Deactivate Department'
        }
        description={
          dialogAction === 'delete' ? 'Are you sure you want to delete this department?' :
            dialogAction === 'activate' ? 'Are you sure you want to activate this department?' :
              'Are you sure you want to deactivate this department?'
        }
        variant={dialogAction === 'delete' || dialogAction === 'deactivate' ? 'danger' : 'default'}
        isLoading={
          deleteMutation.isPending ||
          activateMutation.isPending ||
          deactivateMutation.isPending
        }
      />
    </div>
  );
};
