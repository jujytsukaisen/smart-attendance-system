import { z } from 'zod';

export const leaveSchema = z.object({
  companyId: z.string().optional(),
  employeeId: z.string().optional(),
  leaveType: z.string().min(1, 'Leave Type is required'),
  startDate: z.string().min(1, 'Start date is required'),
  endDate: z.string().min(1, 'End date is required'),
  reason: z.string().min(1, 'Reason is required').max(500, 'Reason must be at most 500 characters'),
}).refine((data) => new Date(data.endDate) >= new Date(data.startDate), {
  message: "End date must be greater than or equal to start date",
  path: ["endDate"],
});

export type LeaveFormValues = z.infer<typeof leaveSchema>;

export const approveLeaveSchema = z.object({
  comment: z.string().max(500, 'Comment must be at most 500 characters').optional().default(''),
});

export type ApproveLeaveFormValues = z.infer<typeof approveLeaveSchema>;

export const rejectLeaveSchema = z.object({
  reason: z.string().min(1, 'Reason is required').max(500, 'Reason must be at most 500 characters'),
});

export type RejectLeaveFormValues = z.infer<typeof rejectLeaveSchema>;

export const cancelLeaveSchema = z.object({
  reason: z.string().min(1, 'Reason is required').max(500, 'Reason must be at most 500 characters'),
});

export type CancelLeaveFormValues = z.infer<typeof cancelLeaveSchema>;
