import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
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
import { createEmployeeSchema, updateEmployeeSchema } from '../schemas/employeeSchema';
import { useCompanies } from '@/features/companies/hooks';
import { useBranches } from '@/features/branches/hooks';
import { useDepartments } from '@/features/departments/hooks';
import type { EmployeeDetailsDto } from '../types';

interface EmployeeFormProps {
  initialData?: EmployeeDetailsDto;
  onSubmit: (data: any) => void;
  isLoading: boolean;
}

export const EmployeeForm = ({ initialData, onSubmit, isLoading }: EmployeeFormProps) => {
  const isEditMode = !!initialData;
  const [selectedCompanyId, setSelectedCompanyId] = useState<string>(initialData?.companyId || '');
  const [selectedBranchId, setSelectedBranchId] = useState<string>(initialData?.branchId || '');

  const form = useForm<any>({
    resolver: zodResolver(isEditMode ? updateEmployeeSchema : createEmployeeSchema) as any,
    defaultValues: isEditMode 
      ? {
          employeeId: initialData.id,
          firstName: initialData.firstName,
          lastName: initialData.lastName,
          middleName: initialData.middleName || '',
          preferredName: initialData.preferredName || '',
          nationalId: initialData.nationalId || '',
          passportNumber: initialData.passportNumber || '',
          dateOfBirth: initialData.dateOfBirth ? new Date(initialData.dateOfBirth).toISOString().split('T')[0] : '',
          gender: initialData.gender,
          email: initialData.email,
          phoneNumber: initialData.phoneNumber,
          street: initialData.street,
          city: initialData.city,
          state: initialData.state,
          zipCode: initialData.zipCode,
          country: initialData.country,
          jobTitle: initialData.jobTitle,
          emergencyContactName: '',
          emergencyContactPhone: '',
          emergencyRelationship: '',
        }
      : {
          companyId: '',
          branchId: '',
          departmentId: '',
          attendancePolicyId: '00000000-0000-0000-0000-000000000000', // Mocked policy ID for now
          employeeNumber: '',
          firstName: '',
          lastName: '',
          middleName: '',
          preferredName: '',
          nationalId: '',
          passportNumber: '',
          email: '',
          phoneNumber: '',
          dateOfBirth: '',
          gender: 0,
          hireDate: '',
          jobTitle: '',
          street: '',
          city: '',
          state: '',
          zipCode: '',
          country: '',
        },
  });

  const { data: companiesData, isLoading: isLoadingCompanies } = useCompanies({ pageNumber: 1, pageSize: 100, isActive: true });
  const { data: branchesData, isLoading: isLoadingBranches } = useBranches({ pageNumber: 1, pageSize: 100, isActive: true });
  const { data: departmentsData, isLoading: isLoadingDepartments } = useDepartments({ pageNumber: 1, pageSize: 100 });

  const filteredBranches = branchesData?.items.filter(b => b.companyId === selectedCompanyId) || [];
  const filteredDepartments = departmentsData?.items.filter(d => d.branchId === selectedBranchId) || [];

  // Reset dependent fields when parent changes
  useEffect(() => {
    if (!isEditMode && selectedCompanyId) {
      form.setValue('branchId', '');
      form.setValue('departmentId', '');
      setSelectedBranchId('');
    }
  }, [selectedCompanyId, form, isEditMode]);

  useEffect(() => {
    if (!isEditMode && selectedBranchId) {
      form.setValue('departmentId', '');
    }
  }, [selectedBranchId, form, isEditMode]);

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
        
        {/* Job Information */}
        <div>
          <h3 className="text-lg font-medium border-b pb-2 mb-4">Job Information</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {!isEditMode && (
              <>
                <FormField
                  control={form.control}
                  name="companyId"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Company</FormLabel>
                      <Select 
                        onValueChange={(val) => {
                          field.onChange(val);
                          setSelectedCompanyId(val);
                        }} 
                        defaultValue={field.value} 
                        disabled={isLoadingCompanies}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder={isLoadingCompanies ? "Loading..." : "Select Company"} />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {companiesData?.items.map((c) => (
                            <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>
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
                        onValueChange={(val) => {
                          field.onChange(val);
                          setSelectedBranchId(val);
                        }} 
                        value={field.value} 
                        disabled={!selectedCompanyId || isLoadingBranches}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder={!selectedCompanyId ? "Select Company first" : "Select Branch"} />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {filteredBranches.map((b) => (
                            <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="departmentId"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Department</FormLabel>
                      <Select 
                        onValueChange={field.onChange} 
                        value={field.value} 
                        disabled={!selectedBranchId || isLoadingDepartments}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder={!selectedBranchId ? "Select Branch first" : "Select Department"} />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {filteredDepartments.map((d) => (
                            <SelectItem key={d.id} value={d.id}>{d.name}</SelectItem>
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
                    <Input value={initialData.companyName || initialData.companyId} disabled />
                  </FormControl>
                </FormItem>
                <FormItem>
                  <FormLabel>Branch</FormLabel>
                  <FormControl>
                    <Input value={initialData.branchName || initialData.branchId} disabled />
                  </FormControl>
                </FormItem>
                <FormItem>
                  <FormLabel>Department</FormLabel>
                  <FormControl>
                    <Input value={initialData.departmentName || initialData.departmentId} disabled />
                  </FormControl>
                </FormItem>
              </>
            )}

            <FormField
              control={form.control}
              name="jobTitle"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Job Title</FormLabel>
                  <FormControl>
                    <Input placeholder="Software Engineer" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {!isEditMode ? (
              <>
                <FormField
                  control={form.control}
                  name="employeeNumber"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Employee Number</FormLabel>
                      <FormControl>
                        <Input placeholder="EMP-001" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="hireDate"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Hire Date</FormLabel>
                      <FormControl>
                        <Input type="date" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </>
            ) : (
              <>
                <FormItem>
                  <FormLabel>Employee Number</FormLabel>
                  <FormControl>
                    <Input value={initialData.employeeNumber} disabled />
                  </FormControl>
                </FormItem>
                <FormItem>
                  <FormLabel>Hire Date</FormLabel>
                  <FormControl>
                    <Input type="date" value={new Date(initialData.hireDate).toISOString().split('T')[0]} disabled />
                  </FormControl>
                </FormItem>
              </>
            )}
          </div>
        </div>

        {/* Personal Information */}
        <div>
          <h3 className="text-lg font-medium border-b pb-2 mb-4">Personal Information</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <FormField
              control={form.control}
              name="firstName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>First Name</FormLabel>
                  <FormControl>
                    <Input placeholder="John" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="lastName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Last Name</FormLabel>
                  <FormControl>
                    <Input placeholder="Doe" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Email</FormLabel>
                  <FormControl>
                    <Input type="email" placeholder="john.doe@example.com" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="phoneNumber"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Phone Number</FormLabel>
                  <FormControl>
                    <Input placeholder="+1 234 567 890" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="dateOfBirth"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Date of Birth</FormLabel>
                  <FormControl>
                    <Input type="date" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="gender"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Gender</FormLabel>
                  <Select 
                    onValueChange={(v) => field.onChange(parseInt(v))} 
                    value={field.value?.toString()}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Select Gender" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectItem value="0">Male</SelectItem>
                      <SelectItem value="1">Female</SelectItem>
                      <SelectItem value="2">Other</SelectItem>
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="nationalId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>National ID (Optional)</FormLabel>
                  <FormControl>
                    <Input {...field} value={field.value || ''} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>
        </div>

        {/* Address */}
        <div>
          <h3 className="text-lg font-medium border-b pb-2 mb-4">Address</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <FormField
              control={form.control}
              name="street"
              render={({ field }) => (
                <FormItem className="md:col-span-2">
                  <FormLabel>Street Address</FormLabel>
                  <FormControl>
                    <Input placeholder="123 Main St" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="city"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>City</FormLabel>
                  <FormControl>
                    <Input placeholder="New York" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="state"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>State/Province</FormLabel>
                  <FormControl>
                    <Input placeholder="NY" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="zipCode"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Zip/Postal Code</FormLabel>
                  <FormControl>
                    <Input placeholder="10001" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="country"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Country</FormLabel>
                  <FormControl>
                    <Input placeholder="USA" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>
        </div>

        {/* Emergency Contact (Edit Mode Only for now based on Backend) */}
        {isEditMode && (
          <div>
            <h3 className="text-lg font-medium border-b pb-2 mb-4">Emergency Contact</h3>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <FormField
                control={form.control}
                name="emergencyContactName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Contact Name</FormLabel>
                    <FormControl>
                      <Input {...field} value={field.value || ''} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="emergencyContactPhone"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Contact Phone</FormLabel>
                    <FormControl>
                      <Input {...field} value={field.value || ''} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="emergencyRelationship"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Relationship</FormLabel>
                    <FormControl>
                      <Input {...field} value={field.value || ''} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
          </div>
        )}

        <div className="flex justify-end gap-4 border-t pt-6">
          <Button type="submit" disabled={isLoading}>
            {isLoading ? 'Saving...' : 'Save Employee'}
          </Button>
        </div>
      </form>
    </Form>
  );
};
