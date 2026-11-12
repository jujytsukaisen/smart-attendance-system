import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { attendanceApi } from '../api';
import type { AttendanceFilter, CheckInCommand, CheckOutCommand, ManualAttendanceCommand, UpdateAttendanceCommand } from '../types';

export const ATTENDANCE_KEYS = {
  all: ['attendances'] as const,
  lists: () => [...ATTENDANCE_KEYS.all, 'list'] as const,
  list: (filter: AttendanceFilter) => [...ATTENDANCE_KEYS.lists(), filter] as const,
  details: () => [...ATTENDANCE_KEYS.all, 'detail'] as const,
  detail: (id: string) => [...ATTENDANCE_KEYS.details(), id] as const,
};

export const useAttendances = (filter: AttendanceFilter, enabled: boolean = true) => {
  return useQuery({
    queryKey: ATTENDANCE_KEYS.list(filter),
    queryFn: () => attendanceApi.getAttendances(filter),
    enabled,
  });
};
export const useMyAttendances = (filter: AttendanceFilter, enabled: boolean = true) => {
  return useQuery({
    queryKey: [...ATTENDANCE_KEYS.lists(), 'my', filter],
    queryFn: () => attendanceApi.getMyAttendances(filter),
    enabled,
  });
};

export const useAttendanceDetails = (id: string) => {
  return useQuery({
    queryKey: ATTENDANCE_KEYS.detail(id),
    queryFn: () => attendanceApi.getAttendanceDetails(id),
    enabled: !!id,
  });
};

export const useCheckIn = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CheckInCommand) => attendanceApi.checkIn(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_KEYS.lists() });
    },
  });
};

export const useCheckOut = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CheckOutCommand) => attendanceApi.checkOut(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_KEYS.lists() });
    },
  });
};

export const useManualAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: ManualAttendanceCommand) => attendanceApi.manualAttendance(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_KEYS.lists() });
    },
  });
};

export const useUpdateAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateAttendanceCommand }) => attendanceApi.updateAttendance(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_KEYS.lists() });
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_KEYS.detail(variables.id) });
    },
  });
};

export const useDeleteAttendance = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => attendanceApi.deleteAttendance(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ATTENDANCE_KEYS.lists() });
    },
  });
};
