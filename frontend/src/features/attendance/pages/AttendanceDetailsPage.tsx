import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { AttendanceDetailsCard, AttendanceTimeline, EmployeeStatusBadge } from '../components';
import { useAttendanceDetails } from '../hooks';

export function AttendanceDetailsPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { data, isLoading } = useAttendanceDetails(id!);

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
        <p className="text-red-500">Failed to load attendance details.</p>
        <Button variant="outline" onClick={() => navigate('/attendance')}>Back to Attendance List</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => navigate('/attendance')}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-semibold text-gray-900">Attendance Details</h1>
              {data.currentState && (
                <EmployeeStatusBadge state={data.currentState} />
              )}
            </div>
            <p className="mt-1 text-sm text-gray-500">
              View complete daily timeline, breaks, check-in, and check-out information.
            </p>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <AttendanceDetailsCard attendance={data} />
        </div>
        <div className="lg:col-span-1">
          <AttendanceTimeline events={data.timeline ?? []} />
        </div>
      </div>
    </div>
  );
}

