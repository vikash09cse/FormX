using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Enums;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Extensions;

namespace WebApi.Features.Users;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = RoleNames.TenantSuperAdmin)]
public class UsersController(UsersService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? filter = null,
        CancellationToken ct = default) =>
        (await service.GetUsersAsync(page, pageSize, filter, ct)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantUserRequest request, CancellationToken ct) =>
        (await service.CreateUserAsync(request, ct)).ToActionResult();

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantUserRequest request, CancellationToken ct) =>
        (await service.UpdateUserAsync(id, request, ct)).ToActionResult();

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, [FromQuery] byte status, CancellationToken ct)
    {
        if (status is not ((byte)UserStatus.Active) and not ((byte)UserStatus.Inactive))
            return Result<bool>.Fail(ErrorCode.Validation, "Invalid status.").ToActionResult();

        return (await service.SetUserStatusAsync(id, (UserStatus)status, ct)).ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        (await service.DeleteUserAsync(id, ct)).ToActionResult();
}
