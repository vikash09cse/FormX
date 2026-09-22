using Dapper;
using SharedKernel.Enums;
using SharedKernel.Utilities.Helpers;
using System.Data;

namespace WebApi.Features.Users.Infrastructure;

public class TenantUserRow
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public byte Role { get; set; }
    public byte Status { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? InitialPassword { get; set; }
    public int TotalCount { get; set; }
}

public class UsersRepository(DbHelper dbHelper) : IUsersRepository
{
    public async Task<(IEnumerable<TenantUserRow> Items, int Total)> GetUsersAsync(
        Guid tenantId, int page, int pageSize, string? filter, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var rows = await conn.QueryAsync<TenantUserRow>(
            "dbo.sp_get_tenant_users",
            new { tenantid = tenantId, page, pagesize = pageSize, filter },
            commandType: CommandType.StoredProcedure);
        var list = rows.ToList();
        var total = list.FirstOrDefault()?.TotalCount ?? 0;
        return (list, total);
    }

    public async Task<TenantUserRow?> GetByIdAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<TenantUserRow>(
            "dbo.sp_get_tenant_user_by_id",
            new { tenantid = tenantId, userid = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> EmailExistsAsync(Guid tenantId, string email, Guid? excludeId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "dbo.sp_tenant_user_email_exists",
            new { tenantid = tenantId, email, excludeid = excludeId },
            commandType: CommandType.StoredProcedure);
        return count > 0;
    }

    public async Task<Guid> CreateAsync(
        Guid tenantId, string email, string firstName, string lastName, string? designation, byte role, string passwordHash,
        string? initialPassword, Guid createdBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<Guid>(
            "dbo.sp_create_tenant_user",
            new
            {
                userid = Guid.NewGuid(),
                tenantid = tenantId,
                email,
                passwordhash = passwordHash,
                initialpassword = initialPassword,
                firstname = firstName,
                lastname = lastName,
                designation,
                usertype = role,
                userstatus = (byte)UserStatus.Active,
                createdby = createdBy
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateAsync(
        Guid tenantId, Guid userId, string firstName, string lastName, string? designation, byte role, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_update_tenant_user",
            new
            {
                tenantid = tenantId,
                userid = userId,
                firstname = firstName,
                lastname = lastName,
                designation,
                updatedesignation = true,
                usertype = role,
                updatedby = updatedBy
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task SetStatusAsync(Guid tenantId, Guid userId, UserStatus status, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_set_tenant_user_status",
            new
            {
                tenantid = tenantId,
                userid = userId,
                userstatus = (byte)status,
                updatedby = updatedBy
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(Guid tenantId, Guid userId, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_delete_tenant_user",
            new { tenantid = tenantId, userid = userId, updatedby = updatedBy },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<Guid>> GetRoleIdsAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var ids = await conn.QueryAsync<Guid>(
            "dbo.sp_user_get_role_ids",
            new { tenantid = tenantId, userid = userId },
            commandType: CommandType.StoredProcedure);
        return ids.ToList();
    }

    public async Task SetRolesAsync(Guid tenantId, Guid userId, IEnumerable<Guid> roleIds, Guid createdBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_user_set_roles",
            new { tenantid = tenantId, userid = userId, roleids = string.Join(",", roleIds), createdby = createdBy },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<Guid>> GetScopeProjectIdsAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var ids = await conn.QueryAsync<Guid>(
            "dbo.sp_user_get_scope_project_ids",
            new { tenantid = tenantId, userid = userId },
            commandType: CommandType.StoredProcedure);
        return ids.ToList();
    }

    public async Task SetScopesAsync(Guid tenantId, Guid userId, IEnumerable<Guid> projectIds, Guid createdBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_user_set_scopes",
            new { tenantid = tenantId, userid = userId, projectids = string.Join(",", projectIds), createdby = createdBy },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<Guid>> GetDistrictScopeIdsAsync(Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var ids = await conn.QueryAsync<Guid>(
            "dbo.sp_user_get_district_scope_ids",
            new { userid = userId },
            commandType: CommandType.StoredProcedure);
        return ids.ToList();
    }

    public async Task SetDistrictScopesAsync(Guid userId, IEnumerable<Guid> districtIds, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_user_set_district_scopes",
            new { userid = userId, districtids = string.Join(",", districtIds) },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdatePasswordAsync(
        Guid tenantId, Guid userId, string passwordHash, string? initialPassword, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_update_user_password",
            new
            {
                tenantid = tenantId,
                userid = userId,
                passwordhash = passwordHash,
                initialpassword = initialPassword,
                updatedby = updatedBy
            },
            commandType: CommandType.StoredProcedure);
    }
}
