using Dapper;
using SharedKernel.Enums;
using SharedKernel.Utilities.Helpers;
using System.Data;

namespace WebApi.Features.Auth.Infrastructure;

public class UserLoginRow
{
    public Guid UserId { get; set; }
    public Guid? TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public byte UserType { get; set; }
    public byte Status { get; set; }
    public string? HospitalName { get; set; }
    public string? Subdomain { get; set; }
    public byte? TenantStatus { get; set; }
}

public class TenantRow
{
    public Guid TenantId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public byte Status { get; set; }
    public string? LogoUrl { get; set; }
    public string Timezone { get; set; } = string.Empty;
}

public class AuthRepository(DbHelper dbHelper) : IAuthRepository
{
    public async Task<UserLoginRow?> GetUserForLoginAsync(string email, Guid? tenantId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<UserLoginRow>(
            "dbo.sp_get_user_for_login",
            new { email, tenantid = tenantId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<UserLoginRow>> GetTenantUsersForLoginByEmailAsync(string email, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var rows = await conn.QueryAsync<UserLoginRow>(
            "dbo.sp_get_tenant_user_for_login",
            new { email },
            commandType: CommandType.StoredProcedure);
        return rows.ToList();
    }

    public async Task<TenantRow?> GetTenantBySubdomainAsync(string subdomain, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<TenantRow>(
            "dbo.sp_get_tenant_by_subdomain",
            new { subdomain },
            commandType: CommandType.StoredProcedure);
    }

    public async Task LogLoginAttemptAsync(
        Guid? tenantId,
        string userIdentifier,
        LoginType loginType,
        bool success,
        string? failureReason,
        string? ipAddress,
        CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_log_login_attempt",
            new
            {
                tenantid = tenantId,
                useridentifier = userIdentifier,
                logintype = (byte)loginType,
                issuccess = success,
                failurereason = failureReason,
                ipaddress = ipAddress
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task SaveRefreshTokenAsync(Guid userId, Guid? tenantId, string tokenHash, DateTime expiresAt, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_save_refresh_token",
            new { userid = userId, tenantid = tenantId, tokenhash = tokenHash, expiresat = expiresAt },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateUserLastLoginAsync(Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_update_user_last_login",
            new { userid = userId },
            commandType: CommandType.StoredProcedure);
    }
}
