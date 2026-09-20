using Dapper;
using SharedKernel.Utilities.Helpers;
using System.Data;

namespace WebApi.Features.Dashboard.Infrastructure;

public class DashboardSettingsRow
{
    public string DashboardLayout { get; set; } = "table";
    public int DashboardTopN { get; set; } = 12;
}

public class DashboardTotalsRow
{
    public int FormCount { get; set; }
    public int EntryCount { get; set; }
}

public class DashboardFormCountRow
{
    public Guid FormId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public int EntryCount { get; set; }
    public int DisplayOrder { get; set; }
}

public class DashboardFormIdRow
{
    public Guid FormId { get; set; }
}

public class DashboardAvailableFormRow
{
    public Guid FormId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public interface IDashboardRepository
{
    Task<(
        DashboardSettingsRow Settings,
        DashboardTotalsRow Totals,
        IReadOnlyList<DashboardFormCountRow> Forms,
        IReadOnlyList<Guid> SelectedFormIds,
        IReadOnlyList<DashboardAvailableFormRow> AvailableForms)>
        GetAsync(Guid tenantId, Guid userId, bool isSuperAdmin, byte dataScope, string districtIds, string projectIds, CancellationToken ct);

    Task<(DashboardSettingsRow Settings, IReadOnlyList<Guid> SelectedFormIds)> SaveSettingsAsync(
        Guid tenantId, string layout, int topN, string? formIdsCsv, Guid actorId, CancellationToken ct);
}

public class DashboardRepository(DbHelper dbHelper) : IDashboardRepository
{
    public async Task<(
        DashboardSettingsRow Settings,
        DashboardTotalsRow Totals,
        IReadOnlyList<DashboardFormCountRow> Forms,
        IReadOnlyList<Guid> SelectedFormIds,
        IReadOnlyList<DashboardAvailableFormRow> AvailableForms)>
        GetAsync(Guid tenantId, Guid userId, bool isSuperAdmin, byte dataScope, string districtIds, string projectIds, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        using var multi = await conn.QueryMultipleAsync(
            "dbo.sp_tenant_dashboard_get",
            new
            {
                tenantid = tenantId,
                userid = userId,
                issuperadmin = isSuperAdmin,
                datascope = dataScope,
                districtids = districtIds,
                projectids = projectIds
            },
            commandType: CommandType.StoredProcedure);

        var settings = await multi.ReadFirstOrDefaultAsync<DashboardSettingsRow>()
            ?? new DashboardSettingsRow();
        var totals = await multi.ReadFirstOrDefaultAsync<DashboardTotalsRow>()
            ?? new DashboardTotalsRow();
        var forms = (await multi.ReadAsync<DashboardFormCountRow>()).ToList();
        var selected = (await multi.ReadAsync<DashboardFormIdRow>()).Select(r => r.FormId).ToList();
        var available = (await multi.ReadAsync<DashboardAvailableFormRow>()).ToList();
        return (settings, totals, forms, selected, available);
    }

    public async Task<(DashboardSettingsRow Settings, IReadOnlyList<Guid> SelectedFormIds)> SaveSettingsAsync(
        Guid tenantId, string layout, int topN, string? formIdsCsv, Guid actorId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        using var multi = await conn.QueryMultipleAsync(
            "dbo.sp_tenant_dashboard_settings_save",
            new
            {
                tenantid = tenantId,
                dashboardlayout = layout,
                dashboardtopn = topN,
                actorid = actorId,
                formids = formIdsCsv
            },
            commandType: CommandType.StoredProcedure);

        var settings = await multi.ReadFirstAsync<DashboardSettingsRow>();
        var selected = (await multi.ReadAsync<DashboardFormIdRow>()).Select(r => r.FormId).ToList();
        return (settings, selected);
    }
}
