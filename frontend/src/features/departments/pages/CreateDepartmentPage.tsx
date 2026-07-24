import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { DepartmentForm } from '../components';
import { useCreateDepartment } from '../hooks';
import type { CreateDepartmentCommand } from '../types';

export const CreateDepartmentPage = () => {
  const navigate = useNavigate();
  const createMutation = useCreateDepartment();

  const handleSubmit = async (data: CreateDepartmentCommand) => {
    await createMutation.mutateAsync(data);
    navigate('/departments');
  };

  return (
    <div className="flex flex-col gap-6 p-6 max-w-4xl mx-auto w-full">
      <div className="flex items-center gap-4">
        <Button variant="outline" size="icon" onClick={() => navigate('/departments')}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Create Department</h1>
          <p className="text-muted-foreground mt-1">
            Add a new department to a specific branch.
          </p>
        </div>
      </div>

      <div className="bg-card border rounded-lg p-6">
        <DepartmentForm
          onSubmit={handleSubmit}
          isLoading={createMutation.isPending}
        />
      </div>
    </div>
  );
};
