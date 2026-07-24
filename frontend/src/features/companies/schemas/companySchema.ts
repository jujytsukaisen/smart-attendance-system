import { z } from 'zod';

export const createCompanySchema = z.object({
  name: z.string().min(1, 'Name is required').max(100, 'Name must be at most 100 characters'),
  legalName: z.string().min(1, 'Legal Name is required').max(100, 'Legal Name must be at most 100 characters'),
  registrationNumber: z.string().min(1, 'Registration Number is required').max(50, 'Registration Number must be at most 50 characters'),
  taxNumber: z.string().min(1, 'Tax Number is required').max(50, 'Tax Number must be at most 50 characters'),
  addressStreet: z.string().min(1, 'Street is required').max(250, 'Street must be at most 250 characters'),
  addressCity: z.string().min(1, 'City is required').max(100, 'City must be at most 100 characters'),
  addressState: z.string().min(1, 'State is required').max(100, 'State must be at most 100 characters'),
  addressCountry: z.string().min(1, 'Country is required').max(100, 'Country must be at most 100 characters'),
  addressZipCode: z.string().min(1, 'Zip Code is required').max(20, 'Zip Code must be at most 20 characters'),
  phoneNumber: z.string().min(1, 'Phone Number is required').max(20, 'Phone Number must be at most 20 characters'),
  email: z.string().min(1, 'Email is required').email('Invalid email address'),
  foundedDate: z.string().min(1, 'Founded Date is required').refine((date) => new Date(date) <= new Date(), {
    message: 'Founded Date cannot be in the future',
  }),
  website: z.string().max(200, 'Website must be at most 200 characters').optional().or(z.literal('')),
  timeZone: z.string().min(1, 'Time Zone is required'),
  currency: z.string().min(1, 'Currency is required'),
});

export type CreateCompanySchemaType = z.infer<typeof createCompanySchema>;

export const updateCompanySchema = createCompanySchema;
export type UpdateCompanySchemaType = z.infer<typeof updateCompanySchema>;
