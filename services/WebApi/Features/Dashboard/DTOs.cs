namespace WebApi.Features.Dashboard;

public record DashboardSettingsDto(
    string Layout,
    int TopN,
    IReadOnlyList<Guid> SelectedFormIds);

public record DashboardTotalsDto(int FormCount, int EntryCount);

public record DashboardFormCountDto(
    Guid Id,
    string Name,
    string? ProjectName,
    int EntryCount,
    int DisplayOrder);

public record DashboardAvailableFormDto(Guid Id, string Name, int DisplayOrder);

public record DashboardResponse(
    DashboardSettingsDto Settings,
    DashboardTotalsDto Totals,
    IReadOnlyList<DashboardFormCountDto> Forms,
    IReadOnlyList<DashboardAvailableFormDto> AvailableForms);

public record SaveDashboardSettingsRequest(
    string Layout,
    int TopN = 12,
    IReadOnlyList<Guid>? FormIds = null);
