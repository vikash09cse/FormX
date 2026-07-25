using Dapper;
using SharedKernel.Utilities.Helpers;
using System.Data;

namespace WebApi.Features.Submissions.Infrastructure;

public class AvailableFormRow
{
    public Guid FormId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

public class SubmissionProjectRow
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class FormDefHeaderRow
{
    public Guid FormId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
}

public class FormDefGroupRow
{
    public Guid FormGroupId { get; set; }
    public Guid FormId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string? GroupDisplayNameKey { get; set; }
    public int DisplayOrder { get; set; }
}

public class FormDefFieldRow
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
    public Guid? ParentFieldId { get; set; }
    public bool IsSendEmailNotification { get; set; }
    public Guid? ValidationRegexPresetId { get; set; }
    public string? ValidationRegexPattern { get; set; }
    public string? ValidationRegexName { get; set; }
}

public class FormDefOptionRow
{
    public Guid OptionId { get; set; }
    public Guid FieldId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public string OptionValue { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class FormDefParentOptionRow
{
    public Guid FieldId { get; set; }
    public Guid OptionId { get; set; }
}

public class SubmissionListColumnRow
{
    public Guid FieldId { get; set; }
    public string ControlLabel { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class SubmissionListValueRow
{
    public Guid SubmissionId { get; set; }
    public Guid FieldId { get; set; }
    public string? ValueText { get; set; }
}

public class SubmissionListRow
{
    public Guid SubmissionId { get; set; }
    public Guid FormId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid SubmittedBy { get; set; }
    public DateTime SubmittedAt { get; set; }
    public byte Status { get; set; }
}

public class SubmissionValueRow
{
    public Guid SubmissionValueId { get; set; }
    public Guid SubmissionId { get; set; }
    public Guid FieldId { get; set; }
    public string? ValueText { get; set; }
}

public class EmailNotifyFieldRow
{
    public Guid FieldId { get; set; }
    public string ControlLabel { get; set; } = string.Empty;
    public string? ValueText { get; set; }
}

public interface ISubmissionsRepository
{
    Task<IEnumerable<AvailableFormRow>> GetAvailableFormsAsync(Guid tenantId, Guid userId, bool isSuperAdmin, CancellationToken ct);
    Task<IEnumerable<SubmissionProjectRow>> GetProjectsForUserAsync(Guid tenantId, Guid userId, CancellationToken ct);
    Task<bool> UserCanAccessFormAsync(Guid tenantId, Guid userId, Guid formId, bool isSuperAdmin, CancellationToken ct);
    Task<(FormDefHeaderRow? Form, IReadOnlyList<FormDefGroupRow> Groups, IReadOnlyList<FormDefFieldRow> Fields, IReadOnlyList<FormDefOptionRow> Options, IReadOnlyList<FormDefParentOptionRow> Parents)>
        GetFormDefinitionAsync(Guid tenantId, Guid formId, CancellationToken ct);
    Task<(int TotalCount, IReadOnlyList<SubmissionListColumnRow> Columns, IReadOnlyList<SubmissionListRow> Items, IReadOnlyList<SubmissionListValueRow> Values)>
        GetMySubmissionsPageAsync(Guid tenantId, Guid userId, Guid formId, int page, int pageSize, CancellationToken ct);
    Task<(SubmissionListRow? Header, IReadOnlyList<SubmissionValueRow> Values)> GetByIdAsync(Guid tenantId, Guid userId, Guid submissionId, CancellationToken ct);
    Task SaveAsync(Guid tenantId, Guid userId, Guid submissionId, Guid formId, string valuesJson, bool isNew, Guid actorId, CancellationToken ct);
    Task DeleteAsync(Guid tenantId, Guid userId, Guid submissionId, Guid actorId, CancellationToken ct);
    Task<IEnumerable<EmailNotifyFieldRow>> GetEmailNotifyFieldsAsync(Guid tenantId, Guid formId, Guid submissionId, CancellationToken ct);
}

public class SubmissionsRepository(DbHelper dbHelper) : ISubmissionsRepository
{
    public async Task<IEnumerable<AvailableFormRow>> GetAvailableFormsAsync(Guid tenantId, Guid userId, bool isSuperAdmin, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryAsync<AvailableFormRow>(
            "dbo.sp_submission_available_forms",
            new { tenantid = tenantId, userid = userId, issuperadmin = isSuperAdmin },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<SubmissionProjectRow>> GetProjectsForUserAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryAsync<SubmissionProjectRow>(
            "dbo.sp_submission_projects_for_user",
            new { tenantid = tenantId, userid = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UserCanAccessFormAsync(Guid tenantId, Guid userId, Guid formId, bool isSuperAdmin, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "dbo.sp_submission_user_can_access_form",
            new { tenantid = tenantId, userid = userId, formid = formId, issuperadmin = isSuperAdmin },
            commandType: CommandType.StoredProcedure);
        if (row == null) return false;
        return (bool)row.hasaccess;
    }

    public async Task<(FormDefHeaderRow? Form, IReadOnlyList<FormDefGroupRow> Groups, IReadOnlyList<FormDefFieldRow> Fields, IReadOnlyList<FormDefOptionRow> Options, IReadOnlyList<FormDefParentOptionRow> Parents)>
        GetFormDefinitionAsync(Guid tenantId, Guid formId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        using var multi = await conn.QueryMultipleAsync(
            "dbo.sp_submission_form_definition",
            new { tenantid = tenantId, formid = formId },
            commandType: CommandType.StoredProcedure);
        var form = await multi.ReadFirstOrDefaultAsync<FormDefHeaderRow>();
        var groups = (await multi.ReadAsync<FormDefGroupRow>()).ToList();
        var fields = (await multi.ReadAsync<FormDefFieldRow>()).ToList();
        var options = (await multi.ReadAsync<FormDefOptionRow>()).ToList();
        var parents = (await multi.ReadAsync<FormDefParentOptionRow>()).ToList();
        return (form, groups, fields, options, parents);
    }

    public async Task<(int TotalCount, IReadOnlyList<SubmissionListColumnRow> Columns, IReadOnlyList<SubmissionListRow> Items, IReadOnlyList<SubmissionListValueRow> Values)>
        GetMySubmissionsPageAsync(Guid tenantId, Guid userId, Guid formId, int page, int pageSize, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        using var multi = await conn.QueryMultipleAsync(
            "dbo.sp_submission_get_list_mine",
            new { tenantid = tenantId, userid = userId, formid = formId, page, pagesize = pageSize },
            commandType: CommandType.StoredProcedure);
        var total = await multi.ReadFirstAsync<int>();
        var columns = (await multi.ReadAsync<SubmissionListColumnRow>()).ToList();
        var items = (await multi.ReadAsync<SubmissionListRow>()).ToList();
        var values = (await multi.ReadAsync<SubmissionListValueRow>()).ToList();
        return (total, columns, items, values);
    }

    public async Task<(SubmissionListRow? Header, IReadOnlyList<SubmissionValueRow> Values)> GetByIdAsync(
        Guid tenantId, Guid userId, Guid submissionId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        using var multi = await conn.QueryMultipleAsync(
            "dbo.sp_submission_get_by_id",
            new { tenantid = tenantId, userid = userId, submissionid = submissionId },
            commandType: CommandType.StoredProcedure);
        var header = await multi.ReadFirstOrDefaultAsync<SubmissionListRow>();
        var values = (await multi.ReadAsync<SubmissionValueRow>()).ToList();
        return (header, values);
    }

    public async Task SaveAsync(
        Guid tenantId, Guid userId, Guid submissionId, Guid formId,
        string valuesJson, bool isNew, Guid actorId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_submission_save",
            new
            {
                tenantid = tenantId,
                userid = userId,
                submissionid = submissionId,
                formid = formId,
                valuesjson = valuesJson,
                isnew = isNew,
                actorid = actorId
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(Guid tenantId, Guid userId, Guid submissionId, Guid actorId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_submission_delete",
            new { tenantid = tenantId, userid = userId, submissionid = submissionId, actorid = actorId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<EmailNotifyFieldRow>> GetEmailNotifyFieldsAsync(
        Guid tenantId, Guid formId, Guid submissionId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryAsync<EmailNotifyFieldRow>(
            "dbo.sp_submission_email_notify_fields",
            new { tenantid = tenantId, formid = formId, submissionid = submissionId },
            commandType: CommandType.StoredProcedure);
    }
}
