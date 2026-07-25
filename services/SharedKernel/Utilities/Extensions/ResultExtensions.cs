using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Utilities;

namespace SharedKernel.Utilities.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.Success)
            return new OkObjectResult(result);

        return result.ErrorCode switch
        {
            ErrorCode.Unauthorized => new UnauthorizedObjectResult(result),
            ErrorCode.Forbidden => new ObjectResult(result) { StatusCode = StatusCodes.Status403Forbidden },
            ErrorCode.NotFound => new NotFoundObjectResult(result),
            ErrorCode.AlreadyExists => new ConflictObjectResult(result),
            _ => new BadRequestObjectResult(result)
        };
    }
}
