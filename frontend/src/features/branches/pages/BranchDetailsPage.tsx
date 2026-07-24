import { useParams, useNavigate } from 'react-router-dom';
import { useBranch } from '../hooks';
import { Button } from '@/shared/components/ui/button';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { ChevronLeft, MapPin, Phone, Mail, FileText, Globe, Building2, Map } from 'lucide-react';
import { BranchStatusBadge } from '../components/BranchStatusBadge';

export const BranchDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const { data: branch, isLoading, isError, error } = useBranch(id!);

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

  if (!branch) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-center text-destructive">
        <h2 className="text-xl font-semibold mb-2">Error loading branch</h2>
        <p>{'Branch not found'}</p>
        <Button className="mt-4" onClick={() => navigate('/branches')}>Back to Branches</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-5xl mx-auto">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate('/branches')}>
            <ChevronLeft className="h-5 w-5" />
          </Button>
          <div>
            <h2 className="text-2xl font-bold tracking-tight flex items-center gap-3">
              {branch.name}
              {branch.isHeadOffice && (
                <span className="text-xs font-semibold text-blue-600 bg-blue-100 px-2 py-1 rounded-md">Head Office</span>
              )}
              <BranchStatusBadge isActive={branch.isActive} />
            </h2>
            <p className="text-muted-foreground">Code: {branch.code}</p>
          </div>
        </div>
        <Button onClick={() => navigate(`/branches/${branch.id}/edit`)}>
          Edit Branch
        </Button>
      </div>
      
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="col-span-1 md:col-span-2">
          <CardHeader>
            <CardTitle>Branch Details</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-1 sm:grid-cols-2 gap-6">
            <div className="space-y-1">
              <span className="text-sm text-muted-foreground flex items-center gap-2">
                <FileText className="h-4 w-4" /> Branch Code
              </span>
              <p className="font-medium">{branch.code}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm text-muted-foreground flex items-center gap-2">
                <Mail className="h-4 w-4" /> Email
              </span>
              <p className="font-medium">{branch.email || 'N/A'}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm text-muted-foreground flex items-center gap-2">
                <Phone className="h-4 w-4" /> Phone Number
              </span>
              <p className="font-medium">{branch.phoneNumber || 'N/A'}</p>
            </div>
            <div className="space-y-1">
              <span className="text-sm text-muted-foreground flex items-center gap-2">
                <Globe className="h-4 w-4" /> Time Zone
              </span>
              <p className="font-medium">{branch.timeZone}</p>
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
                <p>{branch.address}</p>
              </address>
              
              <div className="mt-4 pt-4 border-t space-y-2">
                <div className="text-sm flex items-center gap-2 text-muted-foreground">
                  <Map className="h-4 w-4" /> Geo-fencing
                </div>
                <div className="grid grid-cols-2 text-sm">
                  <div>
                    <span className="text-muted-foreground block text-xs">Latitude</span>
                    {branch.latitude}
                  </div>
                  <div>
                    <span className="text-muted-foreground block text-xs">Longitude</span>
                    {branch.longitude}
                  </div>
                </div>
                <div className="text-sm">
                  <span className="text-muted-foreground block text-xs">Allowed Radius</span>
                  {branch.radiusInMeters} meters
                </div>
              </div>
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
                  <span className="text-sm text-muted-foreground">Departments</span>
                  <span className="font-medium">{branch.totalDepartments}</span>
                </div>
                <div className="flex justify-between items-center border-b pb-2">
                  <span className="text-sm text-muted-foreground">Employees</span>
                  <span className="font-medium">{branch.totalEmployees}</span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-muted-foreground">Devices</span>
                  <span className="font-medium">{branch.totalAttendanceDevices}</span>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
};
