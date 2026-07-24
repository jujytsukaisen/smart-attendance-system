import type { AttendancePolicyDetailsDto } from '../types';
import { AttendanceMode, RoundTimeRule, MissingPunchPolicy, WeekendPolicy, HolidayPolicy } from '../types';
import { AttendancePolicyStatusBadge } from './AttendancePolicyStatusBadge';

interface AttendancePolicyDetailsCardProps {
  policy: AttendancePolicyDetailsDto;
}

export function AttendancePolicyDetailsCard({ policy }: AttendancePolicyDetailsCardProps) {
  
  const getEnumLabel = (enumType: any, value: number) => {
    return Object.keys(enumType).find(key => enumType[key] === value) || 'Unknown';
  };

  return (
    <div className="bg-white shadow overflow-hidden sm:rounded-lg">
      <div className="px-4 py-5 sm:px-6 flex justify-between items-center">
        <div>
          <h3 className="text-lg leading-6 font-medium text-gray-900">
            Attendance Policy Details
          </h3>
          <p className="mt-1 max-w-2xl text-sm text-gray-500">
            Comprehensive details and rules configuration.
          </p>
        </div>
        <AttendancePolicyStatusBadge isActive={policy.isActive} />
      </div>
      <div className="border-t border-gray-200 px-4 py-5 sm:p-0">
        <dl className="sm:divide-y sm:divide-gray-200">
          <div className="py-4 sm:py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
            <dt className="text-sm font-medium text-gray-500">Name / Code</dt>
            <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
              {policy.name} ({policy.code}) {policy.isDefault && <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">Default</span>}
            </dd>
          </div>
          <div className="py-4 sm:py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6 bg-gray-50">
            <dt className="text-sm font-medium text-gray-500">Description</dt>
            <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
              {policy.description || 'N/A'}
            </dd>
          </div>
          <div className="py-4 sm:py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
            <dt className="text-sm font-medium text-gray-500">Attendance Mode</dt>
            <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
              {getEnumLabel(AttendanceMode, policy.attendanceMode)}
            </dd>
          </div>
          
          {/* Validation & Requirements */}
          <div className="py-4 sm:py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6 bg-gray-50">
            <dt className="text-sm font-medium text-gray-500">Validation Requirements</dt>
            <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
              <ul className="border border-gray-200 rounded-md divide-y divide-gray-200">
                <li className="pl-3 pr-4 py-3 flex items-center justify-between text-sm">
                  <div className="w-0 flex-1 flex items-center">
                    <span className="ml-2 flex-1 w-0 truncate">Face Recognition</span>
                  </div>
                  <div className="ml-4 flex-shrink-0">
                    <span className={policy.requireFaceRecognition ? 'text-green-600 font-medium' : 'text-gray-400'}>
                      {policy.requireFaceRecognition ? 'Required' : 'Optional'}
                    </span>
                  </div>
                </li>
                <li className="pl-3 pr-4 py-3 flex items-center justify-between text-sm">
                  <div className="w-0 flex-1 flex items-center">
                    <span className="ml-2 flex-1 w-0 truncate">GPS Verification</span>
                  </div>
                  <div className="ml-4 flex-shrink-0">
                    <span className={policy.requireGPS ? 'text-green-600 font-medium' : 'text-gray-400'}>
                      {policy.requireGPS ? 'Required' : 'Optional'}
                    </span>
                  </div>
                </li>
                <li className="pl-3 pr-4 py-3 flex items-center justify-between text-sm">
                  <div className="w-0 flex-1 flex items-center">
                    <span className="ml-2 flex-1 w-0 truncate">Approved Device</span>
                  </div>
                  <div className="ml-4 flex-shrink-0">
                    <span className={policy.requireApprovedDevice ? 'text-green-600 font-medium' : 'text-gray-400'}>
                      {policy.requireApprovedDevice ? 'Required' : 'Optional'}
                    </span>
                  </div>
                </li>
                <li className="pl-3 pr-4 py-3 flex items-center justify-between text-sm">
                  <div className="w-0 flex-1 flex items-center">
                    <span className="ml-2 flex-1 w-0 truncate">Company Network</span>
                  </div>
                  <div className="ml-4 flex-shrink-0">
                    <span className={policy.requireCompanyNetwork ? 'text-green-600 font-medium' : 'text-gray-400'}>
                      {policy.requireCompanyNetwork ? 'Required' : 'Optional'}
                    </span>
                  </div>
                </li>
              </ul>
            </dd>
          </div>

          {/* Timing Rules */}
          <div className="py-4 sm:py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
            <dt className="text-sm font-medium text-gray-500">Timing Rules</dt>
            <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <span className="block text-gray-500 text-xs">Late Tolerance</span>
                  <span className="block font-medium">{policy.lateToleranceMinutes} min</span>
                </div>
                <div>
                  <span className="block text-gray-500 text-xs">Early Leave Tolerance</span>
                  <span className="block font-medium">{policy.earlyLeaveToleranceMinutes} min</span>
                </div>
                <div>
                  <span className="block text-gray-500 text-xs">Grace Period</span>
                  <span className="block font-medium">{policy.gracePeriodMinutes} min</span>
                </div>
                <div>
                  <span className="block text-gray-500 text-xs">Auto Checkout</span>
                  <span className="block font-medium">
                    {policy.autoCheckoutEnabled ? `Yes (${policy.autoCheckoutTime || 'No Time Set'})` : 'No'}
                  </span>
                </div>
              </div>
            </dd>
          </div>

          {/* Overtime Rules */}
          <div className="py-4 sm:py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6 bg-gray-50">
            <dt className="text-sm font-medium text-gray-500">Overtime Rules</dt>
            <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <span className="block text-gray-500 text-xs">Threshold</span>
                  <span className="block font-medium">{policy.overtimeThresholdMinutes} min</span>
                </div>
                <div>
                  <span className="block text-gray-500 text-xs">Minimum</span>
                  <span className="block font-medium">{policy.minimumOvertimeMinutes} min</span>
                </div>
                <div>
                  <span className="block text-gray-500 text-xs">Maximum</span>
                  <span className="block font-medium">{policy.maximumOvertimeMinutes} min</span>
                </div>
              </div>
            </dd>
          </div>

          {/* Policies */}
          <div className="py-4 sm:py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
            <dt className="text-sm font-medium text-gray-500">Policies & Processing</dt>
            <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <span className="block text-gray-500 text-xs">Round Time Rule</span>
                  <span className="block font-medium">{getEnumLabel(RoundTimeRule, policy.roundTimeRule)}</span>
                </div>
                <div>
                  <span className="block text-gray-500 text-xs">Missing Punch Policy</span>
                  <span className="block font-medium">{getEnumLabel(MissingPunchPolicy, policy.missingPunchPolicy)}</span>
                </div>
                <div>
                  <span className="block text-gray-500 text-xs">Weekend Policy</span>
                  <span className="block font-medium">{getEnumLabel(WeekendPolicy, policy.weekendPolicy)}</span>
                </div>
                <div>
                  <span className="block text-gray-500 text-xs">Holiday Policy</span>
                  <span className="block font-medium">{getEnumLabel(HolidayPolicy, policy.holidayPolicy)}</span>
                </div>
              </div>
            </dd>
          </div>

        </dl>
      </div>
    </div>
  );
}
