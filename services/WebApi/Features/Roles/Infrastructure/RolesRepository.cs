using Dapper;
using SharedKernel.Utilities.Helpers;
using System.Data;

namespace WebApi.Features.Roles.Infrastructure;

public class RoleRow
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsLeader { get; set; }
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public interface IRolesRepository
{
    Task<IEnumerable<RoleRow>> GetListAsync(Guid tenantId, CancellationToken ct);
    Task<RoleRow?> GetByIdAsync(Guid tenantId, Guid roleId, CancellationToken ct);
    Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId, CancellationToken ct);
    Task<Guid> CreateAsync(Guid tenantId, Guid roleId, string name, bool isLeader, byte status, Guid createdBy, CancellationToken ct);
    Task UpdateAsync(Guid tenantId, Guid roleId, string name, bool isLeader, byte status, Guid updatedBy, CancellationToken ct);
    Task DeleteAsync(Guid tenantId, Guid roleId, Guid updatedBy, CancellationToken ct);
}

public class RolesRepository(DbHelper dbHelper) : IRolesRepository
{
    public async Task<IEnumerable<RoleRow>> GetListAsync(Guid tenantId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryAsync<RoleRow>(
            "dbo.sp_role_get_list",
            new { tenantid = tenantId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<RoleRow?> GetByIdAsync(Guid tenantId, Guid roleId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<RoleRow>(
            "dbo.sp_role_get_by_id",
            new { tenantid = tenantId, roleid = roleId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "dbo.sp_role_name_exists",
            new { tenantid = tenantId, name, excludeid = excludeId },
            commandType: CommandType.StoredProcedure);
        return count > 0;
    }

    public async Task<Guid> CreateAsync(Guid tenantId, Guid roleId, string name, bool isLeader, byte status, Guid createdBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<Guid>(
            "dbo.sp_role_create",
            new { roleid = roleId, tenantid = tenantId, name, isleader = isLeader, status, createdby = createdBy },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateAsync(Guid tenantId, Guid roleId, string name, bool isLeader, byte status, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_role_update",
            new { tenantid = tenantId, roleid = roleId, name, isleader = isLeader, status, updatedby = updatedBy },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(Guid tenantId, Guid roleId, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_role_delete",
            new { tenantid = tenantId, roleid = roleId, updatedby = updatedBy },
            commandType: CommandType.StoredProcedure);
    }
}
