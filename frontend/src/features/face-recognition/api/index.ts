import { apiClient } from '@/shared/api/axios';
import type { ApiEnvelope } from '@/features/auth/types';
import type { FaceMatchResultDto, FaceProfileDto, FaceEncodingDto } from '../types';

const BASE_URL = '/v1/FaceProfiles';

export const registerFace = async (employeeId: string, faceEncodings: Float32Array[]): Promise<string> => {
    const response = await apiClient.post<ApiEnvelope<string>>(BASE_URL, {
        employeeId,
        faceEncodings: faceEncodings.map(d => Array.from(d)),
    });
    return response.data.data;
};

export const verifyFace = async (employeeId: string, submittedEncoding: Float32Array): Promise<FaceMatchResultDto> => {
    const response = await apiClient.post<ApiEnvelope<FaceMatchResultDto>>(`${BASE_URL}/verify`, {
        employeeId,
        submittedEncoding: Array.from(submittedEncoding),
    });
    return response.data.data;
};

export const getFaceProfile = async (employeeId: string): Promise<FaceProfileDto> => {
    const response = await apiClient.get<ApiEnvelope<FaceProfileDto>>(`${BASE_URL}/${employeeId}`);
    return response.data.data;
};

export const getAllFaceEncodings = async (): Promise<FaceEncodingDto[]> => {
    const response = await apiClient.get<ApiEnvelope<FaceEncodingDto[]>>(`${BASE_URL}/encodings/all`);
    return response.data.data ?? [];
};