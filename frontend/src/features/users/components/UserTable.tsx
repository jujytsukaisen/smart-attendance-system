import { useState } from 'react';
import { Eye, Edit2, ShieldAlert, ShieldCheck, Shield, Trash2 } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/shared/components/ui/button';
import { UserStatusBadge } from './UserStatusBadge';
import { useActivateUser, useSuspendUser, useDeleteUser } from '../hooks';
import type { UserListDto } from '../types';
import { ActionDialog } from '@/features/leaves/components/ActionDialog';
import { toast } from 'sonner';
import { AssignRolesDialog } from './AssignRolesDialog';

interface UserTableProps {
  data: UserListDto[];
}

export function UserTable({ data }: UserTableProps) {
  const navigate = useNavigate();
  const activateMutation = useActivateUser();
  const suspendMutation = useSuspendUser();
  const deleteMutation = useDeleteUser();

  const [activateId, setActivateId] = useState<string | null>(null);
  const [suspendId, setSuspendId] = useState<string | null>(null);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [assignRolesUser, setAssignRolesUser] = useState<UserListDto | null>(null);

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

  const handleDelete = () => {
    if (!deleteId) return;
    deleteMutation.mutate(deleteId, {
      onSuccess: () => {
        toast.success('User deleted successfully');
        setDeleteId(null);
      },
      onError: () => {
        toast.error('Failed to delete user');
        setDeleteId(null);
      },
    });
  };

  return (
    <>
      <div className="bg-card text-card-foreground shadow border overflow-hidden sm:rounded-lg">
        <table className="min-w-full divide-y divide-border">
          <thead className="bg-muted/50">
            <tr>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                Username
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                Email
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                Status
              </th>
              <th scope="col" className="relative px-6 py-3">
                <span className="sr-only">Actions</span>
              </th>
            </tr>
          </thead>
          <tbody className="bg-card divide-y divide-border">
            {data.map((user) => (
              <tr key={user.id} className="hover:bg-muted transition-colors">
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm font-medium text-foreground">{user.username}</div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm text-muted-foreground">{user.email}</div>
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
                    <Button variant="ghost" size="icon" onClick={() => setAssignRolesUser(user)} title="Assign Roles">
                      <Shield className="h-4 w-4 text-blue-500" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => navigate(`/users/${user.id}/edit`)}>
                      <Edit2 className="h-4 w-4 text-muted-foreground" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => navigate(`/users/${user.id}`)}>
                      <Eye className="h-4 w-4" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => setDeleteId(user.id)} title="Delete User">
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

      <ActionDialog
        isOpen={!!deleteId}
        onClose={() => setDeleteId(null)}
        onConfirm={handleDelete}
        title="Delete User"
        description="Are you sure you want to delete this user? This action cannot be undone."
        confirmText="Delete"
        variant="destructive"
        isLoading={deleteMutation.isPending}
      />

      {assignRolesUser && (
        <AssignRolesDialog
          userId={assignRolesUser.id}
          currentRoles={assignRolesUser.roles}
          isOpen={!!assignRolesUser}
          onClose={() => setAssignRolesUser(null)}
        />
      )}
    </>
  );
}
