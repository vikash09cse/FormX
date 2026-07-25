using SharedKernel.Enums;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Extensions;
using WebApi.Features.GlobalDocumentTemplates.Infrastructure;

namespace WebApi.Features.GlobalDocumentTemplates;

public class GlobalDocumentTemplatesService(
    IGlobalDocumentTemplatesRepository repository,
    IHttpContextAccessor httpContextAccessor)
{
    public async Task<Result<IEnumerable<GlobalDocumentTemplateResponse>>> GetListAsync(
        byte? templateType, CancellationToken ct)
    {
        if (templateType is not null && !Enum.IsDefined(typeof(DocumentTemplateType), templateType.Value))
            return Result<IEnumerable<GlobalDocumentTemplateResponse>>.Fail(ErrorCode.Validation, "Invalid template type.");

        var rows = await repository.GetListAsync(templateType, ct);
        return Result<IEnumerable<GlobalDocumentTemplateResponse>>.Ok(rows.Select(Map));
    }

    public async Task<Result<GlobalDocumentTemplateResponse>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var row = await repository.GetByIdAsync(id, ct);
        if (row == null)
            return Result<GlobalDocumentTemplateResponse>.Fail(ErrorCode.NotFound, "Global template not found.");
        return Result<GlobalDocumentTemplateResponse>.Ok(Map(row));
    }

    public async Task<Result<GlobalDocumentTemplateResponse>> CreateAsync(
        SaveGlobalDocumentTemplateRequest request, CancellationToken ct)
    {
        var validation = Validate(request);
        if (validation != null) return validation;

        var id = await repository.SaveAsync(
            null, request.TemplateType, request.Name.Trim(),
            TrimOrNull(request.Subject), request.BodyHtml.Trim(),
            request.IsDefault, GetUserId(), ct);

        var created = await repository.GetByIdAsync(id, ct);
        return Result<GlobalDocumentTemplateResponse>.Ok(Map(created!), "Global template created and copied to all tenants.");
    }

    public async Task<Result<GlobalDocumentTemplateResponse>> UpdateAsync(
        Guid id, SaveGlobalDocumentTemplateRequest request, CancellationToken ct)
    {
        var validation = Validate(request);
        if (validation != null) return validation;

        var existing = await repository.GetByIdAsync(id, ct);
        if (existing == null)
            return Result<GlobalDocumentTemplateResponse>.Fail(ErrorCode.NotFound, "Global template not found.");

        await repository.SaveAsync(
            id, request.TemplateType, request.Name.Trim(),
            TrimOrNull(request.Subject), request.BodyHtml.Trim(),
            request.IsDefault, GetUserId(), ct);

        var updated = await repository.GetByIdAsync(id, ct);
        return Result<GlobalDocumentTemplateResponse>.Ok(Map(updated!), "Global template updated.");
    }

    public async Task<Result<bool>> SetDefaultAsync(Guid id, CancellationToken ct)
    {
        var existing = await repository.GetByIdAsync(id, ct);
        if (existing == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Global template not found.");

        await repository.SetDefaultAsync(id, GetUserId(), ct);
        return Result<bool>.Ok(true, "Default template updated.");
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken ct)
    {
        var existing = await repository.GetByIdAsync(id, ct);
        if (existing == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Global template not found.");

        await repository.DeleteAsync(id, GetUserId(), ct);
        return Result<bool>.Ok(true, "Global template deleted.");
    }

    private Result<GlobalDocumentTemplateResponse>? Validate(SaveGlobalDocumentTemplateRequest request)
    {
        if (!Enum.IsDefined(typeof(DocumentTemplateType), request.TemplateType))
            return Result<GlobalDocumentTemplateResponse>.Fail(ErrorCode.Validation, "Invalid template type.");
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<GlobalDocumentTemplateResponse>.Fail(ErrorCode.Validation, "Name is required.");
        if (request.Name.Trim().Length > 200)
            return Result<GlobalDocumentTemplateResponse>.Fail(ErrorCode.Validation, "Name cannot exceed 200 characters.");
        if (string.IsNullOrWhiteSpace(request.BodyHtml))
            return Result<GlobalDocumentTemplateResponse>.Fail(ErrorCode.Validation, "Body is required.");
        if (request.TemplateType == (byte)DocumentTemplateType.Email && string.IsNullOrWhiteSpace(request.Subject))
            return Result<GlobalDocumentTemplateResponse>.Fail(ErrorCode.Validation, "Email subject is required.");
        if (request.Subject?.Length > 300)
            return Result<GlobalDocumentTemplateResponse>.Fail(ErrorCode.Validation, "Subject cannot exceed 300 characters.");
        return null;
    }

    private Guid GetUserId()
    {
        var ctx = httpContextAccessor.HttpContext?.TryGetTenantContext();
        if (ctx == null || ctx.UserId == Guid.Empty)
            throw new InvalidOperationException("User context is required.");
        return ctx.UserId;
    }

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static GlobalDocumentTemplateResponse Map(GlobalDocumentTemplateRow row) => new(
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
