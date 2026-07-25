using SharedKernel.Enums;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Extensions;
using WebApi.Features.Roles.Infrastructure;

namespace WebApi.Features.Roles;

public class RolesService(IRolesRepository repository, IHttpContextAccessor httpContextAccessor)
{
    private Result<T>? RequireTenant<T>()
    {
        var ctx = httpContextAccessor.HttpContext?.TryGetTenantContext();
        if (ctx == null || !ctx.IsValidForTenantScope())
            return Result<T>.Fail(ErrorCode.Forbidden, "Tenant context is required.");
        return null;
    }

    private Guid TenantId => httpContextAccessor.GetTenantContext().TenantId;
    private Guid UserId => httpContextAccessor.GetTenantContext().UserId;

    public async Task<Result<IEnumerable<RoleResponse>>> GetAllAsync(CancellationToken ct)
    {
        var err = RequireTenant<IEnumerable<RoleResponse>>();
        if (err != null) return err;
        var rows = await repository.GetListAsync(TenantId, ct);
        return Result<IEnumerable<RoleResponse>>.Ok(rows.Select(Map));
    }

    public async Task<Result<RoleResponse>> CreateAsync(SaveRoleRequest request, CancellationToken ct)
    {
        var err = RequireTenant<RoleResponse>();
        if (err != null) return err;

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length < 2)
            return Result<RoleResponse>.Fail(ErrorCode.Validation, "Role name must be at least 2 characters.");

        if (request.Status is not ((byte)ActiveInactiveStatus.Active) and not ((byte)ActiveInactiveStatus.Inactive))
            return Result<RoleResponse>.Fail(ErrorCode.Validation, "Invalid status.");

        var name = request.Name.Trim();
        if (await repository.NameExistsAsync(TenantId, name, null, ct))
            return Result<RoleResponse>.Fail(ErrorCode.AlreadyExists, "A role with this name already exists.");

        var id = Guid.NewGuid();
        await repository.CreateAsync(TenantId, id, name, request.IsLeader, request.Status, UserId, ct);
        var created = await repository.GetByIdAsync(TenantId, id, ct);
        return Result<RoleResponse>.Ok(Map(created!), "Role created successfully.");
    }

    public async Task<Result<RoleResponse>> UpdateAsync(Guid id, SaveRoleRequest request, CancellationToken ct)
    {
        var err = RequireTenant<RoleResponse>();
        if (err != null) return err;

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length < 2)
            return Result<RoleResponse>.Fail(ErrorCode.Validation, "Role name must be at least 2 characters.");

        if (request.Status is not ((byte)ActiveInactiveStatus.Active) and not ((byte)ActiveInactiveStatus.Inactive))
            return Result<RoleResponse>.Fail(ErrorCode.Validation, "Invalid status.");

        var existing = await repository.GetByIdAsync(TenantId, id, ct);
        if (existing == null)
            return Result<RoleResponse>.Fail(ErrorCode.NotFound, "Role not found.");

        var name = request.Name.Trim();
        if (await repository.NameExistsAsync(TenantId, name, id, ct))
            return Result<RoleResponse>.Fail(ErrorCode.AlreadyExists, "A role with this name already exists.");

        await repository.UpdateAsync(TenantId, id, name, request.IsLeader, request.Status, UserId, ct);
        var updated = await repository.GetByIdAsync(TenantId, id, ct);
        return Result<RoleResponse>.Ok(Map(updated!), "Role updated successfully.");
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken ct)
    {
        var err = RequireTenant<bool>();
        if (err != null) return err;

        var existing = await repository.GetByIdAsync(TenantId, id, ct);
        if (existing == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Role not found.");

        await repository.DeleteAsync(TenantId, id, UserId, ct);
        return Result<bool>.Ok(true, "Role deleted successfully.");
    }

    private static RoleResponse Map(RoleRow row) => new(
        row.RoleId,
        row.Name,
        row.IsLeader,
        row.Status == (byte)ActiveInactiveStatus.Active ? "Active" : "Inactive",
        row.Status,
        row.CreatedAt,
        row.UpdatedAt);
}
