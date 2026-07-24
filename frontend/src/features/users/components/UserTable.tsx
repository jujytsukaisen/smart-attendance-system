import { useState } from 'react';
import { Eye, Edit2, ShieldAlert, ShieldCheck } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/shared/components/ui/button';
import { UserStatusBadge } from './UserStatusBadge';
import { useActivateUser, useSuspendUser } from '../hooks';
import type { UserListDto } from '../types';
import { ActionDialog } from '@/features/leaves/components/ActionDialog';
import { toast } from 'sonner';

interface UserTableProps {
  data: UserListDto[];
}

export function UserTable({ data }: UserTableProps) {
  const navigate = useNavigate();
  const activateMutation = useActivateUser();
  const suspendMutation = useSuspendUser();

  const [activateId, setActivateId] = useState<string | null>(null);
  const [suspendId, setSuspendId] = useState<string | null>(null);

  const handleActivate = () => {
    if (!activateId) return;
    activateMutation.mutate(activateId, {
      onSuccess: () => {
        toast.success('User activated successfully');
        setActivateId(null);
      },
      onError: () => {
        toast.error('Failed to activate user');
        setActivateId(null);
      },
    });
  };

  const handleSuspend = () => {
    if (!suspendId) return;
    suspendMutation.mutate(suspendId, {
      onSuccess: () => {
        toast.success('User suspended successfully');
        setSuspendId(null);
      },
      onError: () => {
        toast.error('Failed to suspend user');
        setSuspendId(null);
      },
    });
  };

  return (
    <>
      <div className="bg-white shadow overflow-hidden sm:rounded-lg">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Username
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Email
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
            {data.map((user) => (
              <tr key={user.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm font-medium text-gray-900">{user.username}</div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm text-gray-500">{user.email}</div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <UserStatusBadge isActive={user.isActive} />
                </td>
                <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                  <div className="flex justify-end gap-2">
                    {user.isActive ? (
                      <Button variant="ghost" size="icon" onClick={() => setSuspendId(user.id)} title="Suspend User">
                        <ShieldAlert className="h-4 w-4 text-red-500" />
                      </Button>
                    ) : (
                      <Button variant="ghost" size="icon" onClick={() => setActivateId(user.id)} title="Activate User">
                        <ShieldCheck className="h-4 w-4 text-green-500" />
                      </Button>
                    )}
                    <Button variant="ghost" size="icon" onClick={() => navigate(`/users/${user.id}/edit`)}>
                      <Edit2 className="h-4 w-4 text-gray-500" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => navigate(`/users/${user.id}`)}>
                      <Eye className="h-4 w-4" />
                    </Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <ActionDialog
        isOpen={!!activateId}
        onClose={() => setActivateId(null)}
        onConfirm={handleActivate}
        title="Activate User"
        description="Are you sure you want to activate this user? They will be able to log in again."
        confirmText="Activate"
        variant="default"
        isLoading={activateMutation.isPending}
      />

      <ActionDialog
        isOpen={!!suspendId}
        onClose={() => setSuspendId(null)}
        onConfirm={handleSuspend}
        title="Suspend User"
        description="Are you sure you want to suspend this user? They will no longer be able to log in."
        confirmText="Suspend"
        variant="destructive"
        isLoading={suspendMutation.isPending}
      />
    </>
  );
}
