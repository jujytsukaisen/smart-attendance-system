import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Edit } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { RoleDetailsCard } from '../components';
import { useRoleDetails } from '../hooks';

export function RoleDetailsPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { data, isLoading } = useRoleDetails(id!);

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="h-10 w-32 bg-gray-200 rounded animate-pulse mb-6" />
        <div className="h-64 bg-gray-200 rounded animate-pulse" />
      </div>
    );
  }

  if (!data) {
    return (
      <div className="flex flex-col items-center justify-center h-64 space-y-4">
        <p className="text-red-500">Failed to load role details.</p>
        <Button variant="outline" onClick={() => navigate('/roles')}>Back to Roles</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate('/roles')}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-2xl font-semibold text-gray-900">Role Details</h1>
            <p className="mt-1 text-sm text-gray-500">
              View role information and permissions.
            </p>
          </div>
        </div>
        
        <Button onClick={() => navigate(`/roles/${id}/edit`)}>
          <Edit className="w-4 h-4 mr-2" />
          Edit Role & Permissions
        </Button>
      </div>

      <RoleDetailsCard role={data} />
    </div>
  );
}
