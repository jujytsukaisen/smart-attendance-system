import { z } from 'zod';

const baseAttendancePolicySchema = z.object({
  companyId: z.string().min(1, 'Company is required'),
  name: z.string().min(1, 'Name is required').max(100, 'Max 100 characters'),
  code: z.string().min(1, 'Code is required').max(50, 'Max 50 characters'),
  description: z.string().max(250, 'Max 250 characters').optional().nullable(),
  
  attendanceMode: z.union([z.literal(1), z.literal(2), z.literal(3), z.literal(4), z.literal(5)]),
  
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
  autoCheckoutTime: z.string().optional().nullable(),
  
  multipleShiftsSupport: z.boolean(),
  
  roundTimeRule: z.union([z.literal(0), z.literal(1), z.literal(2), z.literal(3)]),
  missingPunchPolicy: z.union([z.literal(0), z.literal(1), z.literal(2), z.literal(3)]),
  weekendPolicy: z.union([z.literal(0), z.literal(1), z.literal(2)]),
  holidayPolicy: z.union([z.literal(0), z.literal(1), z.literal(2)]),
});

// Using superRefine to map the FluentValidation rules
const validationRules = (data: any, ctx: z.RefinementCtx) => {
  if (data.maximumOvertimeMinutes < data.minimumOvertimeMinutes) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Maximum overtime cannot be less than minimum overtime.",
      path: ["maximumOvertimeMinutes"]
    });
  }

  // Field (2) requires GPS
  if (data.attendanceMode === 2 && !data.requireGPS) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Field mode requires GPS.",
      path: ["requireGPS"]
    });
  }

  // Office (1) requires Face Recognition
  if (data.attendanceMode === 1 && !data.requireFaceRecognition) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Office mode requires Face Recognition.",
      path: ["requireFaceRecognition"]
    });
  }

  // Remote (4) cannot require Company Network
  if (data.attendanceMode === 4 && data.requireCompanyNetwork) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Remote mode cannot require Company Network.",
      path: ["requireCompanyNetwork"]
    });
  }
};

export const createAttendancePolicySchema = baseAttendancePolicySchema.superRefine(validationRules);

export const updateAttendancePolicySchema = baseAttendancePolicySchema.extend({
  id: z.string().min(1, 'ID is required'),
}).superRefine(validationRules);
