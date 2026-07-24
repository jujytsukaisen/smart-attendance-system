import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { EmployeeForm } from '../components';
import { useCreateEmployee } from '../hooks';

export const CreateEmployeePage = () => {
  const navigate = useNavigate();
  const createMutation = useCreateEmployee();

  const handleSubmit = async (data: any) => {
    try {
      await createMutation.mutateAsync(data);
      navigate('/employees');
    } catch (error) {
      // Error is handled in the hook
    }
  };

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="flex items-center gap-4">
        <Button variant="outline" size="icon" onClick={() => navigate('/employees')}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Add New Employee</h1>
          <p className="text-muted-foreground">
            Fill in the details to onboard a new employee.
          </p>
        </div>
      </div>

      <div className="bg-card border rounded-lg p-6">
        <EmployeeForm
          onSubmit={handleSubmit}
          isLoading={createMutation.isPending}
        />
      </div>
    </div>
  );
};
