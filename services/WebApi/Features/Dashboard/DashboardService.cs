using SharedKernel.Enums;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Extensions;
using WebApi.Features.Dashboard.Infrastructure;
using WebApi.Features.Permissions;

namespace WebApi.Features.Dashboard;

public class DashboardService(
    IDashboardRepository repository,
    IPermissionsRepository permissionsRepository,
    IHttpContextAccessor httpContextAccessor)
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

    private static string ToCsv(IReadOnlyList<Guid> ids) =>
        ids.Count == 0 ? string.Empty : string.Join(',', ids);

    public async Task<Result<DashboardResponse>> GetAsync(CancellationToken ct)
    {
        var err = RequireTenant<DashboardResponse>();
        if (err != null) return err;

        var perms = await permissionsRepository.GetForUserAsync(TenantId, UserId, IsSuperAdmin, ct);
        var (settings, totals, forms, selected, available) =
            await repository.GetAsync(
                TenantId, UserId, IsSuperAdmin,
                perms.DataScope, ToCsv(perms.DistrictIds), ToCsv(perms.ProjectIds), ct);
        return Result<DashboardResponse>.Ok(Map(settings, totals, forms, selected, available));
    }

    public async Task<Result<DashboardSettingsDto>> SaveSettingsAsync(
        SaveDashboardSettingsRequest request, CancellationToken ct)
    {
        var err = RequireTenant<DashboardSettingsDto>();
        if (err != null) return err;

        var layout = (request.Layout ?? string.Empty).Trim();
        if (layout.Equals("topn", StringComparison.OrdinalIgnoreCase))
            layout = "topN";
        else if (layout.Equals("cards", StringComparison.OrdinalIgnoreCase))
            layout = "cards";
        else if (layout.Equals("table", StringComparison.OrdinalIgnoreCase))
            layout = "table";
        else
            return Result<DashboardSettingsDto>.Fail(
                ErrorCode.Validation,
                "Layout must be table, cards, or topN.");

        var topN = request.TopN;
        if (topN < 5 || topN > 50)
            return Result<DashboardSettingsDto>.Fail(ErrorCode.Validation, "Top N must be between 5 and 50.");

        var formIds = (request.FormIds ?? [])
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();
        var formIdsCsv = formIds.Count == 0
            ? null
            : string.Join(',', formIds);

        try
        {
            var (row, selected) = await repository.SaveSettingsAsync(
                TenantId, layout, topN, formIdsCsv, UserId, ct);
            return Result<DashboardSettingsDto>.Ok(
                new DashboardSettingsDto(NormalizeLayout(row.DashboardLayout), row.DashboardTopN, selected),
                "Dashboard settings saved.");
        }
        catch (Exception ex)
        {
            return Result<DashboardSettingsDto>.Fail(ErrorCode.Validation, CleanSqlMessage(ex.Message));
        }
    }

    private static DashboardResponse Map(
        DashboardSettingsRow settings,
        DashboardTotalsRow totals,
        IReadOnlyList<DashboardFormCountRow> forms,
        IReadOnlyList<Guid> selectedFormIds,
        IReadOnlyList<DashboardAvailableFormRow> availableForms) =>
        new(
            new DashboardSettingsDto(
                NormalizeLayout(settings.DashboardLayout),
                settings.DashboardTopN,
                selectedFormIds),
            new DashboardTotalsDto(totals.FormCount, totals.EntryCount),
            forms.Select(f => new DashboardFormCountDto(
                f.FormId, f.Name, f.ProjectName, f.EntryCount, f.DisplayOrder)).ToList(),
            availableForms.Select(f => new DashboardAvailableFormDto(f.FormId, f.Name, f.DisplayOrder)).ToList());

    private static string NormalizeLayout(string? layout)
    {
        if (string.IsNullOrWhiteSpace(layout)) return "table";
        if (layout.Equals("cards", StringComparison.OrdinalIgnoreCase)) return "cards";
        if (layout.Equals("topN", StringComparison.OrdinalIgnoreCase) ||
            layout.Equals("topn", StringComparison.OrdinalIgnoreCase))
            return "topN";
        return "table";
    }

    private static string CleanSqlMessage(string message)
    {
        if (message.Contains("Invalid dashboard layout", StringComparison.OrdinalIgnoreCase))
            return "Layout must be table, cards, or topN.";
        if (message.Contains("Top N must be between", StringComparison.OrdinalIgnoreCase))
            return "Top N must be between 5 and 50.";
        return "Unable to save dashboard settings.";
    }
}
