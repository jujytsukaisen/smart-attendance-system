import { z } from 'zod';

export const createDepartmentSchema = z.object({
  companyId: z.string().min(1, 'Company is required'),
  branchId: z.string().min(1, 'Branch is required'),
  name: z.string().min(1, 'Name is required').max(100, 'Name must be at most 100 characters'),
  code: z.string().min(1, 'Code is required').max(20, 'Code must be at most 20 characters'),
  description: z.string().max(250, 'Description must be at most 250 characters').default(''),
});

export const updateDepartmentSchema = z.object({
  departmentId: z.string().min(1, 'Department ID is required'),
  name: z.string().min(1, 'Name is required').max(100, 'Name must be at most 100 characters'),
  description: z.string().max(250, 'Description must be at most 250 characters').default(''),
});
