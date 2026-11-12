import { z } from 'zod';
import { CheckInMethod } from '../types';

export const checkInSchema = z.object({
  employeeId: z.string().uuid('Invalid Employee ID'),
  shiftId: z.string().uuid('Invalid Shift ID'),
  attendancePolicyId: z.string().uuid('Invalid Attendance Policy ID'),
  attendanceDeviceId: z.string().uuid('Invalid Device ID').optional(),
  checkInMethod: z.nativeEnum(CheckInMethod),
  latitude: z.number().min(-90).max(90),
  longitude: z.number().min(-180).max(180),
  iPAddress: z.string().max(45).optional().default(''),
  deviceInformation: z.string().max(250).optional().default(''),
  checkInTime: z.string().datetime(),
});

export const checkOutSchema = z.object({
  attendanceId: z.string().uuid('Invalid Attendance ID'),
  checkOutMethod: z.nativeEnum(CheckInMethod),
  latitude: z.number().min(-90).max(90),
  longitude: z.number().min(-180).max(180),
  iPAddress: z.string().max(45).optional().default(''),
  deviceInformation: z.string().max(250).optional().default(''),
  checkOutTime: z.string().datetime(),
});

export const manualAttendanceSchema = z.object({
  employeeId: z.string().min(1, 'Please select an employee'),
  attendancePolicyId: z.string(),
  shiftId: z.string(),
  attendanceDeviceId: z.string(),
  checkInTime: z.string().min(1, 'Check-in time is required'),
  checkOutTime: z.string().optional(),
  checkInMethod: z.nativeEnum(CheckInMethod),
}).refine((data) => {
  if (data.checkInTime && data.checkOutTime && data.checkOutTime.trim() !== '') {
    return new Date(data.checkOutTime) > new Date(data.checkInTime);
  }
  return true;
}, {
  message: 'Check-out time must be after check-in time',
  path: ['checkOutTime'],
});

export type CheckInFormValues = z.infer<typeof checkInSchema>;
export type CheckOutFormValues = z.infer<typeof checkOutSchema>;
export type ManualAttendanceFormValues = z.infer<typeof manualAttendanceSchema>;
