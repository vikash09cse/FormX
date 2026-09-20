using SharedKernel.Enums;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Extensions;
using WebApi.Features.Permissions;
using WebApi.Features.Projects.Infrastructure;

namespace WebApi.Features.Projects;

public class ProjectsService(
    IProjectsRepository repository,
    MenuAccessService menuAccess,
    IHttpContextAccessor httpContextAccessor)
{
    private async Task<Result<T>?> RequireProjectsMenu<T>(CancellationToken ct)
    {
        var ctx = httpContextAccessor.HttpContext?.TryGetTenantContext();
        if (ctx == null || !ctx.IsValidForTenantScope())
            return Result<T>.Fail(ErrorCode.Forbidden, "Tenant context is required.");
        if (ctx.UserType == (byte)UserType.TenantSuperAdmin)
            return null;
        var access = await menuAccess.RequireMenuAsync(MenuKeys.Projects, ct);
        if (access.Success) return null;
        // Users admin also needs project list for scopes
        access = await menuAccess.RequireMenuAsync(MenuKeys.Users, ct);
        return access.Success ? null : Result<T>.Fail(ErrorCode.Forbidden, "You do not have access to this feature.");
    }

    private Guid TenantId => httpContextAccessor.GetTenantContext().TenantId;
    private Guid UserId => httpContextAccessor.GetTenantContext().UserId;

    public async Task<Result<IEnumerable<ProjectResponse>>> GetAllAsync(CancellationToken ct)
    {
        var err = await RequireProjectsMenu<IEnumerable<ProjectResponse>>(ct);
        if (err != null) return err;
        var rows = await repository.GetListAsync(TenantId, ct);
        return Result<IEnumerable<ProjectResponse>>.Ok(rows.Select(Map));
    }

    public async Task<Result<ProjectResponse>> CreateAsync(SaveProjectRequest request, CancellationToken ct)
    {
        var err = await RequireProjectsMenu<ProjectResponse>(ct);
        if (err != null) return err;

        if (string.IsNullOrWhiteSpace(request.ProjectName) || request.ProjectName.Trim().Length < 2)
            return Result<ProjectResponse>.Fail(ErrorCode.Validation, "Project name must be at least 2 characters.");

        if (request.Status is not ((byte)ActiveInactiveStatus.Active) and not ((byte)ActiveInactiveStatus.Inactive))
            return Result<ProjectResponse>.Fail(ErrorCode.Validation, "Invalid status.");

        var name = request.ProjectName.Trim();
        if (await repository.NameExistsAsync(TenantId, name, null, ct))
            return Result<ProjectResponse>.Fail(ErrorCode.AlreadyExists, "A project with this name already exists.");

        var code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();
        var id = Guid.NewGuid();
        await repository.CreateAsync(TenantId, id, name, code, request.Status, UserId, ct);
        var created = await repository.GetByIdAsync(TenantId, id, ct);
        return Result<ProjectResponse>.Ok(Map(created!), "Project created successfully.");
    }

    public async Task<Result<ProjectResponse>> UpdateAsync(Guid id, SaveProjectRequest request, CancellationToken ct)
    {
        var err = await RequireProjectsMenu<ProjectResponse>(ct);
        if (err != null) return err;

        if (string.IsNullOrWhiteSpace(request.ProjectName) || request.ProjectName.Trim().Length < 2)
            return Result<ProjectResponse>.Fail(ErrorCode.Validation, "Project name must be at least 2 characters.");

        if (request.Status is not ((byte)ActiveInactiveStatus.Active) and not ((byte)ActiveInactiveStatus.Inactive))
            return Result<ProjectResponse>.Fail(ErrorCode.Validation, "Invalid status.");

        var existing = await repository.GetByIdAsync(TenantId, id, ct);
        if (existing == null)
            return Result<ProjectResponse>.Fail(ErrorCode.NotFound, "Project not found.");

        var name = request.ProjectName.Trim();
        if (await repository.NameExistsAsync(TenantId, name, id, ct))
            return Result<ProjectResponse>.Fail(ErrorCode.AlreadyExists, "A project with this name already exists.");

        var code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();
        await repository.UpdateAsync(TenantId, id, name, code, request.Status, UserId, ct);
        var updated = await repository.GetByIdAsync(TenantId, id, ct);
        return Result<ProjectResponse>.Ok(Map(updated!), "Project updated successfully.");
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken ct)
    {
        var err = await RequireProjectsMenu<bool>(ct);
        if (err != null) return err;

        var existing = await repository.GetByIdAsync(TenantId, id, ct);
        if (existing == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Project not found.");

        await repository.DeleteAsync(TenantId, id, UserId, ct);
        return Result<bool>.Ok(true, "Project deleted successfully.");
    }

    private static ProjectResponse Map(ProjectRow row) => new(
        row.ProjectId,
        row.ProjectName,
        row.Code,
        row.Status == (byte)ActiveInactiveStatus.Active ? "Active" : "Inactive",
        row.Status,
        row.CreatedAt,
        row.UpdatedAt);
}
