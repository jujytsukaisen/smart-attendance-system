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
  employeeId: z.string().uuid('Employee ID is required'),
  attendancePolicyId: z.string().uuid('Policy ID is required'),
  shiftId: z.string().uuid('Shift ID is required'),
  attendanceDeviceId: z.string().uuid('Device ID is required'),
  checkInTime: z.string().min(1, 'Check-in time is required'),
  checkInMethod: z.nativeEnum(CheckInMethod),
});

export type CheckInFormValues = z.infer<typeof checkInSchema>;
export type CheckOutFormValues = z.infer<typeof checkOutSchema>;
export type ManualAttendanceFormValues = z.infer<typeof manualAttendanceSchema>;
