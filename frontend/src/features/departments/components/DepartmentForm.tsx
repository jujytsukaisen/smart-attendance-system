import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Textarea } from '@/shared/components/ui/textarea';
import { 
  Form, 
  FormControl, 
  FormField, 
  FormItem, 
  FormLabel, 
  FormMessage
} from '@/shared/components/ui/form';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { createDepartmentSchema, updateDepartmentSchema } from '../schemas/departmentSchema';
import { useCompanies } from '@/features/companies/hooks';
import { useBranches } from '@/features/branches/hooks';
import type { DepartmentDetailsDto } from '../types';

interface DepartmentFormProps {
  initialData?: DepartmentDetailsDto;
  onSubmit: (data: any) => void;
  isLoading: boolean;
}

export const DepartmentForm = ({ initialData, onSubmit, isLoading }: DepartmentFormProps) => {
  const isEditMode = !!initialData;
  const [selectedCompanyId, setSelectedCompanyId] = useState<string>(initialData?.companyId || '');

  const form = useForm<any>({
    resolver: zodResolver(isEditMode ? updateDepartmentSchema : createDepartmentSchema) as any,
    defaultValues: isEditMode 
      ? {
          departmentId: initialData.id,
          name: initialData.name,
          description: initialData.description || '',
        }
      : {
          companyId: '',
          branchId: '',
          name: '',
          code: '',
          description: '',
        },
  });

  const { data: companiesData, isLoading: isLoadingCompanies } = useCompanies({
    pageNumber: 1,
    pageSize: 100,
    isActive: true,
  });

  const { data: branchesData, isLoading: isLoadingBranches } = useBranches({
    pageNumber: 1,
    pageSize: 100,
    isActive: true,
  });

  const filteredBranches = branchesData?.items.filter(b => b.companyId === selectedCompanyId) || [];

  const companyItems = (companiesData?.items || []).map((c) => ({
    value: c.id,
    label: c.name,
  }));

  const branchItems = filteredBranches.map((b) => ({
    value: b.id,
    label: b.name,
  }));

  const displayCompanyName = isEditMode
    ? initialData.companyName || companiesData?.items.find(c => c.id === initialData.companyId)?.name || initialData.companyId
    : '';

  // When company changes, reset branch
  useEffect(() => {
    if (!isEditMode && selectedCompanyId) {
      const currentBranch = form.getValues('branchId');
      if (currentBranch) {
        form.setValue('branchId', '');
      }
    }
  }, [selectedCompanyId, form, isEditMode]);

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          
          {!isEditMode && (
            <>
              <FormField
                control={form.control}
                name="companyId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Company</FormLabel>
                    <Select 
                      items={companyItems}
                      onValueChange={(val) => {
                        field.onChange(val);
                        setSelectedCompanyId(val);
                      }} 
                      value={field.value} 
                      disabled={isLoadingCompanies}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder={isLoadingCompanies ? "Loading companies..." : "Select a company"} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {companiesData?.items.map((company) => (
                          <SelectItem key={company.id} value={company.id}>
                            {company.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="branchId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Branch</FormLabel>
                    <Select 
                      items={branchItems}
                      onValueChange={field.onChange} 
                      value={field.value} 
                      disabled={!selectedCompanyId || isLoadingBranches}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder={!selectedCompanyId ? "Select a company first" : isLoadingBranches ? "Loading branches..." : "Select a branch"} />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {filteredBranches.map((branch) => (
                          <SelectItem key={branch.id} value={branch.id}>
                            {branch.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </>
          )}

          {isEditMode && (
            <>
              <FormItem>
                <FormLabel>Company</FormLabel>
                <FormControl>
                  <Input value={displayCompanyName} disabled />
                </FormControl>
                <p className="text-xs text-muted-foreground mt-1">Company cannot be changed after creation.</p>
              </FormItem>

              <FormItem>
                <FormLabel>Branch</FormLabel>
                <FormControl>
                  <Input value={initialData.branchName} disabled />
                </FormControl>
                <p className="text-xs text-muted-foreground mt-1">Branch cannot be changed after creation.</p>
              </FormItem>
            </>
          )}

          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Department Name</FormLabel>
                <FormControl>
                  <Input placeholder="Engineering" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {!isEditMode ? (
            <FormField
              control={form.control}
              name="code"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Department Code</FormLabel>
                  <FormControl>
                    <Input placeholder="DEPT-ENG" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          ) : (
            <FormItem>
              <FormLabel>Department Code</FormLabel>
              <FormControl>
                <Input value={initialData.code} disabled />
              </FormControl>
              <p className="text-xs text-muted-foreground mt-1">Code cannot be changed after creation.</p>
            </FormItem>
          )}

          <FormField
            control={form.control}
            name="description"
            render={({ field }) => (
              <FormItem className="col-span-1 md:col-span-2">
                <FormLabel>Description</FormLabel>
                <FormControl>
                  <Textarea 
                    placeholder="Enter description..." 
                    className="resize-none" 
                    {...field} 
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

        </div>
        <div className="flex justify-end gap-4 border-t pt-6">
          <Button type="submit" disabled={isLoading}>
            {isLoading ? 'Saving...' : 'Save Department'}
          </Button>
        </div>
      </form>
    </Form>
  );
};
