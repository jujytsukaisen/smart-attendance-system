import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { RoleForm } from '../components';
import { useCreateRole } from '../hooks';
import type { RoleFormValues } from '../schemas';

export function CreateRolePage() {
  const navigate = useNavigate();
  const mutation = useCreateRole();

  const handleSubmit = (values: RoleFormValues) => {
    mutation.mutate(values, {
      onSuccess: () => {
        toast.success('Role created successfully');
        navigate('/roles');
      },
      onError: (error: any) => {
        toast.error(error.response?.data?.title || 'Failed to create role');
      },
    });
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-gray-900">Create New Role</h1>
        <p className="mt-1 text-sm text-gray-500">
          Create a new role to map permissions.
        </p>
      </div>

      <RoleForm onSubmit={handleSubmit} isLoading={mutation.isPending} />
    </div>
  );
}
