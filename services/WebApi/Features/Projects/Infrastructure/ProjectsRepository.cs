using Dapper;
using SharedKernel.Utilities.Helpers;
using System.Data;

namespace WebApi.Features.Projects.Infrastructure;

public class ProjectRow
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? Code { get; set; }
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public interface IProjectsRepository
{
    Task<IEnumerable<ProjectRow>> GetListAsync(Guid tenantId, CancellationToken ct);
    Task<ProjectRow?> GetByIdAsync(Guid tenantId, Guid projectId, CancellationToken ct);
    Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId, CancellationToken ct);
    Task<Guid> CreateAsync(Guid tenantId, Guid projectId, string projectName, string? code, byte status, Guid createdBy, CancellationToken ct);
    Task UpdateAsync(Guid tenantId, Guid projectId, string projectName, string? code, byte status, Guid updatedBy, CancellationToken ct);
    Task DeleteAsync(Guid tenantId, Guid projectId, Guid updatedBy, CancellationToken ct);
}

public class ProjectsRepository(DbHelper dbHelper) : IProjectsRepository
{
    public async Task<IEnumerable<ProjectRow>> GetListAsync(Guid tenantId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryAsync<ProjectRow>(
            "dbo.sp_project_get_list",
            new { tenantid = tenantId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<ProjectRow?> GetByIdAsync(Guid tenantId, Guid projectId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<ProjectRow>(
            "dbo.sp_project_get_by_id",
            new { tenantid = tenantId, projectid = projectId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "dbo.sp_project_name_exists",
            new { tenantid = tenantId, name, excludeid = excludeId },
            commandType: CommandType.StoredProcedure);
        return count > 0;
    }

    public async Task<Guid> CreateAsync(Guid tenantId, Guid projectId, string projectName, string? code, byte status, Guid createdBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<Guid>(
            "dbo.sp_project_create",
            new { projectid = projectId, tenantid = tenantId, projectname = projectName, code, status, createdby = createdBy },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateAsync(Guid tenantId, Guid projectId, string projectName, string? code, byte status, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_project_update",
            new { tenantid = tenantId, projectid = projectId, projectname = projectName, code, status, updatedby = updatedBy },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(Guid tenantId, Guid projectId, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_project_delete",
            new { tenantid = tenantId, projectid = projectId, updatedby = updatedBy },
            commandType: CommandType.StoredProcedure);
    }
}
