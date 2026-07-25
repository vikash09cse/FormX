using Dapper;
using SharedKernel.Constants;
using SharedKernel.Enums;
using SharedKernel.Utilities.Helpers;
using System.Data;
using WebApi.Features.PlatformAdmin;

namespace WebApi.Features.PlatformAdmin.Infrastructure;

public class TenantDashboardRow
{
    public Guid TenantId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PrimaryContactEmail { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
    public int TotalPatients { get; set; }
    public DateTime? LastActivityAt { get; set; }
}

public class PlatformDashboardRow
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int SuspendedTenants { get; set; }
    public int TotalTenantUsers { get; set; }
    public int TotalPatients { get; set; }
}

public class TenantDetailRow
{
    public Guid TenantId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public byte Status { get; set; }
    public string PrimaryContactFirstName { get; set; } = string.Empty;
    public string PrimaryContactLastName { get; set; } = string.Empty;
    public string PrimaryContactEmail { get; set; } = string.Empty;
    public string PrimaryContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class BackOfficeUserRow
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalCount { get; set; }
}

public class PortalUserRow
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public byte UserType { get; set; }
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalCount { get; set; }
}

public class PlatformAdminRepository(DbHelper dbHelper) : IPlatformAdminRepository
{
    public async Task<IEnumerable<TenantDashboardRow>> GetTenantsAsync(CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryAsync<TenantDashboardRow>(
            "dbo.sp_get_tenants_dashboard",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PlatformDashboardRow> GetPlatformDashboardAsync(CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryFirstAsync<PlatformDashboardRow>(
            "dbo.sp_get_platform_dashboard",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<TenantDetailResponse?> GetTenantByIdAsync(Guid id, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var row = await conn.QueryFirstOrDefaultAsync<TenantDetailRow>(
            "dbo.sp_get_tenant_by_id",
            new { tenantid = id },
            commandType: CommandType.StoredProcedure);
        if (row == null) return null;

        return new TenantDetailResponse(
            row.TenantId, row.HospitalName, row.Subdomain, StatusLabel(row.Status), row.Status,
            row.PrimaryContactFirstName, row.PrimaryContactLastName,
            row.PrimaryContactEmail, row.PrimaryContactPhone,
            row.Address, row.Timezone, row.LogoUrl, row.CreatedAt, row.UpdatedAt);
    }

    public async Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "dbo.sp_tenant_subdomain_exists",
            new { subdomain },
            commandType: CommandType.StoredProcedure);
        return count > 0;
    }

    public async Task<bool> UserEmailExistsAsync(string email, Guid? excludeUserId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "dbo.sp_user_email_exists",
            new { email, excludeid = excludeUserId },
            commandType: CommandType.StoredProcedure);
        return count > 0;
    }

    public async Task<Guid> CreateTenantAsync(CreateTenantRequest req, string passwordHash, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();

        return await conn.ExecuteScalarAsync<Guid>(
            "dbo.sp_create_tenant",
            new
            {
                tenantid = Guid.NewGuid(),
                userid = Guid.NewGuid(),
                tenantsettingsid = Guid.NewGuid(),
                hospitalname = req.HospitalName,
                subdomain = req.Subdomain.ToLowerInvariant(),
                primarycontactfirstname = req.PrimaryContactFirstName.Trim(),
                primarycontactlastname = req.PrimaryContactLastName.Trim(),
                primarycontactemail = req.PrimaryContactEmail.Trim(),
                primarycontactphone = req.PrimaryContactPhone,
                tenantaddress = req.Address,
                timezone = req.Timezone,
                tenantstatus = (byte)TenantStatus.Active,
                passwordhash = passwordHash,
                patientidprefix = GetPrefix(req.HospitalName)
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateTenantAsync(Guid id, UpdateTenantRequest req, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_update_tenant",
            new
            {
                tenantid = id,
                hospitalname = req.HospitalName,
                primarycontactfirstname = req.PrimaryContactFirstName.Trim(),
                primarycontactlastname = req.PrimaryContactLastName.Trim(),
                primarycontactemail = req.PrimaryContactEmail,
                primarycontactphone = req.PrimaryContactPhone,
                tenantaddress = req.Address,
                timezone = req.Timezone,
                logourl = req.LogoUrl,
                website = (string?)null
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task SetTenantStatusAsync(Guid id, TenantStatus status, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_set_tenant_status",
            new { tenantid = id, tenantstatus = (byte)status },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<(IEnumerable<BackOfficeUserRow> Items, int Total)> GetBackOfficeUsersAsync(int page, int pageSize, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var rows = await conn.QueryAsync<BackOfficeUserRow>(
            "dbo.sp_get_platform_users",
            new { page, pagesize = pageSize },
            commandType: CommandType.StoredProcedure);
        var list = rows.ToList();
        var total = list.FirstOrDefault()?.TotalCount ?? 0;
        return (list, total);
    }

    public async Task<(IEnumerable<PortalUserRow> Items, int Total)> GetPortalUsersAsync(int page, int pageSize, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var rows = await conn.QueryAsync<PortalUserRow>(
            "dbo.sp_get_portal_users",
            new { page, pagesize = pageSize },
            commandType: CommandType.StoredProcedure);
        var list = rows.ToList();
        var total = list.FirstOrDefault()?.TotalCount ?? 0;
        return (list, total);
    }

    public async Task<Guid> CreateBackOfficeUserAsync(CreateBackOfficeUserRequest req, string passwordHash, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<Guid>(
            "dbo.sp_create_platform_user",
            new
            {
                userid = Guid.NewGuid(),
                email = req.Email,
                passwordhash = passwordHash,
                firstname = req.FirstName,
                lastname = req.LastName,
                usertype = req.UserType,
                userstatus = (byte)UserStatus.Active
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateBackOfficeUserAsync(Guid id, UpdateBackOfficeUserRequest req, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_update_platform_user",
            new { userid = id, firstname = req.FirstName, lastname = req.LastName },
            commandType: CommandType.StoredProcedure);
    }

    public async Task SetBackOfficeUserStatusAsync(Guid id, UserStatus status, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_set_platform_user_status",
            new { userid = id, userstatus = (byte)status },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> DeleteBackOfficeUserAsync(Guid id, CancellationToken ct)
    {
        if (id == PlatformConstants.SeedSuperAdminUserId)
            return false;

        using var conn = dbHelper.GetConnection();
        var affected = await conn.ExecuteAsync(
            "dbo.sp_delete_platform_user",
            new { userid = id },
            commandType: CommandType.StoredProcedure);
        return affected > 0;
    }

    public async Task<bool> BackOfficeEmailExistsAsync(string email, Guid? excludeId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "dbo.sp_platform_user_email_exists",
            new { email, excludeid = excludeId },
            commandType: CommandType.StoredProcedure);
        return count > 0;
    }

    private static string StatusLabel(byte status) =>
        status == (byte)TenantStatus.Active ? "Active" : "Suspended";

    private static string GetPrefix(string hospitalName)
    {
        var parts = hospitalName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}-".ToUpperInvariant()
            : $"{hospitalName[..Math.Min(2, hospitalName.Length)]}-".ToUpperInvariant();
    }
}
