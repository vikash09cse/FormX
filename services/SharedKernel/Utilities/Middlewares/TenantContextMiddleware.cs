using Microsoft.AspNetCore.Http;

namespace SharedKernel.Utilities.Middlewares;

public class TenantContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, JwtHelper jwtHelper)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
        {
            var token = authHeader["Bearer ".Length..].Trim();
            context.Items["TenantContext"] = jwtHelper.ValidateAndCreateTenantContext(token);
        }

        await next(context);
    }
}
