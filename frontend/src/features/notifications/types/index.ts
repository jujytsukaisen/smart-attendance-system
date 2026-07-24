export const NotificationType = {
  System: 'System',
  LeaveRequest: 'LeaveRequest',
  LeaveApproval: 'LeaveApproval',
  AttendanceAnomaly: 'AttendanceAnomaly',
  ReportGenerated: 'ReportGenerated',
  DeviceAlert: 'DeviceAlert'
} as const;
export type NotificationType = typeof NotificationType[keyof typeof NotificationType];

export const NotificationPriority = {
  Low: 'Low',
  Medium: 'Medium',
  High: 'High',
  Critical: 'Critical'
} as const;
export type NotificationPriority = typeof NotificationPriority[keyof typeof NotificationPriority];

export const NotificationStatus = {
  Unread: 'Unread',
  Read: 'Read',
  Archived: 'Archived'
} as const;
export type NotificationStatus = typeof NotificationStatus[keyof typeof NotificationStatus];

export interface NotificationDto {
  id: string;
  recipientUserId: string;
  title: string;
  message: string;
  type: NotificationType;
  priority: NotificationPriority;
  isRead: boolean;
  createdAt: string;
}

export interface NotificationDetailsDto extends NotificationDto {
  actionUrl?: string;
  metadata?: Record<string, any>;
}

export interface NotificationListDto {
  items: NotificationDto[];
  totalCount: number;
  unreadCount: number;
}

export interface NotificationFilterDto {
  type?: NotificationType;
  priority?: NotificationPriority;
  status?: NotificationStatus;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}
