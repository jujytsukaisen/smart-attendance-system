import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { LeaveDetailsCard } from '../components';
import { useLeaveDetails } from '../hooks';

export function LeaveDetailsPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { data, isLoading } = useLeaveDetails(id!);

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
        <p className="text-red-500">Failed to load leave details.</p>
        <Button variant="outline" onClick={() => navigate('/leaves')}>Back to Leaves</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate('/leaves')}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Leave Details</h1>
          <p className="mt-1 text-sm text-gray-500">
            View complete information for this leave request.
          </p>
        </div>
      </div>

      <LeaveDetailsCard leave={data} />
    </div>
  );
}
