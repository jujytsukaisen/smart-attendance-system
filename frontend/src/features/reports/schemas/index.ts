import { z } from 'zod';

export const reportFilterSchema = z.object({
  startDate: z.string().optional(),
  endDate: z.string().optional(),
  companyId: z.string().optional(),
  branchId: z.string().optional(),
  departmentId: z.string().optional(),
  employeeId: z.string().optional(),
  status: z.string().optional(),
  searchTerm: z.string().optional(),
});

export type ReportFilterFormValues = z.infer<typeof reportFilterSchema>;
