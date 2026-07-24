import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { 
  Form, 
  FormControl, 
  FormField, 
  FormItem, 
  FormLabel, 
  FormMessage
} from '@/shared/components/ui/form';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { updateBranchSchema } from '../schemas/branchSchema';
import { useCompanies } from '@/features/companies/hooks';
import type { CreateBranchCommand, UpdateBranchCommand } from '../types';


interface BranchFormProps {
  initialData?: UpdateBranchCommand;
  onSubmit: (data: CreateBranchCommand) => void;
  isLoading: boolean;
}

export const BranchForm = ({ initialData, onSubmit, isLoading }: BranchFormProps) => {
  const form = useForm<CreateBranchCommand>({
    resolver: zodResolver(updateBranchSchema) as any,
    defaultValues: initialData || {
      companyId: '',
      name: '',
      code: '',
      addressStreet: '',
      addressCity: '',
      addressState: '',
      addressCountry: '',
      addressZipCode: '',
      phoneNumber: '',
      email: '',
      latitude: 0,
      longitude: 0,
      allowedRadiusMeters: 50,
      timeZone: 'UTC',
      isHeadOffice: false,
    },
  });

  const { data: companiesData, isLoading: isLoadingCompanies } = useCompanies({
    pageNumber: 1,
    pageSize: 100, // Fetch up to 100 companies for the dropdown
    isActive: true,
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <FormField
            control={form.control}
            name="companyId"
            render={({ field }) => (
              <FormItem className="col-span-1 md:col-span-2">
                <FormLabel>Company</FormLabel>
                <Select onValueChange={field.onChange} defaultValue={field.value} disabled={isLoadingCompanies}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder={isLoadingCompanies ? "Loading companies..." : "Select a company"} />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {companiesData?.items.map((company) => (
                      <SelectItem key={company.id} value={company.id}>
                        {company.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Branch Name</FormLabel>
                <FormControl>
                  <Input placeholder="Main Office" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="code"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Branch Code</FormLabel>
                <FormControl>
                  <Input placeholder="BR-MAIN" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <div className="col-span-1 md:col-span-2 grid grid-cols-1 md:grid-cols-2 gap-6 mt-4">
            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Contact Email</FormLabel>
                  <FormControl>
                    <Input type="email" placeholder="branch@acme.com" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="phoneNumber"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Phone Number</FormLabel>
                  <FormControl>
                    <Input placeholder="+1 555 123 4567" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

          <div className="col-span-1 md:col-span-2 border-t pt-4 mt-2">
            <h3 className="text-lg font-medium mb-4">Location Settings</h3>
          </div>

          <FormField
            control={form.control}
            name="latitude"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Latitude</FormLabel>
                <FormControl>
                  <Input type="number" step="any" placeholder="40.7128" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="longitude"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Longitude</FormLabel>
                <FormControl>
                  <Input type="number" step="any" placeholder="-74.0060" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          
          <FormField
            control={form.control}
            name="allowedRadiusMeters"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Allowed Radius (Meters)</FormLabel>
                <FormControl>
                  <Input type="number" min="1" placeholder="50" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="timeZone"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Time Zone</FormLabel>
                <Select onValueChange={field.onChange} defaultValue={field.value}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select a timezone" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value="UTC">UTC (Universal Coordinated Time)</SelectItem>
                    <SelectItem value="America/New_York">EST (Eastern Standard Time)</SelectItem>
                    <SelectItem value="Europe/London">GMT (Greenwich Mean Time)</SelectItem>
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />

          <div className="col-span-1 md:col-span-2 border-t pt-4 mt-2">
            <h3 className="text-lg font-medium mb-4">Address Information</h3>
          </div>
          <FormField
            control={form.control}
            name="addressStreet"
            render={({ field }) => (
              <FormItem className="col-span-1 md:col-span-2">
                <FormLabel>Street Address</FormLabel>
                <FormControl>
                  <Input placeholder="123 Branch St" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="addressCity"
            render={({ field }) => (
              <FormItem>
                <FormLabel>City</FormLabel>
                <FormControl>
                  <Input placeholder="New York" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="addressState"
            render={({ field }) => (
              <FormItem>
                <FormLabel>State/Province</FormLabel>
                <FormControl>
                  <Input placeholder="NY" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="addressCountry"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Country</FormLabel>
                <FormControl>
                  <Input placeholder="USA" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="addressZipCode"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Zip Code</FormLabel>
                <FormControl>
                  <Input placeholder="10001" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          
          <FormField
            control={form.control}
            name="isHeadOffice"
            render={({ field }) => (
              <FormItem className="col-span-1 md:col-span-2 flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4 shadow-sm">
                <FormControl>
                  <input
                    type="checkbox"
                    className="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary"
                    checked={field.value}
                    onChange={field.onChange}
                  />
                </FormControl>
                <div className="space-y-1 leading-none">
                  <FormLabel>Head Office</FormLabel>
                  <p className="text-sm text-muted-foreground">
                    Mark this branch as the primary head office for the company.
                  </p>
                </div>
              </FormItem>
            )}
          />

        </div>
        <div className="flex justify-end gap-4 border-t pt-6">
          <Button type="submit" disabled={isLoading}>
            {isLoading ? 'Saving...' : 'Save Branch'}
          </Button>
        </div>
      </form>
    </Form>
  );
};
