using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Enums;
using SharedKernel.Utilities.Extensions;

namespace WebApi.Features.Dashboard;

[Route("api/dashboard")]
[ApiController]
[Authorize(Roles = $"{RoleNames.TenantSuperAdmin},{RoleNames.Staff}")]
public class DashboardController(DashboardService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        (await service.GetAsync(ct)).ToActionResult();

    [HttpPut("settings")]
    [Authorize(Roles = RoleNames.TenantSuperAdmin)]
    public async Task<IActionResult> SaveSettings(
        [FromBody] SaveDashboardSettingsRequest request, CancellationToken ct) =>
        (await service.SaveSettingsAsync(request, ct)).ToActionResult();
}
