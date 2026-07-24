export interface UserDetailsDto {
  id: string;
  username: string;
  email: string;
  isActive: boolean; // Usually returned in similar systems. If not, it defaults to undefined.
  roles: string[];
}

// UserListDto is generally similar to UserDetailsDto for this frontend list view
export type UserListDto = UserDetailsDto;

export interface CreateUserCommand {
  username: string;
  email: string;
  password?: string; // Optional if we generate a password
}

export interface UpdateUserCommand {
  userId: string;
  username: string;
  email: string;
}

export interface AssignRolesCommand {
  userId: string;
  roles: string[];
}

export interface ResetPasswordCommand {
  userId: string;
  newPassword?: string;
}

export interface ChangePasswordCommand {
  userId: string;
  currentPassword?: string;
  newPassword?: string;
}

export interface UserFilter {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  isActive?: boolean;
}
