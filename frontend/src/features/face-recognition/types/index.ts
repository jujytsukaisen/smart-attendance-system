export interface FaceMatchResultDto {
    isMatch: boolean;
    distance: number;
    confidence: number;
}

export interface FaceProfileDto {
    id: string;
    employeeId: string;
    registrationStatus: string;
    isActive: boolean;
    registeredAt: string | null;
}

export interface FaceEncodingDto {
    employeeId: string;
    employeeName: string;
    attendancePolicyId: string;
    faceEncoding: string; // JSON array of 128 floats
}