export interface LoginResponse {
  userId: string;
  email: string;
  fullName: string;
  role: string;
  userType: number;
  designation?: string | null;
  tenantId?: string;
  tenantName?: string;
  subdomain?: string;
  token: string;
  tokenType: string;
  expiresIn: number;
  refreshToken: string;
  refreshTokenExpiry: string;
  dataScope?: number;
  canCreate?: boolean;
  canEdit?: boolean;
  canDelete?: boolean;
  menus?: string[];
}

export interface UserProfile {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  designation?: string | null;
  role: string;
  roleCode: number;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  designation?: string | null;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface ApiResult<T> {
  success: boolean;
  message: string;
  data: T;
  errorCode: number;
  errors: { code: number; error: string }[];
}
