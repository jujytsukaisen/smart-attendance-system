import { useState, useEffect } from 'react';
import { Checkbox } from '@/shared/components/ui/checkbox';
import type { PermissionDto } from '../types';

interface PermissionMatrixProps {
  allPermissions: PermissionDto[];
  selectedPermissions: string[];
  onChange: (permissions: string[]) => void;
  isLoading?: boolean;
}

export function PermissionMatrix({ allPermissions, selectedPermissions, onChange, isLoading }: PermissionMatrixProps) {
  const [modules, setModules] = useState<Record<string, PermissionDto[]>>({});

  useEffect(() => {
    // Group permissions by module
    const grouped = allPermissions.reduce((acc, curr) => {
      if (!acc[curr.module]) {
        acc[curr.module] = [];
      }
      acc[curr.module].push(curr);
      return acc;
    }, {} as Record<string, PermissionDto[]>);
    setModules(grouped);
  }, [allPermissions]);

  const handleToggle = (permissionSystemName: string, checked: boolean) => {
    if (checked) {
      onChange([...selectedPermissions, permissionSystemName]);
    } else {
      onChange(selectedPermissions.filter(p => p !== permissionSystemName));
    }
  };

  const handleToggleModule = (module: string, checked: boolean) => {
    const modulePermissions = modules[module].map(p => p.systemName);
    if (checked) {
      const newSelections = new Set([...selectedPermissions, ...modulePermissions]);
      onChange(Array.from(newSelections));
    } else {
      onChange(selectedPermissions.filter(p => !modulePermissions.includes(p)));
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center mb-4">
        <h3 className="text-lg font-medium text-gray-900">Permissions Mapping</h3>
        <span className="text-sm text-gray-500">{selectedPermissions.length} selected</span>
      </div>

      {Object.entries(modules).map(([moduleName, permissions]) => {
        const allSelected = permissions.every(p => selectedPermissions.includes(p.systemName));
        const someSelected = permissions.some(p => selectedPermissions.includes(p.systemName)) && !allSelected;

        return (
          <div key={moduleName} className="bg-white border border-gray-200 rounded-lg overflow-hidden shadow-sm">
            <div className="bg-gray-50 px-4 py-3 border-b border-gray-200 flex items-center justify-between">
              <div className="flex items-center space-x-3">
                <Checkbox 
                  checked={allSelected ? true : someSelected ? true : false}
                  onCheckedChange={(checked) => handleToggleModule(moduleName, checked === true)}
                  disabled={isLoading}
                />
                <span className="font-semibold text-gray-800">{moduleName}</span>
              </div>
            </div>
            <div className="px-4 py-3 grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
              {permissions.map((p) => (
                <div key={p.id} className="flex items-start space-x-3">
                  <Checkbox 
                    id={`perm-${p.id}`}
                    checked={selectedPermissions.includes(p.systemName)}
                    onCheckedChange={(checked) => handleToggle(p.systemName, checked === true)}
                    disabled={isLoading}
                  />
                  <div className="grid gap-1.5 leading-none">
                    <label 
                      htmlFor={`perm-${p.id}`}
                      className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
                    >
                      {p.name}
                    </label>
                    <p className="text-xs text-muted-foreground">
                      {p.systemName}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        );
      })}
    </div>
  );
}
