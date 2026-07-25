namespace WebApi.Features.Projects;

public record ProjectResponse(
    Guid Id,
    string ProjectName,
    string? Code,
    string Status,
    byte StatusCode,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record SaveProjectRequest(
    string ProjectName,
    string? Code,
    byte Status);
