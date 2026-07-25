using Dapper;
using SharedKernel.Utilities.Helpers;
using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebApi.Features.Forms.Infrastructure;

public class FormRow
{
    public Guid FormId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public byte Status { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class FormGroupRow
{
    public Guid FormGroupId { get; set; }
    public Guid FormId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string? GroupDisplayNameKey { get; set; }
    public int DisplayOrder { get; set; }
}

public class FormFieldRow
{
    public Guid FieldId { get; set; }
    public Guid FormId { get; set; }
    public Guid FormGroupId { get; set; }
    public string ControlLabel { get; set; } = string.Empty;
    public byte ControlType { get; set; }
    public int? ControlMaxLength { get; set; }
    public bool ControlRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? ControlNotes { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string DisplayControlLabel { get; set; } = "visible";
    public string? ClassName { get; set; }
    public Guid? ParentFieldId { get; set; }
    public bool IsSendEmailNotification { get; set; }
    public Guid? ValidationRegexPresetId { get; set; }
    public bool DisplayOnList { get; set; }
}

public class FormFieldOptionRow
{
    public Guid OptionId { get; set; }
    public Guid FieldId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public string OptionValue { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class FormFieldParentOptionRow
{
    public Guid FieldId { get; set; }
    public Guid OptionId { get; set; }
}

public class ValidationRegexPresetRow
{
    public Guid ValidationRegexPresetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

public interface IFormsRepository
{
    Task<IEnumerable<FormRow>> GetFormsAsync(Guid tenantId, CancellationToken ct);
    Task<FormRow?> GetFormByIdAsync(Guid tenantId, Guid formId, CancellationToken ct);
    Task<bool> FormNameExistsAsync(Guid tenantId, string name, Guid? excludeId, CancellationToken ct);
    Task<Guid> CreateFormAsync(Guid tenantId, Guid formId, Guid projectId, string name, string? description, byte status, int displayOrder, Guid createdBy, CancellationToken ct);
    Task UpdateFormAsync(Guid tenantId, Guid formId, Guid projectId, string name, string? description, byte status, int displayOrder, Guid updatedBy, CancellationToken ct);
    Task DeleteFormAsync(Guid tenantId, Guid formId, Guid updatedBy, CancellationToken ct);
    Task<IReadOnlyList<Guid>> GetFormRoleIdsAsync(Guid tenantId, Guid formId, CancellationToken ct);
    Task SetFormRolesAsync(Guid tenantId, Guid formId, IEnumerable<Guid> roleIds, Guid createdBy, CancellationToken ct);

    Task<IEnumerable<FormGroupRow>> GetGroupsAsync(Guid tenantId, Guid formId, CancellationToken ct);
    Task<Guid> SaveGroupAsync(Guid tenantId, Guid formId, Guid formGroupId, string groupName, string? displayNameKey, int displayOrder, Guid actorId, CancellationToken ct);
    Task DeleteGroupAsync(Guid tenantId, Guid formGroupId, Guid updatedBy, CancellationToken ct);

    Task<(IReadOnlyList<FormFieldRow> Fields, IReadOnlyList<FormFieldOptionRow> Options, IReadOnlyList<FormFieldParentOptionRow> ParentOptions)>
        GetFieldsAsync(Guid tenantId, Guid formId, CancellationToken ct);

    Task DeleteFieldAsync(Guid tenantId, Guid fieldId, Guid updatedBy, CancellationToken ct);

    Task SaveFieldAsync(
        Guid tenantId,
        Guid formId,
        Guid fieldId,
        Guid formGroupId,
        string controlLabel,
        byte controlType,
        int? controlMaxLength,
        bool controlRequired,
        int displayOrder,
        string? controlNotes,
        string fieldKey,
        string displayControlLabel,
        string? className,
        Guid? parentFieldId,
        bool isSendEmailNotification,
        Guid? validationRegexPresetId,
        bool displayOnList,
        IReadOnlyList<(Guid OptionId, string Text, string Value, int Order)> options,
        IReadOnlyList<Guid> parentOptionIds,
        Guid actorId,
        bool isNew,
        CancellationToken ct);

    Task<IEnumerable<ValidationRegexPresetRow>> GetRegexPresetsAsync(CancellationToken ct);
}

public class FormsRepository(DbHelper dbHelper) : IFormsRepository
{
    public async Task<IEnumerable<FormRow>> GetFormsAsync(Guid tenantId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryAsync<FormRow>("dbo.sp_form_get_list", new { tenantid = tenantId }, commandType: CommandType.StoredProcedure);
    }

    public async Task<FormRow?> GetFormByIdAsync(Guid tenantId, Guid formId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<FormRow>("dbo.sp_form_get_by_id", new { tenantid = tenantId, formid = formId }, commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> FormNameExistsAsync(Guid tenantId, string name, Guid? excludeId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var count = await conn.ExecuteScalarAsync<int>("dbo.sp_form_name_exists", new { tenantid = tenantId, name, excludeid = excludeId }, commandType: CommandType.StoredProcedure);
        return count > 0;
    }

    public async Task<Guid> CreateFormAsync(Guid tenantId, Guid formId, Guid projectId, string name, string? description, byte status, int displayOrder, Guid createdBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<Guid>("dbo.sp_form_create",
            new { formid = formId, tenantid = tenantId, projectid = projectId, name, description, status, displayorder = displayOrder, createdby = createdBy },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateFormAsync(Guid tenantId, Guid formId, Guid projectId, string name, string? description, byte status, int displayOrder, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_form_update",
            new { tenantid = tenantId, formid = formId, projectid = projectId, name, description, status, displayorder = displayOrder, updatedby = updatedBy },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteFormAsync(Guid tenantId, Guid formId, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_form_delete", new { tenantid = tenantId, formid = formId, updatedby = updatedBy }, commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<Guid>> GetFormRoleIdsAsync(Guid tenantId, Guid formId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var ids = await conn.QueryAsync<Guid>("dbo.sp_form_get_role_ids", new { tenantid = tenantId, formid = formId }, commandType: CommandType.StoredProcedure);
        return ids.ToList();
    }

    public async Task SetFormRolesAsync(Guid tenantId, Guid formId, IEnumerable<Guid> roleIds, Guid createdBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var csv = string.Join(",", roleIds);
        await conn.ExecuteAsync("dbo.sp_form_set_roles",
            new { tenantid = tenantId, formid = formId, roleids = csv, createdby = createdBy },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<FormGroupRow>> GetGroupsAsync(Guid tenantId, Guid formId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryAsync<FormGroupRow>("dbo.sp_form_group_get_list", new { tenantid = tenantId, formid = formId }, commandType: CommandType.StoredProcedure);
    }

    public async Task<Guid> SaveGroupAsync(Guid tenantId, Guid formId, Guid formGroupId, string groupName, string? displayNameKey, int displayOrder, Guid actorId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<Guid>("dbo.sp_form_group_save",
            new
            {
                formgroupid = formGroupId,
                tenantid = tenantId,
                formid = formId,
                groupname = groupName,
                groupdisplaynamekey = displayNameKey,
                displayorder = displayOrder,
                actorid = actorId
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteGroupAsync(Guid tenantId, Guid formGroupId, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_form_group_delete",
            new { tenantid = tenantId, formgroupid = formGroupId, updatedby = updatedBy },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<(IReadOnlyList<FormFieldRow> Fields, IReadOnlyList<FormFieldOptionRow> Options, IReadOnlyList<FormFieldParentOptionRow> ParentOptions)>
        GetFieldsAsync(Guid tenantId, Guid formId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        using var multi = await conn.QueryMultipleAsync("dbo.sp_form_field_get_list",
            new { tenantid = tenantId, formid = formId },
            commandType: CommandType.StoredProcedure);
        var fields = (await multi.ReadAsync<FormFieldRow>()).ToList();
        var options = (await multi.ReadAsync<FormFieldOptionRow>()).ToList();
        var parents = (await multi.ReadAsync<FormFieldParentOptionRow>()).ToList();
        return (fields, options, parents);
    }

    public async Task DeleteFieldAsync(Guid tenantId, Guid fieldId, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_form_field_delete",
            new { tenantid = tenantId, fieldid = fieldId, updatedby = updatedBy },
            commandType: CommandType.StoredProcedure);
    }

    public async Task SaveFieldAsync(
        Guid tenantId,
        Guid formId,
        Guid fieldId,
        Guid formGroupId,
        string controlLabel,
        byte controlType,
        int? controlMaxLength,
        bool controlRequired,
        int displayOrder,
        string? controlNotes,
        string fieldKey,
        string displayControlLabel,
        string? className,
        Guid? parentFieldId,
        bool isSendEmailNotification,
        Guid? validationRegexPresetId,
        bool displayOnList,
        IReadOnlyList<(Guid OptionId, string Text, string Value, int Order)> options,
        IReadOnlyList<Guid> parentOptionIds,
        Guid actorId,
        bool isNew,
        CancellationToken ct)
    {
        var optionsJson = JsonSerializer.Serialize(options.Select(o => new
        {
            optionid = o.OptionId,
            optiontext = o.Text,
            optionvalue = o.Value,
            displayorder = o.Order
        }));

        var parentIds = string.Join(",", parentOptionIds);

        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_form_field_save",
            new
            {
                tenantid = tenantId,
                formid = formId,
                fieldid = fieldId,
                formgroupid = formGroupId,
                controllabel = controlLabel,
                controltype = controlType,
                controlmaxlength = controlMaxLength,
                controlrequired = controlRequired,
                displayorder = displayOrder,
                controlnotes = controlNotes,
                fieldkey = fieldKey,
                displaycontrollabel = displayControlLabel,
                classname = className,
                parentfieldid = parentFieldId,
                issendemailnotification = isSendEmailNotification,
                validationregexpresetid = validationRegexPresetId,
                displayonlist = displayOnList,
                optionsjson = optionsJson,
                parentoptionids = parentIds,
                actorid = actorId,
                isnew = isNew
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ValidationRegexPresetRow>> GetRegexPresetsAsync(CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryAsync<ValidationRegexPresetRow>(
            "dbo.sp_validation_regex_preset_get_list",
            commandType: CommandType.StoredProcedure);
    }

    public static string SlugifyFieldKey(string label)
    {
        var cleaned = Regex.Replace(label.Trim(), @"[^A-Za-z0-9\s_-]", "");
        cleaned = Regex.Replace(cleaned, @"\s+", "_");
        return cleaned.Length > 180 ? cleaned[..180] : cleaned;
    }
}
