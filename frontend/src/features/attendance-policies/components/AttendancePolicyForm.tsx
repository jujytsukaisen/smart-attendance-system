import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Checkbox } from '@/shared/components/ui/checkbox';
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
import { attendancePolicySchema, type AttendancePolicyFormValues } from '../schemas';
import {
  AttendanceMode,
  RoundTimeRule,
  MissingPunchPolicy,
  WeekendPolicy,
  HolidayPolicy,
} from '../types';

interface AttendancePolicyFormProps {
  initialValues?: Partial<AttendancePolicyFormValues>;
  onSubmit: (values: AttendancePolicyFormValues) => void;
  isLoading?: boolean;
  isEdit?: boolean;
}

const defaultValues: Partial<AttendancePolicyFormValues> = {
  companyId: '', // Should be set by context/auth in a real app
  name: '',
  code: '',
  description: '',
  
  attendanceMode: AttendanceMode.Office,
  
  requireFaceRecognition: false,
  requireGPS: false,
  requireApprovedDevice: false,
  requireCompanyNetwork: false,
  allowMobileCheckIn: false,
  allowDesktopCheckIn: false,
  allowWebCheckIn: false,
  allowOfflineAttendance: false,
  allowedRadiusMeters: 0,
  allowMultipleCheckIn: false,
  allowManualAttendance: false,
  requireManagerApproval: false,
  maximumDistanceMeters: 0,
  
  lateToleranceMinutes: 0,
  earlyLeaveToleranceMinutes: 0,
  gracePeriodMinutes: 0,
  overtimeThresholdMinutes: 0,
  minimumOvertimeMinutes: 0,
  maximumOvertimeMinutes: 0,
  
  autoCheckoutEnabled: false,
  autoCheckoutTime: '',
  
  multipleShiftsSupport: false,
  
  roundTimeRule: RoundTimeRule.None,
  missingPunchPolicy: MissingPunchPolicy.Reject,
  weekendPolicy: WeekendPolicy.Regular,
  holidayPolicy: HolidayPolicy.Regular,
};

export function AttendancePolicyForm({
  initialValues,
  onSubmit,
  isLoading,
  isEdit,
}: AttendancePolicyFormProps) {
  const form = useForm<AttendancePolicyFormValues>({
    resolver: zodResolver(attendancePolicySchema),
    defaultValues: { ...defaultValues, ...initialValues } as AttendancePolicyFormValues,
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
        <div className="bg-white shadow px-4 py-5 sm:rounded-lg sm:p-6">
          <div className="md:grid md:grid-cols-3 md:gap-6">
            <div className="md:col-span-1">
              <h3 className="text-lg font-medium leading-6 text-gray-900">Basic Information</h3>
              <p className="mt-1 text-sm text-gray-500">
                Basic details about the attendance policy.
              </p>
            </div>
            <div className="mt-5 md:mt-0 md:col-span-2 space-y-6">
              <div className="grid grid-cols-6 gap-6">
                <FormField
                  control={form.control}
                  name="name"
                  render={({ field }) => (
                    <FormItem className="col-span-6 sm:col-span-3">
                      <FormLabel>Policy Name *</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                
                <FormField
                  control={form.control}
                  name="code"
                  render={({ field }) => (
                    <FormItem className="col-span-6 sm:col-span-3">
                      <FormLabel>Policy Code *</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="companyId"
                  render={({ field }) => (
                    <FormItem className="col-span-6 sm:col-span-4">
                      <FormLabel>Company ID *</FormLabel>
                      <FormControl>
                        <Input {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="description"
                  render={({ field }) => (
                    <FormItem className="col-span-6">
                      <FormLabel>Description</FormLabel>
                      <FormControl>
                        <Input {...field} value={field.value || ''} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white shadow px-4 py-5 sm:rounded-lg sm:p-6">
          <div className="md:grid md:grid-cols-3 md:gap-6">
            <div className="md:col-span-1">
              <h3 className="text-lg font-medium leading-6 text-gray-900">Attendance Settings</h3>
              <p className="mt-1 text-sm text-gray-500">
                Configure how attendance is tracked and verified.
              </p>
            </div>
            <div className="mt-5 md:mt-0 md:col-span-2 space-y-6">
              <div className="grid grid-cols-6 gap-6">
                <FormField
                  control={form.control}
                  name="attendanceMode"
                  render={({ field }) => (
                    <FormItem className="col-span-6 sm:col-span-3">
                      <FormLabel>Attendance Mode</FormLabel>
                      <Select 
                        onValueChange={(val) => field.onChange(parseInt(val || '0'))} 
                        value={field.value?.toString() || ''}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder="Select mode" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value={AttendanceMode.Office.toString()}>Office</SelectItem>
                          <SelectItem value={AttendanceMode.Field.toString()}>Field</SelectItem>
                          <SelectItem value={AttendanceMode.Hybrid.toString()}>Hybrid</SelectItem>
                          <SelectItem value={AttendanceMode.Remote.toString()}>Remote</SelectItem>
                          <SelectItem value={AttendanceMode.Flexible.toString()}>Flexible</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                
                <FormField
                  control={form.control}
                  name="allowedRadiusMeters"
                  render={({ field }) => (
                    <FormItem className="col-span-6 sm:col-span-3">
                      <FormLabel>Allowed Radius (Meters)</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={(e) => field.onChange(parseInt(e.target.value) || 0)} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                
                <FormField
                  control={form.control}
                  name="maximumDistanceMeters"
                  render={({ field }) => (
                    <FormItem className="col-span-6 sm:col-span-3">
                      <FormLabel>Maximum Distance (Meters)</FormLabel>
                      <FormControl>
                        <Input type="number" {...field} onChange={(e) => field.onChange(parseInt(e.target.value) || 0)} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <div className="col-span-6 grid grid-cols-2 gap-4">
                  {[
                    { name: 'requireFaceRecognition', label: 'Require Face Recognition' },
                    { name: 'requireGPS', label: 'Require GPS' },
                    { name: 'requireApprovedDevice', label: 'Require Approved Device' },
                    { name: 'requireCompanyNetwork', label: 'Require Company Network' },
                    { name: 'allowMobileCheckIn', label: 'Allow Mobile Check-In' },
                    { name: 'allowDesktopCheckIn', label: 'Allow Desktop Check-In' },
                    { name: 'allowWebCheckIn', label: 'Allow Web Check-In' },
                    { name: 'allowOfflineAttendance', label: 'Allow Offline Attendance' },
                    { name: 'allowMultipleCheckIn', label: 'Allow Multiple Check-In' },
                    { name: 'allowManualAttendance', label: 'Allow Manual Attendance' },
                    { name: 'requireManagerApproval', label: 'Require Manager Approval' },
                    { name: 'multipleShiftsSupport', label: 'Multiple Shifts Support' },
                  ].map((item) => (
                    <FormField
                      key={item.name}
                      control={form.control}
                      name={item.name as any}
                      render={({ field }) => (
                        <FormItem className="flex flex-row items-start space-x-3 space-y-0 p-2">
                          <FormControl>
                            <Checkbox
                              checked={field.value}
                              onCheckedChange={field.onChange}
                            />
                          </FormControl>
                          <div className="space-y-1 leading-none">
                            <FormLabel>{item.label}</FormLabel>
                          </div>
                        </FormItem>
                      )}
                    />
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white shadow px-4 py-5 sm:rounded-lg sm:p-6">
          <div className="md:grid md:grid-cols-3 md:gap-6">
            <div className="md:col-span-1">
              <h3 className="text-lg font-medium leading-6 text-gray-900">Time & Rules</h3>
              <p className="mt-1 text-sm text-gray-500">
                Configure tolerance, overtime, and specific rules.
              </p>
            </div>
            <div className="mt-5 md:mt-0 md:col-span-2 space-y-6">
              <div className="grid grid-cols-6 gap-6">
                {[
                  { name: 'lateToleranceMinutes', label: 'Late Tolerance (Min)' },
                  { name: 'earlyLeaveToleranceMinutes', label: 'Early Leave Tolerance (Min)' },
                  { name: 'gracePeriodMinutes', label: 'Grace Period (Min)' },
                  { name: 'overtimeThresholdMinutes', label: 'Overtime Threshold (Min)' },
                  { name: 'minimumOvertimeMinutes', label: 'Min Overtime (Min)' },
                  { name: 'maximumOvertimeMinutes', label: 'Max Overtime (Min)' },
                ].map((item) => (
                  <FormField
                    key={item.name}
                    control={form.control}
                    name={item.name as any}
                    render={({ field }) => (
                      <FormItem className="col-span-6 sm:col-span-2">
                        <FormLabel>{item.label}</FormLabel>
                        <FormControl>
                          <Input type="number" {...field} onChange={(e) => field.onChange(parseInt(e.target.value) || 0)} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                ))}

                <FormField
                  control={form.control}
                  name="autoCheckoutEnabled"
                  render={({ field }) => (
                    <FormItem className="col-span-6 sm:col-span-3 flex flex-row items-start space-x-3 space-y-0 p-4">
                      <FormControl>
                        <Checkbox
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </FormControl>
                      <div className="space-y-1 leading-none">
                        <FormLabel>Enable Auto Checkout</FormLabel>
                      </div>
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="autoCheckoutTime"
                  render={({ field }) => (
                    <FormItem className="col-span-6 sm:col-span-3">
                      <FormLabel>Auto Checkout Time</FormLabel>
                      <FormControl>
                        <Input type="time" {...field} value={field.value || ''} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="roundTimeRule"
                  render={({ field }) => (
                    <FormItem className="col-span-6 sm:col-span-3">
                      <FormLabel>Round Time Rule</FormLabel>
                      <Select 
                        onValueChange={(val) => field.onChange(parseInt(val || '0'))} 
                        value={field.value?.toString() || ''}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder="Select rule" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value={RoundTimeRule.None.toString()}>None</SelectItem>
                          <SelectItem value={RoundTimeRule.RoundUp.toString()}>Round Up</SelectItem>
                          <SelectItem value={RoundTimeRule.RoundDown.toString()}>Round Down</SelectItem>
                          <SelectItem value={RoundTimeRule.RoundToNearest.toString()}>Round To Nearest</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                
                <FormField
                  control={form.control}
                  name="missingPunchPolicy"
                  render={({ field }) => (
                    <FormItem className="col-span-6 sm:col-span-3">
                      <FormLabel>Missing Punch Policy</FormLabel>
                      <Select 
                        onValueChange={(val) => field.onChange(parseInt(val || '0'))} 
                        value={field.value?.toString() || ''}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder="Select policy" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value={MissingPunchPolicy.Reject.toString()}>Reject</SelectItem>
                          <SelectItem value={MissingPunchPolicy.MarkAsAbsent.toString()}>Mark As Absent</SelectItem>
                          <SelectItem value={MissingPunchPolicy.RequireApproval.toString()}>Require Approval</SelectItem>
                          <SelectItem value={MissingPunchPolicy.DeductHalfDay.toString()}>Deduct Half Day</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="weekendPolicy"
                  render={({ field }) => (
                    <FormItem className="col-span-6 sm:col-span-3">
                      <FormLabel>Weekend Policy</FormLabel>
                      <Select 
                        onValueChange={(val) => field.onChange(parseInt(val || '0'))} 
                        value={field.value?.toString() || ''}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder="Select policy" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value={WeekendPolicy.Regular.toString()}>Regular</SelectItem>
                          <SelectItem value={WeekendPolicy.Overtime.toString()}>Overtime</SelectItem>
                          <SelectItem value={WeekendPolicy.StrictAbsent.toString()}>Strict Absent</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                
                <FormField
                  control={form.control}
                  name="holidayPolicy"
                  render={({ field }) => (
                    <FormItem className="col-span-6 sm:col-span-3">
                      <FormLabel>Holiday Policy</FormLabel>
                      <Select 
                        onValueChange={(val) => field.onChange(parseInt(val || '0'))} 
                        value={field.value?.toString() || ''}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder="Select policy" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value={HolidayPolicy.Regular.toString()}>Regular</SelectItem>
                          <SelectItem value={HolidayPolicy.Overtime.toString()}>Overtime</SelectItem>
                          <SelectItem value={HolidayPolicy.StrictAbsent.toString()}>Strict Absent</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
            </div>
          </div>
        </div>

        <div className="flex justify-end gap-3">
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
            {isLoading ? 'Saving...' : (isEdit ? 'Update Policy' : 'Create Policy')}
          </Button>
        </div>
      </form>
    </Form>
  );
}
