using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Enums;
using SharedKernel.Utilities.Extensions;

namespace WebApi.Features.Locations;

[Route("api/locations")]
[ApiController]
[Authorize(Roles = $"{RoleNames.TenantSuperAdmin},{RoleNames.Staff}")]
public class LocationsController(LocationsService service) : ControllerBase
{
    [HttpGet("states")]
    public async Task<IActionResult> GetStates(CancellationToken ct) =>
        (await service.GetStatesAsync(ct)).ToActionResult();

    [HttpPost("states")]
    public async Task<IActionResult> CreateState([FromBody] SaveStateRequest request, CancellationToken ct) =>
        (await service.CreateStateAsync(request, ct)).ToActionResult();

    [HttpPut("states/{id:guid}")]
    public async Task<IActionResult> UpdateState(Guid id, [FromBody] SaveStateRequest request, CancellationToken ct) =>
        (await service.UpdateStateAsync(id, request, ct)).ToActionResult();

    [HttpDelete("states/{id:guid}")]
    public async Task<IActionResult> DeleteState(Guid id, CancellationToken ct) =>
        (await service.DeleteStateAsync(id, ct)).ToActionResult();

    [HttpGet("districts")]
    public async Task<IActionResult> GetDistricts([FromQuery] Guid? stateId, CancellationToken ct) =>
        (await service.GetDistrictsAsync(stateId, ct)).ToActionResult();

    [HttpPost("districts")]
    public async Task<IActionResult> CreateDistrict([FromBody] SaveDistrictRequest request, CancellationToken ct) =>
        (await service.CreateDistrictAsync(request, ct)).ToActionResult();

    [HttpPut("districts/{id:guid}")]
    public async Task<IActionResult> UpdateDistrict(Guid id, [FromBody] SaveDistrictRequest request, CancellationToken ct) =>
        (await service.UpdateDistrictAsync(id, request, ct)).ToActionResult();

    [HttpDelete("districts/{id:guid}")]
    public async Task<IActionResult> DeleteDistrict(Guid id, CancellationToken ct) =>
        (await service.DeleteDistrictAsync(id, ct)).ToActionResult();

    [HttpGet("blocks")]
    public async Task<IActionResult> GetBlocks([FromQuery] Guid? districtId, CancellationToken ct) =>
        (await service.GetBlocksAsync(districtId, ct)).ToActionResult();

    [HttpPost("blocks")]
    public async Task<IActionResult> CreateBlock([FromBody] SaveBlockRequest request, CancellationToken ct) =>
        (await service.CreateBlockAsync(request, ct)).ToActionResult();

    [HttpPut("blocks/{id:guid}")]
    public async Task<IActionResult> UpdateBlock(Guid id, [FromBody] SaveBlockRequest request, CancellationToken ct) =>
        (await service.UpdateBlockAsync(id, request, ct)).ToActionResult();

    [HttpDelete("blocks/{id:guid}")]
    public async Task<IActionResult> DeleteBlock(Guid id, CancellationToken ct) =>
        (await service.DeleteBlockAsync(id, ct)).ToActionResult();

    [HttpGet("villages")]
    public async Task<IActionResult> GetVillages([FromQuery] Guid? blockId, CancellationToken ct) =>
        (await service.GetVillagesAsync(blockId, ct)).ToActionResult();

    [HttpPost("villages")]
    public async Task<IActionResult> CreateVillage([FromBody] SaveVillageRequest request, CancellationToken ct) =>
        (await service.CreateVillageAsync(request, ct)).ToActionResult();

    [HttpPut("villages/{id:guid}")]
    public async Task<IActionResult> UpdateVillage(Guid id, [FromBody] SaveVillageRequest request, CancellationToken ct) =>
        (await service.UpdateVillageAsync(id, request, ct)).ToActionResult();

    [HttpDelete("villages/{id:guid}")]
    public async Task<IActionResult> DeleteVillage(Guid id, CancellationToken ct) =>
        (await service.DeleteVillageAsync(id, ct)).ToActionResult();

    [HttpGet("import/template")]
    public async Task<IActionResult> DownloadTemplate(CancellationToken ct)
    {
        var access = await service.EnsureLocationAdminAsync(ct);
        if (!access.Success) return access.ToActionResult();
        var bytes = service.BuildImportTemplate();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "location-import-template.xlsx");
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Excel file is required." });

        await using var stream = file.OpenReadStream();
        return (await service.ImportAsync(stream, ct)).ToActionResult();
    }
}
