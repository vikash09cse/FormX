using Dapper;
using SharedKernel.Utilities.Helpers;
using System.Data;

namespace WebApi.Features.DocumentTemplates.Infrastructure;

public class DocumentTemplatesRepository(DbHelper dbHelper) : IDocumentTemplatesRepository
{
    public async Task<IEnumerable<DocumentTemplateRow>> GetListAsync(
        Guid tenantId, byte? templateType, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryAsync<DocumentTemplateRow>(
            "dbo.sp_document_template_get_list",
            new { tenantid = tenantId, templatetype = templateType },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<DocumentTemplateRow?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<DocumentTemplateRow>(
            "dbo.sp_document_template_get_by_id",
            new { tenantid = tenantId, documenttemplateid = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<DocumentTemplateRow?> GetDefaultAsync(Guid tenantId, byte templateType, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<DocumentTemplateRow>(
            "dbo.sp_document_template_get_default",
            new { tenantid = tenantId, templatetype = templateType },
            commandType: CommandType.StoredProcedure);
    }

    public async Task SaveAsync(
        Guid tenantId,
        Guid id,
        string name,
        string? subject,
        string bodyHtml,
        Guid actorId,
        CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_document_template_save",
            new
            {
                tenantid = tenantId,
                documenttemplateid = id,
                name,
                subject,
                bodyhtml = bodyHtml,
                actorid = actorId
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task SetDefaultAsync(Guid tenantId, Guid id, Guid actorId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_document_template_set_default",
            new { tenantid = tenantId, documenttemplateid = id, actorid = actorId },
            commandType: CommandType.StoredProcedure);
    }
}
