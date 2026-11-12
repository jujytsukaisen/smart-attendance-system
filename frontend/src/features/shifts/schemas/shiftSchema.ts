import { z } from 'zod';

export const createShiftSchema = z.object({
  companyId: z.string().min(1, 'Company is required'),
  workScheduleId: z.string().min(1, 'Work schedule is required'),
  name: z.string().min(2, 'Name must be at least 2 characters').max(100, 'Name cannot exceed 100 characters'),
  code: z.string().min(1, 'Code is required').max(50, 'Code cannot exceed 50 characters'),
  description: z.string().max(250, 'Description cannot exceed 250 characters').optional().default(''),
  startTime: z.string().min(1, 'Start time is required'),
  endTime: z.string().min(1, 'End time is required'),
  shiftType: z.coerce.number().min(1).max(5),
  isFlexible: z.boolean().default(false),
  minimumWorkingHours: z.coerce.number().min(0.5, 'Minimum working hours must be greater than 0').default(8),
  maximumWorkingHours: z.coerce.number().min(0.5, 'Maximum working hours must be greater than 0').default(8),
  gracePeriodMinutes: z.coerce.number().min(0).default(15),
  allowEarlyCheckInMinutes: z.coerce.number().min(0).default(30),
  allowLateCheckOutMinutes: z.coerce.number().min(0).default(30),
  maxBreakDurationMinutes: z.coerce.number().min(0).default(60),
}).refine(data => data.maximumWorkingHours >= data.minimumWorkingHours, {
  message: 'Maximum working hours must be greater than or equal to minimum working hours',
  path: ['maximumWorkingHours'],
});

export const updateShiftSchema = z.object({
  shiftId: z.string().min(1, 'Shift ID is required'),
  name: z.string().min(2, 'Name must be at least 2 characters').max(100, 'Name cannot exceed 100 characters'),
  code: z.string().min(1, 'Code is required').max(50, 'Code cannot exceed 50 characters'),
  description: z.string().max(250, 'Description cannot exceed 250 characters').optional().default(''),
  startTime: z.string().min(1, 'Start time is required'),
  endTime: z.string().min(1, 'End time is required'),
  shiftType: z.coerce.number().min(1).max(5),
  isFlexible: z.boolean().default(false),
  minimumWorkingHours: z.coerce.number().min(0.5, 'Minimum working hours must be greater than 0').default(8),
  maximumWorkingHours: z.coerce.number().min(0.5, 'Maximum working hours must be greater than 0').default(8),
  gracePeriodMinutes: z.coerce.number().min(0).default(15),
  allowEarlyCheckInMinutes: z.coerce.number().min(0).default(30),
  allowLateCheckOutMinutes: z.coerce.number().min(0).default(30),
  maxBreakDurationMinutes: z.coerce.number().min(0).default(60),
}).refine(data => data.maximumWorkingHours >= data.minimumWorkingHours, {
  message: 'Maximum working hours must be greater than or equal to minimum working hours',
  path: ['maximumWorkingHours'],
});

export type CreateShiftFormValues = z.infer<typeof createShiftSchema>;
export type UpdateShiftFormValues = z.infer<typeof updateShiftSchema>;
