import type { PaginationRequest } from '@/shared/types';

export interface CompanyDto {
  id: string;
  name: string;
  legalName: string;
  registrationNumber: string;
  taxNumber: string;
  email: string;
  phoneNumber: string;
  addressStreet: string;
  addressCity: string;
  addressState: string;
  addressCountry: string;
  addressZipCode: string;
  website?: string;
  logoUrl?: string;
  timeZone: string;
  currency: string;
  isActive: boolean;
  foundedDate: string;
}

export interface CompanyListDto {
  id: string;
  name: string;
  registrationNumber: string;
  email: string;
  phoneNumber: string;
  isActive: boolean;
  logoUrl?: string;
  foundedDate: string;
}

export interface CompanyDetailsDto extends CompanyDto {
  branchesCount: number;
  departmentsCount: number;
  employeesCount: number;
}

export interface GetCompaniesQuery extends PaginationRequest {
  name?: string;
  registrationNumber?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface CreateCompanyCommand {
  name: string;
  legalName: string;
  registrationNumber: string;
  taxNumber: string;
  addressStreet: string;
  addressCity: string;
  addressState: string;
  addressCountry: string;
  addressZipCode: string;
  phoneNumber: string;
  email: string;
  foundedDate: string;
  website?: string;
  timeZone: string;
  currency: string;
}

export interface UpdateCompanyCommand extends CreateCompanyCommand {
  companyId: string;
}
