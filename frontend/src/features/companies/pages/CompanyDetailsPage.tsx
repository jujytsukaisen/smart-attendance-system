import { useParams, useNavigate } from 'react-router-dom';
import { useCompany } from '../hooks';
import { Button } from '@/shared/components/ui/button';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { ChevronLeft, Building2, MapPin, Phone, Globe, Calendar, Mail, FileText } from 'lucide-react';
import { CompanyStatusBadge } from '../components/CompanyStatusBadge';

export const CompanyDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const { data: company, isLoading, isError, error } = useCompany(id!);

  if (isError) {
    return (
      <div className="flex items-center justify-center py-10">
        <p className="text-destructive">
          {error instanceof Error && !error.message.toLowerCase().includes('internal server error') && !error.message.includes('status code')
            ? error.message
            : "Failed to load"}
        </p>
      </div>
    );
  }

  if (isLoading) {

    return (
      <div className="space-y-6 max-w-5xl mx-auto">
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10" />
          <div>
            <Skeleton className="h-8 w-[200px] mb-2" />
            <Skeleton className="h-4 w-[100px]" />
          </div>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <Skeleton className="h-64 col-span-2" />
          <Skeleton className="h-64 col-span-1" />
        </div>
      </div>
    );
  }

  if (!company) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-center text-destructive">
        <h2 className="text-xl font-semibold mb-2">Error loading company</h2>
        <p>{'Company not found'}</p>
        <Button className="mt-4" onClick={() => navigate('/companies')}>Back to Companies</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-5xl mx-auto">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate('/companies')}>
            <ChevronLeft className="h-5 w-5" />
          </Button>
          <div className="flex items-center gap-4">
            {company.logoUrl && (
              <img src={company.logoUrl} alt={company.name} className="w-12 h-12 rounded-lg object-cover" />
            )}
            <div>
              <h2 className="text-2xl font-bold tracking-tight flex items-center gap-3">
                {company.name}
                <CompanyStatusBadge isActive={company.isActive} />
              </h2>
              <p className="text-muted-foreground">{company.legalName}</p>
            </div>
          </div>
        </div>
        <Button onClick={() => navigate(`/companies/${company.id}/edit`)}>
          Edit Company
        </Button>
      </div>
      
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="col-span-1 md:col-span-2">
          <CardHeader>
            <CardTitle>Company Details</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-1 sm:grid-cols-2 gap-6">
            <div className="space-y-1">
              <span className="text-sm text-muted-foreground flex items-center gap-2">
                <FileText className="h-4 w-4" /> Registration Number
              </span>
              <p className="font-medium">{company.registrationNumber}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm text-muted-foreground flex items-center gap-2">
                <FileText className="h-4 w-4" /> Tax Number
              </span>
              <p className="font-medium">{company.taxNumber}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm text-muted-foreground flex items-center gap-2">
                <Mail className="h-4 w-4" /> Email
              </span>
              <p className="font-medium">{company.email}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm text-muted-foreground flex items-center gap-2">
                <Phone className="h-4 w-4" /> Phone Number
              </span>
              <p className="font-medium">{company.phoneNumber}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm text-muted-foreground flex items-center gap-2">
                <Globe className="h-4 w-4" /> Website
              </span>
              <p className="font-medium">
                {company.website ? (
                  <a href={company.website} target="_blank" rel="noreferrer" className="text-primary hover:underline">
                    {company.website}
                  </a>
                ) : 'N/A'}
              </p>
            </div>
            <div className="space-y-1">
              <span className="text-sm text-muted-foreground flex items-center gap-2">
                <Calendar className="h-4 w-4" /> Founded Date
              </span>
              <p className="font-medium">{new Date(company.foundedDate).toLocaleDateString()}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm text-muted-foreground">Time Zone</span>
              <p className="font-medium">{company.timeZone}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm text-muted-foreground">Currency</span>
              <p className="font-medium">{company.currency}</p>
            </div>
          </CardContent>
        </Card>

        <div className="space-y-6 col-span-1">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <MapPin className="h-5 w-5" /> Address
              </CardTitle>
            </CardHeader>
            <CardContent>
              <address className="not-italic text-sm space-y-1">
                <p>{company.addressStreet}</p>
                <p>{company.addressCity}, {company.addressState} {company.addressZipCode}</p>
                <p>{company.addressCountry}</p>
              </address>
            </CardContent>
          </Card>
          
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Building2 className="h-5 w-5" /> Statistics
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex justify-between items-center border-b pb-2">
                  <span className="text-sm text-muted-foreground">Branches</span>
                  <span className="font-medium">{company.branchesCount}</span>
                </div>
                <div className="flex justify-between items-center border-b pb-2">
                  <span className="text-sm text-muted-foreground">Departments</span>
                  <span className="font-medium">{company.departmentsCount}</span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-muted-foreground">Employees</span>
                  <span className="font-medium">{company.employeesCount}</span>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
};
