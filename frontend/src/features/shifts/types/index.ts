export const ShiftType = {
  Morning: 1,
  Afternoon: 2,
  Night: 3,
  FullDay: 4,
  Flexible: 5,
} as const;

export type ShiftType = (typeof ShiftType)[keyof typeof ShiftType];

export const ShiftTypeLabels: Record<number, string> = {
  [ShiftType.Morning]: 'Morning',
  [ShiftType.Afternoon]: 'Afternoon',
  [ShiftType.Night]: 'Night',
  [ShiftType.FullDay]: 'Full Day',
  [ShiftType.Flexible]: 'Flexible',
};

export const ShiftTypeArabicLabels: Record<number, string> = {
  [ShiftType.Morning]: 'صباحية',
  [ShiftType.Afternoon]: 'مسائية',
  [ShiftType.Night]: 'ليلية',
  [ShiftType.FullDay]: 'يوم كامل',
  [ShiftType.Flexible]: 'مرنة',
};

export interface ShiftListDto {
  id: string;
  name: string;
  code: string;
  startTime: string;
  endTime: string;
  shiftType: ShiftType;
  isActive: boolean;
  workScheduleId: string;
}

export interface ShiftBreakDto {
  id: string;
  name: string;
  startTime: string;
  endTime: string;
  durationMinutes: number;
  isPaid: boolean;
}

export interface ShiftDetailsDto {
  id: string;
  companyId: string;
  workScheduleId: string;
  name: string;
  code: string;
  description?: string;
  startTime: string;
  endTime: string;
  shiftType: ShiftType;
  isNightShift: boolean;
  isFlexible: boolean;
  minimumWorkingHours: number;
  maximumWorkingHours: number;
  gracePeriodMinutes: number;
  allowEarlyCheckInMinutes: number;
  allowLateCheckOutMinutes: number;
  maxBreakDurationMinutes: number;
  isActive: boolean;
  breaks?: ShiftBreakDto[];
}

export interface CreateShiftCommand {
  companyId: string;
  workScheduleId: string;
  name: string;
  code: string;
  description?: string;
  startTime: string;
  endTime: string;
  shiftType: ShiftType;
  isFlexible: boolean;
  minimumWorkingHours: number;
  maximumWorkingHours: number;
  gracePeriodMinutes: number;
  allowEarlyCheckInMinutes: number;
  allowLateCheckOutMinutes: number;
  maxBreakDurationMinutes: number;
}

export interface UpdateShiftCommand {
  shiftId: string;
  name: string;
  code: string;
  description?: string;
}

export interface ShiftFilter {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  isActive?: boolean;
}

export interface WorkScheduleListDto {
  id: string;
  name: string;
  isDefault: boolean;
  isFlexible: boolean;
  isActive: boolean;
  weeklyWorkingHours: number;
}