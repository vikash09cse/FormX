namespace WebApi.Features.Users;

public record CreateTenantUserRequest(
    string Email,
    string FirstName,
    string LastName,
    byte Role,
    string? TemporaryPassword,
    string? Designation,
    IReadOnlyList<Guid>? RoleIds,
    IReadOnlyList<Guid>? ProjectScopeIds);

public record UpdateTenantUserRequest(
    string FirstName,
    string LastName,
    byte Role,
    string? Designation,
    IReadOnlyList<Guid>? RoleIds,
    IReadOnlyList<Guid>? ProjectScopeIds);

public record TenantUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Designation,
    string Role,
    byte RoleCode,
    string Status,
    byte StatusCode,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    IReadOnlyList<Guid> RoleIds,
    IReadOnlyList<Guid> ProjectScopeIds);

public record TenantUserListResponse(
    IEnumerable<TenantUserResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
