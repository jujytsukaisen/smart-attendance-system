import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/shared/components/ui/dialog';
import { Button } from '@/shared/components/ui/button';
import { Checkbox } from '@/shared/components/ui/checkbox';
import { useRoles } from '@/features/roles/hooks';
import { useAssignRoles } from '../hooks';
import { toast } from 'sonner';
import { Loader2 } from 'lucide-react';

interface AssignRolesDialogProps {
  userId: string;
  currentRoles: string[];
  isOpen: boolean;
  onClose: () => void;
}

export function AssignRolesDialog({ userId, currentRoles, isOpen, onClose }: AssignRolesDialogProps) {
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
  
  const { data: rolesData, isLoading: isLoadingRoles } = useRoles({ pageNumber: 1, pageSize: 100 });
  const assignRolesMutation = useAssignRoles();

  useEffect(() => {
    if (isOpen) {
      setSelectedRoles(currentRoles || []);
    }
  }, [isOpen, currentRoles]);

  const handleToggleRole = (roleName: string) => {
    setSelectedRoles((prev) => 
      prev.includes(roleName)
        ? prev.filter((r) => r !== roleName)
        : [...prev, roleName]
    );
  };

  const handleSave = () => {
    assignRolesMutation.mutate(
      { userId, roles: selectedRoles },
      {
        onSuccess: () => {
          toast.success('Roles assigned successfully');
          onClose();
        },
        onError: (error: any) => {
          toast.error(error.response?.data?.title || 'Failed to assign roles');
        },
      }
    );
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Assign Roles</DialogTitle>
        </DialogHeader>
        
        <div className="py-4">
          {isLoadingRoles ? (
            <div className="flex justify-center py-4">
              <Loader2 className="h-6 w-6 animate-spin text-primary" />
            </div>
          ) : (
            <div className="space-y-4 max-h-[60vh] overflow-y-auto pr-2">
              {rolesData?.items.map((role) => (
                <div key={role.id} className="flex items-start space-x-3 rounded-md border p-3">
                  <Checkbox
                    id={`role-${role.id}`}
                    checked={selectedRoles.includes(role.name)}
                    onCheckedChange={() => handleToggleRole(role.name)}
                  />
                  <div className="space-y-1 leading-none">
                    <label
                      htmlFor={`role-${role.id}`}
                      className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
                    >
                      {role.name}
                    </label>
                  </div>
                </div>
              ))}
              
              {rolesData?.items.length === 0 && (
                <p className="text-sm text-gray-500 text-center">No roles available.</p>
              )}
            </div>
          )}
        </div>
        
        <DialogFooter>
          <Button variant="outline" onClick={onClose} disabled={assignRolesMutation.isPending}>
            Cancel
          </Button>
          <Button onClick={handleSave} disabled={isLoadingRoles || assignRolesMutation.isPending}>
            {assignRolesMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Save Changes
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
