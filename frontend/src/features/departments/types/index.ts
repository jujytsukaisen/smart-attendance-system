export interface DepartmentDto {
  id: string;
  companyId: string;
  branchId: string;
  name: string;
  code: string;
  description?: string;
  managerId?: string;
  isActive: boolean;
}

export interface DepartmentListDto {
  id: string;
  companyId: string;
  branchId: string;
  branchName: string;
  name: string;
  code: string;
  managerId?: string;
  isActive: boolean;
}

export interface DepartmentDetailsDto {
  id: string;
  companyId: string;
  branchId: string;
  branchName: string;
  name: string;
  code: string;
  description?: string;
  managerId?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateDepartmentCommand {
  companyId: string;
  branchId: string;
  name: string;
  code: string;
  description: string;
}

export interface UpdateDepartmentCommand {
  departmentId: string;
  name: string;
  description: string;
}

export interface DepartmentFilter {
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
  pageNumber: number;
  pageSize: number;
}
