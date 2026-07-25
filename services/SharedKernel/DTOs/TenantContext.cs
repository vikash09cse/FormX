using System.Security.Claims;

namespace SharedKernel.DTOs;

public class TenantContext
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public byte UserType { get; set; }
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiry { get; set; }

    public bool IsPlatformAdmin => UserType == (byte)Enums.UserType.PlatformAdmin;

    public bool IsValidForTenantScope() =>
        UserId != Guid.Empty && TenantId != Guid.Empty;

    public static TenantContext FromJwtClaims(Dictionary<string, string> claims)
    {
        _ = byte.TryParse(claims.GetValueOrDefault("user_type", "0"), out var userType);
        _ = Guid.TryParse(claims.GetValueOrDefault("tenant_id", string.Empty), out var tenantId);

        return new TenantContext
        {
            UserId = Guid.Parse(claims.GetValueOrDefault("user_id", Guid.Empty.ToString())),
            UserName = claims.GetValueOrDefault("user_name", string.Empty) ?? string.Empty,
            Email = claims.GetValueOrDefault("email", string.Empty) ?? string.Empty,
            FullName = claims.GetValueOrDefault("full_name", string.Empty) ?? string.Empty,
            Designation = string.IsNullOrWhiteSpace(claims.GetValueOrDefault("designation"))
                ? null
                : claims.GetValueOrDefault("designation"),
            TenantId = tenantId,
            TenantName = claims.GetValueOrDefault("tenant_name", string.Empty) ?? string.Empty,
            Subdomain = claims.GetValueOrDefault("subdomain", string.Empty) ?? string.Empty,
            Role = claims.GetValueOrDefault(ClaimTypes.Role, string.Empty) ?? string.Empty,
            UserType = userType
        };
    }
}

public class GridRequestDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortColumn { get; set; } = "created_at";
    public bool SortDescending { get; set; } = true;
    public string? Filter { get; set; }
}
