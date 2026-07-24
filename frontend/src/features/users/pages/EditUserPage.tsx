import { useParams, useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { UserForm } from '../components';
import { useUserDetails, useUpdateUser } from '../hooks';
import type { UserFormValues } from '../schemas';

export function EditUserPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const { data, isLoading: isLoadingDetails } = useUserDetails(id!);
  const mutation = useUpdateUser();

  const handleSubmit = (values: UserFormValues) => {
    mutation.mutate({
      userId: id!,
      username: values.username,
      email: values.email,
    }, {
      onSuccess: () => {
        toast.success('User updated successfully');
        navigate('/users');
      },
      onError: (error: any) => {
        toast.error(error.response?.data?.title || 'Failed to update user');
      },
    });
  };

  if (isLoadingDetails) {
    return (
      <div className="max-w-3xl mx-auto space-y-6 animate-pulse">
        <div className="h-8 bg-gray-200 rounded w-1/3"></div>
        <div className="h-64 bg-gray-100 rounded w-full"></div>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="text-center py-12">
        <p className="text-red-500">Failed to load user details.</p>
        <button onClick={() => navigate('/users')} className="mt-4 text-blue-500 hover:underline">
          Go back
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-gray-900">Edit User</h1>
        <p className="mt-1 text-sm text-gray-500">
          Update the profile details of this user.
        </p>
      </div>

      <UserForm 
        initialValues={{
          username: data.username,
          email: data.email,
        }} 
        onSubmit={handleSubmit} 
        isLoading={mutation.isPending}
        isEditMode={true}
      />
    </div>
  );
}
