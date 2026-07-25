namespace WebApi.Features.DocumentTemplates.Infrastructure;

public class DocumentTemplateRow
{
    public Guid DocumentTemplateId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? GlobalDocumentTemplateId { get; set; }
    public byte TemplateType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string BodyHtml { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public interface IDocumentTemplatesRepository
{
    Task<IEnumerable<DocumentTemplateRow>> GetListAsync(Guid tenantId, byte? templateType, CancellationToken ct);
    Task<DocumentTemplateRow?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task<DocumentTemplateRow?> GetDefaultAsync(Guid tenantId, byte templateType, CancellationToken ct);
    Task SaveAsync(
        Guid tenantId,
        Guid id,
        string name,
        string? subject,
        string bodyHtml,
        Guid actorId,
        CancellationToken ct);
    Task SetDefaultAsync(Guid tenantId, Guid id, Guid actorId, CancellationToken ct);
}
