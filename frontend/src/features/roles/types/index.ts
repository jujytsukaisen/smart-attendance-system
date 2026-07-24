export interface PermissionDto {
  id: string;
  name: string;
  systemName: string;
  module: string;
  description: string;
  isSystem: boolean;
}

export interface RoleDto {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  permissions?: PermissionDto[]; // Present when fetched by ID
}

export type RoleListDto = RoleDto;

export interface CreateRoleCommand {
  name: string;
  description: string;
}

export interface UpdateRoleCommand {
  roleId: string;
  name: string;
  description: string;
}

export interface AssignPermissionsCommand {
  roleId: string;
  permissions: string[];
}

export interface RoleFilter {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
}
