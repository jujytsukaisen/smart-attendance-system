import { useState } from 'react';
import { Eye, Edit2, ShieldAlert, ShieldCheck, Trash2 } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { ActionDialog } from '@/features/leaves/components/ActionDialog';
import { useActivateRole, useDeactivateRole, useDeleteRole } from '../hooks';
import type { RoleListDto } from '../types';
import { toast } from 'sonner';

interface RoleTableProps {
  data: RoleListDto[];
}

export function RoleTable({ data }: RoleTableProps) {
  const navigate = useNavigate();
  const activateMutation = useActivateRole();
  const deactivateMutation = useDeactivateRole();
  const deleteMutation = useDeleteRole();

  const [activateId, setActivateId] = useState<string | null>(null);
  const [deactivateId, setDeactivateId] = useState<string | null>(null);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const handleActivate = () => {
    if (!activateId) return;
    activateMutation.mutate(activateId, {
      onSuccess: () => {
        toast.success('Role activated successfully');
        setActivateId(null);
      },
      onError: () => {
        toast.error('Failed to activate role');
        setActivateId(null);
      },
    });
  };

  const handleDeactivate = () => {
    if (!deactivateId) return;
    deactivateMutation.mutate(deactivateId, {
      onSuccess: () => {
        toast.success('Role deactivated successfully');
        setDeactivateId(null);
      },
      onError: () => {
        toast.error('Failed to deactivate role');
        setDeactivateId(null);
      },
    });
  };

  const handleDelete = () => {
    if (!deleteId) return;
    deleteMutation.mutate(deleteId, {
      onSuccess: () => {
        toast.success('Role deleted successfully');
        setDeleteId(null);
      },
      onError: () => {
        toast.error('Failed to delete role');
        setDeleteId(null);
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
                Name
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Description
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
            {data.map((role) => (
              <tr key={role.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm font-medium text-gray-900">{role.name}</div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm text-gray-500 truncate max-w-xs">{role.description}</div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  {role.isActive ? (
                    <Badge variant="default" className="bg-green-100 text-green-800 hover:bg-green-100">Active</Badge>
                  ) : (
                    <Badge variant="destructive" className="bg-red-100 text-red-800 hover:bg-red-100">Inactive</Badge>
                  )}
                </td>
                <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                  <div className="flex justify-end gap-2">
                    {role.isActive ? (
                      <Button variant="ghost" size="icon" onClick={() => setDeactivateId(role.id)} title="Deactivate Role">
                        <ShieldAlert className="h-4 w-4 text-red-500" />
                      </Button>
                    ) : (
                      <Button variant="ghost" size="icon" onClick={() => setActivateId(role.id)} title="Activate Role">
                        <ShieldCheck className="h-4 w-4 text-green-500" />
                      </Button>
                    )}
                    <Button variant="ghost" size="icon" onClick={() => navigate(`/roles/${role.id}/edit`)}>
                      <Edit2 className="h-4 w-4 text-gray-500" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => navigate(`/roles/${role.id}`)}>
                      <Eye className="h-4 w-4" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => setDeleteId(role.id)}>
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
        title="Activate Role"
        description="Are you sure you want to activate this role?"
        confirmText="Activate"
        variant="default"
        isLoading={activateMutation.isPending}
      />

      <ActionDialog
        isOpen={!!deactivateId}
        onClose={() => setDeactivateId(null)}
        onConfirm={handleDeactivate}
        title="Deactivate Role"
        description="Are you sure you want to deactivate this role? Users with this role may lose access."
        confirmText="Deactivate"
        variant="destructive"
        isLoading={deactivateMutation.isPending}
      />

      <ActionDialog
        isOpen={!!deleteId}
        onClose={() => setDeleteId(null)}
        onConfirm={handleDelete}
        title="Delete Role"
        description="Are you sure you want to delete this role? This action cannot be undone."
        confirmText="Delete"
        variant="destructive"
        isLoading={deleteMutation.isPending}
      />
    </>
  );
}
