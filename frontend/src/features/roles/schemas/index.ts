import { z } from 'zod';

export const roleSchema = z.object({
  name: z.string().min(1, 'Role name is required').max(50, 'Role name must be at most 50 characters'),
  description: z.string().max(200, 'Description must be at most 200 characters').optional().default(''),
});

export type RoleFormValues = z.infer<typeof roleSchema>;

export const assignPermissionsSchema = z.object({
  permissions: z.array(z.string()),
});

export type AssignPermissionsFormValues = z.infer<typeof assignPermissionsSchema>;
