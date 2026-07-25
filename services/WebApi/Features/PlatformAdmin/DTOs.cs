namespace WebApi.Features.PlatformAdmin;

public record CreateTenantRequest(
    string HospitalName,
    string Subdomain,
    string PrimaryContactFirstName,
    string PrimaryContactLastName,
    string PrimaryContactEmail,
    string PrimaryContactPhone,
    string Address,
    string Timezone,
    string Password);

public record UpdateTenantRequest(
    string HospitalName,
    string PrimaryContactFirstName,
    string PrimaryContactLastName,
    string PrimaryContactEmail,
    string PrimaryContactPhone,
    string Address,
    string Timezone,
    string? LogoUrl);

public record TenantSummaryResponse(
    Guid Id,
    string HospitalName,
    string Subdomain,
    string Status,
    byte StatusCode,
    DateTime CreatedAt,
    int TotalUsers,
    int TotalPatients,
    DateTime? LastActivityAt,
    string PrimaryContactEmail,
    string Timezone);

public record TenantDetailResponse(
    Guid Id,
    string HospitalName,
    string Subdomain,
    string Status,
    byte StatusCode,
    string PrimaryContactFirstName,
    string PrimaryContactLastName,
    string PrimaryContactEmail,
    string PrimaryContactPhone,
    string Address,
    string Timezone,
    string? LogoUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record PlatformDashboardResponse(
    int TotalTenants,
    int ActiveTenants,
    int SuspendedTenants,
    int TotalTenantUsers,
    int TotalPatients);

public record CreateBackOfficeUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    byte UserType);

public record UpdateBackOfficeUserRequest(
    string FirstName,
    string LastName);

public record BackOfficeUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Status,
    byte StatusCode,
    DateTime CreatedAt);

public record BackOfficeUserListResponse(
    IEnumerable<BackOfficeUserResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record PortalUserResponse(
    Guid Id,
    Guid TenantId,
    string HospitalName,
    string Subdomain,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    byte RoleCode,
    string Status,
    byte StatusCode,
    DateTime CreatedAt);

public record PortalUserListResponse(
    IEnumerable<PortalUserResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
