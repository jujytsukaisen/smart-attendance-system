import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { UserStatusBadge } from './UserStatusBadge';
import type { UserDetailsDto } from '../types';
import { Badge } from '@/shared/components/ui/badge';

interface UserDetailsCardProps {
  user: UserDetailsDto;
}

export function UserDetailsCard({ user }: UserDetailsCardProps) {
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
        <CardHeader>
          <CardTitle>Roles</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-2">
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
    </div>
  );
}
