import type { PaginationRequest } from '@/shared/types';

export type Gender = 1 | 2 | 3 | 4 | 'Male' | 'Female' | 'Other' | 'PreferNotToSay';

export type EmployeeStatus = 'Active' | 'Inactive' | 'OnLeave' | 'Terminated';

export interface EmployeeDto {
  id: string;
  companyId: string;
  branchId: string;
  departmentId: string;
  attendancePolicyId: string;
  employeeNumber: string;
  firstName: string;
  lastName: string;
  middleName?: string;
  preferredName?: string;
  nationalId?: string;
  passportNumber?: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
  gender: Gender;
  hireDate: string;
  jobTitle: string;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  employeeStatus: EmployeeStatus;
}

export interface EmployeeListDto {
  id: string;
  companyId: string;
  branchId: string;
  departmentId: string;
  employeeNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  jobTitle: string;
  employeeStatus: EmployeeStatus;
}

export interface EmployeeDetailsDto extends EmployeeDto {
  companyName?: string;
  branchName?: string;
  departmentName?: string;
}

export interface GetEmployeesQuery extends PaginationRequest {
  searchTerm?: string;
  companyId?: string;
  branchId?: string;
  departmentId?: string;
  status?: EmployeeStatus;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface CreateEmployeeCommand {
  companyId: string;
  branchId: string;
  departmentId: string;
  attendancePolicyId: string;
  employeeNumber: string;
  firstName: string;
  lastName: string;
  middleName?: string;
  preferredName?: string;
  nationalId?: string;
  passportNumber?: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
  gender: Gender;
  hireDate: string;
  jobTitle: string;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
}

export interface UpdateEmployeeCommand {
  employeeId: string;
  firstName: string;
  lastName: string;
  middleName?: string;
  preferredName?: string;
  nationalId?: string;
  passportNumber?: string;
  dateOfBirth: string;
  gender: Gender;
  email: string;
  phoneNumber: string;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  jobTitle: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  emergencyRelationship?: string;
}