namespace WebApi.Features.DocumentTemplates;

public record UpdateDocumentTemplateRequest(
    string Name,
    string? Subject,
    string BodyHtml);

public record DocumentTemplateResponse(
    Guid Id,
    Guid? GlobalDocumentTemplateId,
    byte TemplateType,
    string TemplateTypeName,
    string Name,
    string? Subject,
    string BodyHtml,
    bool IsDefault,
    DateTime CreatedAt,
    DateTime UpdatedAt);
