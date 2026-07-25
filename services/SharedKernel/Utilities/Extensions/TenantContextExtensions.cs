using Microsoft.AspNetCore.Http;
using SharedKernel.DTOs;

namespace SharedKernel.Utilities.Extensions;

public static class TenantContextExtensions
{
    public static TenantContext GetTenantContext(this IHttpContextAccessor accessor)
    {
        var context = accessor.HttpContext
            ?? throw new UnauthorizedException("HTTP context is not available.");

        if (context.Items.TryGetValue("TenantContext", out var value) && value is TenantContext tenantContext)
            return tenantContext;

        throw new UnauthorizedException("Tenant context is not available.");
    }

    public static TenantContext? TryGetTenantContext(this HttpContext context)
    {
        if (context.Items.TryGetValue("TenantContext", out var value) && value is TenantContext tenantContext)
            return tenantContext;
        return null;
    }
}

public static class HttpContextExtensions
{
    public static string? GetClientIp(this HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString();
}
