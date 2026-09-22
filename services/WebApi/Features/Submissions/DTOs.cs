namespace WebApi.Features.Submissions;



public record AvailableFormResponse(Guid Id, string Name, string? Description, int DisplayOrder, bool CollectLocation);



public record SubmissionProjectResponse(Guid Id, string ProjectName, string? Code);



public record FormDefinitionOptionDto(Guid Id, string OptionText, string OptionValue, int DisplayOrder);



public record FormDefinitionFieldDto(

    Guid Id,

    Guid FormGroupId,

    string ControlLabel,

    byte ControlType,

    string ControlTypeName,

    int? ControlMaxLength,

    bool ControlRequired,

    int DisplayOrder,

    string? ControlNotes,

    string FieldKey,

    string? ClassName,

    Guid? ParentFieldId,

    bool IsSendEmailNotification,

    Guid? ValidationRegexPresetId,

    string? ValidationRegexPattern,

    string? ValidationRegexName,

    IReadOnlyList<FormDefinitionOptionDto> Options,

    IReadOnlyList<Guid> ParentOptionIds);



public record FormDefinitionGroupDto(

    Guid Id,

    string GroupName,

    string? GroupDisplayNameKey,

    int DisplayOrder,

    IReadOnlyList<FormDefinitionFieldDto> Fields);



public record FormDefinitionResponse(

    Guid FormId,

    string Name,

    string? Description,

    Guid? ProjectId,

    string? ProjectName,

    bool CollectLocation,

    bool HasFollowUp,

    IReadOnlyList<FormDefinitionGroupDto> Groups);



public record SubmissionListColumnDto(Guid FieldId, string Label);



public record SubmissionListItemResponse(

    Guid Id,

    Guid FormId,

    Guid ProjectId,

    string ProjectName,

    DateTime SubmittedAt,

    byte Status,

    IReadOnlyDictionary<string, string?> Values);



public record SubmissionListPageResponse(

    int TotalCount,

    int Page,

    int PageSize,

    IReadOnlyList<SubmissionListColumnDto> Columns,

    IReadOnlyList<SubmissionListItemResponse> Items);



public record SubmissionValueDto(Guid FieldId, string? ValueText);



public record SubmissionDetailResponse(

    Guid Id,

    Guid FormId,

    Guid ProjectId,

    string ProjectName,

    DateTime SubmittedAt,

    byte Status,

    Guid? ParentSubmissionId,

    Guid? StateId,

    Guid? DistrictId,

    Guid? BlockId,

    Guid? VillageId,

    IReadOnlyList<SubmissionValueDto> Values);



public record SaveSubmissionRequest(

    Guid FormId,

    IReadOnlyList<SubmissionValueDto>? Values,

    Guid? StateId,

    Guid? DistrictId,

    Guid? BlockId,

    Guid? VillageId);



public record UpdateSubmissionRequest(

    IReadOnlyList<SubmissionValueDto>? Values,

    Guid? StateId,

    Guid? DistrictId,

    Guid? BlockId,

    Guid? VillageId);



public record SubmissionExportFile(byte[] Content, string FileName);



public record FollowupConfigSummaryDto(

    Guid FollowUpFormId,

    string? FollowUpFormName,

    bool AllowMultiple);



public record SubmissionFollowupsResponse(

    FollowupConfigSummaryDto? Config,

    IReadOnlyList<SubmissionListColumnDto> Columns,

    IReadOnlyList<SubmissionListItemResponse> Items);



public record CreateFollowupRequest(

    IReadOnlyList<SubmissionValueDto>? Values,

    Guid? StateId,

    Guid? DistrictId,

    Guid? BlockId,

    Guid? VillageId);


