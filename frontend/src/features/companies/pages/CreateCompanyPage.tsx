import { useNavigate } from 'react-router-dom';
import { useCreateCompany } from '../hooks';
import { CompanyForm } from '../components/CompanyForm';
import { Button } from '@/shared/components/ui/button';
import { ChevronLeft } from 'lucide-react';
import { toast } from 'sonner';
import type { CreateCompanyCommand } from '../types';

export const CreateCompanyPage = () => {
  const navigate = useNavigate();
  const createMutation = useCreateCompany();

  const handleSubmit = (data: CreateCompanyCommand) => {
    createMutation.mutate(data, {
      onSuccess: () => {
        toast.success('Company created successfully');
        navigate('/companies');
      },
      onError: (err) => {
        toast.error(`Failed to create company: ${err.message}`);
      },
    });
  };

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/companies')}>
          <ChevronLeft className="h-5 w-5" />
        </Button>
        <div>
          <h2 className="text-2xl font-bold tracking-tight">Create Company</h2>
          <p className="text-muted-foreground">Add a new company to the system.</p>
        </div>
      </div>
      
      <div className="rounded-md border p-6 bg-card">
        <CompanyForm 
          onSubmit={handleSubmit} 
          isLoading={createMutation.isPending} 
        />
      </div>
    </div>
  );
};
