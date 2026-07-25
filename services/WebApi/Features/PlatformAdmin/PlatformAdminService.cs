using SharedKernel.Constants;
using SharedKernel.Enums;
using SharedKernel.Services;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Helpers;
using WebApi.Features.PlatformAdmin.Infrastructure;

namespace WebApi.Features.PlatformAdmin;

public class PlatformAdminService(
    IPlatformAdminRepository repository,
    EmailService emailService)
{
    public async Task<Result<IEnumerable<TenantSummaryResponse>>> GetTenantsAsync(CancellationToken ct)
    {
        var rows = await repository.GetTenantsAsync(ct);
        var data = rows.Select(r => new TenantSummaryResponse(
            r.TenantId, r.HospitalName, r.Subdomain,
            r.Status == (byte)TenantStatus.Active ? "Active" : "Suspended", r.Status,
            r.CreatedAt, r.TotalUsers, r.TotalPatients, r.LastActivityAt,
            r.PrimaryContactEmail, r.Timezone));
        return Result<IEnumerable<TenantSummaryResponse>>.Ok(data);
    }

    public async Task<Result<PlatformDashboardResponse>> GetDashboardAsync(CancellationToken ct)
    {
        var row = await repository.GetPlatformDashboardAsync(ct);
        return Result<PlatformDashboardResponse>.Ok(new PlatformDashboardResponse(
            row.TotalTenants, row.ActiveTenants, row.SuspendedTenants,
            row.TotalTenantUsers, row.TotalPatients));
    }

    public async Task<Result<TenantDetailResponse>> GetTenantByIdAsync(Guid id, CancellationToken ct)
    {
        var tenant = await repository.GetTenantByIdAsync(id, ct);
        if (tenant == null)
            return Result<TenantDetailResponse>.Fail(ErrorCode.NotFound, "Tenant not found.");
        return Result<TenantDetailResponse>.Ok(tenant);
    }

    public async Task<Result<TenantDetailResponse>> CreateTenantAsync(CreateTenantRequest request, CancellationToken ct)
    {
        if (!SubdomainValidator.IsValid(request.Subdomain, out var subdomainError))
            return Result<TenantDetailResponse>.Fail(ErrorCode.Validation, subdomainError!);

        if (await repository.SubdomainExistsAsync(request.Subdomain.ToLowerInvariant(), ct))
            return Result<TenantDetailResponse>.Fail(ErrorCode.AlreadyExists, "Subdomain is already in use.");

        if (await repository.UserEmailExistsAsync(request.PrimaryContactEmail.Trim(), null, ct))
            return Result<TenantDetailResponse>.Fail(ErrorCode.AlreadyExists, "Contact email is already registered for login.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return Result<TenantDetailResponse>.Fail(ErrorCode.Validation, "Password must be at least 6 characters.");

        var tenantId = await repository.CreateTenantAsync(request, PasswordHelper.Hash(request.Password), ct);

        await emailService.SendWelcomeEmailAsync(
            request.PrimaryContactEmail, request.HospitalName,
            request.Subdomain.ToLowerInvariant(), request.Password);

        var created = await repository.GetTenantByIdAsync(tenantId, ct);
        return Result<TenantDetailResponse>.Ok(created!,
            "Tenant created successfully. Tenant super admin login created with the contact email.");
    }

    public async Task<Result<TenantDetailResponse>> UpdateTenantAsync(Guid id, UpdateTenantRequest request, CancellationToken ct)
    {
        var existing = await repository.GetTenantByIdAsync(id, ct);
        if (existing == null)
            return Result<TenantDetailResponse>.Fail(ErrorCode.NotFound, "Tenant not found.");

        await repository.UpdateTenantAsync(id, request, ct);
        var updated = await repository.GetTenantByIdAsync(id, ct);
        return Result<TenantDetailResponse>.Ok(updated!, "Tenant updated successfully.");
    }

    public async Task<Result<bool>> SuspendTenantAsync(Guid id, CancellationToken ct)
    {
        var existing = await repository.GetTenantByIdAsync(id, ct);
        if (existing == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Tenant not found.");

        await repository.SetTenantStatusAsync(id, TenantStatus.Suspended, ct);
        return Result<bool>.Ok(true, "Tenant suspended.");
    }

    public async Task<Result<bool>> ReactivateTenantAsync(Guid id, CancellationToken ct)
    {
        var existing = await repository.GetTenantByIdAsync(id, ct);
        if (existing == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Tenant not found.");

        await repository.SetTenantStatusAsync(id, TenantStatus.Active, ct);
        return Result<bool>.Ok(true, "Tenant reactivated.");
    }

    public async Task<Result<BackOfficeUserListResponse>> GetBackOfficeUsersAsync(int page, int pageSize, CancellationToken ct)
    {
        var (items, total) = await repository.GetBackOfficeUsersAsync(page, pageSize, ct);
        var mapped = items.Select(u => new BackOfficeUserResponse(
            u.UserId, u.Email, u.FirstName, u.LastName,
            u.Status == (byte)UserStatus.Active ? "Active" : "Inactive", u.Status, u.CreatedAt));
        return Result<BackOfficeUserListResponse>.Ok(new BackOfficeUserListResponse(mapped, total, page, pageSize));
    }

    public async Task<Result<PortalUserListResponse>> GetPortalUsersAsync(int page, int pageSize, CancellationToken ct)
    {
        var (items, total) = await repository.GetPortalUsersAsync(page, pageSize, ct);
        var mapped = items.Select(u => new PortalUserResponse(
            u.UserId, u.TenantId, u.HospitalName, u.Subdomain,
            u.Email, u.FirstName, u.LastName,
            RoleNames.FromUserType((UserType)u.UserType), u.UserType,
            u.Status == (byte)UserStatus.Active ? "Active" : "Inactive", u.Status,
            u.CreatedAt));
        return Result<PortalUserListResponse>.Ok(new PortalUserListResponse(mapped, total, page, pageSize));
    }

    public async Task<Result<BackOfficeUserResponse>> CreateBackOfficeUserAsync(CreateBackOfficeUserRequest request, CancellationToken ct)
    {
        if (await repository.BackOfficeEmailExistsAsync(request.Email.Trim(), null, ct))
            return Result<BackOfficeUserResponse>.Fail(ErrorCode.AlreadyExists, "Email is already in use.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return Result<BackOfficeUserResponse>.Fail(ErrorCode.Validation, "Password must be at least 6 characters.");

        if (request.UserType is not ((byte)UserType.PlatformAdmin) and not ((byte)UserType.BackOfficeUser))
            return Result<BackOfficeUserResponse>.Fail(ErrorCode.Validation, "Invalid user type.");

        var id = await repository.CreateBackOfficeUserAsync(request, PasswordHelper.Hash(request.Password), ct);

        return Result<BackOfficeUserResponse>.Ok(new BackOfficeUserResponse(
            id, request.Email, request.FirstName, request.LastName, "Active",
            (byte)UserStatus.Active, DateTime.UtcNow), "Platform user created.");
    }

    public async Task<Result<BackOfficeUserResponse>> UpdateBackOfficeUserAsync(Guid id, UpdateBackOfficeUserRequest request, CancellationToken ct)
    {
        await repository.UpdateBackOfficeUserAsync(id, request, ct);
        return Result<BackOfficeUserResponse>.Ok(new BackOfficeUserResponse(
            id, string.Empty, request.FirstName, request.LastName, "Active",
            (byte)UserStatus.Active, DateTime.UtcNow), "Platform user updated.");
    }

    public async Task<Result<bool>> SetBackOfficeUserStatusAsync(Guid id, UserStatus status, CancellationToken ct)
    {
        await repository.SetBackOfficeUserStatusAsync(id, status, ct);
        return Result<bool>.Ok(true, "Status updated.");
    }

    public async Task<Result<bool>> DeleteBackOfficeUserAsync(Guid id, CancellationToken ct)
    {
        if (id == PlatformConstants.SeedSuperAdminUserId)
            return Result<bool>.Fail(ErrorCode.Forbidden, "The super admin account cannot be deleted.");

        var deleted = await repository.DeleteBackOfficeUserAsync(id, ct);
        if (!deleted)
            return Result<bool>.Fail(ErrorCode.NotFound, "Platform user not found.");

        return Result<bool>.Ok(true, "Platform user deleted.");
    }
}
