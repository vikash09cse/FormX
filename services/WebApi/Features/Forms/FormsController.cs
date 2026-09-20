using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Enums;
using SharedKernel.Utilities.Extensions;

namespace WebApi.Features.Forms;

[Route("api/forms")]
[ApiController]
[Authorize(Roles = $"{RoleNames.TenantSuperAdmin},{RoleNames.Staff}")]
public class FormsController(FormsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetForms(CancellationToken ct) =>
        (await service.GetFormsAsync(ct)).ToActionResult();

    [HttpGet("{formId:guid}")]
    public async Task<IActionResult> GetForm(Guid formId, CancellationToken ct) =>
        (await service.GetFormAsync(formId, ct)).ToActionResult();

    [HttpPost]
    public async Task<IActionResult> CreateForm([FromBody] SaveFormRequest request, CancellationToken ct) =>
        (await service.CreateFormAsync(request, ct)).ToActionResult();

    [HttpPut("{formId:guid}")]
    public async Task<IActionResult> UpdateForm(Guid formId, [FromBody] SaveFormRequest request, CancellationToken ct) =>
        (await service.UpdateFormAsync(formId, request, ct)).ToActionResult();

    [HttpDelete("{formId:guid}")]
    public async Task<IActionResult> DeleteForm(Guid formId, CancellationToken ct) =>
        (await service.DeleteFormAsync(formId, ct)).ToActionResult();

    [HttpGet("{formId:guid}/groups")]
    public async Task<IActionResult> GetGroups(Guid formId, CancellationToken ct) =>
        (await service.GetGroupsAsync(formId, ct)).ToActionResult();

    [HttpPost("{formId:guid}/groups")]
    public async Task<IActionResult> CreateGroup(Guid formId, [FromBody] SaveFormGroupRequest request, CancellationToken ct) =>
        (await service.SaveGroupAsync(formId, null, request, ct)).ToActionResult();

    [HttpPut("{formId:guid}/groups/{groupId:guid}")]
    public async Task<IActionResult> UpdateGroup(Guid formId, Guid groupId, [FromBody] SaveFormGroupRequest request, CancellationToken ct) =>
        (await service.SaveGroupAsync(formId, groupId, request, ct)).ToActionResult();

    [HttpDelete("{formId:guid}/groups/{groupId:guid}")]
    public async Task<IActionResult> DeleteGroup(Guid formId, Guid groupId, CancellationToken ct) =>
        (await service.DeleteGroupAsync(formId, groupId, ct)).ToActionResult();

    [HttpGet("{formId:guid}/fields")]
    public async Task<IActionResult> GetFields(Guid formId, CancellationToken ct) =>
        (await service.GetFieldsAsync(formId, ct)).ToActionResult();

    [HttpPost("{formId:guid}/fields")]
    public async Task<IActionResult> CreateField(Guid formId, [FromBody] SaveFormFieldRequest request, CancellationToken ct) =>
        (await service.SaveFieldAsync(formId, null, request, ct)).ToActionResult();

    [HttpPut("{formId:guid}/fields/{fieldId:guid}")]
    public async Task<IActionResult> UpdateField(Guid formId, Guid fieldId, [FromBody] SaveFormFieldRequest request, CancellationToken ct) =>
        (await service.SaveFieldAsync(formId, fieldId, request, ct)).ToActionResult();

    [HttpDelete("{formId:guid}/fields/{fieldId:guid}")]
    public async Task<IActionResult> DeleteField(Guid formId, Guid fieldId, CancellationToken ct) =>
        (await service.DeleteFieldAsync(formId, fieldId, ct)).ToActionResult();

    [HttpGet("{formId:guid}/fields/{fieldId:guid}")]
    public async Task<IActionResult> DeleteFieldWrong() => NotFound();

    [HttpGet("{formId:guid}/followup-config")]
    public async Task<IActionResult> GetFollowupConfig(Guid formId, CancellationToken ct) =>
        (await service.GetFollowupConfigAsync(formId, ct)).ToActionResult();

    [HttpPut("{formId:guid}/followup-config")]
    public async Task<IActionResult> SaveFollowupConfig(
        Guid formId, [FromBody] SaveFormFollowupConfigRequest request, CancellationToken ct) =>
        (await service.SaveFollowupConfigAsync(formId, request, ct)).ToActionResult();

    [HttpGet("~/api/validation-regex-presets")]
    public async Task<IActionResult> GetRegexPresets(CancellationToken ct) =>
        (await service.GetRegexPresetsAsync(ct)).ToActionResult();
}
