namespace WebApi.Features.Roles;

public record RoleResponse(
    Guid Id,
    string Name,
    bool IsLeader,
    string Status,
    byte StatusCode,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record SaveRoleRequest(
    string Name,
    bool IsLeader,
    byte Status);
