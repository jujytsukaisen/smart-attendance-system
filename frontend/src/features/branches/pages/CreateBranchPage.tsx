import { useNavigate } from 'react-router-dom';
import { useCreateBranch } from '../hooks';
import { BranchForm } from '../components/BranchForm';
import { Button } from '@/shared/components/ui/button';
import { ChevronLeft } from 'lucide-react';
import { toast } from 'sonner';
import type { CreateBranchCommand } from '../types';

export const CreateBranchPage = () => {
  const navigate = useNavigate();
  const createMutation = useCreateBranch();

  const handleSubmit = (data: CreateBranchCommand) => {
    createMutation.mutate(data, {
      onSuccess: () => {
        toast.success('Branch created successfully');
        navigate('/branches');
      },
      onError: (err) => {
        toast.error(`Failed to create branch: ${err.message}`);
      },
    });
  };

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/branches')}>
          <ChevronLeft className="h-5 w-5" />
        </Button>
        <div>
          <h2 className="text-2xl font-bold tracking-tight">Create Branch</h2>
          <p className="text-muted-foreground">Add a new branch to the system.</p>
        </div>
      </div>
      
      <div className="rounded-md border p-6 bg-card">
        <BranchForm 
          onSubmit={handleSubmit} 
          isLoading={createMutation.isPending} 
        />
      </div>
    </div>
  );
};
