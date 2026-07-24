import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { UserForm } from '../components';
import { useCreateUser } from '../hooks';
import type { UserFormValues } from '../schemas';

export function CreateUserPage() {
  const navigate = useNavigate();
  const mutation = useCreateUser();

  const handleSubmit = (values: UserFormValues) => {
    mutation.mutate({
      username: values.username,
      email: values.email,
      password: values.password,
    }, {
      onSuccess: () => {
        toast.success('User created successfully');
        navigate('/users');
      },
      onError: (error: any) => {
        toast.error(error.response?.data?.title || 'Failed to create user');
      },
    });
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-gray-900">Create New User</h1>
        <p className="mt-1 text-sm text-gray-500">
          Create an account for a new user with login credentials.
        </p>
      </div>

      <UserForm onSubmit={handleSubmit} isLoading={mutation.isPending} />
    </div>
  );
}
