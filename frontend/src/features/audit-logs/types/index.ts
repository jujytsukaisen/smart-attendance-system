export const AuditActionType = {
  Create: 'Create',
  Update: 'Update',
  Delete: 'Delete',
  Login: 'Login',
  Logout: 'Logout',
  Approve: 'Approve',
  Reject: 'Reject',
  Sync: 'Sync'
} as const;
export type AuditActionType = typeof AuditActionType[keyof typeof AuditActionType];

export const AuditEntityType = {
  User: 'User',
  Role: 'Role',
  Attendance: 'Attendance',
  Leave: 'Leave',
  Device: 'Device',
  Policy: 'Policy',
  System: 'System'
} as const;
export type AuditEntityType = typeof AuditEntityType[keyof typeof AuditEntityType];

export interface AuditLogDto {
  id: string;
  userId: string;
  userName: string;
  action: AuditActionType;
  entityName: AuditEntityType;
  entityId?: string;
  ipAddress: string;
  browser: string;
  result: 'Success' | 'Failure';
  createdAt: string;
}

export interface AuditLogDetailsDto extends AuditLogDto {
  oldValues?: string;
  newValues?: string;
  errorMessage?: string;
}

export interface AuditLogListDto {
  items: AuditLogDto[];
  totalCount: number;
}

export interface AuditFilterDto {
  startDate?: string;
  endDate?: string;
  userId?: string;
  action?: AuditActionType;
  entityType?: AuditEntityType;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}
