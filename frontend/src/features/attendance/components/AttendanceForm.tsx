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
  FormMessage,
} from '@/shared/components/ui/form';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { manualAttendanceSchema, type ManualAttendanceFormValues } from '../schemas';
import { CheckInMethod } from '../types';

interface AttendanceFormProps {
  onSubmit: (values: ManualAttendanceFormValues) => void;
  isLoading?: boolean;
}

const defaultValues: ManualAttendanceFormValues = {
  employeeId: '',
  attendancePolicyId: '00000000-0000-0000-0000-000000000000',
  shiftId: '00000000-0000-0000-0000-000000000000',
  attendanceDeviceId: '00000000-0000-0000-0000-000000000000',
  checkInTime: new Date().toISOString().slice(0, 16), // YYYY-MM-DDThh:mm format for datetime-local input
  checkInMethod: CheckInMethod.Manual,
};

export function AttendanceForm({ onSubmit, isLoading }: AttendanceFormProps) {
  const form = useForm<ManualAttendanceFormValues>({
    resolver: zodResolver(manualAttendanceSchema),
    defaultValues: defaultValues,
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6 bg-white shadow px-4 py-5 sm:rounded-lg sm:p-6">
        <FormField
          control={form.control}
          name="employeeId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Employee ID *</FormLabel>
              <FormControl>
                <Input placeholder="Enter Employee UUID" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="checkInTime"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Check-In Time *</FormLabel>
              <FormControl>
                <Input type="datetime-local" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="checkInMethod"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Check-In Method</FormLabel>
              <Select 
                onValueChange={(val) => field.onChange(parseInt(val || '0'))} 
                value={field.value?.toString()}
                disabled
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Select method" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectItem value={CheckInMethod.Manual.toString()}>Manual Entry</SelectItem>
                </SelectContent>
              </Select>
              <p className="text-sm text-gray-500 mt-1">Manual entries are automatically flagged as Manual.</p>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-3 pt-4">
          <Button
            type="button"
            variant="outline"
            onClick={() => window.history.back()}
            disabled={isLoading}
          >
            Cancel
          </Button>
          <Button
            type="submit"
            disabled={isLoading}
          >
            {isLoading ? 'Saving...' : 'Save Record'}
          </Button>
        </div>
      </form>
    </Form>
  );
}
