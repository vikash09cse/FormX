using SharedKernel.Enums;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Extensions;
using WebApi.Features.Forms.Infrastructure;
using WebApi.Features.Permissions;

namespace WebApi.Features.Forms;

public class FormsService(
    IFormsRepository repository,
    MenuAccessService menuAccess,
    IHttpContextAccessor httpContextAccessor)
{
    private async Task<Result<T>?> RequireFormsMenu<T>(CancellationToken ct)
    {
        var ctx = httpContextAccessor.HttpContext?.TryGetTenantContext();
        if (ctx == null || !ctx.IsValidForTenantScope())
            return Result<T>.Fail(ErrorCode.Forbidden, "Tenant context is required.");
        if (ctx.UserType == (byte)UserType.TenantSuperAdmin)
            return null;
        var access = await menuAccess.RequireMenuAsync(MenuKeys.Forms, ct);
        return access.Success ? null : Result<T>.Fail(access.ErrorCode, access.Message);
    }

    private Guid TenantId => httpContextAccessor.GetTenantContext().TenantId;
    private Guid UserId => httpContextAccessor.GetTenantContext().UserId;

    public async Task<Result<IEnumerable<FormResponse>>> GetFormsAsync(CancellationToken ct)
    {
        var err = await RequireFormsMenu<IEnumerable<FormResponse>>(ct);
        if (err != null) return err;

        var rows = (await repository.GetFormsAsync(TenantId, ct)).ToList();
        var result = new List<FormResponse>();
        foreach (var row in rows)
        {
            var roleIds = await repository.GetFormRoleIdsAsync(TenantId, row.FormId, ct);
            result.Add(MapForm(row, roleIds));
        }
        return Result<IEnumerable<FormResponse>>.Ok(result);
    }

    public async Task<Result<FormResponse>> GetFormAsync(Guid formId, CancellationToken ct)
    {
        var err = await RequireFormsMenu<FormResponse>(ct);
        if (err != null) return err;

        var row = await repository.GetFormByIdAsync(TenantId, formId, ct);
        if (row == null) return Result<FormResponse>.Fail(ErrorCode.NotFound, "Form not found.");
        var roleIds = await repository.GetFormRoleIdsAsync(TenantId, formId, ct);
        return Result<FormResponse>.Ok(MapForm(row, roleIds));
    }

    public async Task<Result<FormResponse>> CreateFormAsync(SaveFormRequest request, CancellationToken ct)
    {
        var err = await RequireFormsMenu<FormResponse>(ct);
        if (err != null) return err;

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length < 2)
            return Result<FormResponse>.Fail(ErrorCode.Validation, "Form name must be at least 2 characters.");

        if (request.ProjectId == Guid.Empty)
            return Result<FormResponse>.Fail(ErrorCode.Validation, "Project is required.");

        if (request.Status is not ((byte)ActiveInactiveStatus.Active) and not ((byte)ActiveInactiveStatus.Inactive))
            return Result<FormResponse>.Fail(ErrorCode.Validation, "Invalid status.");

        var name = request.Name.Trim();
        if (await repository.FormNameExistsAsync(TenantId, name, null, ct))
            return Result<FormResponse>.Fail(ErrorCode.AlreadyExists, "A form with this name already exists.");

        var id = Guid.NewGuid();
        try
        {
            await repository.CreateFormAsync(TenantId, id, request.ProjectId, name, request.Description?.Trim(), request.Status, request.DisplayOrder, request.CollectLocation, UserId, ct);
        }
        catch (Exception ex)
        {
            return Result<FormResponse>.Fail(ErrorCode.Validation, CleanSqlMessage(ex.Message));
        }
        if (request.RoleIds?.Count > 0)
            await repository.SetFormRolesAsync(TenantId, id, request.RoleIds, UserId, ct);

        return await GetFormAsync(id, ct);
    }

    public async Task<Result<FormResponse>> UpdateFormAsync(Guid formId, SaveFormRequest request, CancellationToken ct)
    {
        var err = await RequireFormsMenu<FormResponse>(ct);
        if (err != null) return err;

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length < 2)
            return Result<FormResponse>.Fail(ErrorCode.Validation, "Form name must be at least 2 characters.");

        if (request.ProjectId == Guid.Empty)
            return Result<FormResponse>.Fail(ErrorCode.Validation, "Project is required.");

        var existing = await repository.GetFormByIdAsync(TenantId, formId, ct);
        if (existing == null) return Result<FormResponse>.Fail(ErrorCode.NotFound, "Form not found.");

        var name = request.Name.Trim();
        if (await repository.FormNameExistsAsync(TenantId, name, formId, ct))
            return Result<FormResponse>.Fail(ErrorCode.AlreadyExists, "A form with this name already exists.");

        try
        {
            await repository.UpdateFormAsync(TenantId, formId, request.ProjectId, name, request.Description?.Trim(), request.Status, request.DisplayOrder, request.CollectLocation, UserId, ct);
        }
        catch (Exception ex)
        {
            return Result<FormResponse>.Fail(ErrorCode.Validation, CleanSqlMessage(ex.Message));
        }
        await repository.SetFormRolesAsync(TenantId, formId, request.RoleIds ?? [], UserId, ct);
        return await GetFormAsync(formId, ct);
    }

    public async Task<Result<bool>> DeleteFormAsync(Guid formId, CancellationToken ct)
    {
        var err = await RequireFormsMenu<bool>(ct);
        if (err != null) return err;

        var existing = await repository.GetFormByIdAsync(TenantId, formId, ct);
        if (existing == null) return Result<bool>.Fail(ErrorCode.NotFound, "Form not found.");

        await repository.DeleteFormAsync(TenantId, formId, UserId, ct);
        return Result<bool>.Ok(true, "Form deleted successfully.");
    }

    public async Task<Result<IEnumerable<FormGroupResponse>>> GetGroupsAsync(Guid formId, CancellationToken ct)
    {
        var err = await RequireFormsMenu<IEnumerable<FormGroupResponse>>(ct);
        if (err != null) return err;

        if (await repository.GetFormByIdAsync(TenantId, formId, ct) == null)
            return Result<IEnumerable<FormGroupResponse>>.Fail(ErrorCode.NotFound, "Form not found.");

        var rows = await repository.GetGroupsAsync(TenantId, formId, ct);
        return Result<IEnumerable<FormGroupResponse>>.Ok(rows.Select(g => new FormGroupResponse(
            g.FormGroupId, g.FormId, g.GroupName, g.GroupDisplayNameKey, g.DisplayOrder)));
    }

    public async Task<Result<FormGroupResponse>> SaveGroupAsync(Guid formId, Guid? groupId, SaveFormGroupRequest request, CancellationToken ct)
    {
        var err = await RequireFormsMenu<FormGroupResponse>(ct);
        if (err != null) return err;

        if (string.IsNullOrWhiteSpace(request.GroupName))
            return Result<FormGroupResponse>.Fail(ErrorCode.Validation, "Group name is required.");

        if (await repository.GetFormByIdAsync(TenantId, formId, ct) == null)
            return Result<FormGroupResponse>.Fail(ErrorCode.NotFound, "Form not found.");

        var id = groupId ?? Guid.NewGuid();
        await repository.SaveGroupAsync(TenantId, formId, id, request.GroupName.Trim(),
            string.IsNullOrWhiteSpace(request.GroupDisplayNameKey) ? null : request.GroupDisplayNameKey.Trim(),
            request.DisplayOrder, UserId, ct);

        return Result<FormGroupResponse>.Ok(new FormGroupResponse(
            id, formId, request.GroupName.Trim(),
            string.IsNullOrWhiteSpace(request.GroupDisplayNameKey) ? null : request.GroupDisplayNameKey.Trim(),
            request.DisplayOrder), "Group saved.");
    }

    public async Task<Result<bool>> DeleteGroupAsync(Guid formId, Guid groupId, CancellationToken ct)
    {
        var err = await RequireFormsMenu<bool>(ct);
        if (err != null) return err;

        if (await repository.GetFormByIdAsync(TenantId, formId, ct) == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Form not found.");

        await repository.DeleteGroupAsync(TenantId, groupId, UserId, ct);
        return Result<bool>.Ok(true, "Group deleted.");
    }

    public async Task<Result<IEnumerable<FormFieldResponse>>> GetFieldsAsync(Guid formId, CancellationToken ct)
    {
        var err = await RequireFormsMenu<IEnumerable<FormFieldResponse>>(ct);
        if (err != null) return err;

        if (await repository.GetFormByIdAsync(TenantId, formId, ct) == null)
            return Result<IEnumerable<FormFieldResponse>>.Fail(ErrorCode.NotFound, "Form not found.");

        var (fields, options, parentOptions) = await repository.GetFieldsAsync(TenantId, formId, ct);
        var mapped = fields.Select(f => MapField(
            f,
            options.Where(o => o.FieldId == f.FieldId).ToList(),
            parentOptions.Where(p => p.FieldId == f.FieldId).Select(p => p.OptionId).ToList()));
        return Result<IEnumerable<FormFieldResponse>>.Ok(mapped);
    }

    public async Task<Result<FormFieldResponse>> SaveFieldAsync(Guid formId, Guid? fieldId, SaveFormFieldRequest request, CancellationToken ct)
    {
        var err = await RequireFormsMenu<FormFieldResponse>(ct);
        if (err != null) return err;

        if (await repository.GetFormByIdAsync(TenantId, formId, ct) == null)
            return Result<FormFieldResponse>.Fail(ErrorCode.NotFound, "Form not found.");

        if (string.IsNullOrWhiteSpace(request.ControlLabel))
            return Result<FormFieldResponse>.Fail(ErrorCode.Validation, "Control label is required.");

        if (!Enum.IsDefined(typeof(FormFieldControlType), request.ControlType))
            return Result<FormFieldResponse>.Fail(ErrorCode.Validation, "Invalid control type.");

        var isEmail = request.ControlType == (byte)FormFieldControlType.Email;
        var sendEmail = isEmail && request.IsSendEmailNotification;
        var displayOnList = request.ControlType != (byte)FormFieldControlType.Label && request.DisplayOnList;

        if (displayOnList)
        {
            var (existingFields, _, _) = await repository.GetFieldsAsync(TenantId, formId, ct);
            var excludeId = fieldId is { } fid && fid != Guid.Empty ? fid : (Guid?)null;
            var listCount = existingFields.Count(f => f.DisplayOnList && f.ControlType != (byte)FormFieldControlType.Label
                && (excludeId == null || f.FieldId != excludeId.Value));
            if (listCount >= 5)
                return Result<FormFieldResponse>.Fail(ErrorCode.Validation, "At most 5 fields can be shown on the list page.");
        }

        var needsOptions = request.ControlType is
            (byte)FormFieldControlType.Dropdown or
            (byte)FormFieldControlType.RadioButton or
            (byte)FormFieldControlType.Checkbox or
            (byte)FormFieldControlType.CheckboxSingle;

        var options = (request.Options ?? [])
            .Where(o => !string.IsNullOrWhiteSpace(o.OptionText))
            .Select((o, i) => (
                OptionId: o.Id is { } existing && existing != Guid.Empty ? existing : Guid.NewGuid(),
                Text: o.OptionText.Trim(),
                Value: string.IsNullOrWhiteSpace(o.OptionValue) ? o.OptionText.Trim() : o.OptionValue.Trim(),
                Order: o.DisplayOrder > 0 ? o.DisplayOrder : i + 1))
            .ToList();

        if (needsOptions && options.Count == 0)
            return Result<FormFieldResponse>.Fail(ErrorCode.Validation, "At least one option is required for this control type.");

        var id = fieldId ?? Guid.NewGuid();
        var isNew = fieldId == null || fieldId == Guid.Empty;
        if (isNew) id = Guid.NewGuid();

        var fieldKey = string.IsNullOrWhiteSpace(request.FieldKey)
            ? FormsRepository.SlugifyFieldKey(request.ControlLabel)
            : FormsRepository.SlugifyFieldKey(request.FieldKey);

        var parentOptionIds = request.ParentFieldId == null
            ? (IReadOnlyList<Guid>)Array.Empty<Guid>()
            : (request.ParentOptionIds ?? []).Distinct().ToList();

        try
        {
            await repository.SaveFieldAsync(
                TenantId, formId, id, request.FormGroupId,
                request.ControlLabel.Trim(), request.ControlType, request.ControlMaxLength, request.ControlRequired,
                request.DisplayOrder, request.ControlNotes?.Trim(), fieldKey,
                string.IsNullOrWhiteSpace(request.DisplayControlLabel) ? "visible" : request.DisplayControlLabel.Trim(),
                request.ClassName?.Trim(),
                request.ParentFieldId, sendEmail, request.ValidationRegexPresetId, displayOnList,
                options, parentOptionIds, UserId, isNew, ct);
        }
        catch (Exception ex)
        {
            var msg = ex.Message.Contains("At most 5 fields", StringComparison.OrdinalIgnoreCase)
                ? "At most 5 fields can be shown on the list page."
                : "Unable to save field.";
            return Result<FormFieldResponse>.Fail(ErrorCode.Validation, msg);
        }

        var (fields, opts, parents) = await repository.GetFieldsAsync(TenantId, formId, ct);
        var saved = fields.FirstOrDefault(f => f.FieldId == id);
        if (saved == null) return Result<FormFieldResponse>.Fail(ErrorCode.InternalError, "Field saved but could not be reloaded.");

        return Result<FormFieldResponse>.Ok(MapField(
            saved,
            opts.Where(o => o.FieldId == id).ToList(),
            parents.Where(p => p.FieldId == id).Select(p => p.OptionId).ToList()), "Field saved.");
    }

    public async Task<Result<bool>> DeleteFieldAsync(Guid formId, Guid fieldId, CancellationToken ct)
    {
        var err = await RequireFormsMenu<bool>(ct);
        if (err != null) return err;

        if (await repository.GetFormByIdAsync(TenantId, formId, ct) == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Form not found.");

        await repository.DeleteFieldAsync(TenantId, fieldId, UserId, ct);
        return Result<bool>.Ok(true, "Field deleted.");
    }

    public async Task<Result<IEnumerable<ValidationRegexPresetResponse>>> GetRegexPresetsAsync(CancellationToken ct)
    {
        var err = await RequireFormsMenu<IEnumerable<ValidationRegexPresetResponse>>(ct);
        if (err != null) return err;

        var rows = await repository.GetRegexPresetsAsync(ct);
        return Result<IEnumerable<ValidationRegexPresetResponse>>.Ok(rows.Select(r =>
            new ValidationRegexPresetResponse(r.ValidationRegexPresetId, r.Name, r.Pattern, r.Description, r.DisplayOrder)));
    }

    public async Task<Result<FormFollowupConfigResponse>> GetFollowupConfigAsync(Guid formId, CancellationToken ct)
    {
        var err = await RequireFormsMenu<FormFollowupConfigResponse>(ct);
        if (err != null) return err;

        var form = await repository.GetFormByIdAsync(TenantId, formId, ct);
        if (form == null)
            return Result<FormFollowupConfigResponse>.Fail(ErrorCode.NotFound, "Form not found.");

        var (config, usedAs) = await repository.GetFollowupConfigAsync(TenantId, formId, ct);
        return Result<FormFollowupConfigResponse>.Ok(MapFollowupConfig(formId, form.Name, config, usedAs));
    }

    public async Task<Result<FormFollowupConfigResponse>> SaveFollowupConfigAsync(
        Guid formId, SaveFormFollowupConfigRequest request, CancellationToken ct)
    {
        var err = await RequireFormsMenu<FormFollowupConfigResponse>(ct);
        if (err != null) return err;

        var form = await repository.GetFormByIdAsync(TenantId, formId, ct);
        if (form == null)
            return Result<FormFollowupConfigResponse>.Fail(ErrorCode.NotFound, "Form not found.");

        if (request.FollowUpFormId is Guid followUpId && followUpId != Guid.Empty)
        {
            var followUp = await repository.GetFormByIdAsync(TenantId, followUpId, ct);
            if (followUp == null)
                return Result<FormFollowupConfigResponse>.Fail(ErrorCode.Validation, "Follow-up form not found.");
        }

        try
        {
            await repository.SaveFollowupConfigAsync(
                TenantId,
                formId,
                request.FollowUpFormId is Guid id && id != Guid.Empty ? id : null,
                request.AllowMultiple,
                UserId,
                ct);
        }
        catch (Exception ex)
        {
            return Result<FormFollowupConfigResponse>.Fail(ErrorCode.Validation, CleanFollowupSqlMessage(ex.Message));
        }

        return await GetFollowupConfigAsync(formId, ct);
    }

    private static FormFollowupConfigResponse MapFollowupConfig(
        Guid formId,
        string formName,
        FormFollowupConfigRow? config,
        FormFollowupUsedAsRow? usedAs) =>
        new(
            config?.FormFollowupConfigId,
            config?.PrimaryFormId ?? formId,
            config?.PrimaryFormName ?? formName,
            config?.FollowupFormId,
            config?.FollowupFormName,
            config?.AllowMultiple ?? true,
            usedAs?.PrimaryFormId,
            usedAs?.PrimaryFormName);

    private static FormResponse MapForm(FormRow row, IReadOnlyList<Guid> roleIds) => new(
        row.FormId, row.Name, row.Description, row.ProjectId, row.ProjectName,
        row.Status == (byte)ActiveInactiveStatus.Active ? "Active" : "Inactive",
        row.Status, row.DisplayOrder, row.CollectLocation, roleIds, row.CreatedAt, row.UpdatedAt);

    private static string CleanSqlMessage(string message)
    {
        if (message.Contains("Select a valid active project", StringComparison.OrdinalIgnoreCase))
            return "Select a valid active project.";
        return "Unable to save form.";
    }

    private static string CleanFollowupSqlMessage(string message)
    {
        if (message.Contains("Primary form not found", StringComparison.OrdinalIgnoreCase))
            return "Primary form not found or inactive.";
        if (message.Contains("Follow-up form must be different", StringComparison.OrdinalIgnoreCase))
            return "Follow-up form must be different from the primary form.";
        if (message.Contains("Follow-up form not found", StringComparison.OrdinalIgnoreCase))
            return "Follow-up form not found or inactive.";
        if (message.Contains("already used as a follow-up", StringComparison.OrdinalIgnoreCase))
            return "This form is already used as a follow-up form and cannot have its own follow-up.";
        if (message.Contains("already has its own follow-up", StringComparison.OrdinalIgnoreCase))
            return "Selected follow-up form already has its own follow-up configured.";
        if (message.Contains("already linked to another primary", StringComparison.OrdinalIgnoreCase))
            return "Selected follow-up form is already linked to another primary form.";
        var cleaned = message;
        var idx = cleaned.IndexOf('\n');
        if (idx > 0) cleaned = cleaned[..idx];
        cleaned = cleaned.Replace("dbo.", "", StringComparison.OrdinalIgnoreCase).Trim();
        return cleaned.Length is > 0 and < 200 ? cleaned : "Unable to save follow-up configuration.";
    }

    private static FormFieldResponse MapField(FormFieldRow f, IReadOnlyList<FormFieldOptionRow> options, IReadOnlyList<Guid> parentOptionIds) =>
        new(
            f.FieldId, f.FormId, f.FormGroupId, f.ControlLabel, f.ControlType,
            ControlTypeName(f.ControlType), f.ControlMaxLength, f.ControlRequired, f.DisplayOrder,
            f.ControlNotes, f.FieldKey, f.DisplayControlLabel, f.ClassName, f.ParentFieldId,
            f.IsSendEmailNotification, f.ValidationRegexPresetId, f.DisplayOnList,
            options.Select(o => new FormFieldOptionDto(o.OptionId, o.OptionText, o.OptionValue, o.DisplayOrder)).ToList(),
            parentOptionIds);

    private static string ControlTypeName(byte type) => ((FormFieldControlType)type) switch
    {
        FormFieldControlType.Dropdown => "Dropdown",
        FormFieldControlType.RadioButton => "Radio Button",
        FormFieldControlType.Checkbox => "Checkbox",
        FormFieldControlType.Textbox => "Textbox",
        FormFieldControlType.File => "File",
        FormFieldControlType.CheckboxSingle => "Checkbox - Single",
        FormFieldControlType.Label => "Label",
        FormFieldControlType.Email => "Email",
        FormFieldControlType.Date => "Date",
        _ => "Unknown"
    };
}


