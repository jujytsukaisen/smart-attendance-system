import { format } from 'date-fns';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { AttendanceStatusBadge } from './AttendanceStatusBadge';
import { CheckInMethod, type AttendanceDetailsDto } from '../types';

interface AttendanceDetailsCardProps {
  attendance: AttendanceDetailsDto;
}

const getMethodName = (method?: CheckInMethod) => {
  switch (method) {
    case CheckInMethod.FaceRecognition: return 'Face Recognition';
    case CheckInMethod.GPS: return 'GPS';
    case CheckInMethod.BiometricDevice: return 'Biometric Device';
    case CheckInMethod.Web: return 'Web';
    case CheckInMethod.Mobile: return 'Mobile';
    case CheckInMethod.Manual: return 'Manual';
    default: return 'Unknown';
  }
};

export function AttendanceDetailsCard({ attendance }: AttendanceDetailsCardProps) {
  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Attendance Record Details</CardTitle>
            <AttendanceStatusBadge status={attendance.attendanceStatus} />
          </div>
        </CardHeader>
        <CardContent>
          <dl className="grid grid-cols-1 sm:grid-cols-2 gap-x-4 gap-y-8">
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Employee ID</dt>
              <dd className="mt-1 text-sm text-gray-900">{attendance.employeeId}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Total Working Time</dt>
              <dd className="mt-1 text-sm text-gray-900">{Math.floor(attendance.workingMinutes / 60)}h {attendance.workingMinutes % 60}m</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Late / Early Leave</dt>
              <dd className="mt-1 text-sm text-gray-900">
                {attendance.lateMinutes > 0 && <span className="text-yellow-600 block">{attendance.lateMinutes} mins late</span>}
                {attendance.earlyLeaveMinutes > 0 && <span className="text-orange-600 block">{attendance.earlyLeaveMinutes} mins early leave</span>}
                {attendance.lateMinutes === 0 && attendance.earlyLeaveMinutes === 0 && 'On time'}
              </dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Overtime</dt>
              <dd className="mt-1 text-sm text-gray-900">{attendance.overtimeMinutes} mins</dd>
            </div>
          </dl>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Check-In Info</CardTitle>
          </CardHeader>
          <CardContent>
            <dl className="space-y-4">
              <div>
                <dt className="text-sm font-medium text-gray-500">Time</dt>
                <dd className="mt-1 text-sm text-gray-900">{format(new Date(attendance.checkInTime), 'PPpp')}</dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Method</dt>
                <dd className="mt-1 text-sm text-gray-900">{getMethodName(attendance.checkInMethod)}</dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">Device Info</dt>
                <dd className="mt-1 text-sm text-gray-900">{attendance.checkInDeviceInfo || 'N/A'}</dd>
              </div>
              <div>
                <dt className="text-sm font-medium text-gray-500">IP Address</dt>
                <dd className="mt-1 text-sm text-gray-900">{attendance.checkInIPAddress || 'N/A'}</dd>
              </div>
            </dl>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Check-Out Info</CardTitle>
          </CardHeader>
          <CardContent>
            {attendance.checkOutTime ? (
              <dl className="space-y-4">
                <div>
                  <dt className="text-sm font-medium text-gray-500">Time</dt>
                  <dd className="mt-1 text-sm text-gray-900">{format(new Date(attendance.checkOutTime), 'PPpp')}</dd>
                </div>
                <div>
                  <dt className="text-sm font-medium text-gray-500">Method</dt>
                  <dd className="mt-1 text-sm text-gray-900">{getMethodName(attendance.checkOutMethod)}</dd>
                </div>
                <div>
                  <dt className="text-sm font-medium text-gray-500">Device Info</dt>
                  <dd className="mt-1 text-sm text-gray-900">{attendance.checkOutDeviceInfo || 'N/A'}</dd>
                </div>
                <div>
                  <dt className="text-sm font-medium text-gray-500">IP Address</dt>
                  <dd className="mt-1 text-sm text-gray-900">{attendance.checkOutIPAddress || 'N/A'}</dd>
                </div>
              </dl>
            ) : (
              <div className="text-center py-8 text-gray-500">
                Has not checked out yet.
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
