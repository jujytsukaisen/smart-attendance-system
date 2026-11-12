import { useParams, useNavigate } from 'react-router-dom';
import { useBranch, useUpdateBranch } from '../hooks';
import { BranchForm } from '../components/BranchForm';
import { Button } from '@/shared/components/ui/button';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { ChevronLeft } from 'lucide-react';
import { toast } from 'sonner';
import type { CreateBranchCommand, UpdateBranchCommand } from '../types';

export const EditBranchPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const { data: branch, isLoading, isError, error } = useBranch(id!);
  const updateMutation = useUpdateBranch();

  const handleSubmit = (data: CreateBranchCommand) => {
    if (!id) return;
    
    const command: UpdateBranchCommand = {
      ...data,
      branchId: id,
    };

    updateMutation.mutate(command, {
      onSuccess: () => {
        toast.success('Branch updated successfully');
        navigate('/branches');
      },
      onError: (err) => {
        toast.error(`Failed to update branch: ${err.message}`);
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

  if (!branch) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-center text-destructive">
        <h2 className="text-xl font-semibold mb-2">Error loading branch</h2>
        <p>{'Branch not found'}</p>
        <Button className="mt-4" onClick={() => navigate('/branches')}>Back to Branches</Button>
      </div>
    );
  }

  // We now receive individual address fields from the backend
  const street = branch.addressStreet || '';
  const city = branch.addressCity || '';
  const state = branch.addressState || '';
  const country = branch.addressCountry || '';
  const zip = branch.addressZipCode || '';

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/branches')}>
          <ChevronLeft className="h-5 w-5" />
        </Button>
        <div>
          <h2 className="text-2xl font-bold tracking-tight">Edit Branch</h2>
          <p className="text-muted-foreground">Update information for {branch.name}</p>
        </div>
      </div>
      
      <div className="rounded-md border p-6 bg-card">
        <BranchForm 
          initialData={{
            companyId: branch.companyId,
            name: branch.name,
            code: branch.code,
            addressStreet: street,
            addressCity: city,
            addressState: state,
            addressCountry: country,
            addressZipCode: zip,
            phoneNumber: branch.phoneNumber,
            email: branch.email,
            latitude: branch.latitude,
            longitude: branch.longitude,
            allowedRadiusMeters: branch.radiusInMeters,
            timeZone: branch.timeZone,
            isHeadOffice: branch.isHeadOffice,
            branchId: branch.id,
          }}
          onSubmit={handleSubmit} 
          isLoading={updateMutation.isPending} 
        />
      </div>
    </div>
  );
};
