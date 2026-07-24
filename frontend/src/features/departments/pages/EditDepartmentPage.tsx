import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { DepartmentForm } from '../components';
import { useDepartment, useUpdateDepartment } from '../hooks';
import type { UpdateDepartmentCommand } from '../types';

export const EditDepartmentPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: department, isLoading: isLoadingDepartment, isError, error } = useDepartment(id!);
  const updateMutation = useUpdateDepartment();

  const handleSubmit = async (data: UpdateDepartmentCommand) => {
    await updateMutation.mutateAsync(data);
    navigate('/departments');
  };

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

  if (isLoadingDepartment) {


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
    <div className="flex flex-col gap-6 p-6 max-w-4xl mx-auto w-full">
      <div className="flex items-center gap-4">
        <Button variant="outline" size="icon" onClick={() => navigate('/departments')}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Edit Department</h1>
          <p className="text-muted-foreground mt-1">
            Update {department.name}'s information.
          </p>
        </div>
      </div>

      <div className="bg-card border rounded-lg p-6">
        <DepartmentForm
          initialData={department}
          onSubmit={handleSubmit}
          isLoading={updateMutation.isPending}
        />
      </div>
    </div>
  );
};
