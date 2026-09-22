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

    public bool CollectLocation { get; set; }

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

    public bool CollectLocation { get; set; }

    public bool HasFollowUp { get; set; }

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

    public string? ClassName { get; set; }

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

    public string? CreatedByName { get; set; }

    public DateTime SubmittedAt { get; set; }

    public byte Status { get; set; }

    public Guid? ParentSubmissionId { get; set; }

    public Guid? StateId { get; set; }

    public Guid? DistrictId { get; set; }

    public Guid? BlockId { get; set; }

    public Guid? VillageId { get; set; }

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



public class FollowupConfigSummaryRow

{

    public Guid FollowupFormId { get; set; }

    public string? FollowupFormName { get; set; }

    public bool AllowMultiple { get; set; }

}



public class FollowupFormIdRow

{

    public Guid FollowupFormId { get; set; }

}



public class FollowupExportHeaderRow

{

    public Guid SubmissionId { get; set; }

    public Guid ParentSubmissionId { get; set; }

    public DateTime SubmittedAt { get; set; }

    public string? CreatedByName { get; set; }

}



public interface ISubmissionsRepository

{

    Task<IEnumerable<AvailableFormRow>> GetAvailableFormsAsync(Guid tenantId, Guid userId, bool isSuperAdmin, CancellationToken ct);

    Task<IEnumerable<SubmissionProjectRow>> GetProjectsForUserAsync(Guid tenantId, Guid userId, CancellationToken ct);

    Task<bool> UserCanAccessFormAsync(Guid tenantId, Guid userId, Guid formId, bool isSuperAdmin, CancellationToken ct);

    Task<(FormDefHeaderRow? Form, IReadOnlyList<FormDefGroupRow> Groups, IReadOnlyList<FormDefFieldRow> Fields, IReadOnlyList<FormDefOptionRow> Options, IReadOnlyList<FormDefParentOptionRow> Parents)>

        GetFormDefinitionAsync(Guid tenantId, Guid formId, CancellationToken ct);

    Task<(int TotalCount, IReadOnlyList<SubmissionListColumnRow> Columns, IReadOnlyList<SubmissionListRow> Items, IReadOnlyList<SubmissionListValueRow> Values)>

        GetMySubmissionsPageAsync(Guid tenantId, Guid userId, Guid formId, int page, int pageSize, string? search, bool isSuperAdmin, byte dataScope, string? districtIds, string? projectIds, CancellationToken ct);

    Task<(int TotalCount, string? FormName, IReadOnlyList<SubmissionListColumnRow> Columns, IReadOnlyList<SubmissionListRow> Items, IReadOnlyList<SubmissionListValueRow> Values, bool HasFollowUpForm, IReadOnlyList<SubmissionListColumnRow> FollowUpColumns, IReadOnlyList<FollowupExportHeaderRow> FollowUps, IReadOnlyList<SubmissionListValueRow> FollowUpValues)>

        ExportMineAsync(Guid tenantId, Guid userId, Guid formId, string? search, bool isSuperAdmin, byte dataScope, string? districtIds, string? projectIds, CancellationToken ct);

    Task<(SubmissionListRow? Header, IReadOnlyList<SubmissionValueRow> Values)> GetByIdAsync(Guid tenantId, Guid userId, Guid submissionId, bool isSuperAdmin, byte dataScope, string? districtIds, string? projectIds, CancellationToken ct);

    Task<(FollowupConfigSummaryRow? Config, IReadOnlyList<SubmissionListRow> Items, IReadOnlyList<SubmissionListColumnRow> Columns, IReadOnlyList<SubmissionListValueRow> Values)>

        GetFollowupsAsync(Guid tenantId, Guid userId, Guid parentSubmissionId, bool isSuperAdmin, byte dataScope, string? districtIds, string? projectIds, CancellationToken ct);

    Task SaveAsync(

        Guid tenantId, Guid userId, Guid submissionId, Guid formId,

        string valuesJson, bool isNew, Guid actorId, Guid? parentSubmissionId,

        bool isSuperAdmin, byte dataScope, bool canCreate, bool canEdit,

        string? districtIds, string? projectIds,

        Guid? stateId, Guid? districtId, Guid? blockId, Guid? villageId,

        CancellationToken ct);

    Task DeleteAsync(Guid tenantId, Guid userId, Guid submissionId, Guid actorId, bool isSuperAdmin, byte dataScope, bool canDelete, string? projectIds, CancellationToken ct);

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

        GetMySubmissionsPageAsync(Guid tenantId, Guid userId, Guid formId, int page, int pageSize, string? search, bool isSuperAdmin, byte dataScope, string? districtIds, string? projectIds, CancellationToken ct)

    {

        using var conn = dbHelper.GetConnection();

        using var multi = await conn.QueryMultipleAsync(

            "dbo.sp_submission_get_list_mine",

            new

            {

                tenantid = tenantId,

                userid = userId,

                formid = formId,

                page,

                pagesize = pageSize,

                search,

                issuperadmin = isSuperAdmin,

                datascope = dataScope,

                districtids = districtIds,

                projectids = projectIds

            },

            commandType: CommandType.StoredProcedure);

        var total = await multi.ReadFirstAsync<int>();

        var columns = (await multi.ReadAsync<SubmissionListColumnRow>()).ToList();

        var items = (await multi.ReadAsync<SubmissionListRow>()).ToList();

        var values = (await multi.ReadAsync<SubmissionListValueRow>()).ToList();

        return (total, columns, items, values);

    }



    public async Task<(int TotalCount, string? FormName, IReadOnlyList<SubmissionListColumnRow> Columns, IReadOnlyList<SubmissionListRow> Items, IReadOnlyList<SubmissionListValueRow> Values, bool HasFollowUpForm, IReadOnlyList<SubmissionListColumnRow> FollowUpColumns, IReadOnlyList<FollowupExportHeaderRow> FollowUps, IReadOnlyList<SubmissionListValueRow> FollowUpValues)>

        ExportMineAsync(Guid tenantId, Guid userId, Guid formId, string? search, bool isSuperAdmin, byte dataScope, string? districtIds, string? projectIds, CancellationToken ct)

    {

        using var conn = dbHelper.GetConnection();

        using var multi = await conn.QueryMultipleAsync(

            "dbo.sp_submission_export_mine",

            new

            {

                tenantid = tenantId,

                userid = userId,

                formid = formId,

                search,

                issuperadmin = isSuperAdmin,

                datascope = dataScope,

                districtids = districtIds,

                projectids = projectIds

            },

            commandType: CommandType.StoredProcedure);

        var total = await multi.ReadFirstAsync<int>();

        var formName = await multi.ReadFirstOrDefaultAsync<string>();

        var columns = (await multi.ReadAsync<SubmissionListColumnRow>()).ToList();

        var items = (await multi.ReadAsync<SubmissionListRow>()).ToList();

        var values = (await multi.ReadAsync<SubmissionListValueRow>()).ToList();

        var hasFollowUpForm = (await multi.ReadAsync<FollowupFormIdRow>()).Any();

        var followUpColumns = (await multi.ReadAsync<SubmissionListColumnRow>()).ToList();

        var followUps = (await multi.ReadAsync<FollowupExportHeaderRow>()).ToList();

        var followUpValues = (await multi.ReadAsync<SubmissionListValueRow>()).ToList();

        return (total, formName, columns, items, values, hasFollowUpForm, followUpColumns, followUps, followUpValues);

    }



    public async Task<(SubmissionListRow? Header, IReadOnlyList<SubmissionValueRow> Values)> GetByIdAsync(

        Guid tenantId, Guid userId, Guid submissionId, bool isSuperAdmin, byte dataScope, string? districtIds, string? projectIds, CancellationToken ct)

    {

        using var conn = dbHelper.GetConnection();

        using var multi = await conn.QueryMultipleAsync(

            "dbo.sp_submission_get_by_id",

            new

            {

                tenantid = tenantId,

                userid = userId,

                submissionid = submissionId,

                issuperadmin = isSuperAdmin,

                datascope = dataScope,

                districtids = districtIds,

                projectids = projectIds

            },

            commandType: CommandType.StoredProcedure);

        var header = await multi.ReadFirstOrDefaultAsync<SubmissionListRow>();

        var values = (await multi.ReadAsync<SubmissionValueRow>()).ToList();

        return (header, values);

    }



    public async Task<(FollowupConfigSummaryRow? Config, IReadOnlyList<SubmissionListRow> Items, IReadOnlyList<SubmissionListColumnRow> Columns, IReadOnlyList<SubmissionListValueRow> Values)>

        GetFollowupsAsync(Guid tenantId, Guid userId, Guid parentSubmissionId, bool isSuperAdmin, byte dataScope, string? districtIds, string? projectIds, CancellationToken ct)

    {

        using var conn = dbHelper.GetConnection();

        using var multi = await conn.QueryMultipleAsync(

            "dbo.sp_submission_get_followups",

            new

            {

                tenantid = tenantId,

                userid = userId,

                parentsubmissionid = parentSubmissionId,

                issuperadmin = isSuperAdmin,

                datascope = dataScope,

                districtids = districtIds,

                projectids = projectIds

            },

            commandType: CommandType.StoredProcedure);

        var config = await multi.ReadFirstOrDefaultAsync<FollowupConfigSummaryRow>();

        var items = (await multi.ReadAsync<SubmissionListRow>()).ToList();

        var columns = (await multi.ReadAsync<SubmissionListColumnRow>()).ToList();

        var values = (await multi.ReadAsync<SubmissionListValueRow>()).ToList();

        return (config, items, columns, values);

    }



    public async Task SaveAsync(

        Guid tenantId, Guid userId, Guid submissionId, Guid formId,

        string valuesJson, bool isNew, Guid actorId, Guid? parentSubmissionId,

        bool isSuperAdmin, byte dataScope, bool canCreate, bool canEdit,

        string? districtIds, string? projectIds,

        Guid? stateId, Guid? districtId, Guid? blockId, Guid? villageId,

        CancellationToken ct)

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

                actorid = actorId,

                parentsubmissionid = parentSubmissionId,

                issuperadmin = isSuperAdmin,

                datascope = dataScope,

                cancreate = canCreate,

                canedit = canEdit,

                districtids = districtIds,

                projectids = projectIds,

                stateid = stateId,

                districtid = districtId,

                blockid = blockId,

                villageid = villageId

            },

            commandType: CommandType.StoredProcedure);

    }



    public async Task DeleteAsync(Guid tenantId, Guid userId, Guid submissionId, Guid actorId, bool isSuperAdmin, byte dataScope, bool canDelete, string? projectIds, CancellationToken ct)

    {

        using var conn = dbHelper.GetConnection();

        await conn.ExecuteAsync(

            "dbo.sp_submission_delete",

            new

            {

                tenantid = tenantId,

                userid = userId,

                submissionid = submissionId,

                actorid = actorId,

                issuperadmin = isSuperAdmin,

                datascope = dataScope,

                candelete = canDelete,

                projectids = projectIds

            },

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


