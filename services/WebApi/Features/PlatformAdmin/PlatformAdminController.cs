using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Enums;
using SharedKernel.Utilities;

namespace WebApi.Features.PlatformAdmin;

[Route("api/platform")]
[ApiController]
[Authorize(Roles = RoleNames.PlatformAdmin)]
public class PlatformAdminController(PlatformAdminService service) : ControllerBase
{
    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenants(CancellationToken ct) =>
        Ok(await service.GetTenantsAsync(ct));

    [HttpGet("tenants/dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct) =>
        Ok(await service.GetDashboardAsync(ct));

    [HttpGet("tenants/{id:guid}")]
    public async Task<IActionResult> GetTenantById(Guid id, CancellationToken ct)
    {
        var result = await service.GetTenantByIdAsync(id, ct);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("tenants")]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request, CancellationToken ct)
    {
        var result = await service.CreateTenantAsync(request, ct);
        if (!result.Success && result.ErrorCode == ErrorCode.AlreadyExists)
            return Conflict(result);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("tenants/{id:guid}")]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantRequest request, CancellationToken ct)
    {
        var result = await service.UpdateTenantAsync(id, request, ct);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPatch("tenants/{id:guid}/suspend")]
    public async Task<IActionResult> SuspendTenant(Guid id, CancellationToken ct)
    {
        var result = await service.SuspendTenantAsync(id, ct);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPatch("tenants/{id:guid}/reactivate")]
    public async Task<IActionResult> ReactivateTenant(Guid id, CancellationToken ct)
    {
        var result = await service.ReactivateTenantAsync(id, ct);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("backoffice-users")]
    public async Task<IActionResult> GetBackOfficeUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default) =>
        Ok(await service.GetBackOfficeUsersAsync(page, pageSize, ct));

    [HttpGet("portal-users")]
    public async Task<IActionResult> GetPortalUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default) =>
        Ok(await service.GetPortalUsersAsync(page, pageSize, ct));

    [HttpPost("backoffice-users")]
    public async Task<IActionResult> CreateBackOfficeUser([FromBody] CreateBackOfficeUserRequest request, CancellationToken ct)
    {
        var result = await service.CreateBackOfficeUserAsync(request, ct);
        if (!result.Success && result.ErrorCode == ErrorCode.AlreadyExists)
            return Conflict(result);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("backoffice-users/{id:guid}")]
    public async Task<IActionResult> UpdateBackOfficeUser(Guid id, [FromBody] UpdateBackOfficeUserRequest request, CancellationToken ct) =>
        Ok(await service.UpdateBackOfficeUserAsync(id, request, ct));

    [HttpPatch("backoffice-users/{id:guid}/status")]
    public async Task<IActionResult> SetBackOfficeUserStatus(Guid id, [FromQuery] byte status, CancellationToken ct)
    {
        if (status is not ((byte)UserStatus.Active) and not ((byte)UserStatus.Inactive))
            return BadRequest(Result<bool>.Fail(ErrorCode.Validation, "Invalid status."));

        return Ok(await service.SetBackOfficeUserStatusAsync(id, (UserStatus)status, ct));
    }

    [HttpDelete("backoffice-users/{id:guid}")]
    public async Task<IActionResult> DeleteBackOfficeUser(Guid id, CancellationToken ct)
    {
        var result = await service.DeleteBackOfficeUserAsync(id, ct);
        if (!result.Success && result.ErrorCode == ErrorCode.Forbidden)
            return StatusCode(StatusCodes.Status403Forbidden, result);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }
}
