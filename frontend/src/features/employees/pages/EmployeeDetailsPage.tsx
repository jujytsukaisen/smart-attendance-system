import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, Edit, Trash, Activity, PowerOff, User, MapPin, Briefcase } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { EmployeeStatusBadge, ActionDialog } from '../components';
import { useEmployee, useActivateEmployee, useDeactivateEmployee, useDeleteEmployee } from '../hooks';


export const EmployeeDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [dialogAction, setDialogAction] = useState<'delete' | 'activate' | 'deactivate' | null>(null);

  const { data: employee, isLoading, isError, error } = useEmployee(id!);
  const activateMutation = useActivateEmployee();
  const deactivateMutation = useDeactivateEmployee();
  const deleteMutation = useDeleteEmployee();

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
      <div className="flex justify-center p-8">
        <div className="animate-spin h-8 w-8 border-4 border-primary border-t-transparent rounded-full" />
      </div>
    );
  }

  if (!employee) {
    return <div className="p-8 text-center text-muted-foreground">Employee not found</div>;
  }

  const handleAction = async () => {
    try {
      if (dialogAction === 'delete') {
        await deleteMutation.mutateAsync(id!);
        navigate('/employees');
      } else if (dialogAction === 'activate') {
        await activateMutation.mutateAsync(id!);
        setDialogAction(null);
      } else if (dialogAction === 'deactivate') {
        await deactivateMutation.mutateAsync(id!);
        setDialogAction(null);
      }
    } catch (error) {
      // Error handled by hook
    }
  };

  const getDialogConfig = () => {
    switch (dialogAction) {
      case 'delete':
        return {
          title: 'Delete Employee',
          description: 'Are you sure you want to delete this employee? This can be undone later via restore.',
          confirmText: 'Delete',
          variant: 'danger' as const,
          isLoading: deleteMutation.isPending,
        };
      case 'activate':
        return {
          title: 'Activate Employee',
          description: 'Are you sure you want to activate this employee?',
          confirmText: 'Activate',
          variant: 'default' as const,
          isLoading: activateMutation.isPending,
        };
      case 'deactivate':
        return {
          title: 'Deactivate Employee',
          description: 'Are you sure you want to deactivate this employee? They will not be able to log in.',
          confirmText: 'Deactivate',
          variant: 'danger' as const,
          isLoading: deactivateMutation.isPending,
        };
      default:
        return null;
    }
  };

  const dialogConfig = getDialogConfig();

  return (
    <div className="space-y-6 max-w-5xl mx-auto">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div className="flex items-center gap-4">
          <Button variant="outline" size="icon" onClick={() => navigate('/employees')}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-3xl font-bold tracking-tight">
                {employee.firstName} {employee.lastName}
              </h1>
              <EmployeeStatusBadge status={employee.employeeStatus} />
            </div>
            <p className="text-muted-foreground mt-1">
              {employee.employeeNumber} | {employee.jobTitle}
            </p>
          </div>
        </div>
        <div className="flex gap-2">
          {employee.employeeStatus === 0 ? (
            <Button variant="outline" onClick={() => setDialogAction('deactivate')}>
              <PowerOff className="mr-2 h-4 w-4" /> Deactivate
            </Button>
          ) : (
            <Button variant="outline" onClick={() => setDialogAction('activate')}>
              <Activity className="mr-2 h-4 w-4" /> Activate
            </Button>
          )}
          <Button variant="outline" onClick={() => navigate(`/employees/${id}/edit`)}>
            <Edit className="mr-2 h-4 w-4" /> Edit
          </Button>
          <Button variant="destructive" onClick={() => setDialogAction('delete')}>
            <Trash className="mr-2 h-4 w-4" /> Delete
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center">
              <User className="mr-2 h-4 w-4" /> Personal Information
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="grid grid-cols-2 text-sm gap-y-2">
              <div className="text-muted-foreground">Email</div>
              <div className="font-medium truncate" title={employee.email}>{employee.email}</div>

              <div className="text-muted-foreground">Phone</div>
              <div className="font-medium">{employee.phoneNumber}</div>

              <div className="text-muted-foreground">Date of Birth</div>
              <div className="font-medium">{new Date(employee.dateOfBirth).toLocaleDateString()}</div>

              <div className="text-muted-foreground">Gender</div>
              <div className="font-medium">{employee.gender === 0 ? 'Male' : employee.gender === 1 ? 'Female' : 'Other'}</div>

              {employee.nationalId && (
                <>
                  <div className="text-muted-foreground">National ID</div>
                  <div className="font-medium">{employee.nationalId}</div>
                </>
              )}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center">
              <Briefcase className="mr-2 h-4 w-4" /> Job Information
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="grid grid-cols-2 text-sm gap-y-2">
              <div className="text-muted-foreground">Company</div>
              <div className="font-medium">{employee.companyName || employee.companyId}</div>

              <div className="text-muted-foreground">Branch</div>
              <div className="font-medium">{employee.branchName || employee.branchId}</div>

              <div className="text-muted-foreground">Department</div>
              <div className="font-medium">{employee.departmentName || employee.departmentId}</div>

              <div className="text-muted-foreground">Hire Date</div>
              <div className="font-medium">{new Date(employee.hireDate).toLocaleDateString()}</div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground flex items-center">
              <MapPin className="mr-2 h-4 w-4" /> Address
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="grid grid-cols-2 text-sm gap-y-2">
              <div className="text-muted-foreground">Street</div>
              <div className="font-medium truncate" title={employee.street}>{employee.street}</div>

              <div className="text-muted-foreground">City</div>
              <div className="font-medium">{employee.city}</div>

              <div className="text-muted-foreground">State</div>
              <div className="font-medium">{employee.state}</div>

              <div className="text-muted-foreground">Country</div>
              <div className="font-medium">{employee.country}</div>

              <div className="text-muted-foreground">Zip Code</div>
              <div className="font-medium">{employee.zipCode}</div>
            </div>
          </CardContent>
        </Card>
      </div>

      {dialogConfig && (
        <ActionDialog
          isOpen={!!dialogAction}
          onClose={() => setDialogAction(null)}
          onConfirm={handleAction}
          title={dialogConfig.title}
          description={dialogConfig.description}
          confirmText={dialogConfig.confirmText}
          variant={dialogConfig.variant}
          isLoading={dialogConfig.isLoading}
        />
      )}
    </div>
  );
};
