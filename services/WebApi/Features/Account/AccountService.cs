using SharedKernel.Enums;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Extensions;
using SharedKernel.Utilities.Helpers;
using WebApi.Features.Account.Infrastructure;

namespace WebApi.Features.Account;

public class AccountService(
    IAccountRepository repository,
    IHttpContextAccessor httpContextAccessor)
{
    private Result<T>? RequireTenantContext<T>()
    {
        var ctx = httpContextAccessor.HttpContext?.TryGetTenantContext();
        if (ctx == null || !ctx.IsValidForTenantScope())
            return Result<T>.Fail(ErrorCode.Forbidden, "Tenant context is required.");
        return null;
    }

    public async Task<Result<UserProfileResponse>> GetProfileAsync(CancellationToken ct)
    {
        var tenantError = RequireTenantContext<UserProfileResponse>();
        if (tenantError != null) return tenantError;

        var ctx = httpContextAccessor.GetTenantContext();
        var row = await repository.GetProfileAsync(ctx.TenantId, ctx.UserId, ct);
        if (row == null)
            return Result<UserProfileResponse>.Fail(ErrorCode.NotFound, "User not found.");

        return Result<UserProfileResponse>.Ok(Map(row));
    }

    public async Task<Result<UserProfileResponse>> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || request.FirstName.Trim().Length < 2)
            return Result<UserProfileResponse>.Fail(ErrorCode.Validation, "First name must be at least 2 characters.");

        if (string.IsNullOrWhiteSpace(request.LastName) || request.LastName.Trim().Length < 2)
            return Result<UserProfileResponse>.Fail(ErrorCode.Validation, "Last name must be at least 2 characters.");

        var tenantError = RequireTenantContext<UserProfileResponse>();
        if (tenantError != null) return tenantError;

        var ctx = httpContextAccessor.GetTenantContext();
        var existing = await repository.GetProfileAsync(ctx.TenantId, ctx.UserId, ct);
        if (existing == null)
            return Result<UserProfileResponse>.Fail(ErrorCode.NotFound, "User not found.");

        var designation = string.IsNullOrWhiteSpace(request.Designation) ? null : request.Designation.Trim();
        await repository.UpdateProfileAsync(
            ctx.TenantId, ctx.UserId, request.FirstName.Trim(), request.LastName.Trim(), designation, ctx.UserId, ct);

        var updated = await repository.GetProfileAsync(ctx.TenantId, ctx.UserId, ct);
        return Result<UserProfileResponse>.Ok(Map(updated!), "Profile updated successfully.");
    }

    public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return Result<bool>.Fail(ErrorCode.Validation, "Current password is required.");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            return Result<bool>.Fail(ErrorCode.Validation, "New password must be at least 8 characters.");

        if (request.CurrentPassword == request.NewPassword)
            return Result<bool>.Fail(ErrorCode.Validation, "New password must be different from the current password.");

        var tenantError = RequireTenantContext<bool>();
        if (tenantError != null) return tenantError;

        var ctx = httpContextAccessor.GetTenantContext();
        var hash = await repository.GetPasswordHashAsync(ctx.TenantId, ctx.UserId, ct);
        if (string.IsNullOrEmpty(hash))
            return Result<bool>.Fail(ErrorCode.NotFound, "User not found.");

        if (!PasswordHelper.Verify(request.CurrentPassword, hash))
            return Result<bool>.Fail(ErrorCode.Validation, "Current password is incorrect.");

        await repository.UpdatePasswordAsync(
            ctx.TenantId, ctx.UserId, PasswordHelper.Hash(request.NewPassword), ctx.UserId, ct);

        return Result<bool>.Ok(true, "Password changed successfully.");
    }

    private static UserProfileResponse Map(UserProfileRow row) => new(
        row.UserId,
        row.Email,
        row.FirstName,
        row.LastName,
        row.Designation,
        RoleNames.FromUserType((UserType)row.Role),
        row.Role);
}
