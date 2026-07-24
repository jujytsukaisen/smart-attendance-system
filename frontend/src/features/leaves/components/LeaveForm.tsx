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
  FormMessage,
} from '@/shared/components/ui/form';
import { 
  Select, 
  SelectContent, 
  SelectItem, 
  SelectTrigger, 
  SelectValue 
} from '@/shared/components/ui/select';
import { useEmployees } from '@/features/employees/hooks';
import { leaveSchema, type LeaveFormValues } from '../schemas';
import { LeaveType } from '../types';

interface LeaveFormProps {
  initialValues?: Partial<LeaveFormValues>;
  onSubmit: (values: LeaveFormValues) => void;
  isLoading?: boolean;
}

const defaultValues: LeaveFormValues = {
  companyId: '00000000-0000-0000-0000-000000000000', // Mock since no company auth module
  employeeId: '',
  leaveType: LeaveType.Annual.toString(),
  startDate: new Date().toISOString().slice(0, 10),
  endDate: new Date().toISOString().slice(0, 10),
  reason: '',
};

export function LeaveForm({ initialValues, onSubmit, isLoading }: LeaveFormProps) {
  const { data: employeesData } = useEmployees({ pageNumber: 1, pageSize: 100 });

  const form = useForm<LeaveFormValues>({
    resolver: zodResolver(leaveSchema),
    defaultValues: {
      ...defaultValues,
      ...initialValues,
      // Make sure enums strings map correctly
      leaveType: initialValues?.leaveType?.toString() || defaultValues.leaveType,
    },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6 bg-white p-6 rounded-lg shadow border border-gray-200">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <FormField
            control={form.control}
            name="employeeId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Employee</FormLabel>
                <Select onValueChange={field.onChange} defaultValue={field.value} value={field.value}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select an employee" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {employeesData?.items.map((emp) => (
                      <SelectItem key={emp.id} value={emp.id}>
                        {emp.firstName} {emp.lastName}
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
            name="leaveType"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Leave Type</FormLabel>
                <Select onValueChange={field.onChange} defaultValue={field.value} value={field.value}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select leave type" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value={LeaveType.Annual.toString()}>Annual</SelectItem>
                    <SelectItem value={LeaveType.Sick.toString()}>Sick</SelectItem>
                    <SelectItem value={LeaveType.Maternity.toString()}>Maternity</SelectItem>
                    <SelectItem value={LeaveType.Paternity.toString()}>Paternity</SelectItem>
                    <SelectItem value={LeaveType.Unpaid.toString()}>Unpaid</SelectItem>
                    <SelectItem value={LeaveType.Other.toString()}>Other</SelectItem>
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="startDate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Start Date</FormLabel>
                <FormControl>
                  <Input type="date" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="endDate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>End Date</FormLabel>
                <FormControl>
                  <Input type="date" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="reason"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Reason</FormLabel>
              <FormControl>
                <Textarea 
                  placeholder="Please provide a reason for your leave request..." 
                  className="resize-none h-24"
                  {...field} 
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end pt-4">
          <Button type="submit" disabled={isLoading}>
            {isLoading ? 'Saving...' : 'Save Leave Request'}
          </Button>
        </div>
      </form>
    </Form>
  );
}
