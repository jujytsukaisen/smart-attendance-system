import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { EmployeeForm } from '../components';
import { useEmployee, useUpdateEmployee } from '../hooks';

export const EditEmployeePage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: employee, isLoading: isLoadingEmployee, isError, error } = useEmployee(id!);
  const updateMutation = useUpdateEmployee();

  const handleSubmit = async (data: any) => {
    try {
      await updateMutation.mutateAsync({
        ...data,
        employeeId: id!,
      });
      navigate(`/employees/${id}`);
    } catch (error) {
      // Error is handled in the hook
    }
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

  if (isLoadingEmployee) {


    return (
      <div className="flex justify-center p-8">
        <div className="animate-spin h-8 w-8 border-4 border-primary border-t-transparent rounded-full" />
      </div>
    );
  }

  if (!employee) {
    return <div className="p-8 text-center text-muted-foreground">Employee not found</div>;
  }

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="flex items-center gap-4">
        <Button variant="outline" size="icon" onClick={() => navigate(`/employees/${id}`)}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Edit Employee</h1>
          <p className="text-muted-foreground">
            Update information for {employee.firstName} {employee.lastName}
          </p>
        </div>
      </div>

      <div className="bg-card border rounded-lg p-6">
        <EmployeeForm
          initialData={employee}
          onSubmit={handleSubmit}
          isLoading={updateMutation.isPending}
        />
      </div>
    </div>
  );
};
