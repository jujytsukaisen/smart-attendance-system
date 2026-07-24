import { z } from 'zod';

export const userSchema = z.object({
  username: z.string().min(1, 'Username is required').max(50, 'Username must be at most 50 characters'),
  email: z.string().min(1, 'Email is required').email('Invalid email address'),
  password: z.string().min(8, 'Password must be at least 8 characters').optional().or(z.literal('')),
  confirmPassword: z.string().optional().or(z.literal('')),
}).refine((data) => {
  if (data.password && data.password !== data.confirmPassword) {
    return false;
  }
  return true;
}, {
  message: "Passwords don't match",
  path: ["confirmPassword"],
});

export type UserFormValues = z.infer<typeof userSchema>;

export const assignRolesSchema = z.object({
  roles: z.array(z.string()).min(1, 'Select at least one role'),
});

export type AssignRolesFormValues = z.infer<typeof assignRolesSchema>;
