import { useParams, useNavigate } from 'react-router-dom';
import { useCompany, useUpdateCompany } from '../hooks';
import { CompanyForm } from '../components/CompanyForm';
import { Button } from '@/shared/components/ui/button';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { ChevronLeft } from 'lucide-react';
import { toast } from 'sonner';
import type { CreateCompanyCommand, UpdateCompanyCommand } from '../types';

export const EditCompanyPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const { data: company, isLoading, isError, error } = useCompany(id!);
  const updateMutation = useUpdateCompany();

  const handleSubmit = (data: CreateCompanyCommand) => {
    if (!id) return;
    
    const command: UpdateCompanyCommand = {
      ...data,
      companyId: id,
    };

    updateMutation.mutate(command, {
      onSuccess: () => {
        toast.success('Company updated successfully');
        navigate('/companies');
      },
      onError: (err) => {
        toast.error(`Failed to update company: ${err.message}`);
      },
    });
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

  if (isLoading) {

    return (
      <div className="space-y-6 max-w-4xl mx-auto">
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10" />
          <div>
            <Skeleton className="h-8 w-[200px] mb-2" />
            <Skeleton className="h-4 w-[300px]" />
          </div>
        </div>
        <div className="rounded-md border p-6 bg-card space-y-6">
          <div className="grid grid-cols-2 gap-6">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        </div>
      </div>
    );
  }

  if (!company) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-center text-destructive">
        <h2 className="text-xl font-semibold mb-2">Error loading company</h2>
        <p>{'Company not found'}</p>
        <Button className="mt-4" onClick={() => navigate('/companies')}>Back to Companies</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/companies')}>
          <ChevronLeft className="h-5 w-5" />
        </Button>
        <div>
          <h2 className="text-2xl font-bold tracking-tight">Edit Company</h2>
          <p className="text-muted-foreground">Update information for {company.name}</p>
        </div>
      </div>
      
      <div className="rounded-md border p-6 bg-card">
        <CompanyForm 
          initialData={{
            companyId: company.id,
            name: company.name,
            legalName: company.legalName,
            registrationNumber: company.registrationNumber,
            taxNumber: company.taxNumber,
            email: company.email,
            phoneNumber: company.phoneNumber,
            addressStreet: company.addressStreet,
            addressCity: company.addressCity,
            addressState: company.addressState,
            addressCountry: company.addressCountry,
            addressZipCode: company.addressZipCode,
            website: company.website,
            foundedDate: company.foundedDate,
            timeZone: company.timeZone,
            currency: company.currency,
          }}
          onSubmit={handleSubmit} 
          isLoading={updateMutation.isPending} 
        />
      </div>
    </div>
  );
};
