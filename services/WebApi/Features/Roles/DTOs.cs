namespace WebApi.Features.Roles;

public record RoleResponse(
    Guid Id,
    string Name,
    byte DataScope,
    string DataScopeLabel,
    bool CanCreate,
    bool CanEdit,
    bool CanDelete,
    IReadOnlyList<string> Menus,
    string Status,
    byte StatusCode,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record SaveRoleRequest(
    string Name,
    byte DataScope,
    bool CanCreate,
    bool CanEdit,
    bool CanDelete,
    IReadOnlyList<string>? Menus,
    byte Status);
