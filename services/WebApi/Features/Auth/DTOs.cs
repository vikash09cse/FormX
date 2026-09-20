namespace WebApi.Features.Auth;

public record PlatformLoginRequest(string Email, string Password);
public record TenantLoginRequest(string Email, string Password);
public record TenantBySubdomainResponse(
    Guid Id,
    string HospitalName,
    string Subdomain,
    byte Status,
    string? LogoUrl,
    string Timezone);

public record LoginResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    byte UserType,
    string? Designation,
    Guid? TenantId,
    string? TenantName,
    string? Subdomain,
    string Token,
    string TokenType,
    int ExpiresIn,
    string RefreshToken,
    DateTime RefreshTokenExpiry,
    byte DataScope = 0,
    bool CanCreate = true,
    bool CanEdit = true,
    bool CanDelete = true,
    IReadOnlyList<string>? Menus = null);
