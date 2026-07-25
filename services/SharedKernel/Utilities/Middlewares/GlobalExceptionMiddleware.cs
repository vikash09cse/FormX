using Microsoft.AspNetCore.Http;
using SharedKernel.Utilities;

namespace SharedKernel.Utilities.Middlewares;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            if (context.Response.HasStarted) throw;

            var (status, result) = ex switch
            {
                BadRequestException e => (StatusCodes.Status400BadRequest, Result<object>.Fail(ErrorCode.BadRequest, e.Message)),
                NotFoundException e => (StatusCodes.Status404NotFound, Result<object>.Fail(ErrorCode.NotFound, e.Message)),
                UnauthorizedException e => (StatusCodes.Status401Unauthorized, Result<object>.Fail(ErrorCode.Unauthorized, e.Message)),
                ForbiddenException e => (StatusCodes.Status403Forbidden, Result<object>.Fail(ErrorCode.Forbidden, e.Message)),
                AlreadyExistsException e => (StatusCodes.Status409Conflict, Result<object>.Fail(ErrorCode.AlreadyExists, e.Message)),
                _ => (StatusCodes.Status500InternalServerError, Result<object>.Fail(ErrorCode.InternalError, ex.Message))
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(result);
        }
    }
}
