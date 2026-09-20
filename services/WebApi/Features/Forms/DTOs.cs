namespace WebApi.Features.Forms;

public record FormResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ProjectId,
    string? ProjectName,
    string Status,
    byte StatusCode,
    int DisplayOrder,
    bool CollectLocation,
    IReadOnlyList<Guid> RoleIds,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record SaveFormRequest(
    string Name,
    string? Description,
    Guid ProjectId,
    byte Status,
    int DisplayOrder,
    bool CollectLocation,
    IReadOnlyList<Guid>? RoleIds);

public record FormGroupResponse(
    Guid Id,
    Guid FormId,
    string GroupName,
    string? GroupDisplayNameKey,
    int DisplayOrder);

public record SaveFormGroupRequest(
    string GroupName,
    string? GroupDisplayNameKey,
    int DisplayOrder);

public record FormFieldOptionDto(
    Guid? Id,
    string OptionText,
    string OptionValue,
    int DisplayOrder);

public record FormFieldResponse(
    Guid Id,
    Guid FormId,
    Guid FormGroupId,
    string ControlLabel,
    byte ControlType,
    string ControlTypeName,
    int? ControlMaxLength,
    bool ControlRequired,
    int DisplayOrder,
    string? ControlNotes,
    string FieldKey,
    string DisplayControlLabel,
    string? ClassName,
    Guid? ParentFieldId,
    bool IsSendEmailNotification,
    Guid? ValidationRegexPresetId,
    bool DisplayOnList,
    IReadOnlyList<FormFieldOptionDto> Options,
    IReadOnlyList<Guid> ParentOptionIds);

public record SaveFormFieldRequest(
    Guid FormGroupId,
    string ControlLabel,
    byte ControlType,
    int? ControlMaxLength,
    bool ControlRequired,
    int DisplayOrder,
    string? ControlNotes,
    string? FieldKey,
    string? DisplayControlLabel,
    string? ClassName,
    Guid? ParentFieldId,
    bool IsSendEmailNotification,
    Guid? ValidationRegexPresetId,
    bool DisplayOnList,
    IReadOnlyList<FormFieldOptionDto>? Options,
    IReadOnlyList<Guid>? ParentOptionIds);

public record ValidationRegexPresetResponse(
    Guid Id,
    string Name,
    string Pattern,
    string? Description,
    int DisplayOrder);

public record FormFollowupConfigResponse(
    Guid? Id,
    Guid PrimaryFormId,
    string? PrimaryFormName,
    Guid? FollowUpFormId,
    string? FollowUpFormName,
    bool AllowMultiple,
    Guid? UsedAsFollowUpForFormId,
    string? UsedAsFollowUpForFormName);

public record SaveFormFollowupConfigRequest(
    Guid? FollowUpFormId,
    bool AllowMultiple = true);
