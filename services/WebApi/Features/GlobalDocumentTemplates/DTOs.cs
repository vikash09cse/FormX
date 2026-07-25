namespace WebApi.Features.GlobalDocumentTemplates;

public record SaveGlobalDocumentTemplateRequest(
    byte TemplateType,
    string Name,
    string? Subject,
    string BodyHtml,
    bool IsDefault = false);

public record GlobalDocumentTemplateResponse(
    Guid Id,
    byte TemplateType,
    string TemplateTypeName,
    string Name,
    string? Subject,
    string BodyHtml,
    bool IsDefault,
    DateTime CreatedAt,
    DateTime UpdatedAt);
