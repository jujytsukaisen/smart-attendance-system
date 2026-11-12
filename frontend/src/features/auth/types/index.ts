export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  permissions?: string[];
}

// الشكل الفعلي الذي يرجعه الباك إند داخل "data" (بعد فك الـ ResponseWrapperMiddleware)
export interface AuthResponse {
  token: string;
  refreshToken: string;
  requiresTwoFactor: boolean;
}

// الغلاف العام الذي يضيفه ResponseWrapperMiddleware لكل ردود الـ API
export interface ApiEnvelope<T> {
  data: T;
  statusCode: number;
  isSuccess: boolean;
}

export interface LoginCredentials {
  email: string;
  password: string;
  rememberMe?: boolean;
}