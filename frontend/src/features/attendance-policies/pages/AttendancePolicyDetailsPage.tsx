import { useParams, Link, useNavigate } from 'react-router-dom';
import { ArrowLeft, Edit2 } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { AttendancePolicyDetailsCard } from '../components';
import { useAttendancePolicy } from '../hooks';

export function AttendancePolicyDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: policy, isLoading } = useAttendancePolicy(id!);

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-6">
        <div className="h-8 bg-gray-200 rounded w-1/4"></div>
        <div className="h-64 bg-gray-100 rounded w-full"></div>
      </div>
    );
  }

  if (!policy) {
    return (
      <div className="text-center py-12 bg-red-50 rounded-lg border border-red-200">
        <p className="text-red-500">Failed to load attendance policy details.</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate('/attendance-policies')}>
          Go Back
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div className="flex items-center gap-4">
          <Button variant="ghost" onClick={() => navigate('/attendance-policies')} className="p-2">
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-2xl font-semibold text-gray-900">{policy.name}</h1>
            <p className="mt-1 text-sm text-gray-500">
              Policy Code: {policy.code}
            </p>
          </div>
        </div>
        <Link to={`/attendance-policies/${policy.id}/edit`}>
          <Button>
            <Edit2 className="h-4 w-4 mr-2" />
            Edit Policy
          </Button>
        </Link>
      </div>

      <AttendancePolicyDetailsCard policy={policy} />
    </div>
  );
}
