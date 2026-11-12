import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { registerFace, verifyFace, getFaceProfile } from '../api';

export function useFaceProfile(employeeId: string) {
    return useQuery({
        queryKey: ['face-profile', employeeId],
        queryFn: () => getFaceProfile(employeeId),
        enabled: !!employeeId,
        retry: false,
    });
}

export function useRegisterFace(employeeId: string) {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (descriptors: Float32Array[]) => registerFace(employeeId, descriptors),
        onSuccess: () => {
            toast.success('تم تسجيل بصمة الوجه بنجاح');
            queryClient.invalidateQueries({ queryKey: ['face-profile', employeeId] });
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.title || 'فشل تسجيل بصمة الوجه');
        },
    });
}

export function useVerifyFace() {
    return useMutation({
        mutationFn: ({ employeeId, descriptor }: { employeeId: string; descriptor: Float32Array }) =>
            verifyFace(employeeId, descriptor),
        onError: (error: any) => {
            toast.error(error.response?.data?.title || 'فشل التحقق من الوجه');
        },
    });
}