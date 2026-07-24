import { useParams, useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { RoleForm, PermissionMatrix } from '../components';
import { useRoleDetails, useUpdateRole, usePermissions, useAssignPermissions } from '../hooks';
import type { RoleFormValues } from '../schemas';
import { Button } from '@/shared/components/ui/button';

export function EditRolePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const { data: role, isLoading: isLoadingDetails } = useRoleDetails(id!);
  const { data: allPermissions, isLoading: isLoadingPermissions } = usePermissions();
  
  const updateMutation = useUpdateRole();
  const assignMutation = useAssignPermissions();

  const handleUpdateRole = (values: RoleFormValues) => {
    updateMutation.mutate({
      roleId: id!,
      ...values,
    }, {
      onSuccess: () => {
        toast.success('Role updated successfully');
      },
      onError: (error: any) => {
        toast.error(error.response?.data?.title || 'Failed to update role');
      },
    });
  };

  const handleAssignPermissions = (permissions: string[]) => {
    assignMutation.mutate({
      roleId: id!,
      permissions,
    }, {
      onSuccess: () => {
        toast.success('Permissions updated successfully');
      },
      onError: (error: any) => {
        toast.error(error.response?.data?.title || 'Failed to update permissions');
      },
    });
  };

  if (isLoadingDetails || isLoadingPermissions) {
    return (
      <div className="max-w-5xl mx-auto space-y-6 animate-pulse">
        <div className="h-8 bg-gray-200 rounded w-1/3"></div>
        <div className="h-64 bg-gray-100 rounded w-full"></div>
      </div>
    );
  }

  if (!role || !allPermissions) {
    return (
      <div className="text-center py-12">
        <p className="text-red-500">Failed to load role details.</p>
        <Button variant="outline" onClick={() => navigate('/roles')} className="mt-4">
          Go back
        </Button>
      </div>
    );
  }

  const selectedPermissionNames = role.permissions?.map(p => p.systemName) || [];

  return (
    <div className="max-w-5xl mx-auto space-y-8 mb-12">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Edit Role</h1>
          <p className="mt-1 text-sm text-gray-500">
            Update the role definition and its assigned permissions.
          </p>
        </div>
        <Button variant="outline" onClick={() => navigate('/roles')}>Done</Button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-1 space-y-6">
          <RoleForm 
            initialValues={{
              name: role.name,
              description: role.description,
            }} 
            onSubmit={handleUpdateRole} 
            isLoading={updateMutation.isPending}
          />
        </div>

        <div className="lg:col-span-2">
          <PermissionMatrix 
            allPermissions={allPermissions}
            selectedPermissions={selectedPermissionNames}
            onChange={handleAssignPermissions}
            isLoading={assignMutation.isPending}
          />
        </div>
      </div>
    </div>
  );
}
