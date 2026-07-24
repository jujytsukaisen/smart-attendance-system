import type { PaginationRequest } from '@/shared/types';

export interface BranchDto {
  id: string;
  companyId: string;
  name: string;
  code: string;
  address: string;
  phoneNumber: string;
  email: string;
  latitude: number;
  longitude: number;
  radiusInMeters: number;
  timeZone: string;
  isHeadOffice: boolean;
  isActive: boolean;
}

export interface BranchListDto {
  id: string;
  companyId: string;
  name: string;
  code: string;
  isHeadOffice: boolean;
  isActive: boolean;
}

export interface BranchDetailsDto extends BranchDto {
  totalDepartments: number;
  totalEmployees: number;
  totalAttendanceDevices: number;
}

export interface GetBranchesQuery extends PaginationRequest {
  name?: string;
  code?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface CreateBranchCommand {
  companyId: string;
  name: string;
  code: string;
  addressStreet: string;
  addressCity: string;
  addressState: string;
  addressCountry: string;
  addressZipCode: string;
  phoneNumber: string;
  email: string;
  latitude: number;
  longitude: number;
  allowedRadiusMeters: number;
  timeZone: string;
  isHeadOffice: boolean;
}

export interface UpdateBranchCommand extends CreateBranchCommand {
  branchId: string;
}
