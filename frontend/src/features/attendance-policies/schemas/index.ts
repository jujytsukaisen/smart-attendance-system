import { z } from 'zod';
import {
  AttendanceMode,
  RoundTimeRule,
  MissingPunchPolicy,
  WeekendPolicy,
  HolidayPolicy,
} from '../types';

export const attendancePolicySchema = z
  .object({
    companyId: z.string().uuid('Invalid company ID'),
    name: z.string().min(1, 'Name is required').max(100, 'Name must be 100 characters or less'),
    code: z.string().min(1, 'Code is required').max(50, 'Code must be 50 characters or less'),
    description: z.string().max(250, 'Description must be 250 characters or less').optional(),

    attendanceMode: z.nativeEnum(AttendanceMode),
    requireFaceRecognition: z.boolean(),
    requireGPS: z.boolean(),
    requireApprovedDevice: z.boolean(),
    requireCompanyNetwork: z.boolean(),
    allowMobileCheckIn: z.boolean(),
    allowDesktopCheckIn: z.boolean(),
    allowWebCheckIn: z.boolean(),
    allowOfflineAttendance: z.boolean(),
    allowedRadiusMeters: z.number().min(0, 'Must be greater than or equal to 0'),
    allowMultipleCheckIn: z.boolean(),
    allowManualAttendance: z.boolean(),
    requireManagerApproval: z.boolean(),
    maximumDistanceMeters: z.number().min(0, 'Must be greater than or equal to 0'),

    lateToleranceMinutes: z.number().min(0, 'Must be greater than or equal to 0'),
    earlyLeaveToleranceMinutes: z.number().min(0, 'Must be greater than or equal to 0'),
    gracePeriodMinutes: z.number().min(0, 'Must be greater than or equal to 0'),
    overtimeThresholdMinutes: z.number().min(0, 'Must be greater than or equal to 0'),
    minimumOvertimeMinutes: z.number().min(0, 'Must be greater than or equal to 0'),
    maximumOvertimeMinutes: z.number().min(0, 'Must be greater than or equal to 0'),

    autoCheckoutEnabled: z.boolean(),
    autoCheckoutTime: z.string().optional(),

    multipleShiftsSupport: z.boolean(),

    roundTimeRule: z.nativeEnum(RoundTimeRule),
    missingPunchPolicy: z.nativeEnum(MissingPunchPolicy),
    weekendPolicy: z.nativeEnum(WeekendPolicy),
    holidayPolicy: z.nativeEnum(HolidayPolicy),
  })
  .superRefine((data, ctx) => {
    if (data.maximumOvertimeMinutes < data.minimumOvertimeMinutes) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Maximum overtime cannot be less than minimum overtime.',
        path: ['maximumOvertimeMinutes'],
      });
    }

    if (data.attendanceMode === AttendanceMode.Field && !data.requireGPS) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Field mode requires GPS.',
        path: ['requireGPS'],
      });
    }

    if (data.attendanceMode === AttendanceMode.Office && !data.requireFaceRecognition) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Office mode requires Face Recognition.',
        path: ['requireFaceRecognition'],
      });
    }

    if (data.attendanceMode === AttendanceMode.Remote && data.requireCompanyNetwork) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Remote mode cannot require Company Network.',
        path: ['requireCompanyNetwork'],
      });
    }
  });

export type AttendancePolicyFormValues = z.infer<typeof attendancePolicySchema>;
