import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Edit2, Trash2, Eye, Play, Square } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { ActionDialog } from './ActionDialog';
import { AttendancePolicyStatusBadge } from './AttendancePolicyStatusBadge';
import { toast } from 'sonner';
import {
  useDeleteAttendancePolicy,
  useActivateAttendancePolicy,
  useDeactivateAttendancePolicy,
} from '../hooks';
import type { AttendancePolicyListDto } from '../types';
import { AttendanceMode } from '../types';

interface AttendancePolicyTableProps {
  policies: AttendancePolicyListDto[];
  isLoading: boolean;
}

export function AttendancePolicyTable({ policies, isLoading }: AttendancePolicyTableProps) {
  const deleteMutation = useDeleteAttendancePolicy();
  const activateMutation = useActivateAttendancePolicy();
  const deactivateMutation = useDeactivateAttendancePolicy();

  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [toggleStatusId, setToggleStatusId] = useState<string | null>(null);
  const [isActivating, setIsActivating] = useState(false);

  const handleDelete = () => {
    if (!deleteId) return;
    deleteMutation.mutate(deleteId, {
      onSuccess: () => {
        toast.success('Policy deleted successfully');
        setDeleteId(null);
      },
      onError: () => {
        toast.error('Failed to delete policy');
        setDeleteId(null);
      },
    });
  };

  const handleToggleStatus = () => {
    if (!toggleStatusId) return;
    const mutation = isActivating ? activateMutation : deactivateMutation;
    const actionText = isActivating ? 'activated' : 'deactivated';
    
    mutation.mutate(toggleStatusId, {
      onSuccess: () => {
        toast.success(`Policy ${actionText} successfully`);
        setToggleStatusId(null);
      },
      onError: () => {
        toast.error(`Failed to ${isActivating ? 'activate' : 'deactivate'} policy`);
        setToggleStatusId(null);
      },
    });
  };

  const getAttendanceModeText = (mode: AttendanceMode) => {
    switch (mode) {
      case AttendanceMode.Office: return 'Office';
      case AttendanceMode.Field: return 'Field';
      case AttendanceMode.Hybrid: return 'Hybrid';
      case AttendanceMode.Remote: return 'Remote';
      case AttendanceMode.Flexible: return 'Flexible';
      default: return 'Unknown';
    }
  };

  if (isLoading) {
    return (
      <div className="animate-pulse">
        <div className="h-10 bg-gray-200 rounded w-full mb-4"></div>
        {[...Array(5)].map((_, i) => (
          <div key={i} className="h-16 bg-gray-100 rounded w-full mb-2"></div>
        ))}
      </div>
    );
  }

  if (!policies?.length) {
    return (
      <div className="text-center py-12 bg-white rounded-lg border border-gray-200">
        <p className="text-gray-500">No attendance policies found.</p>
      </div>
    );
  }

  return (
    <>
      <div className="bg-white shadow overflow-hidden sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Name / Code
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Mode
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th scope="col" className="relative px-6 py-3">
                <span className="sr-only">Actions</span>
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {policies.map((policy) => (
              <tr key={policy.id} className="hover:bg-gray-50 transition-colors duration-150">
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm font-medium text-gray-900">{policy.name}</div>
                  <div className="text-sm text-gray-500">{policy.code}</div>
                  {policy.isDefault && (
                    <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800 mt-1">
                      Default
                    </span>
                  )}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span className="text-sm text-gray-900">
                    {getAttendanceModeText(policy.attendanceMode)}
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <AttendancePolicyStatusBadge isActive={policy.isActive} />
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <div className="flex justify-end gap-2">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => {
                        setToggleStatusId(policy.id);
                        setIsActivating(!policy.isActive);
                      }}
                      title={policy.isActive ? 'Deactivate' : 'Activate'}
                    >
                      {policy.isActive ? (
                        <Square className="h-4 w-4 text-orange-500" />
                      ) : (
                        <Play className="h-4 w-4 text-green-500" />
                      )}
                    </Button>
                    <Link to={`/attendance-policies/${policy.id}`}>
                      <Button variant="ghost" size="sm">
                        <Eye className="h-4 w-4 text-blue-500" />
                      </Button>
                    </Link>
                    <Link to={`/attendance-policies/${policy.id}/edit`}>
                      <Button variant="ghost" size="sm">
                        <Edit2 className="h-4 w-4 text-gray-500" />
                      </Button>
                    </Link>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setDeleteId(policy.id)}
                    >
                      <Trash2 className="h-4 w-4 text-red-500" />
                    </Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <ActionDialog
        isOpen={!!deleteId}
        onClose={() => setDeleteId(null)}
        onConfirm={handleDelete}
        title="Delete Policy"
        description="Are you sure you want to delete this attendance policy? This action cannot be undone."
        confirmText="Delete"
        variant="destructive"
        isLoading={deleteMutation.isPending}
      />

      <ActionDialog
        isOpen={!!toggleStatusId}
        onClose={() => setToggleStatusId(null)}
        onConfirm={handleToggleStatus}
        title={`${isActivating ? 'Activate' : 'Deactivate'} Policy`}
        description={`Are you sure you want to ${isActivating ? 'activate' : 'deactivate'} this policy?`}
        confirmText={isActivating ? 'Activate' : 'Deactivate'}
        variant={isActivating ? 'default' : 'destructive'}
        isLoading={isActivating ? activateMutation.isPending : deactivateMutation.isPending}
      />
    </>
  );
}
