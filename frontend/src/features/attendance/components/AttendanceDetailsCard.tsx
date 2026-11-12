import { format } from 'date-fns';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { AttendanceStatusBadge } from './AttendanceStatusBadge';
import { CheckInMethod, type AttendanceDetailsDto } from '../types';

interface AttendanceDetailsCardProps {
  attendance: AttendanceDetailsDto;
}

const getMethodName = (method?: CheckInMethod | string | number) => {
  const m = String(method || '').toLowerCase();
  switch (m) {
    case 'facerecognition':
    case '1':
      return 'Face Recognition';
    case 'gps':
    case '2':
      return 'GPS';
    case 'biometricdevice':
    case '3':
      return 'Biometric Device';
    case 'web':
    case '4':
      return 'Web';
    case 'mobile':
    case '5':
      return 'Mobile';
    case 'manual':
    case '6':
      return 'Manual';
    default:
      return String(method || 'Unknown');
  }
};

function formatUtcDateTime(dateStr?: string | null, formatPattern: string = 'PPpp'): string {
  if (!dateStr) return '-';
  let s = dateStr.trim();
  if (s.includes('T') && !s.endsWith('Z') && !s.includes('+') && !s.match(/-\d\d:\d\d$/)) {
    s += 'Z';
  }
  const d = new Date(s);
  if (isNaN(d.getTime())) return dateStr;
  return format(d, formatPattern);
}

function formatDuration(minutes: number): string {
  if (!minutes || minutes <= 0) return '0m';
  const h = Math.floor(minutes / 60);
  const m = Math.floor(minutes % 60);
  if (h > 0 && m > 0) return `${h}h ${m}m`;
  if (h > 0) return `${h}h`;
  return `${m}m`;
}

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
          <dl className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-x-4 gap-y-6">
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Employee</dt>
              <dd className="mt-1 text-sm font-semibold text-gray-900">
                {attendance.employeeName || 'Unknown Employee'}
              </dd>
              <dd className="text-xs text-gray-500 font-mono">
                {attendance.employeeNumber ? `ID: ${attendance.employeeNumber}` : attendance.employeeId}
              </dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Shift</dt>
              <dd className="mt-1 text-sm text-gray-900">
                {attendance.shiftName || attendance.shiftId || 'Standard Shift'}
              </dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Policy</dt>
              <dd className="mt-1 text-sm text-gray-900">
                {attendance.policyName || attendance.attendancePolicyId || 'Standard Policy'}
              </dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Total Working Time</dt>
              <dd className="mt-1 text-sm text-gray-900">{formatDuration(attendance.workingMinutes)}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Late</dt>
              <dd className="mt-1 text-sm text-gray-900">
                {attendance.lateMinutes > 0 ? <span className="text-yellow-600">{formatDuration(attendance.lateMinutes)}</span> : '0m'}
              </dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Early Leave</dt>
              <dd className="mt-1 text-sm text-gray-900">
                {attendance.earlyLeaveMinutes > 0 ? <span className="text-orange-600">{formatDuration(attendance.earlyLeaveMinutes)}</span> : '0m'}
              </dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Overtime</dt>
              <dd className="mt-1 text-sm text-gray-900">{formatDuration(attendance.overtimeMinutes)}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Excess Break</dt>
              <dd className="mt-1 text-sm text-gray-900">
                {attendance.excessBreakMinutes > 0 ? <span className="text-red-600">{formatDuration(attendance.excessBreakMinutes)}</span> : '0m'}
              </dd>
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
                <dd className="mt-1 text-sm text-gray-900">{formatUtcDateTime(attendance.checkInTime, 'PPpp')}</dd>
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
                  <dd className="mt-1 text-sm text-gray-900">{formatUtcDateTime(attendance.checkOutTime, 'PPpp')}</dd>
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
