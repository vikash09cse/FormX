using SharedKernel.Enums;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Extensions;
using WebApi.Features.Permissions;

namespace WebApi.Features.Permissions;

public class MenuAccessService(IPermissionsRepository permissionsRepository, IHttpContextAccessor httpContextAccessor)
{
    public async Task<Result<bool>> RequireMenuAsync(string menuKey, CancellationToken ct)
    {
        var ctx = httpContextAccessor.HttpContext?.TryGetTenantContext();
        if (ctx == null || !ctx.IsValidForTenantScope())
            return Result<bool>.Fail(ErrorCode.Forbidden, "Tenant context is required.");

        if (ctx.UserType == (byte)UserType.TenantSuperAdmin)
            return Result<bool>.Ok(true);

        var perms = await permissionsRepository.GetForUserAsync(ctx.TenantId, ctx.UserId, false, ct);
        if (perms.Menus.Contains(menuKey, StringComparer.OrdinalIgnoreCase))
            return Result<bool>.Ok(true);

        return Result<bool>.Fail(ErrorCode.Forbidden, "You do not have access to this feature.");
    }
}
