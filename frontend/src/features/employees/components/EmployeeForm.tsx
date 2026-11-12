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
import { useAttendancePolicies } from '@/features/attendance-policies/hooks';
import type { EmployeeDetailsDto } from '../types';
import { useTranslation } from '@/shared/i18n';

interface EmployeeFormProps {
  initialData?: EmployeeDetailsDto;
  onSubmit: (data: any) => void;
  isLoading: boolean;
}

export const EmployeeForm = ({ initialData, onSubmit, isLoading }: EmployeeFormProps) => {
  const { t, isRtl } = useTranslation();
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
        attendancePolicyId: '',
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
        gender: 1,
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
  const { data: attendancePoliciesData, isLoading: isLoadingAttendancePolicies } = useAttendancePolicies({ pageNumber: 1, pageSize: 100, isActive: true });

  const filteredBranches = branchesData?.items.filter(b => b.companyId === selectedCompanyId) || [];
  const filteredDepartments = departmentsData?.items.filter(d => d.branchId === selectedBranchId) || [];
  const filteredAttendancePolicies = attendancePoliciesData?.items.filter(p => p.companyId === selectedCompanyId) || [];

  const companyItems = (companiesData?.items || []).map((c) => ({
    value: c.id,
    label: c.name,
  }));

  const branchItems = filteredBranches.map((b) => ({
    value: b.id,
    label: b.name,
  }));

  const departmentItems = filteredDepartments.map((d) => ({
    value: d.id,
    label: d.name,
  }));

  const attendancePolicyItems = filteredAttendancePolicies.map((p) => ({
    value: p.id,
    label: p.name,
  }));

  const genderItems = [
    { value: "1", label: isRtl ? "ذكر" : "Male" },
    { value: "2", label: isRtl ? "أنثى" : "Female" },
    { value: "3", label: isRtl ? "أخرى" : "Other" },
    { value: "4", label: isRtl ? "أفضل عدم التحديد" : "Prefer not to say" },
  ];

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
          <h3 className="text-lg font-medium border-b border-border pb-2 mb-4 text-foreground">
            {isRtl ? 'المعلومات الوظيفية' : 'Job Information'}
          </h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {!isEditMode && (
              <>
                <FormField
                  control={form.control}
                  name="companyId"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{isRtl ? 'الشركة' : 'Company'}</FormLabel>
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
                            <SelectValue placeholder={isLoadingCompanies ? (isRtl ? "جاري التحميل..." : "Loading...") : (isRtl ? "اختر الشركة" : "Select Company")} />
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
                      <FormLabel>{isRtl ? 'الفرع' : 'Branch'}</FormLabel>
                      <Select
                        items={branchItems}
                        onValueChange={(val) => {
                          field.onChange(val);
                          setSelectedBranchId(val);
                        }}
                        value={field.value}
                        disabled={!selectedCompanyId || isLoadingBranches}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder={!selectedCompanyId ? (isRtl ? "اختر الشركة أولاً" : "Select Company first") : (isRtl ? "اختر الفرع" : "Select Branch")} />
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
                      <FormLabel>{t('employees.department')}</FormLabel>
                      <Select
                        items={departmentItems}
                        onValueChange={field.onChange}
                        value={field.value}
                        disabled={!selectedBranchId || isLoadingDepartments}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder={!selectedBranchId ? (isRtl ? "اختر الفرع أولاً" : "Select Branch first") : (isRtl ? "اختر القسم" : "Select Department")} />
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

                <FormField
                  control={form.control}
                  name="attendancePolicyId"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>{isRtl ? 'سياسة الحضور' : 'Attendance Policy'}</FormLabel>
                      <Select
                        items={attendancePolicyItems}
                        onValueChange={field.onChange}
                        value={field.value}
                        disabled={!selectedCompanyId || isLoadingAttendancePolicies}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder={!selectedCompanyId ? (isRtl ? "اختر الشركة أولاً" : "Select Company first") : (isRtl ? "اختر سياسة الحضور" : "Select Attendance Policy")} />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {filteredAttendancePolicies.map((p) => (
                            <SelectItem key={p.id} value={p.id}>{p.name}</SelectItem>
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
                  <FormLabel>{isRtl ? 'الشركة' : 'Company'}</FormLabel>
                  <FormControl>
                    <Input value={initialData.companyName || initialData.companyId} disabled className="bg-muted" />
                  </FormControl>
                </FormItem>
                <FormItem>
                  <FormLabel>{isRtl ? 'الفرع' : 'Branch'}</FormLabel>
                  <FormControl>
                    <Input value={initialData.branchName || initialData.branchId} disabled className="bg-muted" />
                  </FormControl>
                </FormItem>
                <FormItem>
                  <FormLabel>{t('employees.department')}</FormLabel>
                  <FormControl>
                    <Input value={initialData.departmentName || initialData.departmentId} disabled className="bg-muted" />
                  </FormControl>
                </FormItem>
              </>
            )}

            <FormField
              control={form.control}
              name="jobTitle"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t('employees.jobTitle')}</FormLabel>
                  <FormControl>
                    <Input placeholder={isRtl ? "مهندس برمجيات" : "Software Engineer"} {...field} />
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
                      <FormLabel>{t('employees.empNo')}</FormLabel>
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
                      <FormLabel>{t('employees.hireDate')}</FormLabel>
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
                  <FormLabel>{t('employees.empNo')}</FormLabel>
                  <FormControl>
                    <Input value={initialData.employeeNumber} disabled className="bg-muted" />
                  </FormControl>
                </FormItem>
                <FormItem>
                  <FormLabel>{t('employees.hireDate')}</FormLabel>
                  <FormControl>
                    <Input type="date" value={new Date(initialData.hireDate).toISOString().split('T')[0]} disabled className="bg-muted" />
                  </FormControl>
                </FormItem>
              </>
            )}
          </div>
        </div>

        {/* Personal Information */}
        <div>
          <h3 className="text-lg font-medium border-b border-border pb-2 mb-4 text-foreground">
            {isRtl ? 'المعلومات الشخصية' : 'Personal Information'}
          </h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <FormField
              control={form.control}
              name="firstName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>{t('employees.firstName')}</FormLabel>
                  <FormControl>
                    <Input placeholder={isRtl ? "محمد" : "John"} {...field} />
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
                  <FormLabel>{t('employees.lastName')}</FormLabel>
                  <FormControl>
                    <Input placeholder={isRtl ? "علي" : "Doe"} {...field} />
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
                  <FormLabel>{t('employees.email')}</FormLabel>
                  <FormControl>
                    <Input type="email" placeholder="example@company.com" {...field} />
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
                  <FormLabel>{t('employees.phone')}</FormLabel>
                  <FormControl>
                    <Input placeholder="+966 50 000 0000" {...field} />
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
                  <FormLabel>{isRtl ? 'تاريخ الميلاد' : 'Date of Birth'}</FormLabel>
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
                  <FormLabel>{isRtl ? 'الجنس' : 'Gender'}</FormLabel>
                  <Select
                    items={genderItems}
                    onValueChange={(v) => field.onChange(parseInt(v))}
                    value={field.value?.toString()}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder={isRtl ? "اختر الجنس" : "Select Gender"} />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {genderItems.map((g) => (
                        <SelectItem key={g.value} value={g.value}>{g.label}</SelectItem>
                      ))}
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
                  <FormLabel>{isRtl ? 'الهوية الوطنية / الإقامة (اختياري)' : 'National ID (Optional)'}</FormLabel>
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
          <h3 className="text-lg font-medium border-b border-border pb-2 mb-4 text-foreground">
            {isRtl ? 'العنوان والموقع' : 'Address'}
          </h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <FormField
              control={form.control}
              name="street"
              render={({ field }) => (
                <FormItem className="md:col-span-2">
                  <FormLabel>{isRtl ? 'عنوان الشارع' : 'Street Address'}</FormLabel>
                  <FormControl>
                    <Input placeholder={isRtl ? "شارع الملك فهد" : "123 Main St"} {...field} />
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
                  <FormLabel>{isRtl ? 'المدينة' : 'City'}</FormLabel>
                  <FormControl>
                    <Input placeholder={isRtl ? "الرياض" : "Riyadh"} {...field} />
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
                  <FormLabel>{isRtl ? 'المنطقة / المحافظة' : 'State/Province'}</FormLabel>
                  <FormControl>
                    <Input placeholder={isRtl ? "المنطقة الوسطى" : "Central Region"} {...field} />
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
                  <FormLabel>{isRtl ? 'الرمز البريدي' : 'Zip/Postal Code'}</FormLabel>
                  <FormControl>
                    <Input placeholder="12345" {...field} />
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
                  <FormLabel>{isRtl ? 'الدولة' : 'Country'}</FormLabel>
                  <FormControl>
                    <Input placeholder={isRtl ? "المملكة العربية السعودية" : "Saudi Arabia"} {...field} />
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
            <h3 className="text-lg font-medium border-b border-border pb-2 mb-4 text-foreground">
              {isRtl ? 'جهة اتصال الطوارئ' : 'Emergency Contact'}
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <FormField
                control={form.control}
                name="emergencyContactName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{isRtl ? 'اسم الشخص' : 'Contact Name'}</FormLabel>
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
                    <FormLabel>{isRtl ? 'رقم هاتف الطوارئ' : 'Contact Phone'}</FormLabel>
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
                    <FormLabel>{isRtl ? 'صلة القرابة' : 'Relationship'}</FormLabel>
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

        <div className="flex justify-end gap-4 border-t border-border pt-6">
          <Button type="submit" disabled={isLoading}>
            {isLoading ? (isRtl ? 'جاري الحفظ...' : 'Saving...') : (isRtl ? 'حفظ بيانات الموظف' : 'Save Employee')}
          </Button>
        </div>
      </form>
    </Form>
  );
};