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
}

export interface ApiResult<T> {
  success: boolean;
  message: string;
  data: T;
  errorCode: number;
  errors: { code: number; error: string }[];
}

export interface PlatformDashboard {
  totalTenants: number;
  activeTenants: number;
  suspendedTenants: number;
  totalTenantUsers: number;
  totalPatients: number;
}

export interface TenantSummary {
  id: string;
  hospitalName: string;
  subdomain: string;
  status: string;
  statusCode: number;
  createdAt: string;
  totalUsers: number;
  totalPatients: number;
  lastActivityAt?: string;
  primaryContactEmail: string;
  timezone: string;
}

export interface TenantDetail {
  id: string;
  hospitalName: string;
  subdomain: string;
  status: string;
  statusCode: number;
  primaryContactFirstName: string;
  primaryContactLastName: string;
  primaryContactEmail: string;
  primaryContactPhone: string;
  address: string;
  timezone: string;
  logoUrl?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTenantRequest {
  hospitalName: string;
  subdomain: string;
  primaryContactFirstName: string;
  primaryContactLastName: string;
  primaryContactEmail: string;
  primaryContactPhone: string;
  address: string;
  timezone: string;
  password: string;
}

export interface UpdateTenantRequest {
  hospitalName: string;
  primaryContactFirstName: string;
  primaryContactLastName: string;
  primaryContactEmail: string;
  primaryContactPhone: string;
  address: string;
  timezone: string;
  logoUrl?: string;
}

export interface BackOfficeUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  status: string;
  statusCode: number;
  createdAt: string;
}

export interface BackOfficeUserList {
  items: BackOfficeUser[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateBackOfficeUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  userType: number;
}

export const PlatformUserTypes = [
  { label: 'Admin', value: 0 },
  { label: 'Back office user', value: 4 }
] as const;

export interface PortalUser {
  id: string;
  tenantId: string;
  hospitalName: string;
  subdomain: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  roleCode: number;
  status: string;
  statusCode: number;
  createdAt: string;
}

export interface PortalUserList {
  items: PortalUser[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface GlobalDocumentTemplate {
  id: string;
  templateType: number;
  templateTypeName: string;
  name: string;
  subject?: string | null;
  bodyHtml: string;
  isDefault: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveDocumentTemplateRequest {
  templateType: number;
  name: string;
  subject?: string | null;
  bodyHtml: string;
  isDefault: boolean;
}
