using Dapper;
using SharedKernel.Utilities.Helpers;
using System.Data;

namespace WebApi.Features.Roles.Infrastructure;

public class RoleRow
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsLeader { get; set; }
    public byte DataScope { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public interface IRolesRepository
{
    Task<IEnumerable<RoleRow>> GetListAsync(Guid tenantId, CancellationToken ct);
    Task<RoleRow?> GetByIdAsync(Guid tenantId, Guid roleId, CancellationToken ct);
    Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId, CancellationToken ct);
    Task<Guid> CreateAsync(Guid tenantId, Guid roleId, string name, byte dataScope, bool canCreate, bool canEdit, bool canDelete, byte status, Guid createdBy, CancellationToken ct);
    Task UpdateAsync(Guid tenantId, Guid roleId, string name, byte dataScope, bool canCreate, bool canEdit, bool canDelete, byte status, Guid updatedBy, CancellationToken ct);
    Task DeleteAsync(Guid tenantId, Guid roleId, Guid updatedBy, CancellationToken ct);
    Task SetMenusAsync(Guid roleId, IEnumerable<string> menuKeys, CancellationToken ct);
    Task<IReadOnlyList<string>> GetMenusAsync(Guid roleId, CancellationToken ct);
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

    public async Task<Guid> CreateAsync(Guid tenantId, Guid roleId, string name, byte dataScope, bool canCreate, bool canEdit, bool canDelete, byte status, Guid createdBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<Guid>(
            "dbo.sp_role_create",
            new
            {
                roleid = roleId,
                tenantid = tenantId,
                name,
                isleader = dataScope is 1 or 2 or 3,
                datascope = dataScope,
                cancreate = canCreate,
                canedit = canEdit,
                candelete = canDelete,
                status,
                createdby = createdBy
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateAsync(Guid tenantId, Guid roleId, string name, byte dataScope, bool canCreate, bool canEdit, bool canDelete, byte status, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_role_update",
            new
            {
                tenantid = tenantId,
                roleid = roleId,
                name,
                isleader = dataScope is 1 or 2 or 3,
                datascope = dataScope,
                cancreate = canCreate,
                canedit = canEdit,
                candelete = canDelete,
                status,
                updatedby = updatedBy
            },
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

    public async Task SetMenusAsync(Guid roleId, IEnumerable<string> menuKeys, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_role_set_menus",
            new { roleid = roleId, menukeys = string.Join(",", menuKeys) },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<string>> GetMenusAsync(Guid roleId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var rows = await conn.QueryAsync<string>(
            "dbo.sp_role_get_menus",
            new { roleid = roleId },
            commandType: CommandType.StoredProcedure);
        return rows.ToList();
    }
}
