using System.Text.Json;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using SharedKernel.Enums;
using SharedKernel.Services;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Extensions;
using WebApi.Features.Permissions;
using WebApi.Features.Submissions.Infrastructure;

namespace WebApi.Features.Submissions;

public class SubmissionsService(
    ISubmissionsRepository repository,
    IPermissionsRepository permissionsRepository,
    IHttpContextAccessor httpContextAccessor,
    EmailService emailService,
    ILogger<SubmissionsService> logger)
{
    private Result<T>? RequireTenant<T>()
    {
        var ctx = httpContextAccessor.HttpContext?.TryGetTenantContext();
        if (ctx == null || !ctx.IsValidForTenantScope())
            return Result<T>.Fail(ErrorCode.Forbidden, "Tenant context is required.");
        return null;
    }

    private Guid TenantId => httpContextAccessor.GetTenantContext().TenantId;
    private Guid UserId => httpContextAccessor.GetTenantContext().UserId;
    private bool IsSuperAdmin => httpContextAccessor.GetTenantContext().UserType == (byte)UserType.TenantSuperAdmin;

    private async Task<EffectivePermissions> GetPerms(CancellationToken ct)
    {
        var ctx = httpContextAccessor.GetTenantContext();
        return await permissionsRepository.GetForUserAsync(ctx.TenantId, ctx.UserId, ctx.UserType == (byte)UserType.TenantSuperAdmin, ct);
    }

    private static string? ToCsv(IReadOnlyList<Guid> ids) =>
        ids.Count == 0 ? null : string.Join(",", ids);

    public async Task<Result<IEnumerable<AvailableFormResponse>>> GetAvailableFormsAsync(CancellationToken ct)
    {
        var err = RequireTenant<IEnumerable<AvailableFormResponse>>();
        if (err != null) return err;

        var rows = await repository.GetAvailableFormsAsync(TenantId, UserId, IsSuperAdmin, ct);
        return Result<IEnumerable<AvailableFormResponse>>.Ok(
            rows.Select(r => new AvailableFormResponse(r.FormId, r.Name, r.Description, r.DisplayOrder, r.CollectLocation)));
    }

    public async Task<Result<IEnumerable<SubmissionProjectResponse>>> GetProjectsAsync(CancellationToken ct)
    {
        var err = RequireTenant<IEnumerable<SubmissionProjectResponse>>();
        if (err != null) return err;

        var rows = await repository.GetProjectsForUserAsync(TenantId, UserId, ct);
        return Result<IEnumerable<SubmissionProjectResponse>>.Ok(
            rows.Select(r => new SubmissionProjectResponse(r.ProjectId, r.ProjectName, r.Code)));
    }

    public async Task<Result<FormDefinitionResponse>> GetDefinitionAsync(Guid formId, CancellationToken ct)
    {
        var err = RequireTenant<FormDefinitionResponse>();
        if (err != null) return err;

        if (!await repository.UserCanAccessFormAsync(TenantId, UserId, formId, IsSuperAdmin, ct))
            return Result<FormDefinitionResponse>.Fail(ErrorCode.Forbidden, "You do not have access to this form.");

        var (form, groups, fields, options, parents) = await repository.GetFormDefinitionAsync(TenantId, formId, ct);
        if (form == null)
            return Result<FormDefinitionResponse>.Fail(ErrorCode.NotFound, "Form not found.");

        var groupDtos = groups.Select(g =>
        {
            var groupFields = fields.Where(f => f.FormGroupId == g.FormGroupId)
                .Select(f => MapField(f, options, parents))
                .ToList();
            return new FormDefinitionGroupDto(g.FormGroupId, g.GroupName, g.GroupDisplayNameKey, g.DisplayOrder, groupFields);
        }).ToList();

        return Result<FormDefinitionResponse>.Ok(new FormDefinitionResponse(
            form.FormId, form.Name, form.Description, form.ProjectId, form.ProjectName, form.CollectLocation, form.HasFollowUp, groupDtos));
    }

    public async Task<Result<SubmissionListPageResponse>> GetMyListAsync(
        Guid formId, int page, int pageSize, string? search, CancellationToken ct)
    {
        var err = RequireTenant<SubmissionListPageResponse>();
        if (err != null) return err;

        if (!await repository.UserCanAccessFormAsync(TenantId, UserId, formId, IsSuperAdmin, ct))
            return Result<SubmissionListPageResponse>.Fail(ErrorCode.Forbidden, "You do not have access to this form.");

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 25;
        if (pageSize > 100) pageSize = 100;

        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        if (search is { Length: > 100 })
            search = search[..100];

        var perms = await GetPerms(ct);
        var (total, columns, items, values) = await repository.GetMySubmissionsPageAsync(
            TenantId, UserId, formId, page, pageSize, search,
            perms.IsSuperAdmin, perms.DataScope, ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds), ct);

        var valuesBySubmission = values
            .GroupBy(v => v.SubmissionId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<string, string?>)g.ToDictionary(
                    x => x.FieldId.ToString(),
                    x => x.ValueText,
                    StringComparer.OrdinalIgnoreCase));

        var columnDtos = columns
            .Select(c => new SubmissionListColumnDto(c.FieldId, c.ControlLabel))
            .ToList();

        var itemDtos = items.Select(r =>
        {
            valuesBySubmission.TryGetValue(r.SubmissionId, out var map);
            map ??= new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            return new SubmissionListItemResponse(
                r.SubmissionId, r.FormId, r.ProjectId, r.ProjectName, r.SubmittedAt, r.Status, map);
        }).ToList();

        return Result<SubmissionListPageResponse>.Ok(
            new SubmissionListPageResponse(total, page, pageSize, columnDtos, itemDtos));
    }

    private const int MaxExportRows = 5000;

    public async Task<Result<SubmissionExportFile>> ExportMyAsync(Guid formId, string? search, CancellationToken ct)
    {
        var err = RequireTenant<SubmissionExportFile>();
        if (err != null) return err;

        if (!await repository.UserCanAccessFormAsync(TenantId, UserId, formId, IsSuperAdmin, ct))
            return Result<SubmissionExportFile>.Fail(ErrorCode.Forbidden, "You do not have access to this form.");

        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        if (search is { Length: > 100 })
            search = search[..100];

        var perms = await GetPerms(ct);
        var (total, formName, columns, items, values, hasFollowUpForm, followUpColumns, followUps, followUpValues) =
            await repository.ExportMineAsync(
                TenantId, UserId, formId, search,
                perms.IsSuperAdmin, perms.DataScope, ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds), ct);

        if (string.IsNullOrWhiteSpace(formName))
            return Result<SubmissionExportFile>.Fail(ErrorCode.NotFound, "Form not found.");

        if (total > MaxExportRows)
            return Result<SubmissionExportFile>.Fail(
                ErrorCode.Validation,
                $"Too many rows to export (max {MaxExportRows:N0}). Narrow your search and try again.");

        var valuesBySubmission = values
            .GroupBy(v => v.SubmissionId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(x => x.FieldId, x => x.ValueText));

        var followUpsByParent = followUps
            .GroupBy(f => f.ParentSubmissionId)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.SubmittedAt).ToList());

        var followUpValuesBySubmission = followUpValues
            .GroupBy(v => v.SubmissionId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(x => x.FieldId, x => x.ValueText));

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Entries");

        var col = 1;
        sheet.Cell(1, col++).Value = "Project";
        foreach (var column in columns)
            sheet.Cell(1, col++).Value = column.ControlLabel;
        sheet.Cell(1, col++).Value = "Created by";
        sheet.Cell(1, col++).Value = "Submitted";

        if (hasFollowUpForm)
        {
            sheet.Cell(1, col++).Value = "Follow-up #";
            sheet.Cell(1, col++).Value = "Follow-up submitted";
            sheet.Cell(1, col++).Value = "Follow-up created by";
            foreach (var column in followUpColumns)
                sheet.Cell(1, col++).Value = column.ControlLabel;
        }

        var headerRange = sheet.Range(1, 1, 1, col - 1);
        headerRange.Style.Font.Bold = true;

        var row = 2;
        foreach (var item in items)
        {
            valuesBySubmission.TryGetValue(item.SubmissionId, out var map);
            followUpsByParent.TryGetValue(item.SubmissionId, out var parentFollowUps);
            parentFollowUps ??= [];

            if (!hasFollowUpForm || parentFollowUps.Count == 0)
            {
                WriteExportRow(sheet, row++, item, columns, map, hasFollowUpForm, followUpColumns, null, null, null);
                continue;
            }

            var fuIndex = 1;
            foreach (var fu in parentFollowUps)
            {
                followUpValuesBySubmission.TryGetValue(fu.SubmissionId, out var fuMap);
                WriteExportRow(sheet, row++, item, columns, map, true, followUpColumns, fuIndex, fu, fuMap);
                fuIndex++;
            }
        }

        sheet.Columns().AdjustToContents(8.0, 60.0);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var fileName = $"{SanitizeFileName(formName)}-entries.xlsx";
        return Result<SubmissionExportFile>.Ok(new SubmissionExportFile(stream.ToArray(), fileName));
    }

    private static void WriteExportRow(
        IXLWorksheet sheet,
        int row,
        SubmissionListRow item,
        IReadOnlyList<SubmissionListColumnRow> columns,
        Dictionary<Guid, string?>? map,
        bool includeFollowUpColumns,
        IReadOnlyList<SubmissionListColumnRow> followUpColumns,
        int? followUpNumber,
        FollowupExportHeaderRow? followUp,
        Dictionary<Guid, string?>? followUpMap)
    {
        var col = 1;
        sheet.Cell(row, col++).Value = item.ProjectName;
        foreach (var column in columns)
        {
            string? text = null;
            if (map != null)
                map.TryGetValue(column.FieldId, out text);
            if (!string.IsNullOrEmpty(text))
                sheet.Cell(row, col).Value = text;
            col++;
        }
        if (!string.IsNullOrEmpty(item.CreatedByName))
            sheet.Cell(row, col).Value = item.CreatedByName;
        col++;
        sheet.Cell(row, col++).Value = item.SubmittedAt.ToString("dd/MM/yyyy, HH:mm:ss");

        if (!includeFollowUpColumns)
            return;

        if (followUpNumber.HasValue && followUp != null)
        {
            sheet.Cell(row, col++).Value = followUpNumber.Value;
            sheet.Cell(row, col++).Value = followUp.SubmittedAt.ToString("dd/MM/yyyy, HH:mm:ss");
            if (!string.IsNullOrEmpty(followUp.CreatedByName))
                sheet.Cell(row, col).Value = followUp.CreatedByName;
            col++;
            foreach (var column in followUpColumns)
            {
                string? text = null;
                if (followUpMap != null)
                    followUpMap.TryGetValue(column.FieldId, out text);
                if (!string.IsNullOrEmpty(text))
                    sheet.Cell(row, col).Value = text;
                col++;
            }
        }
        else
        {
            // Blank follow-up meta + field columns for primaries with no follow-ups
            col += 3 + followUpColumns.Count;
        }
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray()).Trim();
        if (string.IsNullOrWhiteSpace(cleaned))
            cleaned = "form";
        return cleaned.Length > 80 ? cleaned[..80] : cleaned;
    }

    public async Task<Result<SubmissionDetailResponse>> GetByIdAsync(Guid submissionId, CancellationToken ct)
    {
        var err = RequireTenant<SubmissionDetailResponse>();
        if (err != null) return err;

        var perms = await GetPerms(ct);
        var (header, values) = await repository.GetByIdAsync(
            TenantId, UserId, submissionId,
            perms.IsSuperAdmin, perms.DataScope, ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds), ct);
        if (header == null)
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.NotFound, "Submission not found.");

        if (!await repository.UserCanAccessFormAsync(TenantId, UserId, header.FormId, IsSuperAdmin, ct))
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.Forbidden, "You do not have access to this form.");

        return Result<SubmissionDetailResponse>.Ok(new SubmissionDetailResponse(
            header.SubmissionId, header.FormId, header.ProjectId, header.ProjectName, header.SubmittedAt, header.Status,
            header.ParentSubmissionId,
            header.StateId, header.DistrictId, header.BlockId, header.VillageId,
            values.Select(v => new SubmissionValueDto(v.FieldId, v.ValueText)).ToList()));
    }

    public async Task<Result<SubmissionDetailResponse>> CreateAsync(SaveSubmissionRequest request, CancellationToken ct)
    {
        var err = RequireTenant<SubmissionDetailResponse>();
        if (err != null) return err;

        var perms = await GetPerms(ct);
        if (!perms.IsSuperAdmin && !perms.CanCreate)
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.Forbidden, "You do not have permission to create submissions.");

        var validation = await ValidateAndBuildValuesAsync(request.FormId, request.Values, ct);
        if (validation.Error != null) return validation.Error;

        var id = Guid.NewGuid();
        try
        {
            await repository.SaveAsync(
                TenantId, UserId, id, request.FormId, validation.ValuesJson!, true, UserId, null,
                perms.IsSuperAdmin, perms.DataScope, perms.CanCreate, perms.CanEdit,
                ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds),
                request.StateId, request.DistrictId, request.BlockId, request.VillageId, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Submission create failed");
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.Validation, ex.Message.Contains("RAISERROR") || ex.Message.Length < 200
                ? CleanSqlMessage(ex.Message)
                : "Unable to save submission.");
        }

        await TrySendEmailNotificationsAsync(request.FormId, id, validation.FormName!, ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<Result<SubmissionDetailResponse>> UpdateAsync(Guid submissionId, UpdateSubmissionRequest request, CancellationToken ct)
    {
        var err = RequireTenant<SubmissionDetailResponse>();
        if (err != null) return err;

        var perms = await GetPerms(ct);
        if (!perms.IsSuperAdmin && !perms.CanEdit)
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.Forbidden, "You do not have permission to edit submissions.");

        var (header, _) = await repository.GetByIdAsync(
            TenantId, UserId, submissionId,
            perms.IsSuperAdmin, perms.DataScope, ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds), ct);
        if (header == null)
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.NotFound, "Submission not found.");

        var validation = await ValidateAndBuildValuesAsync(header.FormId, request.Values, ct);
        if (validation.Error != null) return validation.Error;

        try
        {
            await repository.SaveAsync(
                TenantId, UserId, submissionId, header.FormId, validation.ValuesJson!, false, UserId, header.ParentSubmissionId,
                perms.IsSuperAdmin, perms.DataScope, perms.CanCreate, perms.CanEdit,
                ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds),
                request.StateId, request.DistrictId, request.BlockId, request.VillageId, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Submission update failed");
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.Validation, CleanSqlMessage(ex.Message));
        }

        await TrySendEmailNotificationsAsync(header.FormId, submissionId, validation.FormName!, ct);
        return await GetByIdAsync(submissionId, ct);
    }

    public async Task<Result<bool>> DeleteAsync(Guid submissionId, CancellationToken ct)
    {
        var err = RequireTenant<bool>();
        if (err != null) return err;

        var perms = await GetPerms(ct);
        if (!perms.IsSuperAdmin && !perms.CanDelete)
            return Result<bool>.Fail(ErrorCode.Forbidden, "You do not have permission to delete submissions.");

        var (header, _) = await repository.GetByIdAsync(
            TenantId, UserId, submissionId,
            perms.IsSuperAdmin, perms.DataScope, ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds), ct);
        if (header == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Submission not found.");

        await repository.DeleteAsync(
            TenantId, UserId, submissionId, UserId,
            perms.IsSuperAdmin, perms.DataScope, perms.CanDelete, ToCsv(perms.ProjectIds), ct);
        return Result<bool>.Ok(true, "Submission deleted.");
    }

    public async Task<Result<SubmissionFollowupsResponse>> GetFollowupsAsync(Guid parentSubmissionId, CancellationToken ct)
    {
        var err = RequireTenant<SubmissionFollowupsResponse>();
        if (err != null) return err;

        var perms = await GetPerms(ct);
        var (parent, _) = await repository.GetByIdAsync(
            TenantId, UserId, parentSubmissionId,
            perms.IsSuperAdmin, perms.DataScope, ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds), ct);
        if (parent == null || parent.ParentSubmissionId != null)
            return Result<SubmissionFollowupsResponse>.Fail(ErrorCode.NotFound, "Parent submission not found.");

        if (!await repository.UserCanAccessFormAsync(TenantId, UserId, parent.FormId, IsSuperAdmin, ct))
            return Result<SubmissionFollowupsResponse>.Fail(ErrorCode.Forbidden, "You do not have access to this form.");

        var (config, items, columns, values) = await repository.GetFollowupsAsync(
            TenantId, UserId, parentSubmissionId,
            perms.IsSuperAdmin, perms.DataScope, ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds), ct);

        var valuesBySubmission = values
            .GroupBy(v => v.SubmissionId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<string, string?>)g.ToDictionary(
                    x => x.FieldId.ToString(),
                    x => x.ValueText,
                    StringComparer.OrdinalIgnoreCase));

        var columnDtos = columns
            .Select(c => new SubmissionListColumnDto(c.FieldId, c.ControlLabel))
            .ToList();

        var itemDtos = items.Select(r =>
        {
            valuesBySubmission.TryGetValue(r.SubmissionId, out var map);
            map ??= new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            return new SubmissionListItemResponse(
                r.SubmissionId, r.FormId, r.ProjectId, r.ProjectName, r.SubmittedAt, r.Status, map);
        }).ToList();

        FollowupConfigSummaryDto? configDto = config == null
            ? null
            : new FollowupConfigSummaryDto(config.FollowupFormId, config.FollowupFormName, config.AllowMultiple);

        return Result<SubmissionFollowupsResponse>.Ok(
            new SubmissionFollowupsResponse(configDto, columnDtos, itemDtos));
    }

    public async Task<Result<SubmissionDetailResponse>> CreateFollowupAsync(
        Guid parentSubmissionId, CreateFollowupRequest request, CancellationToken ct)
    {
        var err = RequireTenant<SubmissionDetailResponse>();
        if (err != null) return err;

        var perms = await GetPerms(ct);
        if (!perms.IsSuperAdmin && !perms.CanCreate)
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.Forbidden, "You do not have permission to create submissions.");

        var (parent, _) = await repository.GetByIdAsync(
            TenantId, UserId, parentSubmissionId,
            perms.IsSuperAdmin, perms.DataScope, ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds), ct);
        if (parent == null || parent.ParentSubmissionId != null)
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.NotFound, "Parent submission not found.");

        if (!await repository.UserCanAccessFormAsync(TenantId, UserId, parent.FormId, IsSuperAdmin, ct))
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.Forbidden, "You do not have access to this form.");

        var (config, _, _, _) = await repository.GetFollowupsAsync(
            TenantId, UserId, parentSubmissionId,
            perms.IsSuperAdmin, perms.DataScope, ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds), ct);
        if (config == null)
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.Validation, "No follow-up form is configured for this entry.");

        if (!await repository.UserCanAccessFormAsync(TenantId, UserId, config.FollowupFormId, IsSuperAdmin, ct))
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.Forbidden, "You do not have access to the follow-up form.");

        var validation = await ValidateAndBuildValuesAsync(config.FollowupFormId, request.Values, ct);
        if (validation.Error != null) return validation.Error;

        var id = Guid.NewGuid();
        try
        {
            await repository.SaveAsync(
                TenantId, UserId, id, config.FollowupFormId, validation.ValuesJson!, true, UserId, parentSubmissionId,
                perms.IsSuperAdmin, perms.DataScope, perms.CanCreate, perms.CanEdit,
                ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds),
                request.StateId, request.DistrictId, request.BlockId, request.VillageId, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Follow-up create failed");
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.Validation, CleanSqlMessage(ex.Message));
        }

        await TrySendEmailNotificationsAsync(config.FollowupFormId, id, validation.FormName!, ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<Result<SubmissionDetailResponse>> GetFollowupAsync(
        Guid parentSubmissionId, Guid followUpId, CancellationToken ct)
    {
        var err = RequireTenant<SubmissionDetailResponse>();
        if (err != null) return err;

        var detail = await GetByIdAsync(followUpId, ct);
        if (!detail.Success || detail.Data == null)
            return detail;

        if (detail.Data.ParentSubmissionId != parentSubmissionId)
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.NotFound, "Follow-up not found for this entry.");

        return detail;
    }

    public async Task<Result<SubmissionDetailResponse>> UpdateFollowupAsync(
        Guid parentSubmissionId, Guid followUpId, UpdateSubmissionRequest request, CancellationToken ct)
    {
        var err = RequireTenant<SubmissionDetailResponse>();
        if (err != null) return err;

        var perms = await GetPerms(ct);
        var (header, _) = await repository.GetByIdAsync(
            TenantId, UserId, followUpId,
            perms.IsSuperAdmin, perms.DataScope, ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds), ct);
        if (header == null || header.ParentSubmissionId != parentSubmissionId)
            return Result<SubmissionDetailResponse>.Fail(ErrorCode.NotFound, "Follow-up not found for this entry.");

        return await UpdateAsync(followUpId, request, ct);
    }

    public async Task<Result<bool>> DeleteFollowupAsync(Guid parentSubmissionId, Guid followUpId, CancellationToken ct)
    {
        var err = RequireTenant<bool>();
        if (err != null) return err;

        var perms = await GetPerms(ct);
        var (header, _) = await repository.GetByIdAsync(
            TenantId, UserId, followUpId,
            perms.IsSuperAdmin, perms.DataScope, ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds), ct);
        if (header == null || header.ParentSubmissionId != parentSubmissionId)
            return Result<bool>.Fail(ErrorCode.NotFound, "Follow-up not found for this entry.");

        return await DeleteAsync(followUpId, ct);
    }

    private async Task<(Result<SubmissionDetailResponse>? Error, string? ValuesJson, string? FormName)> ValidateAndBuildValuesAsync(
        Guid formId, IReadOnlyList<SubmissionValueDto>? values, CancellationToken ct)
    {
        if (!await repository.UserCanAccessFormAsync(TenantId, UserId, formId, IsSuperAdmin, ct))
            return (Result<SubmissionDetailResponse>.Fail(ErrorCode.Forbidden, "You do not have access to this form."), null, null);

        var (form, _, fields, options, parents) = await repository.GetFormDefinitionAsync(TenantId, formId, ct);
        if (form == null)
            return (Result<SubmissionDetailResponse>.Fail(ErrorCode.NotFound, "Form not found."), null, null);

        if (form.ProjectId is null || form.ProjectId == Guid.Empty)
            return (Result<SubmissionDetailResponse>.Fail(ErrorCode.Validation, "Form has no project assigned."), null, null);

        var projects = (await repository.GetProjectsForUserAsync(TenantId, UserId, ct)).ToList();
        if (projects.All(p => p.ProjectId != form.ProjectId.Value))
            return (Result<SubmissionDetailResponse>.Fail(ErrorCode.Validation, "Form project is not in your scope."), null, null);

        var valueMap = (values ?? [])
            .GroupBy(v => v.FieldId)
            .ToDictionary(g => g.Key, g => g.Last().ValueText?.Trim());

        var fieldById = fields.ToDictionary(f => f.FieldId);
        var optionsByField = options.GroupBy(o => o.FieldId).ToDictionary(g => g.Key, g => g.ToList());
        var parentOptsByField = parents.GroupBy(p => p.FieldId).ToDictionary(g => g.Key, g => g.Select(x => x.OptionId).ToHashSet());

        foreach (var field in fields)
        {
            if (!IsFieldVisible(field, valueMap, fieldById, optionsByField, parentOptsByField))
                continue;

            var raw = valueMap.GetValueOrDefault(field.FieldId);
            var empty = string.IsNullOrWhiteSpace(raw);

            if (field.ControlRequired && empty && field.ControlType != (byte)FormFieldControlType.Label)
                return (Result<SubmissionDetailResponse>.Fail(ErrorCode.Validation, $"'{field.ControlLabel}' is required."), null, null);

            if (empty) continue;

            if (field.ControlMaxLength is > 0 && raw!.Length > field.ControlMaxLength)
                return (Result<SubmissionDetailResponse>.Fail(ErrorCode.Validation, $"'{field.ControlLabel}' exceeds max length."), null, null);

            if (!string.IsNullOrWhiteSpace(field.ValidationRegexPattern))
            {
                try
                {
                    if (!Regex.IsMatch(raw!, field.ValidationRegexPattern))
                        return (Result<SubmissionDetailResponse>.Fail(ErrorCode.Validation,
                            $"'{field.ControlLabel}' does not match the required format ({field.ValidationRegexName ?? "pattern"})."), null, null);
                }
                catch (RegexParseException)
                {
                    // ignore bad preset at runtime
                }
            }

            if (NeedsOptions(field.ControlType) && optionsByField.TryGetValue(field.FieldId, out var opts))
            {
                var allowed = opts.Select(o => o.OptionValue).ToHashSet(StringComparer.OrdinalIgnoreCase);
                // Checkbox may be multi-value comma-separated
                var parts = raw!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 0 || parts.Any(p => !allowed.Contains(p)))
                    return (Result<SubmissionDetailResponse>.Fail(ErrorCode.Validation, $"'{field.ControlLabel}' has an invalid option."), null, null);
            }
        }

        var persist = fields
            .Where(f => IsFieldVisible(f, valueMap, fieldById, optionsByField, parentOptsByField))
            .Where(f => f.ControlType != (byte)FormFieldControlType.Label)
            .Select(f => new { fieldid = f.FieldId, valuetext = valueMap.GetValueOrDefault(f.FieldId) })
            .ToList();

        return (null, JsonSerializer.Serialize(persist), form.Name);
    }

    private static bool NeedsOptions(byte controlType) =>
        controlType is (byte)FormFieldControlType.Dropdown
            or (byte)FormFieldControlType.RadioButton
            or (byte)FormFieldControlType.Checkbox
            or (byte)FormFieldControlType.CheckboxSingle;

    private static bool IsFieldVisible(
        FormDefFieldRow field,
        Dictionary<Guid, string?> valueMap,
        Dictionary<Guid, FormDefFieldRow> fieldById,
        Dictionary<Guid, List<FormDefOptionRow>> optionsByField,
        Dictionary<Guid, HashSet<Guid>> parentOptsByField)
    {
        if (field.ParentFieldId == null) return true;
        if (!fieldById.TryGetValue(field.ParentFieldId.Value, out var parent)) return false;
        if (!parentOptsByField.TryGetValue(field.FieldId, out var allowedOptionIds) || allowedOptionIds.Count == 0)
            return false;

        var parentRaw = valueMap.GetValueOrDefault(parent.FieldId);
        if (string.IsNullOrWhiteSpace(parentRaw)) return false;

        if (!optionsByField.TryGetValue(parent.FieldId, out var parentOptions)) return false;

        var selectedValues = parentRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var selectedOptionIds = parentOptions
            .Where(o => selectedValues.Contains(o.OptionValue, StringComparer.OrdinalIgnoreCase))
            .Select(o => o.OptionId)
            .ToHashSet();

        return selectedOptionIds.Overlaps(allowedOptionIds);
    }

    private async Task TrySendEmailNotificationsAsync(Guid formId, Guid submissionId, string formName, CancellationToken ct)
    {
        try
        {
            var rows = await repository.GetEmailNotifyFieldsAsync(TenantId, formId, submissionId, ct);
            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.ValueText)) continue;
                await emailService.SendFormFieldNotificationAsync(row.ValueText.Trim(), formName, row.ControlLabel);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Form email notification failed for submission {SubmissionId}", submissionId);
        }
    }

    private static FormDefinitionFieldDto MapField(
        FormDefFieldRow f,
        IReadOnlyList<FormDefOptionRow> options,
        IReadOnlyList<FormDefParentOptionRow> parents) =>
        new(
            f.FieldId, f.FormGroupId, f.ControlLabel, f.ControlType, ControlTypeName(f.ControlType),
            f.ControlMaxLength, f.ControlRequired, f.DisplayOrder, f.ControlNotes, f.FieldKey,
            f.ClassName, f.ParentFieldId, f.IsSendEmailNotification, f.ValidationRegexPresetId,
            f.ValidationRegexPattern, f.ValidationRegexName,
            options.Where(o => o.FieldId == f.FieldId)
                .Select(o => new FormDefinitionOptionDto(o.OptionId, o.OptionText, o.OptionValue, o.DisplayOrder)).ToList(),
            parents.Where(p => p.FieldId == f.FieldId).Select(p => p.OptionId).ToList());

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

    private static string CleanSqlMessage(string message)
    {
        if (message.Contains("Form not found", StringComparison.OrdinalIgnoreCase)) return "Form not found, inactive, or has no project assigned.";
        if (message.Contains("Form project not found", StringComparison.OrdinalIgnoreCase)) return "Form project not found or inactive.";
        if (message.Contains("not in your scope", StringComparison.OrdinalIgnoreCase)) return "Form project is not in your scope.";
        if (message.Contains("Submission not found", StringComparison.OrdinalIgnoreCase)) return "Submission not found.";
        if (message.Contains("Parent submission not found", StringComparison.OrdinalIgnoreCase)) return "Parent submission not found.";
        if (message.Contains("not configured as a follow-up", StringComparison.OrdinalIgnoreCase))
            return "This form is not configured as a follow-up for the parent entry.";
        if (message.Contains("Only one follow-up", StringComparison.OrdinalIgnoreCase))
            return "Only one follow-up is allowed for this entry.";
        if (message.Contains("configured as a follow-up form", StringComparison.OrdinalIgnoreCase))
            return "This form is configured as a follow-up form and cannot be used for new primary entries.";
        return "Unable to save submission.";
    }
}
