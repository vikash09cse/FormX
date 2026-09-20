using SharedKernel.Enums;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Extensions;
using WebApi.Features.Permissions;
using WebApi.Features.Roles.Infrastructure;

namespace WebApi.Features.Roles;

public class RolesService(
    IRolesRepository repository,
    MenuAccessService menuAccess,
    IHttpContextAccessor httpContextAccessor)
{
    private static readonly HashSet<string> AllowedMenus = new(StringComparer.OrdinalIgnoreCase)
    {
        MenuKeys.Dashboard, MenuKeys.MyForms, MenuKeys.Users, MenuKeys.Roles,
        MenuKeys.Projects, MenuKeys.Forms, MenuKeys.Templates, MenuKeys.Location
    };

    private async Task<Result<T>?> RequireAccess<T>(CancellationToken ct, params string[] menus)
    {
        var ctx = httpContextAccessor.HttpContext?.TryGetTenantContext();
        if (ctx == null || !ctx.IsValidForTenantScope())
            return Result<T>.Fail(ErrorCode.Forbidden, "Tenant context is required.");
        if (ctx.UserType == (byte)UserType.TenantSuperAdmin)
            return null;
        foreach (var menu in menus)
        {
            var access = await menuAccess.RequireMenuAsync(menu, ct);
            if (access.Success) return null;
        }
        return Result<T>.Fail(ErrorCode.Forbidden, "You do not have access to this feature.");
    }

    private Guid TenantId => httpContextAccessor.GetTenantContext().TenantId;
    private Guid UserId => httpContextAccessor.GetTenantContext().UserId;

    private static string DataScopeLabel(byte scope) => scope switch
    {
        (byte)DataScope.Own => "Own",
        (byte)DataScope.District => "District",
        (byte)DataScope.Project => "Project",
        (byte)DataScope.All => "All",
        _ => "Own"
    };

    public async Task<Result<IEnumerable<RoleResponse>>> GetAllAsync(CancellationToken ct)
    {
        var err = await RequireAccess<IEnumerable<RoleResponse>>(ct, MenuKeys.Roles, MenuKeys.Users);
        if (err != null) return err;
        var rows = await repository.GetListAsync(TenantId, ct);
        var list = new List<RoleResponse>();
        foreach (var row in rows)
            list.Add(await MapAsync(row, ct));
        return Result<IEnumerable<RoleResponse>>.Ok(list);
    }

    public async Task<Result<RoleResponse>> CreateAsync(SaveRoleRequest request, CancellationToken ct)
    {
        var err = await RequireAccess<RoleResponse>(ct, MenuKeys.Roles);
        if (err != null) return err;
        var validation = Validate(request);
        if (validation != null) return validation;

        var name = request.Name.Trim();
        if (await repository.NameExistsAsync(TenantId, name, null, ct))
            return Result<RoleResponse>.Fail(ErrorCode.AlreadyExists, "A role with this name already exists.");

        var menus = NormalizeMenus(request.Menus);
        var id = Guid.NewGuid();
        await repository.CreateAsync(TenantId, id, name, request.DataScope, request.CanCreate, request.CanEdit, request.CanDelete, request.Status, UserId, ct);
        await repository.SetMenusAsync(id, menus, ct);
        var created = await repository.GetByIdAsync(TenantId, id, ct);
        return Result<RoleResponse>.Ok(await MapAsync(created!, ct), "Role created successfully.");
    }

    public async Task<Result<RoleResponse>> UpdateAsync(Guid id, SaveRoleRequest request, CancellationToken ct)
    {
        var err = await RequireAccess<RoleResponse>(ct, MenuKeys.Roles);
        if (err != null) return err;
        var validation = Validate(request);
        if (validation != null) return validation;

        var existing = await repository.GetByIdAsync(TenantId, id, ct);
        if (existing == null)
            return Result<RoleResponse>.Fail(ErrorCode.NotFound, "Role not found.");

        var name = request.Name.Trim();
        if (await repository.NameExistsAsync(TenantId, name, id, ct))
            return Result<RoleResponse>.Fail(ErrorCode.AlreadyExists, "A role with this name already exists.");

        var menus = NormalizeMenus(request.Menus);
        await repository.UpdateAsync(TenantId, id, name, request.DataScope, request.CanCreate, request.CanEdit, request.CanDelete, request.Status, UserId, ct);
        await repository.SetMenusAsync(id, menus, ct);
        var updated = await repository.GetByIdAsync(TenantId, id, ct);
        return Result<RoleResponse>.Ok(await MapAsync(updated!, ct), "Role updated successfully.");
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken ct)
    {
        var err = await RequireAccess<bool>(ct, MenuKeys.Roles);
        if (err != null) return err;

        var existing = await repository.GetByIdAsync(TenantId, id, ct);
        if (existing == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Role not found.");

        await repository.DeleteAsync(TenantId, id, UserId, ct);
        return Result<bool>.Ok(true, "Role deleted successfully.");
    }

    private static Result<RoleResponse>? Validate(SaveRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length < 2)
            return Result<RoleResponse>.Fail(ErrorCode.Validation, "Role name must be at least 2 characters.");
        if (request.Status is not ((byte)ActiveInactiveStatus.Active) and not ((byte)ActiveInactiveStatus.Inactive))
            return Result<RoleResponse>.Fail(ErrorCode.Validation, "Invalid status.");
        if (!Enum.IsDefined(typeof(DataScope), request.DataScope))
            return Result<RoleResponse>.Fail(ErrorCode.Validation, "Invalid data scope.");
        return null;
    }

    private static IReadOnlyList<string> NormalizeMenus(IReadOnlyList<string>? menus) =>
        (menus ?? [])
            .Select(m => m.Trim().ToLowerInvariant())
            .Where(m => AllowedMenus.Contains(m))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private async Task<RoleResponse> MapAsync(RoleRow row, CancellationToken ct)
    {
        var menus = await repository.GetMenusAsync(row.RoleId, ct);
        return new RoleResponse(
            row.RoleId,
            row.Name,
            row.DataScope,
            DataScopeLabel(row.DataScope),
            row.CanCreate,
            row.CanEdit,
            row.CanDelete,
            menus,
            row.Status == (byte)ActiveInactiveStatus.Active ? "Active" : "Inactive",
            row.Status,
            row.CreatedAt,
            row.UpdatedAt);
    }
}
