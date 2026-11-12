import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { UserStatusBadge } from './UserStatusBadge';
import type { UserDetailsDto } from '../types';
import { Badge } from '@/shared/components/ui/badge';
import { Button } from '@/shared/components/ui/button';
import { Settings2 } from 'lucide-react';
import { AssignRolesDialog } from './AssignRolesDialog';

interface UserDetailsCardProps {
  user: UserDetailsDto;
}

export function UserDetailsCard({ user }: UserDetailsCardProps) {
  const [isAssignRolesOpen, setIsAssignRolesOpen] = useState(false);
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
      <Card>
        <CardHeader>
          <CardTitle>User Profile</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <span className="block text-sm font-medium text-gray-500">Username</span>
            <span className="block text-sm text-gray-900 mt-1">{user.username}</span>
          </div>
          <div>
            <span className="block text-sm font-medium text-gray-500">Email</span>
            <span className="block text-sm text-gray-900 mt-1">{user.email}</span>
          </div>
          <div>
            <span className="block text-sm font-medium text-gray-500">Status</span>
            <div className="mt-1">
              <UserStatusBadge isActive={user.isActive} />
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between pb-2">
          <CardTitle>Roles</CardTitle>
          <Button variant="outline" size="sm" onClick={() => setIsAssignRolesOpen(true)} className="gap-2">
            <Settings2 className="h-4 w-4" />
            Assign Roles
          </Button>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-2 mt-2">
            {user.roles && user.roles.length > 0 ? (
              user.roles.map((role) => (
                <Badge key={role} variant="secondary">
                  {role}
                </Badge>
              ))
            ) : (
              <span className="text-sm text-gray-500">No roles assigned.</span>
            )}
          </div>
        </CardContent>
      </Card>

      <AssignRolesDialog
        userId={user.id}
        currentRoles={user.roles || []}
        isOpen={isAssignRolesOpen}
        onClose={() => setIsAssignRolesOpen(false)}
      />
    </div>
  );
}
