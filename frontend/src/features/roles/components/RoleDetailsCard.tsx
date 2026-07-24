import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Badge } from '@/shared/components/ui/badge';
import type { RoleDto } from '../types';

interface RoleDetailsCardProps {
  role: RoleDto;
}

export function RoleDetailsCard({ role }: RoleDetailsCardProps) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
      <Card className="md:col-span-1 h-fit">
        <CardHeader>
          <CardTitle>Role Profile</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <span className="block text-sm font-medium text-gray-500">Name</span>
            <span className="block text-sm text-gray-900 mt-1">{role.name}</span>
          </div>
          <div>
            <span className="block text-sm font-medium text-gray-500">Description</span>
            <span className="block text-sm text-gray-900 mt-1">{role.description || 'No description provided.'}</span>
          </div>
          <div>
            <span className="block text-sm font-medium text-gray-500">Status</span>
            <div className="mt-1">
              {role.isActive ? (
                <Badge variant="default" className="bg-green-100 text-green-800 hover:bg-green-100">Active</Badge>
              ) : (
                <Badge variant="destructive" className="bg-red-100 text-red-800 hover:bg-red-100">Inactive</Badge>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      <Card className="md:col-span-2">
        <CardHeader>
          <CardTitle>Assigned Permissions ({role.permissions?.length || 0})</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {role.permissions && role.permissions.length > 0 ? (
              role.permissions.map((permission) => (
                <div key={permission.id} className="border border-gray-200 rounded-md p-3">
                  <div className="text-sm font-medium text-gray-900">{permission.name}</div>
                  <div className="text-xs text-gray-500 mt-1">{permission.systemName}</div>
                  <div className="text-xs bg-gray-100 text-gray-600 inline-block px-2 py-1 rounded mt-2">
                    {permission.module}
                  </div>
                </div>
              ))
            ) : (
              <span className="text-sm text-gray-500">No permissions assigned.</span>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
