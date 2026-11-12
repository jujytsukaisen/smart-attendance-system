import { z } from 'zod';

export const createBranchSchema = z.object({
  companyId: z.string().min(1, 'Company is required'),
  name: z.string().min(1, 'Name is required').max(100, 'Name must be at most 100 characters'),
  code: z.string().min(1, 'Code is required').max(20, 'Code must be at most 20 characters'),
  addressStreet: z.string().min(1, 'Street is required').max(100, 'Street must be at most 100 characters'),
  addressCity: z.string().min(1, 'City is required').max(50, 'City must be at most 50 characters'),
  addressState: z.string().min(1, 'State is required').max(50, 'State must be at most 50 characters'),
  addressCountry: z.string().min(1, 'Country is required').max(50, 'Country must be at most 50 characters'),
  addressZipCode: z.string().min(1, 'Zip Code is required').max(20, 'Zip Code must be at most 20 characters'),
  phoneNumber: z.string().max(20, 'Phone Number must be at most 20 characters').default(''),
  email: z.union([z.string().email('Invalid email address'), z.literal('')]).default(''),
  latitude: z.coerce.number().min(-90, 'Invalid latitude').max(90, 'Invalid latitude').optional().nullable(),
  longitude: z.coerce.number().min(-180, 'Invalid longitude').max(180, 'Invalid longitude').optional().nullable(),
  allowedRadiusMeters: z.coerce.number().positive('Radius must be greater than 0').optional().nullable(),
  timeZone: z.string().optional().nullable(),
  isHeadOffice: z.boolean().default(false),
});

export type CreateBranchSchemaType = z.infer<typeof createBranchSchema>;

export const updateBranchSchema = createBranchSchema;
export type UpdateBranchSchemaType = z.infer<typeof updateBranchSchema>;
