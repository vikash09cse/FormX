using Dapper;
using SharedKernel.Enums;
using SharedKernel.Utilities.Helpers;
using System.Data;

namespace WebApi.Features.Permissions;

public class EffectivePermissions
{
    public byte DataScope { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public IReadOnlyList<string> Menus { get; set; } = [];
    public IReadOnlyList<Guid> DistrictIds { get; set; } = [];
    public IReadOnlyList<Guid> ProjectIds { get; set; } = [];
    public bool IsSuperAdmin { get; set; }

    public static EffectivePermissions ForSuperAdmin() => new()
    {
        DataScope = (byte)SharedKernel.Enums.DataScope.All,
        CanCreate = true,
        CanEdit = true,
        CanDelete = true,
        Menus = MenuKeys.AllAdmin,
        IsSuperAdmin = true
    };
}

public interface IPermissionsRepository
{
    Task<EffectivePermissions> GetForUserAsync(Guid tenantId, Guid userId, bool isSuperAdmin, CancellationToken ct);
}

public class PermissionsRepository(DbHelper dbHelper) : IPermissionsRepository
{
    public async Task<EffectivePermissions> GetForUserAsync(Guid tenantId, Guid userId, bool isSuperAdmin, CancellationToken ct)
    {
        if (isSuperAdmin)
            return EffectivePermissions.ForSuperAdmin();

        using var conn = dbHelper.GetConnection();
        using var multi = await conn.QueryMultipleAsync(
            "dbo.sp_user_get_effective_permissions",
            new { userid = userId, tenantid = tenantId },
            commandType: CommandType.StoredProcedure);

        var caps = await multi.ReadFirstOrDefaultAsync<CapRow>() ?? new CapRow();
        var menus = (await multi.ReadAsync<string>()).ToList();
        var districts = (await multi.ReadAsync<Guid>()).ToList();
        var projects = (await multi.ReadAsync<Guid>()).ToList();

        if (menus.Count == 0)
            menus = [MenuKeys.Dashboard, MenuKeys.MyForms];

        return new EffectivePermissions
        {
            DataScope = caps.DataScope,
            CanCreate = caps.CanCreate,
            CanEdit = caps.CanEdit,
            CanDelete = caps.CanDelete,
            Menus = menus,
            DistrictIds = districts,
            ProjectIds = projects,
            IsSuperAdmin = false
        };
    }

    private class CapRow
    {
        public byte DataScope { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
