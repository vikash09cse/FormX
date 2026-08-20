using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Enums;
using SharedKernel.Utilities.Extensions;

namespace WebApi.Features.Submissions;

[Route("api/submissions")]
[ApiController]
[Authorize(Roles = $"{RoleNames.TenantSuperAdmin},{RoleNames.Staff}")]
public class SubmissionsController(SubmissionsService service) : ControllerBase
{
    [HttpGet("available-forms")]
    public async Task<IActionResult> GetAvailableForms(CancellationToken ct) =>
        (await service.GetAvailableFormsAsync(ct)).ToActionResult();

    [HttpGet("projects")]
    public async Task<IActionResult> GetProjects(CancellationToken ct) =>
        (await service.GetProjectsAsync(ct)).ToActionResult();

    [HttpGet("forms/{formId:guid}/definition")]
    public async Task<IActionResult> GetDefinition(Guid formId, CancellationToken ct) =>
        (await service.GetDefinitionAsync(formId, ct)).ToActionResult();

    [HttpGet("forms/{formId:guid}/export")]
    public async Task<IActionResult> ExportMine(
        Guid formId,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await service.ExportMyAsync(formId, search, ct);
        if (!result.Success)
            return result.ToActionResult();

        return File(
            result.Data!.Content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            result.Data.FileName);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyList(
        [FromQuery] Guid formId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        CancellationToken ct = default) =>
        (await service.GetMyListAsync(formId, page, pageSize, search, ct)).ToActionResult();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        (await service.GetByIdAsync(id, ct)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveSubmissionRequest request, CancellationToken ct) =>
        (await service.CreateAsync(request, ct)).ToActionResult();

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubmissionRequest request, CancellationToken ct) =>
        (await service.UpdateAsync(id, request, ct)).ToActionResult();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        (await service.DeleteAsync(id, ct)).ToActionResult();
}
