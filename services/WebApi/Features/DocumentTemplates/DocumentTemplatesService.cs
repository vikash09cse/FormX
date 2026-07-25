using SharedKernel.Enums;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Extensions;
using WebApi.Features.DocumentTemplates.Infrastructure;

namespace WebApi.Features.DocumentTemplates;

public class DocumentTemplatesService(
    IDocumentTemplatesRepository repository,
    IHttpContextAccessor httpContextAccessor)
{
    public async Task<Result<IEnumerable<DocumentTemplateResponse>>> GetListAsync(
        byte? templateType, CancellationToken ct)
    {
        var tenantError = RequireTenantContext<IEnumerable<DocumentTemplateResponse>>();
        if (tenantError != null) return tenantError;

        if (templateType is not null && !Enum.IsDefined(typeof(DocumentTemplateType), templateType.Value))
            return Result<IEnumerable<DocumentTemplateResponse>>.Fail(ErrorCode.Validation, "Invalid template type.");

        var rows = await repository.GetListAsync(GetTenantId(), templateType, ct);
        return Result<IEnumerable<DocumentTemplateResponse>>.Ok(rows.Select(Map));
    }

    public async Task<Result<DocumentTemplateResponse>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var tenantError = RequireTenantContext<DocumentTemplateResponse>();
        if (tenantError != null) return tenantError;

        var row = await repository.GetByIdAsync(GetTenantId(), id, ct);
        if (row == null)
            return Result<DocumentTemplateResponse>.Fail(ErrorCode.NotFound, "Document template not found.");
        return Result<DocumentTemplateResponse>.Ok(Map(row));
    }

    public async Task<Result<DocumentTemplateResponse>> UpdateAsync(
        Guid id, UpdateDocumentTemplateRequest request, CancellationToken ct)
    {
        var tenantError = RequireTenantContext<DocumentTemplateResponse>();
        if (tenantError != null) return tenantError;

        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<DocumentTemplateResponse>.Fail(ErrorCode.Validation, "Name is required.");
        if (request.Name.Trim().Length > 200)
            return Result<DocumentTemplateResponse>.Fail(ErrorCode.Validation, "Name cannot exceed 200 characters.");
        if (string.IsNullOrWhiteSpace(request.BodyHtml))
            return Result<DocumentTemplateResponse>.Fail(ErrorCode.Validation, "Body is required.");
        if (request.Subject?.Length > 300)
            return Result<DocumentTemplateResponse>.Fail(ErrorCode.Validation, "Subject cannot exceed 300 characters.");

        var existing = await repository.GetByIdAsync(GetTenantId(), id, ct);
        if (existing == null)
            return Result<DocumentTemplateResponse>.Fail(ErrorCode.NotFound, "Document template not found.");

        if (existing.TemplateType == (byte)DocumentTemplateType.Email && string.IsNullOrWhiteSpace(request.Subject))
            return Result<DocumentTemplateResponse>.Fail(ErrorCode.Validation, "Email subject is required.");

        await repository.SaveAsync(
            GetTenantId(), id, request.Name.Trim(),
            string.IsNullOrWhiteSpace(request.Subject) ? null : request.Subject.Trim(),
            request.BodyHtml.Trim(), GetUserId(), ct);

        var updated = await repository.GetByIdAsync(GetTenantId(), id, ct);
        return Result<DocumentTemplateResponse>.Ok(Map(updated!), "Template updated.");
    }

    public async Task<Result<bool>> SetDefaultAsync(Guid id, CancellationToken ct)
    {
        var tenantError = RequireTenantContext<bool>();
        if (tenantError != null) return tenantError;

        var existing = await repository.GetByIdAsync(GetTenantId(), id, ct);
        if (existing == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Document template not found.");

        await repository.SetDefaultAsync(GetTenantId(), id, GetUserId(), ct);
        return Result<bool>.Ok(true, "Default template updated.");
    }

    private Result<T>? RequireTenantContext<T>()
    {
        var ctx = httpContextAccessor.HttpContext?.TryGetTenantContext();
        if (ctx == null || !ctx.IsValidForTenantScope())
            return Result<T>.Fail(ErrorCode.Forbidden, "Tenant context is required.");
        return null;
    }

    private Guid GetTenantId() => httpContextAccessor.GetTenantContext().TenantId;
    private Guid GetUserId() => httpContextAccessor.GetTenantContext().UserId;

    private static DocumentTemplateResponse Map(DocumentTemplateRow row) => new(
        row.DocumentTemplateId,
        row.GlobalDocumentTemplateId,
        row.TemplateType,
        row.TemplateType == (byte)DocumentTemplateType.Receipt ? "Receipt" : "Email",
        row.Name,
        row.Subject,
        row.BodyHtml,
        row.IsDefault,
        row.CreatedAt,
        row.UpdatedAt);
}
