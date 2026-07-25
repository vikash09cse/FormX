namespace WebApi.Features.GlobalDocumentTemplates.Infrastructure;

public class GlobalDocumentTemplateRow
{
    public Guid GlobalDocumentTemplateId { get; set; }
    public byte TemplateType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string BodyHtml { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public interface IGlobalDocumentTemplatesRepository
{
    Task<IEnumerable<GlobalDocumentTemplateRow>> GetListAsync(byte? templateType, CancellationToken ct);
    Task<GlobalDocumentTemplateRow?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Guid> SaveAsync(
        Guid? id,
        byte templateType,
        string name,
        string? subject,
        string bodyHtml,
        bool isDefault,
        Guid actorId,
        CancellationToken ct);
    Task SetDefaultAsync(Guid id, Guid actorId, CancellationToken ct);
    Task DeleteAsync(Guid id, Guid actorId, CancellationToken ct);
}
