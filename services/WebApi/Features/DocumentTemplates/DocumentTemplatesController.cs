using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Enums;
using SharedKernel.Utilities.Extensions;

namespace WebApi.Features.DocumentTemplates;

[Route("api/document-templates")]
[ApiController]
[Authorize(Roles = RoleNames.TenantSuperAdmin)]
public class DocumentTemplatesController(DocumentTemplatesService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] byte? templateType = null, CancellationToken ct = default) =>
        (await service.GetListAsync(templateType, ct)).ToActionResult();

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        (await service.GetByIdAsync(id, ct)).ToActionResult();

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentTemplateRequest request, CancellationToken ct) =>
        (await service.UpdateAsync(id, request, ct)).ToActionResult();

    [HttpPatch("{id:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken ct) =>
        (await service.SetDefaultAsync(id, ct)).ToActionResult();
}
