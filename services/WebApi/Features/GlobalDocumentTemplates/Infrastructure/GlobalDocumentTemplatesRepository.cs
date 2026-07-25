using Dapper;
using SharedKernel.Utilities.Helpers;
using System.Data;

namespace WebApi.Features.GlobalDocumentTemplates.Infrastructure;

public class GlobalDocumentTemplatesRepository(DbHelper dbHelper) : IGlobalDocumentTemplatesRepository
{
    public async Task<IEnumerable<GlobalDocumentTemplateRow>> GetListAsync(byte? templateType, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryAsync<GlobalDocumentTemplateRow>(
            "dbo.sp_global_document_template_get_list",
            new { templatetype = templateType },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<GlobalDocumentTemplateRow?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<GlobalDocumentTemplateRow>(
            "dbo.sp_global_document_template_get_by_id",
            new { globaldocumenttemplateid = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Guid> SaveAsync(
        Guid? id,
        byte templateType,
        string name,
        string? subject,
        string bodyHtml,
        bool isDefault,
        Guid actorId,
        CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var result = await conn.QueryFirstAsync<dynamic>(
            "dbo.sp_global_document_template_save",
            new
            {
                globaldocumenttemplateid = id,
                templatetype = templateType,
                name,
                subject,
                bodyhtml = bodyHtml,
                isdefault = isDefault,
                actorid = actorId
            },
            commandType: CommandType.StoredProcedure);
        return (Guid)result.globaldocumenttemplateid;
    }

    public async Task SetDefaultAsync(Guid id, Guid actorId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_global_document_template_set_default",
            new { globaldocumenttemplateid = id, actorid = actorId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(Guid id, Guid actorId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_global_document_template_delete",
            new { globaldocumenttemplateid = id, actorid = actorId },
            commandType: CommandType.StoredProcedure);
    }
}
